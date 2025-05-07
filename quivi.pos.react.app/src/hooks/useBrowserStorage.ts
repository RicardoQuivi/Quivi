import { useEffect, useState } from "react";
import { useSearchParams } from "react-router";

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
    const [searchParams, setSearchParams] = storageType == BrowserStorageType.UrlParam ? useSearchParams() : [];
    
    const buildSearchParams = (searchParamsObj: URLSearchParams | undefined): IBrowserStorage => {
        return {
            setItem: <T>(k: string, v: T) => {
                if (v == undefined || v == null)
                    return;
                setSearchParams!(prev => {
                    prev.set(k, typeof v == "string" ? v : JSON.stringify(v));
                    return prev;
                });
                onChangeInternal(k);
            },
            getItem: <T>(k: string, defaultValue?: T): T | undefined => {
                const rawValue = searchParamsObj!.get(k);
                if(rawValue == undefined || rawValue == null) {
                    return defaultValue;
                }

                const tryGetDate = (dateStr: string): Date | undefined => {
                    const date = new Date(dateStr);
                    if(isNaN(date.getTime())) {
                        return undefined;
                    }
                    return date;
                }

                
                try {
                    const ret = JSON.parse(rawValue) as T;
                    if(typeof ret == "string") {
                        const date = tryGetDate(ret);
                        if(date != undefined) {
                            return date as T;
                        }
                    }
                    return ret
                } catch (e) {
                    if(e instanceof SyntaxError) {
                        return rawValue as T;
                    }
                    throw e;
                }
            },
            removeItem: (k: string) => {
                setSearchParams!(prev => {
                    prev.delete(k);
                    return prev;
                });
                onChangeInternal(k);
            },
        };
    }

    const buildStorage = (store: Storage): IBrowserStorage => {
        const getItem = <T>(k: string, defaultValue?: T) => {
            const rawValue = store.getItem(k);
            return rawValue == undefined || rawValue == null ? defaultValue : JSON.parse(rawValue) as T;
        };

        return {
            setItem: <T>(k: string, v: T) => {
                if (v == undefined || v == null)
                    return;
                const parsedValue = JSON.stringify(v);
                store.setItem(k, parsedValue);
                triggerOnChangeEvent(k);
            },
            getItem: getItem,
            removeItem: (k: string) => {
                if (!!store.getItem(k)) {
                    store.removeItem(k);
                    triggerOnChangeEvent(k);
                }
            },
        };
    }

    const discoverStorage = () => {
        switch (storageType) {
            case BrowserStorageType.UrlParam: return buildSearchParams(searchParams);
            case BrowserStorageType.LocalStorage: return buildStorage(localStorage);
            case BrowserStorageType.SessionStorage:
            default:
                return buildStorage(sessionStorage);
        }
    }

    const [storageObj, setStorageObj] = useState(discoverStorage());

    const onChangeInternal = (key: string) => {
        onChange?.(key, storageObj);
        //setStorageObj(discoverStorage());
    }

    const triggerOnChangeEvent = (key: string) => {
        const event = new StorageEvent('storage', {
            key: key,
        });
        window.dispatchEvent(event);
    }

    useEffect(() => {
        if (searchParams == undefined)
            return;

        setStorageObj(discoverStorage());
    }, [searchParams]);

    useEffect(() => {
        if (!onChange)
            return;

        const handleStorageChange = (evt: StorageEvent) => {
            if (!evt.key)
                return;
        
            onChangeInternal(evt.key);
        };
    
        // Event listener
        window.addEventListener('storage', handleStorageChange);
    
        // Cleanup
        return () => window.removeEventListener('storage', handleStorageChange);
    }, []);

    return storageObj;
}
export default useBrowserStorage;