import { PeriodSelector } from "./PeriodSelector";
import { SalesPeriod } from "../../../hooks/api/Dtos/reporting/SalesPeriod";
import { Skeleton } from "../../../components/ui/skeleton/Skeleton";
import { useTranslation } from "react-i18next";

interface TopTableWidgetProps<T,> {
    readonly period?: SalesPeriod;
    readonly onPeriodChange: (s?: SalesPeriod) => any;

    readonly isLoading: boolean;
    readonly title: string;
    readonly data: T[];
    readonly nameLabel: string;
    readonly valueLabel: string;
    readonly getKey: (d: T) => string;
    readonly getName: (d: T) => React.ReactNode;
    readonly getValue: (d: T) => React.ReactNode;
}
export const TopTableWidget = <T,>(props: TopTableWidgetProps<T>) => {
    const { t } = useTranslation();

    return <div className="rounded-2xl border border-gray-200 bg-white p-4 dark:border-gray-800 dark:bg-white/[0.03] md:p-6 flex flex-col">
        <div className="flex items-start justify-between">
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

        <div className="my-6 flex-1">
            <div className="flex items-center justify-between pb-4 border-b border-gray-100 dark:border-gray-800">
                <span className="text-gray-400 text-theme-xs"> {props.nameLabel} </span>
                <span className="text-right text-gray-400 text-theme-xs">
                    {" "}
                    {props.valueLabel}{" "}
                </span>
            </div>

            <div className="relative h-full">
            {
                props.isLoading
                ?
                [1, 2, 3, 4, 5, 6, 7, 8, 9, 10].map(d => (
                    <div
                        key={`Loading-${d}`}
                        className="flex items-center justify-between py-3 border-b border-gray-100 dark:border-gray-800"
                    >
                        <span className="text-gray-500 text-theme-sm dark:text-gray-400">
                            <Skeleton />
                        </span>
                        <span className="text-right text-gray-500 text-theme-sm dark:text-gray-400">
                            <Skeleton />
                        </span>
                    </div>
                ))
                :
                (
                    props.data.length == 0
                    ?
                    <div
                        className="flex items-center justify-center py-3 border-b border-gray-100 dark:border-gray-800 h-full"
                    >
                        <span className="text-center text-gray-500 text-theme-sm dark:text-gray-400">
                            {t("common.noDataAvailable")}
                        </span>
                    </div>
                    :
                    props.data.map(d => (
                        <div
                            key={props.getKey(d)}
                            className="flex items-center justify-between py-3 border-b border-gray-100 dark:border-gray-800"
                        >
                            <span className="text-gray-500 text-theme-sm dark:text-gray-400">
                                {props.getName(d)}
                            </span>
                            <span className="text-right text-gray-500 text-theme-sm dark:text-gray-400">
                                {props.getValue(d)}
                            </span>
                        </div>
                    ))
                )
            }
            </div>
        </div>
    </div>
}