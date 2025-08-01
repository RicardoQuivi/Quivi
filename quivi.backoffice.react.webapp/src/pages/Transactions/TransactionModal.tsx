import { Modal, ModalSize } from "../../components/ui/modal";
import { useTranslation } from "react-i18next";
import { PublicId } from "../../components/publicids/PublicId";
import Button from "../../components/ui/button/Button";
import { useTransactionsQuery } from "../../hooks/queries/implementations/useTransactionsQuery";
import { useMemo } from "react";
import { DownloadIcon, FeesIcon, PrinterIcon, QuiviIcon } from "../../icons";
import { Skeleton } from "../../components/ui/skeleton/Skeleton";
import { DateUtils } from "../../utilities/dateutils";
import { useMerchantsQuery } from "../../hooks/queries/implementations/useMerchantsQuery";
import { useChannelsQuery } from "../../hooks/queries/implementations/useChannelsQuery";
import { useChannelProfilesQuery } from "../../hooks/queries/implementations/useChannelProfilesQuery";
import CurrencySpan from "../../components/currency/CurrencySpan";
import { Tooltip } from "../../components/ui/tooltip/Tooltip";
import Badge from "../../components/ui/badge/Badge";
import { useAuthenticatedUser } from "../../context/AuthContext";
import { useCustomChargeMethodsQuery } from "../../hooks/queries/implementations/useCustomChargeMethodsQuery";
import { Spinner } from "../../components/spinners/Spinner";
import Avatar from "../../components/ui/avatar/Avatar";
import { useMerchantDocumentsQuery } from "../../hooks/queries/implementations/useMerchantDocumentsQuery";
import { Files } from "../../utilities/files";
import { useTransactionApi } from "../../hooks/api/useTransactionsApi";

