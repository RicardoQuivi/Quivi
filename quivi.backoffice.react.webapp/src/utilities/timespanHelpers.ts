import { TimeSpan } from "../hooks/api/Dtos/TimeSpan";

export const fromStringToTimespan = (time: string): TimeSpan => {
    const aux = time.split(":");
    if(aux.length == 0) {
        return {
            hours: 0,
            minutes: 0,
            seconds: 0,
        };
    }
    if(aux.length == 1) {
        return {
            hours: 0,
            minutes: 0,
            seconds: +aux[0],
        }
    }
    if(aux.length == 2) {
        return {
            hours: 0,
            minutes: +aux[0],
            seconds: +aux[1],
        }
    }
    if(aux.length == 3) {
        return {
            hours: +aux[0],
            minutes: +aux[1],
            seconds: +aux[2],
        }
    }

    return {
        hours: (+aux[0]*24) + (+aux[1]),
        minutes: +aux[2],
        seconds: +aux[3],
    }
}

export const fromTimespanToDate = (input: TimeSpan): Date => {
    const t = new Date(1970, 0, 1);
    const totalSeconds = input.hours*60*60+input.minutes*60+input.seconds;
    t.setSeconds(totalSeconds);
    return t;
}

export const fromDateToTimespan = (timespan: Date): TimeSpan => ({
    hours: timespan.getHours(),
    minutes: timespan.getMinutes(),
    seconds: timespan.getSeconds(),
})

export const fromTimespanToString = (timespan: TimeSpan) => {
    const values = [
        timespan.hours.toString().padStart(2, '0'),
        timespan.minutes.toString().padStart(2, '0'),
        timespan.seconds.toString().padStart(2, '0'),
    ]

    return values.join(":")
}