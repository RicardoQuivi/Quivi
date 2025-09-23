import { useState } from "react";
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

export const PayoutsPage = () => {
    const { t } = useTranslation();
    const dateHelper = useDateHelper();

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
                                key: "amount",
                                label: t("common.currencyAmount"),
                                render: t => <CurrencySpan value={t.amount} />
                            },
                            {
                                key: "tip",
                                label: t("common.tip"),
                                render: t => <CurrencySpan value={t.tip} />
                            },
                            {
                                key: "services",
                                label: t("common.services"),
                                render: t => t.services == undefined ? "N/A" : <CurrencySpan value={t.tip} />,
                            },
                        ]}
                        data={settlementsQuery.data}
                        getKey={d => d.id}
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
    </>
}