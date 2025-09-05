import { Dispatch, SetStateAction, useEffect, useMemo, useState } from "react";
import { IBrowserStorage } from "./useBrowserStorage";

export const useStoredState = <T>(
    paramKey: string,
    initialState: T | (() => T),
    storage: IBrowserStorage
): [T, Dispatch<SetStateAction<T>>] => {
    const [value, setValue] = useState<T>(() => {
        const stored = storage.getItem<T>(paramKey);
        if(stored != undefined) {
            return stored;
        }
        return (typeof initialState === 'function' ? (initialState as () => T)() : initialState);
    });

    useEffect(() => setValue(prev => {
        let newValue = storage.getItem<T>(paramKey);
        
        if (newValue === prev)
            return prev;

        if (newValue == undefined || newValue == null)
            return prev;

        return newValue;
    }), [storage]);

    useMemo(() => {
        if (value == undefined || value == null) {
            storage.removeItem(paramKey);
            return;
        }

        if (storage.getItem<T>(paramKey) === value)
            return;

        storage.setItem<T>(paramKey, value);
    }, [value]);

    return [
        value,
        setValue,
    ];
};
