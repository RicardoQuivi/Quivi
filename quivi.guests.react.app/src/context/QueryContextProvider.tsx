import { keepPreviousData, Query, QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { getKey, type EntityType } from "../hooks/queries/QueryKeys";
import { createContext, useContext, useEffect, useMemo } from "react";
import { useWebEvents } from "../hooks/signalR/useWebEvents";
import { Entity, getEntityType } from "../hooks/EntitiesName";

const queryClient = new QueryClient({
    defaultOptions: {
        queries: {
            staleTime: Infinity,
            placeholderData: keepPreviousData,
        }
    }
});

const containsId = (query: Query, key: string): boolean => {
    const meta = query.meta;
    if(meta == undefined) {
        return false;
    }

    if(query.state.data == null || query.state.data == undefined) {
        return false;
    }

    const ids = (query.state.data as any)['__idsIndexer'] as Set<string> | undefined;
    if(ids == undefined) {
        return false;
    }
    return ids.has(key);
}

const queryMatches = (q: Query, type: EntityType, id?: string): boolean => {
    if(containsId(q, getKey(type, id))) {
        return true;
    }

    if(q.meta == undefined) {
        return false;
    }

    const onAny = q.meta.refreshOnAnyUpdate as (boolean | undefined);
    if(onAny == undefined || onAny == false) {
        return false;
    }

    return q.queryKey.includes(getKey(type));
}

const invalidateQuery = (query: QueryClient, type: EntityType, id?: string): Promise<void> => {
    console.debug(`Event ${type} of Id ${id} received!`)
    return query.invalidateQueries({ 
        predicate: (q) => queryMatches(q, type, id),
    });
}

interface InvalidatorContextType {
    readonly invalidate: (entitity: Entity, id?: string) => Promise<void>;
}
const InvalidatorContext = createContext<InvalidatorContextType | undefined>(undefined);

interface QueryContextProviderProps {
    readonly children: React.ReactNode;
}
export const QueryContextProvider = (props: QueryContextProviderProps) => {
    const webEvents = useWebEvents();
    // const auth = useAuth();

    useEffect(() => {
        const onFocus = () => queryClient.invalidateQueries();
        window.addEventListener("focus", onFocus)
        return () => window.removeEventListener("focus", onFocus);
    }, [])

    useEffect(() => {
        if(webEvents.connected) {
            return;
        }

        console.debug("No connection to SignalR! Polling data as fallback.")
        const interval = setInterval(() => queryClient.invalidateQueries(), 30000);
        return () => clearInterval(interval);
    }, [webEvents.connected])
    
    // useEffect(() => {
    //     const listener: UserEventListener = {
    //         onMerchantAssociatedEvent: (evt) => invalidateQuery(queryClient, getEntityType(Entity.Merchants), evt.merchantId),
    //     }
    //     webEvents.client.addUserListener(listener);
    //     return () => webEvents.client.removeUserListener(listener);
    // }, [webEvents.client])
    
    const result = useMemo<InvalidatorContextType>(() => ({
        invalidate: (entity: Entity, id?: string) => invalidateQuery(queryClient, getEntityType(entity), id),
    }), []);

    return (
        <InvalidatorContext.Provider value={result}>
            <QueryClientProvider client={queryClient}>
                {props.children}
            </QueryClientProvider>
        </InvalidatorContext.Provider>
    )
}

export const useInvalidator = (): InvalidatorContextType => {
    const context = useContext(InvalidatorContext);
    if(context === undefined) {
        throw Error("useInvalidator can only be used inside QueryContextProvider");
    }
    return context;
}
