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
            return t("dateHelper.units.justNow");
        }

        res = getTimeTaken(reference, dateStr, TimeUnit.Minutes);
        if (res < 60) {
            return t("dateHelper.units.timeAgo", {value: res, time: res == 1 ? t("dateHelper.units.minute").toLowerCase() : t("dateHelper.units.minutes").toLowerCase()});
        }

        res = getTimeTaken(reference, dateStr, TimeUnit.Hours);
        if(res < 24) {
            return t("dateHelper.units.timeAgo", {value: res, time: res == 1 ? t("dateHelper.units.hour").toLowerCase() : t("dateHelper.units.hours").toLowerCase()});
        }

        res = getTimeTaken(reference, dateStr, TimeUnit.Days);
        if(res < 31) {
            return t("dateHelper.units.timeAgo", {value: res, time: res == 1 ? t("dateHelper.units.day").toLowerCase() : t("dateHelper.units.days").toLowerCase()});
        }
        
        res = getTimeTaken(reference, dateStr, TimeUnit.Months);
        if(res <= 12) {
            return t("dateHelper.units.timeAgo", {value: res, time: res == 1 ? t("dateHelper.units.month").toLowerCase() : t("dateHelper.units.months").toLowerCase()});
        }
        
        return t("dateHelper.units.longTimeAgo");
    }

    const toLocalString = (rawDate: string | Date, format: string): string => {
        const date = toDate(rawDate);

        const day = date.getDate();
        const month = date.getMonth();
        const year = date.getFullYear();
        const hours = date.getHours();
        const minutes = date.getMinutes();
        const seconds = date.getSeconds();

        const monthNames = [
            "january", "february", "march", "april", "may", " june", "july", "august", "september", "october", "november", "december"
        ];

        // Define tokens and their replacement logic
        const tokens: { token: string; replace: () => string }[] = [
            { token: "MMMM", replace: () => t(`dateHelper.months.full.${monthNames[month]}`) }, // Full month name
            { token: "MMM", replace: () => t(`dateHelper.months.short.${monthNames[month]}`) }, // Short month name
            { token: "MM", replace: () => String(month + 1).padStart(2, "0") }, // Two-digit month
            { token: "M", replace: () => String(month + 1) }, // Single-digit month
            { token: "YYYY", replace: () => year.toString() }, // Four-digit year
            { token: "YY", replace: () => year.toString().slice(-2) }, // Two-digit year
            { token: "DD", replace: () => String(day).padStart(2, "0") }, // Two-digit day
            { token: "D", replace: () => String(day) }, // Single-digit day
            { token: "HH", replace: () => String(hours).padStart(2, "0") }, // 24-hour format with leading zero
            { token: "H", replace: () => String(hours) }, // 24-hour format without leading zero
            { token: "mm", replace: () => String(minutes).padStart(2, "0") }, // Minutes with leading zero
            { token: "m", replace: () => String(minutes) }, // Minutes without leading zero
            { token: "ss", replace: () => String(seconds).padStart(2, "0") }, // Seconds with leading zero
            { token: "s", replace: () => String(seconds) }, // Seconds without leading zero
        ];

        // Sort tokens by length (descending) to prioritize longer tokens (e.g., MMMM before MM)
        const sortedTokens = tokens.sort((a, b) => b.token.length - a.token.length);

        // Build a regex to match any token, escaping special characters
        const tokenRegex = new RegExp(
            sortedTokens.map(t => t.token.replace(/[-[\]{}()*+?.,\\^$|#\s]/g, "\\$&")).join("|"),
            "g"
        );

        // Split the format string into tokens and literals
        let result = "";
        let lastIndex = 0;

        // Iterate through all matches of tokens in the format string
        tokenRegex.lastIndex = 0; // Ensure regex starts from the beginning
        let match: RegExpExecArray | null;
        while ((match = tokenRegex.exec(format)) !== null) {
            // Add any literal text before the current token
            if (match.index > lastIndex) {
                result += format.slice(lastIndex, match.index);
            }

            // Find the matching token and apply its replacement
            const aux = match;
            const matchedToken = sortedTokens.find(t => t.token === aux[0]);
            if (matchedToken) {
                result += matchedToken.replace();
            }

            lastIndex = match.index + match[0].length;
        }

        // Add any remaining literal text after the last token
        if (lastIndex < format.length) {
            result += format.slice(lastIndex);
        }

        return result;
    };

    const result = useMemo(() => ({
        toDate,
        getTimeAgo,
        getTimeTaken,
        toLocalString,
    }), [t])

    return result;
}