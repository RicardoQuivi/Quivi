import { useTranslation } from "react-i18next";
import Badge from "../../../components/ui/badge/Badge";
import { OptionSelector } from "./OptionSelector";
import { useEffect, useState } from "react";
import { useNow } from "../../../hooks/useNow";
import { useDateHelper } from "../../../utilities/dateHelper";
import { Skeleton } from "../../../components/ui/skeleton/Skeleton";
import { Spinner } from "../../../components/spinners/Spinner";

enum MetricReferencePeriod {
    Yesterday,
    LastWeek,
    LastMonth,
}
interface AnalyticMetricWidgetProps {
    readonly title: string;
    readonly children: React.ReactNode;
    readonly percentage: number;
    readonly onPeriodChange: (now: Date, analysisFrom: Date, analysisTo: Date, referenceFrom: Date, referenceTo: Date) => any;
    readonly isLoading?: boolean;
}
export const AnalyticMetricWidget = (props: AnalyticMetricWidgetProps) => {
    const { t } = useTranslation();
    const dateHelper = useDateHelper();

    const now = useNow(1000 * 60);
    const [state, setState] = useState(() => getRange(now, MetricReferencePeriod.LastMonth));

    useEffect(() => props.onPeriodChange?.(state.now, state.analysisPeriod.from, state.analysisPeriod.to, state.referencePeriod.from, state.referencePeriod.to), [state])

    const getAnalysedPeriodLavel = () => {
        switch(state.period) {
            case MetricReferencePeriod.Yesterday: return t("widgets.analyticMetric.analysedYesterday");
            case MetricReferencePeriod.LastWeek: return t("widgets.analyticMetric.analysedLastWeek", {
                to: dateHelper.toLocalString(state.analysisPeriod.to, "HH:mm"),
            });
            case MetricReferencePeriod.LastMonth: return t("widgets.analyticMetric.analysedLastMonth", {
                to: dateHelper.toLocalString(state.analysisPeriod.to, "HH:mm"),
            });
        }
    }

    const getReferecePeriodLavel = () => {
        switch(state.period)
        {
            case MetricReferencePeriod.Yesterday: return t("widgets.analyticMetric.vsYesterday");
            case MetricReferencePeriod.LastWeek: return t("widgets.analyticMetric.vsLastWeek");
            case MetricReferencePeriod.LastMonth: return t("widgets.analyticMetric.vsLastMonth");
        }
    }

    return (
        <div
            className="rounded-2xl border border-gray-200 bg-white p-5 dark:border-gray-800 dark:bg-white/[0.03]"
        >
            <div className="flex items-start justify-between">
                <p className="text-gray-500 text-theme-sm dark:text-gray-400">
                    {props.title}
                </p>
                <OptionSelector
                    options={[
                        MetricReferencePeriod.Yesterday,
                        MetricReferencePeriod.LastWeek,
                        MetricReferencePeriod.LastMonth,
                    ]}
                    getKey={s => s}
                    render={s => {
                        switch(s) {
                            case MetricReferencePeriod.Yesterday: return t("widgets.analyticMetric.yesterday")
                            case MetricReferencePeriod.LastWeek: return t("widgets.analyticMetric.lastWeek")
                            case MetricReferencePeriod.LastMonth: return t("widgets.analyticMetric.lastMonth")
                        }
                    }}
                    selected={state.period}
                    onChange={s => setState(getRange(new Date(), s))}
                />
            </div>

            <div className="flex items-end justify-between mt-3 gap-2">
                <div className="flex items-start justify-between flex-col">
                    <h4 className="text-2xl font-bold text-gray-800 dark:text-white/90">
                        {
                            props.isLoading == true
                            ?
                            <Skeleton />
                            :
                            props.children
                        }
                    </h4>
                    <span className="text-gray-500 text-theme-xs dark:text-gray-400">
                        {getAnalysedPeriodLavel()}
                    </span>
                </div>
                <div className="flex items-center gap-1 flex-col">
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
                    {
                        props.isLoading
                        ?
                        <Spinner />
                        :
                        <span className="text-xs"> {props.percentage.toFixed(2)}%</span>
                    }
                    </Badge>
                    <span className="text-gray-500 text-theme-xs dark:text-gray-400">
                        {getReferecePeriodLavel()}
                    </span>
                </div>
            </div>
        </div>
    )
}


const getRange = (now: Date, period: MetricReferencePeriod) => {
    const currentDate = now.getUTCDate(); // Day of the month (1-31)
    
    switch (period) {
        case MetricReferencePeriod.Yesterday: {
            // Analysis: from yesterday 00:00 to now (same time of day)
            const analysisFrom = new Date(Date.UTC(
                now.getUTCFullYear(),
                now.getUTCMonth(),
                now.getUTCDate() - 1,
                0, 0, 0, 0
            ));
            const analysisTo = now;

            // Reference: from day before yesterday 00:00 to same time as now yesterday
            const referenceFrom = new Date(Date.UTC(
                now.getUTCFullYear(),
                now.getUTCMonth(),
                now.getUTCDate() - 2,
                0, 0, 0, 0
            ));
            const referenceTo = new Date(referenceFrom);
            referenceTo.setUTCDate(referenceFrom.getUTCDate() + 1);
            referenceTo.setUTCHours(now.getUTCHours(), now.getUTCMinutes(), now.getUTCSeconds(), now.getUTCMilliseconds());

            return {
                period: period,
                now: now,
                referencePeriod: {
                    from: referenceFrom,
                    to: referenceTo,
                },
                analysisPeriod: {
                    from: analysisFrom,
                    to: analysisTo,
                },
            };
        }

        case MetricReferencePeriod.LastWeek: {
            // "Last weekday" = same weekday, previous week
            const analysisFrom = new Date(Date.UTC(
                now.getUTCFullYear(),
                now.getUTCMonth(),
                now.getUTCDate() - 7,
                0, 0, 0, 0
            ));
            const analysisTo = now;

            const referenceFrom = new Date(Date.UTC(
                now.getUTCFullYear(),
                now.getUTCMonth(),
                now.getUTCDate() - 14,
                0, 0, 0, 0
            ));
            const referenceTo = new Date(referenceFrom);
            referenceTo.setUTCDate(referenceTo.getUTCDate() + 7);
            referenceTo.setUTCHours(now.getUTCHours(), now.getUTCMinutes(), now.getUTCSeconds(), now.getUTCMilliseconds());

            return {
                period: period,
                now: now,
                referencePeriod: {
                    from: referenceFrom,
                    to: referenceTo,
                },
                analysisPeriod: {
                    from: analysisFrom,
                    to: analysisTo,
                },
            };
        }

        case MetricReferencePeriod.LastMonth:
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
                period: period,
                now: now,
                referencePeriod: {
                    from: previousMonthFrom,
                    to: previousMonthTo,
                },
                analysisPeriod: {
                    from: currentMonthFrom,
                    to: now,
                },
            };
    }
};
