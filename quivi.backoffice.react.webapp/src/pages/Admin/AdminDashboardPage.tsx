import { useTranslation } from "react-i18next";
import PageMeta from "../../components/common/PageMeta";
import PageBreadcrumb from "../../components/common/PageBreadCrumb";
import ComponentCard from "../../components/common/ComponentCard";
import { MonthlySalesMetricsWidget } from "../Reports/widgets/MonthlySalesMetricsWidget";
import { SalesChartWidget } from "../Reports/widgets/SalesChartWidget";


export const AdminDashboardPage = () => {
    const { t } = useTranslation();

    return <>
        <PageMeta
            title={t("pages.adminDashboard.title")}
            description={t("quivi.product.description")}
        />
        <PageBreadcrumb pageTitle={t("pages.adminDashboard.title")} />

        <ComponentCard title={t("pages.adminDashboard.title")}>
            <MonthlySalesMetricsWidget adminView />
            <SalesChartWidget adminView />
        </ComponentCard>
    </>
}