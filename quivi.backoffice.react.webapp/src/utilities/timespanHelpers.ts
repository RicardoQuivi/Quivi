import { TimeSpan } from "../hooks/api/Dtos/TimeSpan";

export class TimeSpanHelper {
    static fromString = (timespan: string): TimeSpan => {
        const ms = parseTimeSpan(timespan);
        const abs = Math.abs(ms);

        const days = Math.floor(abs / 86400000);
        const hours = Math.floor((abs % 86400000) / 3600000);
        const minutes = Math.floor((abs % 3600000) / 60000);
        const seconds = Math.floor((abs % 60000) / 1000);
        //const milliseconds = abs % 1000;

        return {
            days: days,
            hours: hours,
            minutes: minutes,
            seconds: seconds,
        }
    }

    static toDate = (input: TimeSpan): Date => {
        const t = new Date(1970, 0, 1);
        const totalSeconds = input.hours*60*60+input.minutes*60+input.seconds;
        t.setSeconds(totalSeconds);
        return t;
    }

    static fromDate = (timespan: Date): TimeSpan => this.fromSeconds(timespan.getMilliseconds() / 1000);

    static toString = (timespan: TimeSpan) => {
        let values = [
            timespan.hours.toString().padStart(2, '0'),
            timespan.minutes.toString().padStart(2, '0'),
            timespan.seconds.toString().padStart(2, '0'),
        ]

        let result = values.join(":");
        if(timespan.days != 0) {
            result = `${timespan.days}.${result}`;
        }

        return result;
    }

    static fromSeconds = (totalSeconds: number): TimeSpan => {
        const days = Math.floor(totalSeconds / 86400);
        const hours = Math.floor((totalSeconds % 86400) / 3600);
        const minutes = Math.floor((totalSeconds % 3600) / 60);
        const seconds = totalSeconds % 60;
        return {
            days,
            hours,
            minutes,
            seconds
        };
    }

    static toSeconds = (timespan: TimeSpan): number => {
        let result = timespan.seconds;
        result += timespan.minutes * 60;
        result += timespan.hours * 60 * 60;
        result += timespan.days * 60 * 60 * 24;
        return result;
    }
}

function parseTimeSpan(timespan: string): number {
    // Returns total milliseconds
    const regex = /^(-)?(?:(\d+)\.)?(\d{1,2}):(\d{2}):(\d{2})(?:\.(\d+))?$/;
    const match = timespan.match(regex);

    if (!match) {
        throw new Error(`Invalid TimeSpan format: ${timespan}`);
    }

    const [
    , sign, days, hours, minutes, seconds, fraction
    ] = match;

    const signFactor = sign ? -1 : 1;

    const totalMilliseconds =
    ((parseInt(days || "0") * 24 * 60 * 60) +
        (parseInt(hours) * 60 * 60) +
        (parseInt(minutes) * 60) +
        parseInt(seconds)) * 1000 +
    (fraction ? parseFloat("0." + fraction) * 1000 : 0);

    return totalMilliseconds * signFactor;
}