import { Trans, useTranslation } from "react-i18next";
import { Placement, Tooltip } from "../ui/tooltip/Tooltip";
import { Modal, ModalSize } from "../ui/modal";
import { useMemo, useState } from "react";
import { ModalButtonsFooter } from "../ui/modal/ModalButtonsFooter";
import { useAuth, useAuthenticatedUser } from "../../context/AuthContext";
import { useMerchantsQuery } from "../../hooks/queries/implementations/useMerchantsQuery";
import { useChannelProfilesQuery } from "../../hooks/queries/implementations/useChannelProfilesQuery";
import { useChannelsQuery } from "../../hooks/queries/implementations/useChannelsQuery";
import { useCustomChargeMethodsQuery } from "../../hooks/queries/implementations/useCustomChargeMethodsQuery";
import { useEmployeesQuery } from "../../hooks/queries/implementations/useEmployeesQuery";
import { useMenuItemsQuery } from "../../hooks/queries/implementations/useMenuItemsQuery";
import { Spinner } from "../spinners/Spinner";
import { Skeleton } from "../ui/skeleton/Skeleton";
import { useNavigate } from "react-router";

export const GoToPosButton: React.FC = () => {
    const { t } = useTranslation();
    const [isOpen, setIsOpen] = useState(false);

    return <>
        <Tooltip message={t("appHeader.pos.openMessage")} placement={Placement.Bottom}>
            <button
                onClick={() => setIsOpen(true)}
                className="relative flex items-center justify-center text-gray-500 transition-colors bg-white border border-gray-200 rounded-full hover:text-dark-900 h-11 hover:bg-gray-100 hover:text-gray-700 dark:border-gray-800 dark:bg-gray-900 dark:text-gray-400 dark:hover:bg-gray-800 dark:hover:text-white px-3"
            >
                {t("appHeader.pos.open")}
            </button>
        </Tooltip>
        <GoToPosModal
            isOpen={isOpen}
            onClose={() => setIsOpen(false)}
        />
    </>
};