interface Props {
    readonly id?: string;
    readonly onClose: () => any;
}
export const TransactionModal = (props: Props) => {
    const { t } = useTranslation();
    const user = useAuthenticatedUser();
    const api = useTransactionApi();

    const transactionQuery = useTransactionsQuery(props.id == undefined ? undefined : {
        ids: [props.id],
        page: 0,
        pageSize: 1,
    })
    const transaction = useMemo(() => transactionQuery.data.length == 0 ? undefined : transactionQuery.data[0], [transactionQuery.data]);

    const customChargeMethodQuery = useCustomChargeMethodsQuery(transaction?.customChargeMethodId == undefined ? undefined : {
        ids: [transaction.customChargeMethodId],
        page: 0,
        pageSize: undefined,
    })
    const customChargeMethod = useMemo(() => customChargeMethodQuery.data.length == 0 ? undefined : customChargeMethodQuery.data[0], [customChargeMethodQuery.data]);
    
    const merchantQuery = useMerchantsQuery(transaction == undefined ? undefined : {
        ids: [transaction.merchantId],
        page: 0,
        pageSize: 1,
    })
    const merchant = useMemo(() => merchantQuery.data.length == 0 ? undefined : merchantQuery.data[0], [merchantQuery.data]);

    const channelQuery = useChannelsQuery(transaction == undefined ? undefined :{
        ids: [transaction.channelId],
        page: 0,
        pageSize: 1,
    })
    const channel = useMemo(() => channelQuery.data.length == 0 ? undefined : channelQuery.data[0], [channelQuery.data]);

    const profileQuery = useChannelProfilesQuery(channel == undefined ? undefined : {
        ids: [channel.channelProfileId],
        page: 0,
        pageSize: 1,
    })
    const profile = useMemo(() => profileQuery.data.length == 0 ? undefined : profileQuery.data[0], [profileQuery.data]);

    const documentsQuery = useMerchantDocumentsQuery(props.id == undefined ? undefined : {
        transactionIds: [props.id],
        page: 0,
    })
    
    const {
        total,
        discounts,
    } = useMemo(() => {
        let totalOriginal = 0;
        let totalFinal = 0;
        for(const item of transaction?.items ?? []) {
            totalFinal += item.finalPrice * item.quantity;
            totalOriginal += item.originalPrice * item.quantity;
        }
        return {
            total: totalFinal,
            discounts: totalOriginal - totalFinal,
        }
    }, [transaction])

    const getIcon = () => {
        if(transaction?.customChargeMethodId == undefined) {
            return <Avatar src={<QuiviIcon className="h-full w-auto" />} size="medium" alt="Quivi" />
        }
        
        if(customChargeMethod == undefined) {
            return <Avatar src={<Spinner className="h-full w-auto" />} size="medium" alt="Loading" />
        }

        return <Avatar src={customChargeMethod.logoUrl} size="medium" alt={customChargeMethod.name} />
    }

    return (
        <Modal
            isOpen={props.id != undefined}
            onClose={props.onClose}
            size={ModalSize.Auto}
            title={<>
                {t("common.entities.transaction")}&nbsp;<PublicId id={props.id} />
            </>}
        >
            <div className="mb-10 flex flex-wrap items-center justify-end gap-3.5">
                {
                    transaction != undefined &&
                    transaction.refundedAmount == 0 &&
                    <Button
                        onClick={() => api.refund({ id: transaction.id, })}
                    >
                        {t("common.refund")}
                    </Button>
                }
                <Button>
                    <PrinterIcon />
                    {t("common.print")}
                </Button>
            </div>

            <div className="flex flex-wrap justify-between gap-5">
                <div>
                    <p className="mb-1.5 font-medium text-black dark:text-white">
                        {t("common.entities.merchant")}
                    </p>
                    <h4 className="mb-3 text-xl font-bold text-black dark:text-white">
                    {
                        merchant == undefined
                        ?
                        <Skeleton />
                        :
                        merchant.name
                    }
                    </h4>
                    <span className="mt-1.5 block dark:text-white">
                        <span className="font-medium text-black dark:text-white">
                            {t("common.entities.channel")}:{' '}
                        </span>
                        {
                            channel == undefined || profile == undefined
                            ?
                            <Skeleton />
                            :
                            `${profile.name} ${channel.name}`
                        }
                    </span>
                </div>

                <div>
                    <p className="mb-1.5 font-medium text-black dark:text-white">
                        {t("common.client")}
                    </p>
                    <span className="mt-1.5 block dark:text-white">
                        <span className="font-medium text-black dark:text-white">
                            {t("common.email")}:{' '}
                        </span>
                        {
                            transaction == undefined
                            ?
                            <Skeleton />
                            :
                            (
                                transaction.email == undefined
                                ?
                                t("common.notSpecified")
                                :
                                transaction.email
                            )
                        }
                    </span>
                    <span className="mt-1.5 block dark:text-white">
                        <span className="font-medium text-black dark:text-white">
                        {t("common.vatNumber")}:{' '}
                        </span>
                        {
                            transaction == undefined
                            ?
                            <Skeleton />
                            :
                            (
                                transaction.vatNumber == undefined
                                ?
                                t("common.notSpecified")
                                :
                                transaction.vatNumber
                            )
                        }
                    </span>
                </div>
            </div>

            <div className="my-7.5 grid grid-cols-1 border border-stroke dark:border-strokedark xsm:grid-cols-2 sm:grid-cols-4">
                <div className="border-b border-stroke px-5 py-4 last:border-r-0 dark:border-strokedark sm:border-b-0 sm:border-r">
                    <h5 className="mb-1.5 font-bold text-black dark:text-white">
                        {t("common.date")}
                    </h5>
                    <span className="text-sm font-medium dark:text-white">
                    {
                        transaction == undefined
                        ?
                        <Skeleton />
                        :
                        DateUtils.toString(transaction.capturedDate)
                    }
                    </span>
                </div>

                <div className="border-r border-stroke px-5 py-4 last:border-r-0 dark:border-strokedark">
                    <h5 className="mb-1.5 font-bold text-black dark:text-white">
                        {t("common.currencyAmount")}:
                    </h5>
                    <span className="text-sm font-medium dark:text-white">
                    {
                        transaction == undefined
                        ?
                        <Skeleton />
                        :
                        <CurrencySpan value={transaction.payment} />
                    }
                    </span>
                </div>

                <div className="border-r border-stroke px-5 py-4 last:border-r-0 dark:border-strokedark">
                    <h5 className="mb-1.5 font-bold text-black dark:text-white">
                        {t("common.tip")}:
                    </h5>
                    <span className="text-sm font-medium dark:text-white">
                    {
                        transaction == undefined
                        ?
                        <Skeleton />
                        :
                        <CurrencySpan value={transaction.tip} />
                    }
                    </span>
                </div>

                <div className="border-r border-stroke px-5 py-4 last:border-r-0 dark:border-strokedark">
                    <h5 className="mb-1.5 font-bold text-black dark:text-white">
                        {t("common.total")}:
                    </h5>
                    <span className="text-sm font-medium dark:text-white flex items-center gap-3">
                    {
                        transaction == undefined
                        ?
                        <Skeleton />
                        :
                        <>
                            <CurrencySpan value={transaction.tip + transaction.payment} />
                            {
                                user.isAdmin && transaction.surcharge > 0 &&
                                <Tooltip message={t("common.surcharge")}>
                                    <Badge 
                                        variant="solid"
                                        color="warning"
                                        endIcon={<FeesIcon />}
                                    >
                                        + <CurrencySpan value={transaction.surcharge}/>
                                    </Badge>
                                </Tooltip>
                            }
                        </>
                    }
                    </span>
                </div>
            </div>

            <div className="border border-stroke dark:border-strokedark">
                <div className="max-w-full overflow-x-auto">
                    <div className="min-w-[670px]">
                        <div className="grid grid-cols-12 border-b border-stroke py-3.5 pl-5 pr-6 dark:border-strokedark">
                            <div className="col-span-7">
                                <h5 className="font-medium text-black dark:text-white">
                                    {t("common.name")}
                                </h5>
                            </div>

                            <div className="col-span-2">
                                <h5 className="font-medium text-black dark:text-white">
                                    {t("common.quantity")}
                                </h5>
                            </div>

                            <div className="col-span-2">
                                <h5 className="font-medium text-black dark:text-white">
                                    {t("common.unitPrice")}
                                </h5>
                            </div>

                            <div className="col-span-1">
                                <h5 className="text-right font-medium text-black dark:text-white">
                                    {t("common.total")}
                                </h5>
                            </div>
                        </div>

                        {
                            transaction == undefined
                            ?
                            [1, 2, 3, 4, 5].map((_, index) => (
                                <div key={index} className="grid grid-cols-12 border-b border-stroke py-3.5 pl-5 pr-6 dark:border-strokedark">
                                    <div className="col-span-7">
                                        <p className="font-medium">
                                            <Skeleton />
                                        </p>
                                    </div>

                                    <div className="col-span-2">
                                        <p className="font-medium">
                                            <Skeleton />
                                        </p>
                                    </div>

                                    <div className="col-span-2">
                                        <p className="font-medium">
                                            <Skeleton />
                                        </p>
                                    </div>

                                    <div className="col-span-1">
                                        <p className="font-medium">
                                            <Skeleton />
                                        </p>
                                    </div>
                                </div>
                            ))
                            :
                            transaction.items.map((item) => (
                                <div key={item.id} className="grid grid-cols-12 border-b border-stroke py-3.5 pl-5 pr-6 dark:border-strokedark">
                                    <div className="col-span-7">
                                        <p className="font-medium dark:text-white">
                                            {item.name}
                                        </p>
                                    </div>

                                    <div className="col-span-2">
                                        <p className="font-medium dark:text-white">
                                            {item.quantity}
                                        </p>
                                    </div>

                                    <div className="col-span-2">
                                        <p className="font-medium dark:text-white">
                                            <CurrencySpan value={item.finalPrice} />
                                        </p>
                                    </div>

                                    <div className="col-span-1">
                                        <p className="text-right font-medium dark:text-white">
                                            <CurrencySpan value={item.finalPrice * item.quantity} />
                                        </p>
                                    </div>
                                </div>
                            ))
                        }
                    </div>
                </div>

                <div className="-mx-4 flex flex-wrap p-6">
                    <div className="w-full px-4 sm:w-1/3 xl:w-3/12">
                        <div className="mb-10 flex flex-col justify-center">
                            <h4 className="mb-4 text-title-sm2 font-medium leading-[30px] text-black dark:text-white md:text-2xl">
                                {t("common.paymentMethod")}
                            </h4>
                            <p className="font-medium dark:text-white">
                                {getIcon()}
                            </p>
                        </div>
                    </div>
                    <div className="w-full px-4 sm:w-2/3 xl:w-9/12">
                        <div className="mr-10 text-right md:ml-auto">
                            <div className="ml-auto sm:w-1/2">
                                <p className="mb-4 flex justify-between font-medium text-black dark:text-white">
                                    <span>
                                        {t("common.subTotal")}
                                    </span>
                                    <span>
                                    {
                                        transaction == undefined
                                        ?
                                        <Skeleton />
                                        :
                                        <CurrencySpan value={total - discounts} />
                                    }
                                    </span>
                                </p>
                                <p className="mb-4 flex justify-between font-medium text-black dark:text-white">
                                    <span> 
                                        {t("common.discount")}
                                    </span>
                                    <span> 
                                    {
                                        transaction == undefined
                                        ?
                                        <Skeleton />
                                        :
                                        <CurrencySpan value={discounts} />
                                    }
                                    </span>
                                </p>
                                <p className="mb-4 mt-2 flex justify-between border-t border-stroke pt-6 font-medium text-black dark:border-strokedark dark:text-white">
                                    <span>
                                        {t("common.total")}
                                    </span>
                                    <span> 
                                    {
                                        transaction == undefined
                                        ?
                                        <Skeleton />
                                        :
                                        <CurrencySpan value={total} />
                                    }
                                    </span>
                                </p>
                            </div>

                            <div className="mt-10 flex flex-col justify-end gap-4 sm:flex-row">
                                <Tooltip
                                    message={(
                                        documentsQuery.isFirstLoading == false && documentsQuery.data.length == 0
                                        ?
                                        t("pages.transactions.noDocumentAvailable")
                                        :
                                        t("pages.transactions.downloadDocument")
                                    )}
                                >
                                    <Button
                                        className="float-right mt-4"
                                        disabled={documentsQuery.isFirstLoading || documentsQuery.data.length == 0}
                                        onClick={() => documentsQuery.data.forEach(d => Files.saveFileFromURL(d.downloadUrl, d.name))}
                                    >
                                        {
                                            documentsQuery.isFirstLoading
                                            ?
                                            <Spinner />
                                            :
                                            <>
                                                {t("common.download")}
                                                <DownloadIcon />
                                            </>
                                        }
                                    </Button>
                                </Tooltip>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </Modal>
    )
}