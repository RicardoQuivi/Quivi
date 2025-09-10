import { useEffect } from "react";
import { useNow } from "../../hooks/useNow";

interface Props {
    readonly targetDate: Date;
    readonly onComplete: () => any;
    readonly footer?: React.ReactNode;
}
export const Countdown = (props: Props) => {
    const now = useNow(1000);

    const diff = getTimeDifference(props.targetDate, now);

    useEffect(() => {
        const diff = props.targetDate.getTime() - new Date().getTime();
        const timeout = setTimeout(props.onComplete, diff);
        return () => clearTimeout(timeout);
    }, [props.targetDate])

    return <div className="mb-10">
        <div className="mb-2 flex flex-wrap justify-center gap-1 text-title-md font-bold text-brand-500 dark:text-brand-400 xl:text-title-lg">
            <div className="timer-box">
                <span>{diff.minutes.toString().padStart(2, '0')}</span>
            </div>
            :

            <div className="timer-box">
                <span>{diff.seconds.toString().padStart(2, '0')}</span>
            </div>
        </div>

        {
            props.footer != undefined &&
            <div className="text-center text-base text-gray-500 dark:text-gray-400">
                {props.footer}
            </div>
        }
    </div>
}

const getTimeDifference = (date1: Date, date2: Date) => {
    // Get the absolute difference in milliseconds
    const diffMs = Math.abs(date2.getTime() - date1.getTime());
    
    // Convert to total seconds
    const totalSeconds = Math.floor(diffMs / 1000);
    
    // Calculate minutes and seconds
    const minutes = Math.floor(totalSeconds / 60);
    const seconds = totalSeconds % 60;
    
    return { minutes, seconds };
}
