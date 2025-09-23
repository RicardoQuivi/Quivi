import { useState } from "react";
import { useTranslation } from "react-i18next";
import PageMeta from "../../components/common/PageMeta";
import PageBreadcrumb from "../../components/common/PageBreadCrumb";
import ComponentCard from "../../components/common/ComponentCard";
import { ResponsiveTable } from "../../components/tables/ResponsiveTable";
import { Divider } from "../../components/dividers/Divider";
import { QueryPagination } from "../../components/pagination/QueryPagination";
import { useDateHelper } from "../../utilities/dateHelper";
import { useMerchantDocumentsQuery } from "../../hooks/queries/implementations/useMerchantDocumentsQuery";
import { DownloadIcon } from "../../icons";
import { Files } from "../../utilities/files";

export const InvoicingPage = () => {
    const { t } = useTranslation();
    const dateHelper = useDateHelper();

    const [state, setState] = useState({
        page: 0,
        pageSize: 25,
    })
    const monthlyInvoices = useMerchantDocumentsQuery({
        monthlyInvoiceOnly: true,
        page: state.page,
        pageSize: state.pageSize,
    })

    return <>
        <PageMeta
            title={t("pages.invoicing.title")}
            description={t("quivi.product.description")}
        />
        <PageBreadcrumb pageTitle={t("pages.invoicing.title")} />

        <ComponentCard title={t("pages.invoicing.title")}>
            <div className="overflow-hidden rounded-2xl border border-gray-200 bg-white pt-4 dark:border-white/[0.05] dark:bg-white/[0.03]">
                <div className="max-w-full overflow-x-auto">
                    <ResponsiveTable
                        isLoading={monthlyInvoices.isFirstLoading}
                        name={{
                            key: "name",
                            label: t("common.name"),
                            render: d => d.name,
                        }}
                        columns={[
                            {
                                key: "date",
                                label: t("common.date"),
                                render: (d) => dateHelper.toLocalString(d.createdDate, "DD/MM/YYYY"),
                            },
                        ]}
                        actions={[
                            {
                                key: "download",
                                label: t("common.download"),
                                render: () => <DownloadIcon className="size-5" />,
                                onClick: d => Files.saveFileFromURL(d.downloadUrl, d.name),
                            }
                        ]}
                        data={monthlyInvoices.data}
                        getKey={d => d.id}
                    />
                    <Divider />
                    <QueryPagination
                        query={monthlyInvoices}
                        onPageIndexChange={p => setState(s => ({ ...s, page: p }))}
                        pageSize={state.pageSize}
                    />
                </div>
            </div>
        </ComponentCard>
    </>
}