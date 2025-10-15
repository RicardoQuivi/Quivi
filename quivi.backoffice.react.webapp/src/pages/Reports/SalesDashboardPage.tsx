import { useTranslation } from "react-i18next";
import PageMeta from "../../components/common/PageMeta";
import PageBreadcrumb from "../../components/common/PageBreadCrumb";
import ComponentCard from "../../components/common/ComponentCard";
import { MonthlySalesMetricsWidget } from "./widgets/MonthlySalesMetricsWidget";
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
            <MonthlySalesMetricsWidget />
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