import { useTranslation } from "react-i18next";
import PageMeta from "../../components/common/PageMeta";
import PageBreadcrumb from "../../components/common/PageBreadCrumb";
import ComponentCard from "../../components/common/ComponentCard";
import { SalesMetricsWidget, SalesMetricType } from "../Reports/widgets/SalesMetricsWidget";
import { SalesChartWidget } from "../Reports/widgets/SalesChartWidget";
import { ChargesSuccessRateWidget } from "../Reports/widgets/ChargesSuccessRateWidget";
import { ChargeMethod } from "../../hooks/api/Dtos/ChargeMethod";
import { ChargePartner } from "../../hooks/api/Dtos/acquirerconfigurations/ChargePartner";
import { AcquirerMetricsWidget } from "../Reports/widgets/AcquirerMetricsWidget";

export const AdminDashboardPage = () => {
    const { t } = useTranslation();

    return <>
        <PageMeta
            title={t("pages.adminDashboard.title")}
            description={t("quivi.product.description")}
        />
        <PageBreadcrumb pageTitle={t("pages.adminDashboard.title")} />

        <ComponentCard title={t("pages.adminDashboard.title")}>
            <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 md:gap-6 xl:grid-cols-4">
                <SalesMetricsWidget adminView type={SalesMetricType.Total}/>
                <SalesMetricsWidget adminView type={SalesMetricType.Payment}/>
                <SalesMetricsWidget adminView type={SalesMetricType.Tip}/>
                <SalesMetricsWidget adminView type={SalesMetricType.Refunds}/>
            </div>
            <SalesChartWidget adminView />
            <div className="grid grid-cols-1 gap-4 sm:grid-cols-1 lg:grid-cols-3 md:gap-6">
                <AcquirerMetricsWidget adminView partner={ChargePartner.Paybyrd} method={ChargeMethod.CreditCard} />
                <AcquirerMetricsWidget adminView partner={ChargePartner.Paybyrd} method={ChargeMethod.MbWay} />
                <AcquirerMetricsWidget adminView partner={ChargePartner.Paybyrd} method={ChargeMethod.PaymentTerminal} />
            </div>
            <div className="grid grid-cols-1 gap-4 sm:grid-cols-1 lg:grid-cols-3 md:gap-6">
                <ChargesSuccessRateWidget adminView partner={ChargePartner.Paybyrd} method={ChargeMethod.CreditCard} />
                <ChargesSuccessRateWidget adminView partner={ChargePartner.Paybyrd} method={ChargeMethod.MbWay} />
                <ChargesSuccessRateWidget adminView partner={ChargePartner.Paybyrd} method={ChargeMethod.PaymentTerminal} />
            </div>
        </ComponentCard>
    </>
}