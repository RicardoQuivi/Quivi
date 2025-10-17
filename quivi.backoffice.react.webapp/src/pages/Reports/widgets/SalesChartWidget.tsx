import { useTranslation } from "react-i18next";
import { SalesPeriod } from "../../../hooks/api/Dtos/reporting/SalesPeriod";
import { useDateHelper } from "../../../utilities/dateHelper";
import { useNow } from "../../../hooks/useNow";
import { ApexOptions } from "apexcharts";
import { useEffect, useMemo, useState } from "react";
import { useSalesQuery } from "../../../hooks/queries/implementations/useSalesQuery";
import { Collections } from "../../../utilities/Collectionts";
import { CurrencyHelper } from "../../../utilities/currencyHelper";
import Chart from "react-apexcharts";
import { OptionSelector } from "./OptionSelector";
import { DownloadIcon } from "../../../icons";
import { Modal } from "../../../components/ui/modal/Modal";
import { ExportFileType } from "../../../hooks/api/Dtos/ExportFileType";
import { Files } from "../../../utilities/files";
import { SingleSelect } from "../../../components/inputs/SingleSelect";
import { ModalButtonsFooter } from "../../../components/ui/modal/ModalButtonsFooter";
import { DatePicker } from "../../../components/inputs/DatePicker";
import { useReportingApi } from "../../../hooks/api/useReportingApi";
import { useToast } from "../../../layout/ToastProvider";

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

    const [isExportModalOpen, setIsExportModalOpen] = useState(false);
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

    return <>
        <div className="rounded-2xl border border-gray-200 bg-white px-5 pt-5 dark:border-gray-800 dark:bg-white/[0.03] sm:px-6 sm:pt-6">
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
                    options={props.adminView == true ? [
                        SalesPeriod.Hourly,
                        SalesPeriod.Daily,
                        SalesPeriod.Monthly,
                    ] : [
                        undefined,
                        SalesPeriod.Hourly,
                        SalesPeriod.Daily,
                        SalesPeriod.Monthly,
                    ]}
                    getKey={s => s == undefined ? "export" : s}
                    render={s => {
                        if(s == undefined) {
                            return <div className="flex items-center gap-2">
                                <DownloadIcon />
                                {t("widgets.sales.export")}
                            </div>
                        }
                        switch(s) {
                            case SalesPeriod.Hourly: return `24 ${t("dateHelper.units.hours")}`;
                            case SalesPeriod.Daily: return `31 ${t("dateHelper.units.days")}`;
                            case SalesPeriod.Monthly: return `12 ${t("dateHelper.units.months")}`;
                        }
                    }}
                    selected={state.period}
                    onChange={p => {
                        if(p == undefined) {
                            setIsExportModalOpen(true);
                            return;
                        }
                        setState(s => ({ ...s, period: p }));
                    }}
                />
            </div>
            <div className="max-w-full overflow-x-auto custom-scrollbar">
                <div className="-ml-5 min-w-[1300px] xl:min-w-full pl-2">
                    <Chart options={options} series={series} type="bar" height={350} />
                </div>
            </div>
        </div>
        <ExportSalesModal
            isOpen={isExportModalOpen}
            onClose={() => setIsExportModalOpen(false)}
        />
    </>
}


const getTodayRange = (now: Date) => {
    // Today at 00:00
    const todayAtMidnight = new Date(now);
    todayAtMidnight.setHours(0, 0, 0, 0);

    // Tomorrow at 00:00
    const tomorrowAtMidnight = new Date(todayAtMidnight);
    tomorrowAtMidnight.setDate(tomorrowAtMidnight.getDate() + 1);

    return [todayAtMidnight, tomorrowAtMidnight]
}

interface ExportSalesModalProps {
    readonly isOpen: boolean;
    readonly onClose: () => any;
}
const ExportSalesModal = (props: ExportSalesModalProps) => {
    const { t } = useTranslation();
    const reportingApi = useReportingApi();
    const toast = useToast();

    const [dateRange, setDateRange] = useState(() => getTodayRange(new Date()));
    const [state, setState] = useState(() => ({
        isOpen: false,
        type: ExportFileType.Excel,
        isLoading: false,
    }));

    useEffect(() => setDateRange(() => getTodayRange(new Date())), [props.isOpen])

    const submit = async () => {
        setState(p => ({...p, isLoading: true}))

        const to = new Date(dateRange[1]);
        to.setDate(to.getDate() + 1);
        try {
            const result = await reportingApi.exportSales({
                type: state.type,
                from: dateRange[0].toISOString(),
                to: to.toISOString(),
                labels: {
                    date: t("widgets.sales.exportLabels.date"),
                    transactionId: t("widgets.sales.exportLabels.transactionId"),
                    invoice: t("widgets.sales.exportLabels.invoice"),
                    id: t("widgets.sales.exportLabels.id"),
                    method: t("widgets.sales.exportLabels.method"),
                    menuId: t("widgets.sales.exportLabels.menuId"),
                    item: t("widgets.sales.exportLabels.item"),
                    unitPrice: t("widgets.sales.exportLabels.unitPrice"),
                    quantity: t("widgets.sales.exportLabels.quantity"),
                    total: t("widgets.sales.exportLabels.total"),
                }
            });
            Files.saveBase64File(result.data, result.name, getContentType(state.type))
            toast.success(t("widgets.sales.exportSuccess"));
            props.onClose();
        } catch {
            toast.error(t("common.operations.failure.generic"));
        } finally {
            setState(p => ({...p, isLoading: false}))
        }
    }

    const getContentType = (type: ExportFileType): string => {
        switch(type)
        {
            case ExportFileType.CSV: return "data:text/csv;base64";
            case ExportFileType.Excel: return "application/xls;base64";
        }
    }
    
    return <Modal
        title={t("widgets.sales.export")}
        isOpen={props.isOpen}
        onClose={props.onClose}
        footer={<ModalButtonsFooter
            primaryButton={{
                isLoading: state.isLoading,
                content: t("common.confirm"),
                disabled: state.isLoading,
                onClick: submit,
            }}
            secondaryButton={{
                content: t("common.close"),
                onClick: props.onClose,
            }}
        />}
    >
        <div className="grid grid-cols-1 gap-4 md:gap-6">
            <DatePicker
                label={t("widgets.sales.dateRange")}
                mode="range"
                defaultDate={dateRange}
                onChange={ (selectedDates, _dateStr, instance) => {
                    if (instance.config.mode !== "range") {
                        throw new Error();
                    }

                    if (selectedDates.length == 2) {
                        setDateRange(selectedDates)
                    }
                    // else if (selectedDates.length == 1) {
                    //     setDateRange([selectedDates[0], selectedDates[0]])
                    // }
                }}
            />
        </div>
        <SingleSelect
            label={t("common.fileFormat")}
            value={state.type}
            options={[
                ExportFileType.CSV,
                ExportFileType.Excel,
            ]}
            getId={e => ExportFileType[e]}
            render={e => ExportFileType[e]}
            onChange={o => setState(s => ({ ...s, type: o}))}
        />
    </Modal>
}