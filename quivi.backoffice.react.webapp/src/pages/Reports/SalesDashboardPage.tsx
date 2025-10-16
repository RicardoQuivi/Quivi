import { useTranslation } from "react-i18next";
import PageMeta from "../../components/common/PageMeta";
import PageBreadcrumb from "../../components/common/PageBreadCrumb";
import ComponentCard from "../../components/common/ComponentCard";
import { SalesMetricsWidget, SalesMetricType } from "./widgets/SalesMetricsWidget";
import { SalesChartWidget } from "./widgets/SalesChartWidget";
import { TopItemsWidget } from "./widgets/TopItemsWidget";
import { ProductSalesSortBy } from "../../hooks/api/Dtos/reporting/ProductSalesSortBy";
import { TopCategoriesWidget } from "./widgets/TopCategoriesWidget";
import { TopChargeMethodsWidget } from "./widgets/TopChargeMethodsWidget";
import { ChargeMethodsComparisonChartWidget } from "./widgets/ChargeMethodsComparisonChartWidget";


export const SalesDashboardPage = () => {
    const { t } = useTranslation();

    return <>
        <PageMeta
            title={t("pages.reports.sales.title")}
            description={t("quivi.product.description")}
        />
        <PageBreadcrumb pageTitle={t("pages.reports.sales.title")} />

        <ComponentCard title={t("pages.reports.sales.title")}>
            <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 md:gap-6 xl:grid-cols-4">
                <SalesMetricsWidget type={SalesMetricType.Total}/>
                <SalesMetricsWidget type={SalesMetricType.Payment}/>
                <SalesMetricsWidget type={SalesMetricType.Tip}/>
                <SalesMetricsWidget type={SalesMetricType.Refunds}/>
            </div>
            <SalesChartWidget />
            <div className="grid grid-cols-1 gap-4 sm:grid-cols-1 md:gap-6 xl:grid-cols-2">
                <TopItemsWidget by={ProductSalesSortBy.SoldQuantity} />
                <TopItemsWidget by={ProductSalesSortBy.BilledAmount} />
            </div>
            <div className="grid grid-cols-1 gap-4 sm:grid-cols-1 md:gap-6 xl:grid-cols-2">
                <TopCategoriesWidget by={ProductSalesSortBy.SoldQuantity}/>
                <TopCategoriesWidget by={ProductSalesSortBy.BilledAmount}/>
            </div>
            <div className="grid grid-cols-1 gap-4 sm:grid-cols-1 md:gap-6 xl:grid-cols-2">
                <TopChargeMethodsWidget by={ProductSalesSortBy.SoldQuantity}/>
                <TopChargeMethodsWidget by={ProductSalesSortBy.BilledAmount}/>
            </div>
            <div className="grid grid-cols-1 gap-4 sm:grid-cols-1 md:gap-6 xl:grid-cols-2">
                <ChargeMethodsComparisonChartWidget by={ProductSalesSortBy.SoldQuantity}/>
                <ChargeMethodsComparisonChartWidget by={ProductSalesSortBy.BilledAmount}/>
            </div>
        </ComponentCard>
    </>
}