import { useEffect, useMemo, useState } from "react";
import Avatar from "../ui/avatar/Avatar";
import { useAuth, useAuthenticatedUser } from "../../context/AuthContext";
import { useMerchantsQuery } from "../../hooks/queries/implementations/useMerchantsQuery";
import { Modal, ModalSize } from "../ui/modal";
import { useTranslation } from "react-i18next";
import { createPortal } from "react-dom";
import { QueryPagination } from "../pagination/QueryPagination";
import { useToast } from "../../layout/ToastProvider";
import { Merchant } from "../../hooks/api/Dtos/merchants/Merchant";
import Button from "../ui/button/Button";
import { useNavigate, useParams } from "react-router";
import { Spinner } from "../spinners/Spinner";

interface Props {
    readonly collapsed?: boolean;
    readonly className?: string;
}
export const SwitchMerchantButton = (props: Props) => {
    const user = useAuthenticatedUser();
    
    const merchantId = user.subMerchantId ?? user.merchantId;
    const merchantQuery = useMerchantsQuery(merchantId == undefined ? undefined : {
        ids: [merchantId],
        page: 0,
    })
    const merchant = useMemo(() => merchantQuery.data.length == 0 ? undefined : merchantQuery.data[0], [merchantQuery.data]);
    
    const [isOpen, setIsOpen] = useState(false);

    if(merchantId == undefined || merchant == undefined) {
        return <></>
    }

    return <>
        {
            props.collapsed == true
            ?
            <div
                className="cursor-pointer"
                onClick={() => setIsOpen(true)}
            >
                <Avatar src={merchant.logoUrl} alt={merchant.name} size="large" />
            </div>
            :
            <div 
                className={`rounded-2xl border border-gray-200 p-3 lg:p-4 dark:border-gray-800 cursor-pointer hover:bg-brand-500/[0.12] hover:border-brand-500 ${props.className ?? ""}`}
                onClick={() => setIsOpen(true)}
            >
                <div className="flex w-full flex-row items-center gap-6">
                    <Avatar src={merchant.logoUrl} alt={merchant.name} size="large" />
                    <div className="flex-1">
                        <h4 className="text-center text-lg font-semibold text-gray-800 dark:text-white/90">
                            {merchant.name}
                        </h4>
                    </div>
                </div>
            </div>
        }
        {createPortal(<SwitchMerchantModal isOpen={isOpen} onClose={() => setIsOpen(false)} />, document.body)}
    </>
};

interface SwitchMerchantModalProps {
    readonly isOpen: boolean;
    readonly onClose: () => any;
}

const defaultState = {
    parentMerchant: undefined as Merchant | undefined,
    page: 0,
    pageSize: 6,
}
const SwitchMerchantModal = (props: SwitchMerchantModalProps) => {
    const { t } = useTranslation();
    const { id } = useParams();
    
    const auth = useAuth();
    const toast = useToast();
    const navigate = useNavigate();

    const [state, setState] = useState(defaultState)

    const merchantsQuery = useMerchantsQuery({
        isParent: state.parentMerchant == undefined,
        parentId: state.parentMerchant?.id,
        page: state.page,
        pageSize: state.pageSize,
    })

    useEffect(() => {
        if(props.isOpen) {
            setState(defaultState);
        }
    }, [props.isOpen])

    const switchToMerchant = async (m: Merchant) => {
        try {
            await auth.switchMerchant(m.id);
            toast.success(t("sidebar.switchMerchant.success", {
                name: m.name,
            }))
            props.onClose();

            if(id != undefined) {
                navigate("/");
            }
        } catch {
            toast.error(t("common.operations.failure.generic"));
        }
    }

    return <Modal
        isOpen={props.isOpen}
        onClose={props.onClose}
        title={t("sidebar.switchMerchant.title")}
        size={ModalSize.Auto}
    >
        <nav>
            <ol className="flex flex-wrap items-center gap-1.5">
                <li 
                    className={`flex items-center gap-1.5 text-sm text-gray-500 hover:text-brand-500 dark:text-gray-400 dark:hover:text-brand-400 ${state.parentMerchant != undefined ? "cursor-pointer" : ""}`}
                    onClick={() => setState(s => s.parentMerchant == undefined ? s : ({...s, parentMerchant: undefined}))}
                >
                    {t("common.all")}
                </li>
                {
                    state.parentMerchant != undefined &&
                    <li className="flex items-center gap-1.5 text-sm text-gray-800 dark:text-white/90">
                        <span> / </span>
                        <span>{state.parentMerchant.name}</span>
                    </li>
                }
            </ol>
        </nav>

        <div className="grid grid-cols-1 gap-5 sm:grid-cols-2 xl:grid-cols-6">
            {
                merchantsQuery.data.map(m => <MerchantCard
                    key={m.id}
                    merchant={m}
                    onCardClick={async () => {
                        if(m.parentId == undefined) {
                            setState(s => ({ ...s, parentMerchant: m, page: 0, }));
                            return;
                        }
                        await switchToMerchant(m);
                    }}
                    onMerchantSelect={() => switchToMerchant(m)}
                />)
            }
        </div>
        <QueryPagination query={merchantsQuery} pageSize={state.pageSize} onPageIndexChange={p => setState(s => ({...s, page: p}))} />
    </Modal>
}

interface MerchantCardProps {
    readonly merchant: Merchant;
    readonly onCardClick: () => Promise<any>;
    readonly onMerchantSelect: () => Promise<any>;
}
const MerchantCard = (props: MerchantCardProps) => {
    const  { t } = useTranslation();
    const [isLoading, setIsLoading] = useState(false);

    return <div 
        className={`rounded-xl border border-gray-200 bg-white p-4 dark:border-gray-800 dark:bg-white/[0.03] ${isLoading ? "" : "cursor-pointer"}`}
        onClick={async () => {
            setIsLoading(true);
            await props.onCardClick();
            setIsLoading(false);
        }}
    >
        <div className="mb-5 overflow-hidden rounded-lg">
            <img src={props.merchant.logoUrl} alt={props.merchant.name} className="overflow-hidden rounded-lg" />
        </div>

        <div>
            <h4 className="mb-1 text-theme-xl font-medium text-gray-800 dark:text-white/90">
                {props.merchant.name}
            </h4>

            {
                props.merchant.parentId != undefined &&
                <Button
                    className="mt-4 w-full"
                    onClick={async () => {
                        setIsLoading(true);
                        await props.onMerchantSelect();
                        setIsLoading(false);
                    }}
                    disabled={isLoading}
                >
                {
                    isLoading
                    ?
                        <Spinner />
                    :
                    (
                        props.merchant.parentId == undefined
                        ?
                        t("sidebar.switchMerchant.selectGroup")
                        :
                        t("sidebar.switchMerchant.selectSubMerchant")
                    )
                }
                </Button>
            }
        </div>
    </div>
}