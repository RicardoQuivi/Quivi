import { useMemo } from "react";
import CurrencySpan from "../../../components/currency/CurrencySpan";
import { SalesPeriod } from "../../../hooks/api/Dtos/reporting/SalesPeriod";
import { useSalesQuery } from "../../../hooks/queries/implementations/useSalesQuery";
import { useNow } from "../../../hooks/useNow";
import { useTranslation } from "react-i18next";
import Badge from "../../../components/ui/badge/Badge";

const getImprovement = (current: number, previous: number) => {
    if (previous === 0) {
        return 0;
    }
    return ((current - previous) / previous * 100);
}

interface Props {
    readonly adminView?: boolean;
}
export const MonthlySalesMetricsWidget = (props: Props) => {
    const now = useNow(1000 * 60);
    const { t } = useTranslation();
    
    const stats = useMemo(() => {
        const now = new Date(); // Current date and time
        const currentDate = now.getUTCDate(); // Day of the month (1-31)

        // Current month interval: from the 1st of the current month to now
        const currentMonthFrom = new Date(now);
        currentMonthFrom.setUTCMonth(now.getUTCMonth(), 1); // Set to 1st of current month
        currentMonthFrom.setUTCHours(0, 0, 0, 0); // Midnight UTC

        // Previous month interval: from the 1st of the previous month to the same day as now
        const previousMonthFrom = new Date(now);
        previousMonthFrom.setUTCMonth(now.getUTCMonth() - 1, 1); // Set to 1st of previous month
        previousMonthFrom.setUTCHours(0, 0, 0, 0); // Midnight UTC

        const previousMonthTo = new Date(previousMonthFrom);
        previousMonthTo.setUTCDate(currentDate); // Set to same day as current date
        previousMonthTo.setUTCHours(0, 0, 0, 0); // Midnight UTC

        return {
            previousMonthInterval: {
                from: previousMonthFrom,
                to: previousMonthTo,
            },
            currentMonthInterval: {
                from: currentMonthFrom,
                to: now,
            },
        };
    }, [now]);

    const currentMonthSales = useSalesQuery({
        adminView: props.adminView,
        period: SalesPeriod.Monthly,
        from: stats.currentMonthInterval.from.toISOString(),
        to: stats.currentMonthInterval.to.toISOString(),
        page: 0,
        pageSize: 1,
    })
    const previousMonthSales = useSalesQuery({
        adminView: props.adminView,
        period: SalesPeriod.Monthly,
        from: stats.previousMonthInterval.from.toISOString(),
        to: stats.previousMonthInterval.to.toISOString(),
        page: 0,
        pageSize: 1,
    })

    const data = useMemo(() => {
        const currentMonth = currentMonthSales.data.length == 0 ? {
            from: stats.currentMonthInterval.from,
            to: stats.currentMonthInterval.to,
            payment: 0,
            paymentRefund: 0,
            tip: 0,
            tipRefund: 0,
            total: 0,
            totalRefund: 0,
        } : currentMonthSales.data[0];

        const previousMonth = previousMonthSales.data.length == 0 ? {
            from: stats.previousMonthInterval.from,
            to: stats.previousMonthInterval.to,
            payment: 0,
            paymentRefund: 0,
            tip: 0,
            tipRefund: 0,
            total: 0,
            totalRefund: 0,
        } : previousMonthSales.data[0];

        return {
            currentMonth: currentMonth,
            previousMonth: previousMonth,
        }
    }, [currentMonthSales.data, previousMonthSales.data, stats])

    return (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 md:gap-6 xl:grid-cols-4">
            <AnalyticMetric
                title={t("widgets.analytics.totalSales")}
                comparisonText={t("widgets.analytics.vsLastMonth")}
                percentage={getImprovement(data.currentMonth.total, data.previousMonth.total)}
            >
                <CurrencySpan value={data.currentMonth.total} />
            </AnalyticMetric>
            <AnalyticMetric
                title={t("widgets.analytics.totalAmount")}
                comparisonText={t("widgets.analytics.vsLastMonth")}
                percentage={getImprovement(data.currentMonth.payment, data.previousMonth.payment)}
            >
                <CurrencySpan value={data.currentMonth.payment} />
            </AnalyticMetric>
            <AnalyticMetric
                title={t("widgets.analytics.totalTips")}
                comparisonText={t("widgets.analytics.vsLastMonth")}
                percentage={getImprovement(data.currentMonth.tip, data.previousMonth.tip)}
            >
                <CurrencySpan value={data.currentMonth.tip} />
            </AnalyticMetric>
            <AnalyticMetric
                title={t("widgets.analytics.totalRefunds")}
                comparisonText={t("widgets.analytics.vsLastMonth")}
                percentage={-1 * getImprovement(data.currentMonth.totalRefund, data.previousMonth.totalRefund)}
            >
                <CurrencySpan value={data.currentMonth.totalRefund} />
            </AnalyticMetric>
        </div>
    );
};

interface AnalyticMetricProps {
    readonly title: string;
    readonly children: React.ReactNode;
    readonly comparisonText: string;
    readonly percentage: number;
}
const AnalyticMetric = (props: AnalyticMetricProps) => {
    return (
        <div
            className="rounded-2xl border border-gray-200 bg-white p-5 dark:border-gray-800 dark:bg-white/[0.03]"
        >
            <p className="text-gray-500 text-theme-sm dark:text-gray-400">
                {props.title}
            </p>
            <div className="flex items-end justify-between mt-3">
                <div>
                    <h4 className="text-2xl font-bold text-gray-800 dark:text-white/90">
                        {props.children}
                    </h4>
                </div>
                <div className="flex items-center gap-1">
                    <Badge
                        color={
                            props.percentage > 0
                                ?
                                "success"
                                :
                                (
                                    props.percentage < 0
                                        ?
                                        "error"
                                        :
                                        "warning"
                                )
                        }
                    >
                        <span className="text-xs"> {props.percentage.toFixed(2)}%</span>
                    </Badge>
                    <span className="text-gray-500 text-theme-xs dark:text-gray-400">
                        {props.comparisonText}
                    </span>
                </div>
            </div>
        </div>
    )
}