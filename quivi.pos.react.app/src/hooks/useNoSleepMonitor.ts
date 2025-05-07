import { useEffect, useState } from "react";
import NoSleep from 'nosleep.js';

interface Result {
    readonly isCurrentlyActive: boolean
}
const useNoSleepMonitor = (enabled?: boolean): Result => {
    const [state, setState] = useState({
        isCurrentlyActive: false,
        noSleepObj: new NoSleep(),
    });

    const enableNoSleep = async () => {
        if (!!state.noSleepObj.isEnabled)
            return;

        await state.noSleepObj.enable();
    }

    const disableNoSleep = async () => {
        if (!state.noSleepObj.isEnabled)
            return;
        
        await state.noSleepObj.disable();
    }

    const toggleNoSleep = async (enable: boolean) => {
        if (enable) {
            await enableNoSleep();
        } else {
            await disableNoSleep();
        }

        setState(s => ({ ...s, isCurrentlyActive: s.noSleepObj.isEnabled}))
    }

    useEffect(() => {
        return () => { disableNoSleep(); }
    }, []);

    useEffect(() => {
        if(enabled == undefined) {
            return;
        }

        toggleNoSleep(enabled);
    }, [enabled, state.noSleepObj])

    return state;
}
export default useNoSleepMonitor;