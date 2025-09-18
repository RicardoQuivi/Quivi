import { Modal, ModalSize } from "../../components/ui/modal";
import { useTranslation } from "react-i18next";
import { PublicId } from "../../components/publicids/PublicId";
import Button from "../../components/ui/button/Button";
import { useTransactionsQuery } from "../../hooks/queries/implementations/useTransactionsQuery";
import { useEffect, useMemo, useState } from "react";
import { DownloadIcon, FeesIcon, PrinterIcon, QuiviIcon, StarIcon } from "../../icons";
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
import { useTransactionMutator } from "../../hooks/mutators/useTransactionMutator";
import { useToast } from "../../layout/ToastProvider";
import { usePosIntegrationsQuery } from "../../hooks/queries/implementations/usePosIntegrationsQuery";
import { useQuiviForm } from "../../hooks/api/exceptions/useQuiviForm";
import * as yup from 'yup';
import { useSearchParams } from "react-router";
import { Transaction } from "../../hooks/api/Dtos/transactions/Transaction";
import { CurrencyField } from "../../components/inputs/CurrencyField";
import { useReviewsQuery } from "../../hooks/queries/implementations/useReviewsQuery";
import { useDateHelper } from "../../utilities/dateHelper";
import { useNow } from "../../hooks/useNow";

enum Tabs {
    Details = "Details",
    Refunds = "Refunds",
}

interface Props {
    readonly id?: string;
    readonly onClose: () => any;
}
export const TransactionModal = (props: Props) => {
    const { t } = useTranslation();

    const [searchParams, setSearchParams] = useSearchParams();
    const tab = searchParams.get("tab") ?? Tabs.Details;

    const transactionQuery = useTransactionsQuery(props.id == undefined ? undefined : {
        ids: [props.id],
        page: 0,
        pageSize: 1,
    })
    const transaction = useMemo(() => transactionQuery.data.length == 0 ? undefined : transactionQuery.data[0], [transactionQuery.data]);

    return (
        <Modal
            isOpen={props.id != undefined}
            onClose={props.onClose}
            size={tab == Tabs.Details ? ModalSize.Auto : ModalSize.Small}
            title={<>
                {t("common.entities.transaction")}&nbsp;<PublicId id={props.id} />
            </>}
            className="transition-all duration-500 ease-in-out"
        >
            <nav className="flex overflow-x-auto rounded-lg bg-gray-100 p-1 dark:bg-gray-800 [&::-webkit-scrollbar]:h-1.5 [&::-webkit-scrollbar-track]:bg-white dark:[&::-webkit-scrollbar-track]:bg-transparent [&::-webkit-scrollbar-thumb]:rounded-full [&::-webkit-scrollbar-thumb]:bg-gray-200 dark:[&::-webkit-scrollbar-thumb]:bg-gray-600">
                <button
                    onClick={() => setSearchParams(s => ({
                        ...s,
                        tab: Tabs.Details, 
                    }))}
                    className={`inline-flex items-center rounded-md px-3 py-2 text-sm font-medium transition-colors duration-200 ease-in-out ${
                        tab === Tabs.Details
                        ? "bg-white text-gray-900 shadow-theme-xs dark:bg-white/[0.03] dark:text-white"
                        : "bg-transparent text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200"
                    }`}
                >
                    {t("common.details")}
                </button>
                
                <button
                    onClick={() => setSearchParams(s => ({
                        ...s,
                        tab: Tabs.Refunds, 
                    }))}
                    className={`inline-flex items-center rounded-md px-3 py-2 text-sm font-medium transition-colors duration-200 ease-in-out ${
                        tab === Tabs.Refunds
                        ? "bg-white text-gray-900 shadow-theme-xs dark:bg-white/[0.03] dark:text-white"
                        : "bg-transparent text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200"
                    }`}
                >
                    {t("common.refund")}
                </button>
            </nav>

            {
                tab == Tabs.Details
                ?
                <Details transaction={transaction} />
                :
                <Refund
                    transaction={transaction}
                    onRefunded={() => setSearchParams(s => ({
                        ...s,
                        tab: Tabs.Details, 
                    }))}
                />
            }
        </Modal>
    )
}

