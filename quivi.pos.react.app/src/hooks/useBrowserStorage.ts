import { useEffect, useMemo, useRef } from "react";

export enum BrowserStorageType {
    SessionStorage,
    LocalStorage,
    UrlParam,
}

export interface IBrowserStorage {
    setItem: <T>(key: string, value: T) => void;
    getItem: <T>(key: string, defaultValue?: T) => T | undefined;
    removeItem: (key: string) => void;
}

const useBrowserStorage = (storageType: BrowserStorageType, onChange?: (key: string, storage: IBrowserStorage) => void) => {
    const isUpdating = useRef(false);

    const buildSearchParams = (): IBrowserStorage => {
        const storage = {
            setItem: <T>(k: string, v: T) => {
                if (v == undefined || v == null || isUpdating.current) {
                    return;
                }

                isUpdating.current = true;
                const queryParams = new URLSearchParams(window.location.search);
                queryParams.set(k, typeof v == "string" ? v : JSON.stringify(v));
                const newUrl = `${window.location.pathname}?${queryParams.toString()}${window.location.hash}`;
                window.history.replaceState(null, document.title, newUrl);
                onChange?.(k, storage);
                isUpdating.current = false;
            },
            getItem: <T>(k: string, defaultValue?: T): T | undefined => {
                const queryParams = new URLSearchParams(window.location.search);
                const rawValue = queryParams.get(k);

                if (rawValue == undefined || rawValue == null) {
                    return defaultValue;
                }

                const tryGetDate = (dateStr: string): Date | undefined => {
                    const date = new Date(dateStr);
                    return isNaN(date.getTime()) ? undefined : date;
                };

                try {
                    const ret = JSON.parse(rawValue) as T;
                    if (typeof ret == "string") {
                        const date = tryGetDate(ret);
                        if (date != undefined) {
                            return date as T;
                        }
                    }
                    return ret;
                } catch (e) {
                    if (e instanceof SyntaxError) {
                        return rawValue as T;
                    }
                    throw e;
                }
            },
            removeItem: (k: string) => {
                const queryParams = new URLSearchParams(window.location.search);
                queryParams.delete(k);
                const newUrl = `${window.location.pathname}?${queryParams.toString()}${window.location.hash}`;
                window.history.replaceState(null, document.title, newUrl);
                onChange?.(k, storage);
            },
        };
        return storage;
    };

    const buildStorage = (store: Storage): IBrowserStorage => {
        const storage = {
            setItem: <T>(k: string, v: T) => {
                if (v == undefined || v == null) {
                    return;
                }
                store.setItem(k, JSON.stringify(v));
                onChange?.(k, storage);
            },
            getItem: <T>(k: string, defaultValue?: T) => {
                const rawValue = store.getItem(k);
                if (rawValue == null) {
                    return defaultValue;
                }
                
                try {
                    return JSON.parse(rawValue) as T;
                } catch (e) {
                    if (e instanceof SyntaxError) {
                        return rawValue as T;
                    }
                    return defaultValue;
                }
            },
            removeItem: (k: string) => {
                if (store.getItem(k)) {
                    store.removeItem(k);
                    onChange?.(k, storage);
                }
            },
        };
        return storage;
    };

    const discoverStorage = () => {
        switch (storageType) {
            case BrowserStorageType.UrlParam: return buildSearchParams();
            case BrowserStorageType.LocalStorage: return buildStorage(localStorage);
            case BrowserStorageType.SessionStorage:
            default:
                return buildStorage(sessionStorage);
        }
    };

    const storageObj = useMemo(() => discoverStorage(), [storageType, onChange]);

    useEffect(() => {
        if (!onChange || storageType === BrowserStorageType.UrlParam) {
            return;
        }

        const handleStorageChange = (evt: StorageEvent) => {
            if (!evt.key) {
                return;
            }
            onChange(evt.key, storageObj);
        };

        window.addEventListener('storage', handleStorageChange);
        return () => window.removeEventListener('storage', handleStorageChange);
    }, [storageObj, storageType]);

    return storageObj;
};
export default useBrowserStorage;