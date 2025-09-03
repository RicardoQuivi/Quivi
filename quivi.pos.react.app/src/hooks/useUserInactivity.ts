import { useEffect, useState, useCallback, useRef } from "react";

interface InactivityResult {
    readonly isInactive: boolean;
}
interface Props {
    readonly timeout?: number;
}
export const useUserInactivity = ({ timeout }: Props): InactivityResult => {
    const [isInactive, setIsInactive] = useState(false);
    const timeoutId = useRef<number | null>(null);

    const resetTimer = useCallback(() => {
        if (timeoutId.current) {
            clearTimeout(timeoutId.current);
        }
        setIsInactive(false);
        if(timeout != undefined) {
            timeoutId.current = window.setTimeout(() => setIsInactive(true), timeout);
        }
    }, [timeout]);

    useEffect(() => {
        const events = ['mousemove', 'mousedown', 'keypress', 'touchstart'];

        const handleEvent = () => resetTimer();
        events.forEach(event => window.addEventListener(event, handleEvent));
        resetTimer();
        return () => {
            events.forEach(event => window.removeEventListener(event, handleEvent));
            if (timeoutId.current) {
                clearTimeout(timeoutId.current);
            }
        };
    }, [resetTimer]);
    
    return { isInactive };
}