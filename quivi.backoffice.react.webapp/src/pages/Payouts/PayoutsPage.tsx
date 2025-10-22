import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import PageMeta from "../../components/common/PageMeta";
import PageBreadcrumb from "../../components/common/PageBreadCrumb";
import ComponentCard from "../../components/common/ComponentCard";
import { ResponsiveTable } from "../../components/tables/ResponsiveTable";
import { Divider } from "../../components/dividers/Divider";
import { QueryPagination } from "../../components/pagination/QueryPagination";
import { useSettlementsQuery } from "../../hooks/queries/implementations/useSettlementsQuery";
import { useDateHelper } from "../../utilities/dateHelper";
import CurrencySpan from "../../components/currency/CurrencySpan";
import { Modal, ModalSize } from "../../components/ui/modal/Modal";
import { ModalButtonsFooter } from "../../components/ui/modal/ModalButtonsFooter";
import { Settlement } from "../../hooks/api/Dtos/settlements/Settlement";
import { useSettlementDetailsQuery } from "../../hooks/queries/implementations/useSettlementDetailsQuery";
import { MetricWidget } from "../Reports/widgets/MetricWidget";

export const PayoutsPage = () => {
    const { t } = useTranslation();
    const dateHelper = useDateHelper();

    const [settlement, setSettlement] = useState<Settlement>();
    const [state, setState] = useState({
        page: 0,
        pageSize: 25,
    })
    const settlementsQuery = useSettlementsQuery({
        page: state.page,
        pageSize: state.pageSize,
    })

    return <>
        <PageMeta
            title={t("pages.payouts.title")}
            description={t("quivi.product.description")}
        />
        <PageBreadcrumb pageTitle={t("pages.payouts.title")} />

        <ComponentCard title={t("pages.payouts.title")}>
            <div className="overflow-hidden rounded-2xl border border-gray-200 bg-white pt-4 dark:border-white/[0.05] dark:bg-white/[0.03]">
                <div className="max-w-full overflow-x-auto">
                    <ResponsiveTable
                        isLoading={settlementsQuery.isFirstLoading}
                        name={{
                            key: "name",
                            render: (d) => dateHelper.toLocalString(d.date, "DD/MM/YYYY"),
                            label: t("common.name"),
                        }}
                        columns={[
                            {
                                key: "gross",
                                label: t("common.gross"),
                                children: [
                                    {
                                        key: "grossAmount",
                                        label: t("common.currencyAmount"),
                                        render: t => <CurrencySpan value={t.grossAmount} />,
                                    },
                                    {
                                        key: "grossTip",
                                        label: t("common.tip"),
                                        render: t => <CurrencySpan value={t.grossTip} />,
                                    },
                                    {
                                        key: "grossTotal",
                                        label: t("common.total"),
                                        render: t => <CurrencySpan value={t.grossTotal} />,
                                    },
                                ]
                            },
                            {
                                key: "net",
                                label: t("common.net"),
                                children: [
                                    {
                                        key: "amount",
                                        label: t("common.currencyAmount"),
                                        render: t => <CurrencySpan value={t.netAmount} />
                                    },
                                    {
                                        key: "tip",
                                        label: t("common.tip"),
                                        render: t => <CurrencySpan value={t.netTip} />
                                    },
                                    {
                                        key: "total",
                                        label: t("common.total"),
                                        render: t => <CurrencySpan value={t.netTotal} />,
                                    },
                                ]
                            },
                        ]}
                        data={settlementsQuery.data}
                        getKey={d => d.id}
                        onRowClick={setSettlement}
                    />
                    <Divider />
                    <QueryPagination
                        query={settlementsQuery}
                        onPageIndexChange={p => setState(s => ({ ...s, page: p }))}
                        pageSize={state.pageSize}
                    />
                </div>
            </div>
        </ComponentCard>
        <SettlementDetailModal
            settlement={settlement}
            onClose={() => setSettlement(undefined)}
        />
    </>
}

interface SettlementDetailModalProps {
    readonly settlement?: Settlement;
    readonly onClose: () => any;
}
const SettlementDetailModal = (props: SettlementDetailModalProps) => {
    const { t } = useTranslation();
    const dateHelper = useDateHelper();

    const [state, setState] = useState(() => ({
        page: 0,
        pageSize: 25,
    }))

    const detailsQuery = useSettlementDetailsQuery(props.settlement == undefined ? undefined : {
        settlementId: props.settlement.id,
        page: state.page,
        pageSize: state.pageSize,
    })

    useEffect(() => setState({
        page: 0,
        pageSize: 25,
    }), [props.settlement])

    const getTitle = () => {
        if(props.settlement == undefined) {
            return;
        }

        return t("pages.payouts.details", {
            date: dateHelper.toLocalString(props.settlement.date, "DD/MM/YYYY"),
        })
    }

    return <Modal
        title={getTitle()}
        isOpen={props.settlement != undefined}
        size={ModalSize.Default}
        onClose={props.onClose}
        footer={<ModalButtonsFooter
            primaryButton={{
                content: t("common.close"),
                onClick: props.onClose,
            }}
        />}
    >
        <div className="grid grid-cols-3 gap-4 md:gap-6">
            <MetricWidget
                title={t("common.currencyAmount")}
                isLoading={props.settlement == undefined}
            >
                <CurrencySpan value={props.settlement?.netAmount ?? 0} />
            </MetricWidget>

            <MetricWidget
                title={t("common.tip")}
                isLoading={props.settlement == undefined}
            >
                <CurrencySpan value={props.settlement?.netTip ?? 0} />
            </MetricWidget>

            <MetricWidget
                title={t("common.total")}
                isLoading={props.settlement == undefined}
            >
                <CurrencySpan value={props.settlement?.netTotal ?? 0} />
            </MetricWidget>
        </div>
        <br/>
        <ResponsiveTable
            isLoading={detailsQuery.isFirstLoading}
            data={detailsQuery.data}
            getKey={d => d.id}
            name={{
                key: "name",
                render: (d) => dateHelper.toLocalString(d.date, "DD/MM/YYYY HH:mm:ss"),
                label: t("common.name"),
            }}
            columns={[
                {
                    key: "gross",
                    label: t("common.gross"),
                    children: [
                        {
                            key: "grossAmount",
                            label: t("common.currencyAmount"),
                            render: t => <CurrencySpan value={t.grossAmount} />,
                        },
                        {
                            key: "grossTip",
                            label: t("common.tip"),
                            render: t => <CurrencySpan value={t.grossTip} />,
                        },
                        {
                            key: "grossTotal",
                            label: t("common.total"),
                            render: t => <CurrencySpan value={t.grossTotal} />,
                        },
                    ]
                },
                {
                    key: "net",
                    label: t("common.net"),
                    children: [
                        {
                            key: "amount",
                            label: t("common.currencyAmount"),
                            render: t => <CurrencySpan value={t.netAmount} />
                        },
                        {
                            key: "tip",
                            label: t("common.tip"),
                            render: t => <CurrencySpan value={t.netTip} />
                        },
                        {
                            key: "total",
                            label: t("common.total"),
                            render: t => <CurrencySpan value={t.netTotal} />,
                        },
                    ]
                },
            ]}
        />
    </Modal>
}