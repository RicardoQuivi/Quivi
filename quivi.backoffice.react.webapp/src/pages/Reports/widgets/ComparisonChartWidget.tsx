import { ApexOptions } from "apexcharts";
import { useMemo } from "react";
import Chart from "react-apexcharts";
import { SalesPeriod } from "../../../hooks/api/Dtos/reporting/SalesPeriod";
import { PeriodSelector } from "./PeriodSelector";

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
}
export const ComparisonChartWidget = <T,>(props: Props<T>) => {
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
                                formatter: formatter == undefined ? undefined : d => {
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
        <div className="rounded-2xl border border-gray-200 bg-white p-5 dark:border-gray-800 dark:bg-white/[0.03] sm:p-6">
            <div className="flex items-center justify-between mb-9">
                <div>
                    <h3 className="text-lg font-semibold text-gray-800 dark:text-white/90">
                        {props.title}
                    </h3>
                </div>
                <PeriodSelector
                    period={props.period}
                    showFromEver
                    onChange={props.onPeriodChange}
                />
            </div>
            <div>
                <div className="flex justify-center mx-auto">
                    <Chart
                        options={options}
                        series={series}
                        type="donut"
                        height={290}
                    />
                </div>
            </div>
        </div>
    );
}