import { useEffect, useState } from "react";

export const useNow = (intervalMs: number) => {
    const [now, setNow] = useState<Date>(new Date());
    
    useEffect(() => {
        const interval = setInterval(() => setNow(new Date()), intervalMs);
        return () => clearInterval(interval);
    }, [intervalMs]);

    return now;
}