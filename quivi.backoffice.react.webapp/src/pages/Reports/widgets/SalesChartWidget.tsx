import { useTranslation } from "react-i18next";
import { SalesPeriod } from "../../../hooks/api/Dtos/reporting/SalesPeriod";
import { useDateHelper } from "../../../utilities/dateHelper";
import { useNow } from "../../../hooks/useNow";
import { ApexOptions } from "apexcharts";
import { useMemo, useState } from "react";
import { useSalesQuery } from "../../../hooks/queries/implementations/useSalesQuery";
import { Collections } from "../../../utilities/Collectionts";
import { CurrencyHelper } from "../../../utilities/currencyHelper";
import Chart from "react-apexcharts";
import { OptionSelector } from "./OptionSelector";

const defaultOptions: ApexOptions = {
    colors: [
        "#465fff"
    ],
    chart: {
        type: "bar",
        height: 350,
        toolbar: {
            show: false,
        },
    },
    stroke: {
        show: true,
        width: 4,
        colors: ["transparent"],
    },
    plotOptions: {
        bar: {
            horizontal: false,
            columnWidth: "45%",
            borderRadius: 5,
            borderRadiusApplication: "end",
        },
    },
    dataLabels: {
        enabled: false,
    },
    xaxis: {
        axisBorder: {
            show: false,
        },
        axisTicks: {
            show: false,
        },
    },
    legend: {
        show: true,
        position: "top",
        horizontalAlign: "left",
        fontFamily: "Outfit",
    },
    grid: {
        yaxis: {
            lines: {
                show: true,
            },
        },
    },
    fill: {
        opacity: 1,
    },
    tooltip: {
        x: {
            show: false,
        },
        y: {
            formatter: (val: number) => `${val}`,
        },
    },
};

const getRangeData = (
    now: Date,
    period: SalesPeriod,
    transform: (rawDate: Date) => string
) => {
    const startDate = new Date(now);
    const endDate = new Date(now);

    const units = [];

    switch (period) {
        case SalesPeriod.Hourly:
            // Last 24 hours
            startDate.setUTCHours(now.getHours() - 24, 0, 0, 0);
            endDate.setUTCHours(now.getHours() + 1, 0, 0, 0);

            for (let d = new Date(startDate); d < endDate; d.setUTCHours(d.getUTCHours() + 1)) {
                units.push(new Date(d));
            }
            break;

        case SalesPeriod.Daily:
            // Last 31 days
            startDate.setUTCDate(now.getUTCDate() - 31);
            startDate.setUTCHours(0, 0, 0, 0);

            endDate.setUTCDate(now.getUTCDate() + 1);
            endDate.setUTCHours(0, 0, 0, 0);

            for (let d = new Date(startDate); d < endDate; d.setUTCDate(d.getUTCDate() + 1)) {
                units.push(new Date(d));
            }
            break;

        case SalesPeriod.Monthly:
            // Last 12 months
            startDate.setUTCMonth(now.getUTCMonth() - 11, 1);
            startDate.setUTCHours(0, 0, 0, 0);

            endDate.setMonth(now.getUTCMonth() + 1, 1);
            endDate.setUTCHours(0, 0, 0, 0);

            for (let d = new Date(startDate); d < endDate; d.setUTCMonth(d.getUTCMonth() + 1)) {
                units.push(new Date(d));
            }
            break;
    }

    const labels = units.map(transform);

    return {
        now,
        from: startDate,
        to: endDate,
        units: labels,
        period: period,
    };
};

