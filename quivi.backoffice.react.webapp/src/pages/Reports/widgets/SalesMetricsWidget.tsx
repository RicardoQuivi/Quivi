import { useMemo, useState } from "react";
import CurrencySpan from "../../../components/currency/CurrencySpan";
import { SalesPeriod } from "../../../hooks/api/Dtos/reporting/SalesPeriod";
import { useSalesQuery } from "../../../hooks/queries/implementations/useSalesQuery";
import { useTranslation } from "react-i18next";
import { AnalyticMetricWidget } from "./AnalyticMetricWidget";

const getImprovement = (current: number, previous: number) => {
    if (previous === 0) {
        return 0;
    }
    return ((current - previous) / previous * 100);
}

export enum SalesMetricType {
    Total,
    Tip,
    Payment,
    Refunds,
}

interface Period {
    readonly from: Date;
    readonly to: Date;
}
interface State {
    readonly now: Date;
    readonly reference: Period;
    readonly analysis: Period;
}
interface Props {
    readonly adminView?: boolean;
    readonly type: SalesMetricType;
}
export const SalesMetricsWidget = (props: Props) => {
    const { t } = useTranslation();
    
    const [stats, setStats] = useState<State>();

    const analysisSales = useSalesQuery(stats == undefined ? undefined : {
        adminView: props.adminView,
        period: SalesPeriod.Monthly,
        from: stats.analysis.from.toISOString(),
        to: stats.analysis.to.toISOString(),
        page: 0,
        pageSize: 1,
    })
    const referenceSales = useSalesQuery(stats == undefined ? undefined : {
        adminView: props.adminView,
        period: SalesPeriod.Monthly,
        from: stats.reference.from.toISOString(),
        to: stats.reference.to.toISOString(),
        page: 0,
        pageSize: 1,
    })

    const data = useMemo(() => {
        const analysisData = analysisSales.data.length == 0 ? {
            payment: 0,
            paymentRefund: 0,
            tip: 0,
            tipRefund: 0,
            total: 0,
            totalRefund: 0,
        } : analysisSales.data[0];

        const referenceData = referenceSales.data.length == 0 ? {
            payment: 0,
            paymentRefund: 0,
            tip: 0,
            tipRefund: 0,
            total: 0,
            totalRefund: 0,
        } : referenceSales.data[0];

        return {
            currentMonth: analysisData,
            previousMonth: referenceData,
        }
    }, [analysisSales.data, referenceSales.data, stats])

    const getProps = () => {
        switch(props.type)
        {
            case SalesMetricType.Total: return {
                title: t("widgets.salesAnalytics.totalSales"),
                percentage: getImprovement(data.currentMonth.total, data.previousMonth.total),
                amount: data.currentMonth.total,
            }
            case SalesMetricType.Payment: return {
                title: t("widgets.salesAnalytics.totalAmount"),
                percentage: getImprovement(data.currentMonth.payment, data.previousMonth.payment),
                amount: data.currentMonth.payment,
            }
            case SalesMetricType.Tip: return {
                title: t("widgets.salesAnalytics.totalTips"),
                percentage: getImprovement(data.currentMonth.tip, data.previousMonth.tip),
                amount: data.currentMonth.tip,
            }
            case SalesMetricType.Refunds: return {
                title: t("widgets.salesAnalytics.totalRefunds"),
                percentage: getImprovement(data.currentMonth.totalRefund, data.previousMonth.totalRefund),
                amount: data.currentMonth.totalRefund,
            }
        }
    }

    const analyticsProps = getProps();
    return (
        <AnalyticMetricWidget
            title={analyticsProps.title}
            percentage={analyticsProps.percentage}
            onPeriodChange={(now, aFrom, aTo, rFrom, rTo) => setStats({
                now: now,
                analysis: {
                    from: aFrom,
                    to: aTo,
                },
                reference: {
                    from: rFrom,
                    to: rTo,
                }
            })}
            isLoading={analysisSales.isFirstLoading || referenceSales.isFirstLoading}
        >
            <CurrencySpan value={analyticsProps.amount} />
        </AnalyticMetricWidget>
    );
};