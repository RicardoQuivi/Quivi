import { useTranslation } from "react-i18next";
import { SalesPeriod } from "../../../hooks/api/Dtos/reporting/SalesPeriod";

interface PeriodSelectorProps {
    readonly period?: SalesPeriod;
    readonly showFromEver?: boolean;
    readonly onChange: (s?: SalesPeriod) => any;
}
export const PeriodSelector = (props: PeriodSelectorProps) => {
    const { t } = useTranslation();

    const getButtonClass = (option?: SalesPeriod) => props.period === option ? "shadow-theme-xs text-gray-900 dark:text-white bg-white dark:bg-gray-800" : "text-gray-500 dark:text-gray-400";

    return (
        <div className="flex items-center gap-0.5 rounded-lg bg-gray-100 p-0.5 dark:bg-gray-900">
            <button
                onClick={() => props.onChange(SalesPeriod.Hourly)}
                className={`px-3 py-2 font-medium w-full rounded-md text-theme-sm hover:text-gray-900 dark:hover:text-white ${getButtonClass(SalesPeriod.Hourly)} whitespace-nowrap`}
            >
                24 {t("dateHelper.units.hours")}
            </button>

            <button
                onClick={() => props.onChange(SalesPeriod.Daily)}
                className={`px-3 py-2 font-medium w-full rounded-md text-theme-sm hover:text-gray-900 dark:hover:text-white ${getButtonClass(SalesPeriod.Daily)} whitespace-nowrap`}
            >
                31 {t("dateHelper.units.days")}
            </button>

            <button
                onClick={() => props.onChange(SalesPeriod.Monthly)}
                className={`px-3 py-2 font-medium w-full rounded-md text-theme-sm hover:text-gray-900 dark:hover:text-white ${getButtonClass(SalesPeriod.Monthly)} whitespace-nowrap`}
            >
                12 {t("dateHelper.units.months")}
            </button>
            {
                props.showFromEver == true &&
                <button
                    onClick={() => props.onChange(undefined)}
                    className={`px-3 py-2 font-medium w-full rounded-md text-theme-sm hover:text-gray-900 dark:hover:text-white ${getButtonClass(undefined)} whitespace-nowrap`}
                >
                    {t("dateHelper.allTime")}
                </button>
            }
        </div>
    );
};