interface PageProps {
    readonly transaction?: Transaction
}
const Details = ({
    transaction
}: PageProps) => {
    const { t } = useTranslation();
    const dateHelper = useDateHelper();
    const user = useAuthenticatedUser();
    const now = useNow(1000);

    const reviewsQuery = useReviewsQuery(transaction == undefined ? undefined : {
        ids: [transaction.id],
        page: 0,
        pageSize: undefined,
    })
    const review = useMemo(() => reviewsQuery.data.length == 0 ? undefined : reviewsQuery.data[0], [reviewsQuery.data]);

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

    const documentsQuery = useMerchantDocumentsQuery(transaction?.id == undefined ? undefined : {
        transactionIds: [transaction.id],
        page: 0,
    })

    const {
        total,
        totalAfterRefund,
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
            totalAfterRefund: totalFinal - (transaction?.refundedAmount ?? 0),
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

    return <>
        <div className="mb-10 flex flex-wrap items-center justify-between gap-3.5">
            <div>
            {
                review != undefined &&
                <div className="rounded-2xl border border-gray-200 bg-white p-6 dark:border-gray-800 dark:bg-white/3">
                    <h2 className="mb-5 text-lg font-semibold text-gray-800 dark:text-white/90">
                        {t("common.entities.review")}
                    </h2>

                    <div className="relative pb-7 pl-11">
                        <div className="absolute top-0 left-0 z-10 flex h-12 w-12 items-center justify-center text-gray-700 dark:text-gray-400">
                            <Badge 
                                variant="solid"
                                color={review.stars > 3 ? "success" : "warning"}
                                startIcon={<StarIcon fill="currentColor" />}
                            >
                                {review.stars}
                            </Badge>
                        </div>

                        <div className="ml-4 flex justify-between gap-6">
                            <div>
                                <h4 className="font-medium text-gray-800 dark:text-white/90">
                                    {t("common.comment")}
                                </h4>
                                <p className="text-sm text-gray-500 dark:text-gray-400">
                                    {review.comment}
                                </p>
                            </div>

                            <div>
                                <span className="text-xs text-gray-500 dark:text-gray-400">{dateHelper.toLocalString(review.modifiedDate, "HH:MM DD/MM/YYYY")}</span>
                                <p className="text-xs text-gray-500 dark:text-gray-400">
                                    {dateHelper.getTimeAgo(now, review.modifiedDate)}
                                </p>
                            </div>
                        </div>
                    </div>
                </div>
            }
            </div>
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
            <div className="border-b border-stroke px-5 py-4 last:border-r-0 dark:border-strokedark sm:border-b-0 sm:border-r xsm:col-span-2 sm:col-span-1 xsm:justify-center flex flex-col flex-wrap content-center">
                <h5 className="mb-1.5 font-bold text-black dark:text-white xsm:text-center">
                    {t("common.date")}
                </h5>
                <span className="text-sm font-medium dark:text-white xsm:text-center">
                {
                    transaction == undefined
                    ?
                    <Skeleton />
                    :
                    DateUtils.toString(transaction.capturedDate)
                }
                </span>
            </div>

            <div className="border-r border-stroke px-5 py-4 last:border-r-0 xsm:border-b sm:border-b-0 dark:border-strokedark xsm:justify-center flex flex-col flex-wrap content-center">
                <h5 className="mb-1.5 font-bold text-black dark:text-white xsm:text-center">
                    {t("common.currencyAmount")}:
                </h5>
                <span className="text-sm font-medium dark:text-white xsm:text-center">
                {
                    transaction == undefined
                    ?
                    <Skeleton />
                    :
                    <CurrencySpan value={transaction.payment} />
                }
                </span>
            </div>

            <div className="border-r border-stroke px-5 py-4 last:border-r-0 xsm:border-b sm:border-b-0 dark:border-strokedark xsm:justify-center flex flex-col flex-wrap content-center">
                <h5 className="mb-1.5 font-bold text-black dark:text-white xsm:text-center">
                    {t("common.tip")}:
                </h5>
                <span className="text-sm font-medium dark:text-white xsm:text-center">
                {
                    transaction == undefined
                    ?
                    <Skeleton />
                    :
                    <CurrencySpan value={transaction.tip} />
                }
                </span>
            </div>

            <div className="border-r border-stroke px-5 py-4 last:border-r-0 dark:border-strokedark xsm:col-span-2 sm:col-span-1 xsm:justify-center flex flex-col flex-wrap content-center">
                <h5 className="mb-1.5 font-bold text-black dark:text-white xsm:text-center">
                    {t("common.total")}:
                </h5>
                <span className="text-sm font-medium dark:text-white flex items-center gap-3 xsm:text-center">
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

            {
                transaction != undefined && transaction.refundedAmount > 0 &&
                <div className="border-r border-stroke px-5 py-4 last:border-r-0 border-t dark:border-strokedark xsm:col-span-2 sm:col-span-4 xsm:justify-center flex flex-col flex-wrap content-center dark:bg-[var(--color-error-900)!important] bg-[var(--color-error-200)!important]">
                    <h5 className="mb-1.5 font-bold text-black dark:text-white xsm:text-center">
                        {t("common.refunded")}:
                    </h5>
                    <span className="text-sm font-medium dark:text-white flex items-center gap-3 xsm:text-center">
                        <CurrencySpan value={transaction.refundedAmount} />
                    </span>
                </div>
            }
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
                            {
                                transaction != undefined && transaction.refundedAmount > 0 &&
                                <p className="mb-4 flex justify-between font-medium text-black dark:text-white">
                                    <span> 
                                        {t("common.refunded")}
                                    </span>
                                    <span> 
                                        <CurrencySpan value={-transaction.refundedAmount} />
                                    </span>
                                </p>
                            }
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
                                    <CurrencySpan value={totalAfterRefund} />
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
    </>
}

const refundSchema = yup.object({
    id: yup.string().required(),
    amount: yup.number().required(),
});

interface RefundProps extends PageProps {
    readonly onRefunded?: () => any;
}
const Refund = ({
    transaction,
    onRefunded,
}: RefundProps) => {
    const  { t } = useTranslation();
    const toast = useToast();
    const mutator = useTransactionMutator();

    const [state, setState] = useState(() => ({
        id: transaction?.id,
        amount: (transaction?.payment ?? 0) + (transaction?.tip ?? 0),
    }));

    const [isRefunding, setIsRefunding] = useState(false);
    const form = useQuiviForm(state, refundSchema);

    const posIntegrationQuery = usePosIntegrationsQuery(transaction == undefined ? undefined : {
        channelId: transaction.channelId,
        page: 0,
        pageSize: 1,
    })
    const posIntegration = useMemo(() => posIntegrationQuery.data.length == 0 ? undefined : posIntegrationQuery.data[0], [posIntegrationQuery.data]);

    useEffect(() => setState(_ => ({
        id: transaction?.id,
        amount: (transaction?.payment ?? 0) + (transaction?.tip ?? 0),
    })), [transaction?.id])

    useEffect(() => {
        if(form.errors.size == 0) {
            return;
        }

        toast.error(t("pages.transactions.refundFailed", {
            reason: form.touchedErrors.get("")?.message
        }));
    }, [form.errors])

    
    const getRefundButton = (isCancelation: boolean) => {

        const isRefundAvailable = posIntegration?.features.allowsRefunds != false;
        const baseButton = (
            <Button
                onClick={() => form.submit(async () => {
                    if(transaction == undefined) {
                        return;
                    }

                    setIsRefunding(true)
                    try {
                        await mutator.refund(transaction, {
                            amount: state.amount,
                            cancelation: isCancelation,
                        });
                        toast.success(t("pages.transactions.refundSuccess"));
                        onRefunded?.();
                    } finally {
                        setIsRefunding(false);
                    }
                }, () => toast.error(t("common.operations.failure.generic")))}
                disabled={isRefundAvailable == false || (transaction?.refundedAmount ?? 0) > 0}
                variant={isCancelation ? "outline" : "primary"}
                className="w-full"
            >
            {
                transaction == undefined || isRefunding || posIntegrationQuery.isFirstLoading
                ?
                <Spinner />
                :
                t(`pages.transactions.${isCancelation ? "cancelationRefund" : "creditNoteRefund"}`)
            }
            </Button>
        )

        if(isRefundAvailable) {
            if((transaction?.refundedAmount ?? 0) > 0) {
                return <Tooltip
                    message={t("pages.transactions.alreadyRefunded")}
                >
                    {baseButton}
                </Tooltip>
            }
            return baseButton;
        }

        return <Tooltip
            message={t("pages.transactions.refundNotSupported")}
        >
            {baseButton}
        </Tooltip>
    }

    return <div
        className="max-w-screen-md mx-auto"
    >
        <div
            className="flex flex-col gap-4"
        >
            <CurrencyField
                label={t("pages.transactions.refundAmount")}
                value={state.amount}
                onChange={(e) => setState(s => ({ ...s, price: e }))}
                errorMessage={form.touchedErrors.get("amount")?.message}
                endElement={<span className="text-gray-700 dark:bg-gray-900 dark:text-gray-400 mx-4 h-full flex flex-wrap justify-center content-center">€</span>}
                decimalPlaces={2}
                minValue={0}
                maxValue={(transaction?.payment ?? 0) + (transaction?.tip ?? 0)}
                className="col-span-5 sm:col-span-7 md:col-span-7 lg:col-span-7 xl:col-span-5"
            />

            <div
                className="grid grid-cols-2 gap-4"
            >
                {getRefundButton(true)}
                {getRefundButton(false)}
            </div>
        </div>
    </div>
}