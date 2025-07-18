import { Modal, ModalSize } from "../../components/ui/modal";
import { useTranslation } from "react-i18next";
import { PublicId } from "../../components/publicids/PublicId";
import Button from "../../components/ui/button/Button";
import { useTransactionsQuery } from "../../hooks/queries/implementations/useTransactionsQuery";
import { useMemo } from "react";
import { DownloadIcon, FeesIcon, PrinterIcon } from "../../icons";
import { Skeleton } from "../../components/ui/skeleton/Skeleton";
import { DateUtils } from "../../utilities/dateutils";
import { useMerchantsQuery } from "../../hooks/queries/implementations/useMerchantsQuery";
import { useChannelsQuery } from "../../hooks/queries/implementations/useChannelsQuery";
import { useChannelProfilesQuery } from "../../hooks/queries/implementations/useChannelProfilesQuery";
import CurrencySpan from "../../components/currency/CurrencySpan";
import { Tooltip } from "../../components/ui/tooltip/Tooltip";
import Badge from "../../components/ui/badge/Badge";
import { useAuth } from "../../context/AuthContext";

interface Props {
    readonly id?: string;
    readonly onClose: () => any;
}
export const TransactionModal = (props: Props) => {
    const { t } = useTranslation();
    const auth = useAuth();

    const transactionQuery = useTransactionsQuery(props.id == undefined ? undefined : {
        ids: [props.id],
        page: 0,
        pageSize: 1,
    })
    const transaction = useMemo(() => transactionQuery.data.length == 0 ? undefined : transactionQuery.data[0], [transactionQuery.data]);

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

    const {
        total,
        discounts,
    } = useMemo(() => {
        let totalOriginal = 0;
        let totalFinal = 0;
        for(const item of transaction?.items ?? []) {
            totalFinal += item.finalPrice * item.quantity;
            totalOriginal = item.originalPrice * item.quantity;
        }
        return {
            total: totalFinal,
            discounts: totalOriginal - totalFinal,
        }
    }, [transaction])
    return (
        <Modal
            isOpen={props.id != undefined}
            onClose={props.onClose}
            size={ModalSize.Auto}
            title={<>
                {t("common.entities.transaction")}
                <PublicId id={props.id} />
            </>}
        >
        <div>
            <div className="mb-10 flex flex-wrap items-center justify-end gap-3.5">
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
                                auth.isAdmin &&
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

                <div className="flex justify-end p-6">
                    <div className="max-w-65 w-full">
                        <div className="flex flex-col gap-4">
                            <p className="flex justify-between font-medium text-black dark:text-white">
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

                            <p className="flex justify-between font-medium text-black dark:text-white">
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
                        </div>

                        <p className="mt-4 flex justify-between border-t border-stroke pt-5 dark:border-strokedark">
                            <span className="font-medium text-black dark:text-white">
                                {t("common.total")}
                            </span>
                            <span className="font-bold text-meta-3 dark:text-white">
                            {
                                transaction == undefined
                                ?
                                <Skeleton />
                                :
                                <CurrencySpan value={total} />
                            }
                            </span>
                        </p>

                        <Button
                            className="float-right mt-4"
                        >
                            {t("common.download")}
                            <DownloadIcon />
                        </Button>
                    </div>
                </div>
            </div>
        </div>
        </Modal>
    )
}