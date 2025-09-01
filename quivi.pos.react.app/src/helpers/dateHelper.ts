import { useMemo } from "react";
import { useTranslation } from "react-i18next";

export enum TimeUnit {
    Milliseconds,
    Seconds,
    Minutes,
    Hours,
    Days,
    Months,
}

export const useDateHelper = () => {
    const { t } = useTranslation();

    const toDate = (date: string | Date): Date => {
        if (typeof date === 'string') {
            return new Date(Date.parse(date));
        }
        return date;
    }

    const getTimeTaken = (date: string | Date, reference: string | Date, unit: TimeUnit) => {
        const referenceM = toDate(reference).getTime();
        const dateM = toDate(date).getTime();

        let diff = referenceM - dateM;
        switch(unit)
        {
            case TimeUnit.Milliseconds: break;
            case TimeUnit.Seconds: diff /= 1000; break;
            case TimeUnit.Minutes: diff /= 1000 * 60; break;
            case TimeUnit.Hours: diff /= 1000 * 60 * 60; break;
            case TimeUnit.Days: diff /= 1000 * 60 * 60 * 24; break;
            case TimeUnit.Months: diff /= 1000 * 60 * 60 * 24 * 30; break;
        }
        return Math.round(diff);
    }

    const getTimeAgo = (dateStr: string | Date, reference: string | Date) => {
        let res = getTimeTaken(reference, dateStr, TimeUnit.Seconds);
        if (res < 60) {
            return t("justNow");
        }

        res = getTimeTaken(reference, dateStr, TimeUnit.Minutes);
        if (res < 60) {
            return t("timeAgo", {value: res, time: res == 1 ? t("minute").toLowerCase() : t("minutes").toLowerCase()});
        }

        res = getTimeTaken(reference, dateStr, TimeUnit.Hours);
        if(res < 24) {
            return t("timeAgo", {value: res, time: res == 1 ? t("hour").toLowerCase() : t("hours").toLowerCase()});
        }

        res = getTimeTaken(reference, dateStr, TimeUnit.Days);
        if(res < 31) {
            return t("timeAgo", {value: res, time: res == 1 ? t("day").toLowerCase() : t("days").toLowerCase()});
        }
        
        res = getTimeTaken(reference, dateStr, TimeUnit.Months);
        if(res <= 12) {
            return t("timeAgo", {value: res, time: res == 1 ? t("month").toLowerCase() : t("months").toLowerCase()});
        }
        
        return t("longTimeAgo");
    }

    const toLocalString = (rawDate: string | Date, format: string): string => {
        const date = toDate(rawDate);

        const day = date.getDate();
        const month = date.getMonth();
        const year = date.getFullYear();

        const monthNames = ["january", "february", "march", "april", "may", "june", "july", "august", "september", "october", "november", "december"];

        // Replace tokens in the format string
        return format.replace(/YYYY/g, year.toString())
                    .replace(/YY/g, year.toString().slice(-2))
                    .replace(/MMMM/g, t(`dateHelper.months.full.${monthNames[month]}`))
                    .replace(/MMM/g, t(`dateHelper.months.short.${monthNames[month]}`))
                    .replace(/MM/g, String(month + 1).padStart(2, "0"))
                    .replace(/M/g, String(month + 1))
                    .replace(/DD/g, String(day).padStart(2, "0"))
                    .replace(/D/g, String(day));
    }

    const result = useMemo(() => ({
        toDate,
        getTimeAgo,
        getTimeTaken,
        toLocalString,
    }), [t])

    return result;
}