interface Props {
    readonly adminView?: boolean;
}
export const SalesChartWidget = (props: Props) => {
    const { t, i18n } = useTranslation();
    const dateHelper = useDateHelper();
    const now = useNow(1000 * 60);

    const [state, setState] = useState(() => ({
        period: SalesPeriod.Daily,
        page: 0,
        pageSize: 25,
    }))
    const stats = useMemo(() => getRangeData(now, state.period, d => {
        switch (state.period) {
            case SalesPeriod.Hourly: return dateHelper.toLocalString(d, "HH:mm");
            case SalesPeriod.Daily: return dateHelper.toLocalString(d, "DD MMM");
            case SalesPeriod.Monthly: return dateHelper.toLocalString(d, "MMM");
        }
    }), [now, dateHelper, state.period]);

    const salesQuery = useSalesQuery({
        adminView: props.adminView,
        period: state.period,
        from: stats.from.toISOString(),
        to: stats.to.toISOString(),
        page: state.page,
        pageSize: state.pageSize,
    })

    const series = useMemo(() => {
        const map = Collections.toMap(salesQuery.data, d => dateHelper.toDate(d.from).toISOString());
        const units = [];

        switch (stats.period) {
            case SalesPeriod.Hourly:
                for (let d = new Date(stats.from); d < stats.to; d.setUTCHours(d.getUTCHours() + 1)) {
                    const aux = map.get(d.toISOString());
                    units.push(aux?.total ?? 0);
                }
                break;

            case SalesPeriod.Daily:
                for (let d = new Date(stats.from); d < stats.to; d.setUTCDate(d.getUTCDate() + 1)) {
                    const aux = map.get(d.toISOString());
                    units.push(aux?.total ?? 0);
                }
                break;

            case SalesPeriod.Monthly:
                for (let d = new Date(stats.from); d < stats.to; d.setUTCMonth(d.getUTCMonth() + 1)) {
                    const aux = map.get(d.toISOString());
                    units.push(aux?.total ?? 0);
                }
                break;
        }

        return [
            {
                name: t("widgets.sales.title"),
                data: units,
            },
        ];
    }, [salesQuery.data, stats])

    const options = useMemo<ApexOptions>(() => {
        const formatter = (value: number) => CurrencyHelper.format({
            value: value,
            culture: i18n.language,
        });
        return {
            ...defaultOptions,
            xaxis: {
                ...defaultOptions.xaxis,
                categories: stats.units,
            },
            yaxis: {
                ...defaultOptions.yaxis,
                labels: {
                    formatter: formatter,
                },
            },
            tooltip: {
                ...defaultOptions.tooltip,
                y: {
                    ...defaultOptions.tooltip?.y,
                    formatter: formatter,
                }
            }
        }
    }, [i18n.language, stats.units])

    const getChartLabel = () => {
        switch (stats.period) {
            case SalesPeriod.Hourly: return t("widgets.sales.descriptionHourly", {
                value: 24,
            });
            case SalesPeriod.Daily: return t("widgets.sales.descriptionDaily", {
                value: 31,
            });
            case SalesPeriod.Monthly: return t("widgets.sales.descriptionMonthly", {
                value: 12,
            });
        }
    }


    return <div className="rounded-2xl border border-gray-200 bg-white px-5 pt-5 dark:border-gray-800 dark:bg-white/[0.03] sm:px-6 sm:pt-6">
        <div className="flex flex-wrap items-start justify-between gap-5">
            <div>
                <h3 className="mb-1 text-lg font-semibold text-gray-800 dark:text-white/90">
                    {t("widgets.sales.title")}
                </h3>
                <span className="block text-gray-500 text-theme-sm dark:text-gray-400">
                    {getChartLabel()}
                </span>
            </div>
            <OptionSelector
                options={[
                    SalesPeriod.Hourly,
                    SalesPeriod.Daily,
                    SalesPeriod.Monthly,
                ]}
                getKey={s => s}
                render={s => {
                    switch(s) {
                        case SalesPeriod.Hourly: return `24 ${t("dateHelper.units.hours")}`;
                        case SalesPeriod.Daily: return `31 ${t("dateHelper.units.days")}`;
                        case SalesPeriod.Monthly: return `12 ${t("dateHelper.units.months")}`;
                    }
                }}
                selected={state.period}
                onChange={p => setState(s => ({ ...s, period: p! }))}
            />
        </div>
        <div className="max-w-full overflow-x-auto custom-scrollbar">
            <div className="-ml-5 min-w-[1300px] xl:min-w-full pl-2">
                <Chart options={options} series={series} type="bar" height={350} />
            </div>
        </div>
    </div>
}