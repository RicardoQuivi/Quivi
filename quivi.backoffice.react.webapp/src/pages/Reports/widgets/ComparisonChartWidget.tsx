import { ApexOptions } from "apexcharts";
import { useMemo } from "react";
import Chart from "react-apexcharts";
import { SalesPeriod } from "../../../hooks/api/Dtos/reporting/SalesPeriod";
import { OptionSelector } from "./OptionSelector";
import { useTranslation } from "react-i18next";
import { Spinner } from "../../../components/spinners/Spinner";

const defaultOptions: ApexOptions = {
    chart: {
        fontFamily: "Outfit, sans-serif",
        type: "donut",
        width: 445,
        height: 290,
    },
    plotOptions: {
        pie: {
            donut: {
                size: "65%",
                background: "transparent",
                labels: {
                    show: true,
                    value: {
                        show: true,
                        offsetY: 0,
                    },
                },
            },
        },
    },
    states: {
        hover: {
            filter: {
                type: "none",
            },
        },
        active: {
            allowMultipleDataPointsSelection: false,
            filter: {
                type: "darken",
            },
        },
    },
    dataLabels: {
        enabled: false,
    },
    tooltip: {
        enabled: false,
    },
    stroke: {
        show: false,
        width: 4, // Creates a gap between the series
    },
    legend: {
        show: true,
        position: "bottom",
        horizontalAlign: "center",
        fontFamily: "Outfit",
        fontSize: "14px",
        fontWeight: 400,
        markers: {
            size: 4,
            shape: "circle",
            strokeWidth: 0,
        },
        itemMargin: {
            horizontal: 10,
            vertical: 0,
        },
        labels: {
            useSeriesColors: true, // Optional: this makes each label color match the corresponding segment color
        },
    },
    responsive: [
        {
            breakpoint: 640,
            options: {
                chart: {
                    width: 370,
                    height: 290,
                },
            },
        },
    ],
};

interface Props<T> {
    readonly period?: SalesPeriod;
    readonly onPeriodChange: (s?: SalesPeriod) => any;

    readonly title: string;
    readonly data: T[];
    readonly getName: (d: T) => string;
    readonly getValue: (d: T) => number;
    readonly formatter?: (s: number) => string;
    readonly isLoading?: boolean;
}
export const ComparisonChartWidget = <T,>(props: Props<T>) => {
    const { t } = useTranslation();

    const series = useMemo(() => {
        const result = [];
        for(const d of props.data) {
            result.push(props.getValue(d));
        }
        return result;
    }, [props.data, props.getName])

    const options = useMemo<ApexOptions>(() => {
        const labels = [];
        for(const d of props.data) {
            labels.push(props.getName(d));
        }

        const formatter = props.formatter;
        return {
            ...defaultOptions,
            labels: labels,
            
            plotOptions: {
                ...defaultOptions?.plotOptions ?? {},
                pie: {
                    ...(defaultOptions.plotOptions?.pie ?? {}),

                    donut: {
                        ...(defaultOptions.plotOptions?.pie?.donut ?? {}),
                        labels: {
                            ...(defaultOptions.plotOptions?.pie?.donut?.labels ?? {}),
                            value: {
                                ...(defaultOptions.plotOptions?.pie?.donut?.labels?.value ?? {}),
                                formatter: formatter == undefined ? d => d : d => {
                                    return formatter(+d);
                                },
                            }
                        }
                    },
                }
            },
        }
    }, [props.data, props.getName, props.formatter])

    return (
        <div className="rounded-2xl border border-gray-200 bg-white p-5 dark:border-gray-800 dark:bg-white/[0.03] sm:p-6 flex flex-col">
            <div className="flex items-center justify-between mb-9 gap-2">
                <div>
                    <h3 className="text-lg font-semibold text-gray-800 dark:text-white/90">
                        {props.title}
                    </h3>
                </div>
                <OptionSelector
                    options={[
                        SalesPeriod.Hourly,
                        SalesPeriod.Daily,
                        SalesPeriod.Monthly,
                        undefined,
                    ]}
                    getKey={s => s == undefined ? "ever" : s}
                    render={s => {
                        if(s == undefined) {
                            return t("dateHelper.allTime");
                        }
                        switch(s) {
                            case SalesPeriod.Hourly: return `24 ${t("dateHelper.units.hours")}`;
                            case SalesPeriod.Daily: return `31 ${t("dateHelper.units.days")}`;
                            case SalesPeriod.Monthly: return `12 ${t("dateHelper.units.months")}`;
                        }
                    }}
                    selected={props.period}
                    onChange={props.onPeriodChange}
                />
            </div>
            <div className="flex items-center justify-center mx-auto flex-1">
            {
                props.isLoading
                ?
                <Spinner
                    className="size-full h-[290px]"
                />
                :
                (
                    props.data.length == 0
                    ?
                    <span className="text-center text-gray-500 text-theme-sm dark:text-gray-400">
                        {t("common.noDataAvailable")}
                    </span>
                    :
                    <Chart
                        options={options}
                        series={series}
                        type="donut"
                        height={290}
                    />
                )
            }
            </div>
        </div>
    );
}