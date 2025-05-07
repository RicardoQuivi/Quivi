import { Dispatch, SetStateAction, useEffect, useState } from "react";
import { IBrowserStorage } from "./useBrowserStorage";

export const useStoredState = <T>(
    paramKey: string,
    initialState: T | (() => T),
    storage: IBrowserStorage
): [T, Dispatch<SetStateAction<T>>] => {
    const [value, setValue] = useState<T>(storage.getItem<T>(paramKey) ?? initialState);

    useEffect(() => {
        const timer = setTimeout(() => {
            let newValue = storage.getItem<T>(paramKey);
            
            if (newValue === value)
                return;

            if (newValue == undefined || newValue == null)
                return;

            setValue(newValue);
        }, 50);

        return () => clearTimeout(timer);
    }, [storage]);

    useEffect(() => {
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