interface Props {
    readonly isOpen: boolean;
    readonly onClose: () => any;
}
const GoToPosModal = (props: Props) => {
    const { t, i18n } = useTranslation();
    const navigate = useNavigate();
    const auth = useAuth();
    const user = useAuthenticatedUser();

    const merchantsQuery = useMerchantsQuery(props.isOpen == false ? undefined : {
        page: 0,
        pageSize: 0,
    })

    const channelProfilesQuery = useChannelProfilesQuery(props.isOpen == false ? undefined : {
        page: 0,
        pageSize: 0,
    })

    const channelsQuery = useChannelsQuery(props.isOpen == false ? undefined : {
        page: 0,
        pageSize: 0,
    })
    
    const customChargeMethodsQuery = useCustomChargeMethodsQuery(props.isOpen == false ? undefined : {
        page: 0,
        pageSize: 0,
    })

    const employeesQuery = useEmployeesQuery(props.isOpen == false ? undefined : {
        page: 0,
        pageSize: 0,
    })

    const menuItemsQuery = useMenuItemsQuery(props.isOpen == false ? undefined : {
        page: 0,
        pageSize: 0,
    })

    const openPos = (keepSession: boolean) => {
        try {
            if(keepSession == false) {
                auth.signOut();
            }

            const queryParams = new URLSearchParams();
            queryParams.set("subjectToken", user.token);
            queryParams.set("language", i18n.language);

            const url = new URL(`signIn?${queryParams}`, import.meta.env.VITE_POS_APP_URL).toString();
            window.open(url, keepSession ? '_blank' : '_self', 'noopener,noreferrer');
        } finally {
            props.onClose();
        }
    }

    const getRequiresFooter = (to: string, stepName: string) => <ModalButtonsFooter 
            primaryButton={{
                content: <Trans
                    t={t}
                    i18nKey="appHeader.pos.takeMeToStep"
                    shouldUnescape={true}
                    components={{
                        b: <b/>,
                    }}
                    values={{
                        name: stepName,
                    }}
                />,
                onClick: () => {
                    navigate(to);
                    props.onClose();
                },
            }}
            secondaryButton={{
                content: t("appHeader.pos.takeMeToOnboarding"),
                onClick: () => {
                    navigate("/");
                    props.onClose();
                }
            }}
        />;

    const modal = useMemo(() => {
        const loadingState = {
            footer: <ModalButtonsFooter 
                primaryButton={{
                    content: <Spinner />,
                    disabled: true,
                }}
                secondaryButton={{
                    content: <Spinner />,
                    disabled: true,
                }}
            />,
            content: <div className="flex flex-col gap-2">
                <Skeleton />
                <br/>
                <Skeleton />
                <Skeleton />
            </div>
        };

        if(merchantsQuery.isFirstLoading) {
            return loadingState;
        } else if(merchantsQuery.totalItems == 0) {
            return {
                footer: getRequiresFooter("/businessProfile/merchant/setup", t("pages.onboarding.createMerchant.")),
                content: <Trans
                    t={t}
                    i18nKey="appHeader.pos.requires.merchant"
                    shouldUnescape={true}
                    components={{
                        b: <b/>,
                        br: <><br/><br/></>,
                    }}
                />
            }
        }

        if(user.merchantActivated != true) {
            return {
                footer: getRequiresFooter("/termsAndConditions", t("pages.onboarding.termsAndConditions.")),
                content: <Trans
                    t={t}
                    i18nKey="appHeader.pos.requires.termsAndConditions"
                    shouldUnescape={true}
                    components={{
                        b: <b/>,
                        br: <><br/><br/></>,
                    }}
                />
            }
        }

        if(channelProfilesQuery.isFirstLoading) {
            return loadingState;
        } else if(channelProfilesQuery.totalItems == 0) {
            return {
                footer: getRequiresFooter("/businessProfile/channelprofiles/add", t("pages.onboarding.createChannelProfile.")),
                content: <Trans
                    t={t}
                    i18nKey="appHeader.pos.requires.channelProfiles"
                    shouldUnescape={true}
                    components={{
                        b: <b/>,
                        br: <><br/><br/></>,
                    }}
                />
            }
        }

        if(channelsQuery.isFirstLoading) {
            return loadingState;
        } else if(channelsQuery.totalItems == 0) {
            return {
                footer: getRequiresFooter("/businessProfile/channels", t("pages.onboarding.createChannel.")),
                content: <Trans
                    t={t}
                    i18nKey="appHeader.pos.requires.channels"
                    shouldUnescape={true}
                    components={{
                        b: <b/>,
                        br: <><br/><br/></>,
                    }}
                />
            }
        }

        if(customChargeMethodsQuery.isFirstLoading) {
            return loadingState;
        } else if(customChargeMethodsQuery.totalItems == 0) {
            return {
                footer: getRequiresFooter("/settings/chargemethods", t("pages.onboarding.createCustomChargeMethod.")),
                content: <Trans
                    t={t}
                    i18nKey="appHeader.pos.requires.customChargeMethods"
                    shouldUnescape={true}
                    components={{
                        b: <b/>,
                        br: <><br/><br/></>,
                    }}
                />
            }
        }

        if(employeesQuery.isFirstLoading) {
            return loadingState;
        } else if(employeesQuery.totalItems == 0) {
            return {
                footer: getRequiresFooter("/settings/employees", t("pages.onboarding.createEmployee.")),
                content: <Trans
                    t={t}
                    i18nKey="appHeader.pos.requires.employees"
                    shouldUnescape={true}
                    components={{
                        b: <b/>,
                        br: <><br/><br/></>,
                    }}
                />
            }
        }

        if(menuItemsQuery.isFirstLoading) {
            return loadingState;
        } else if(menuItemsQuery.totalItems == 0) {
            return {
                footer: getRequiresFooter("/businessProfile/menumanagement", t("pages.onboarding.createMenuItem.")),
                content: <Trans
                    t={t}
                    i18nKey="appHeader.pos.requires.menuItems"
                    shouldUnescape={true}
                    components={{
                        b: <b/>,
                        br: <><br/><br/></>,
                    }}
                />
            }
        }

        return {
            footer: <ModalButtonsFooter 
                primaryButton={{
                    content: t("appHeader.pos.goAndLogout"),
                    onClick: () => openPos(false),
                }}
                secondaryButton={{
                    content: t("appHeader.pos.goAndKeepSession"),
                    onClick: () => openPos(true),
                }}
            />,
            content: <Trans
                t={t}
                i18nKey="appHeader.pos.openDescription"
                shouldUnescape={true}
                components={{
                    b: <b/>,
                    br: <><br/><br/></>,
                }}
            />
        }
    }, [
        merchantsQuery.isFirstLoading, merchantsQuery.totalItems,
        user.merchantActivated,
        channelProfilesQuery.isFirstLoading, channelProfilesQuery.totalItems,
        channelsQuery.isFirstLoading, channelsQuery.totalItems,
        customChargeMethodsQuery.isFirstLoading, customChargeMethodsQuery.totalItems,
        employeesQuery.isFirstLoading, employeesQuery.totalItems,
        menuItemsQuery.isFirstLoading, menuItemsQuery.totalItems,
    ])

    return <Modal
            isOpen={props.isOpen}
            onClose={props.onClose} 
            title={t("appHeader.pos.open")}
            footer={modal.footer}
            size={ModalSize.Medium}
        >
            <p
                className="dark:text-white"
            >
                {modal.content}
            </p>
        </Modal>
}
