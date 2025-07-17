import { keepPreviousData, Query, QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { useEffect } from "react";
import { useWebEvents } from "../hooks/signalR/useWebEvents";
import { UserEventListener } from "../hooks/signalR/UserEventListener";
import { EntityType, getKey } from "../hooks/queries/QueryKeys";
import { Entity, getEntityType } from "../hooks/EntitiesName";
import { useAuth } from "./AuthContext";
import { MerchantEventListener } from "../hooks/signalR/MerchantEventListener";

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
        predicate: (q) =>  queryMatches(q, type, id),
    });
}

interface QueryContextProviderProps {
    readonly children: React.ReactNode;
}
export const QueryContextProvider = (props: QueryContextProviderProps) => {
    const webEvents = useWebEvents();
    const auth = useAuth();

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
    
    useEffect(() => {
        const listener: UserEventListener = {
            onMerchantAssociatedEvent: (evt) => invalidateQuery(queryClient, getEntityType(Entity.Merchants), evt.merchantId),
        }
        webEvents.client.addUserListener(listener);
        return () => webEvents.client.removeUserListener(listener);
    }, [webEvents.client])

    useEffect(() => {
        if(auth.subMerchantId == undefined) {
            return;
        }

        const listener: MerchantEventListener = {
            onChannelEvent: (evt) => invalidateQuery(queryClient, getEntityType(Entity.Channels), evt.id),
            onChannelProfileEvent: (evt) => invalidateQuery(queryClient, getEntityType(Entity.ChannelProfiles), evt.id),
            onItemCategoryEvent: (evt) => invalidateQuery(queryClient, getEntityType(Entity.MenuCategories), evt.id),
            onLocalEvent: (evt) => invalidateQuery(queryClient, getEntityType(Entity.Locals), evt.id),
            onMenuItemEvent: (evt) => invalidateQuery(queryClient, getEntityType(Entity.MenuItems), evt.id),
            onEmployeeEvent: (evt) => invalidateQuery(queryClient, getEntityType(Entity.Employees), evt.id),
            onItemsModifierGroupEvent: (evt) => invalidateQuery(queryClient, getEntityType(Entity.ModifierGroups), evt.id),
            onCustomChargeMethodEvent: (evt) => invalidateQuery(queryClient, getEntityType(Entity.CustomChargeMethods), evt.id),
            onPrinterWorkerEvent: (evt) => invalidateQuery(queryClient, getEntityType(Entity.PrinterWorkers), evt.id),
            onPrinterEvent: (evt) => invalidateQuery(queryClient, getEntityType(Entity.Printers), evt.id),
            onPrinterMessageOperation: (evt) => invalidateQuery(queryClient, getEntityType(Entity.PrinterMessages), `${evt.printerId}-${evt.messageId}`),
            onAcquirerConfigurationOperation: (evt) => invalidateQuery(queryClient, getEntityType(Entity.AcquirerConfigurations), evt.id),
        }
        webEvents.client.addMerchantListener(listener);
        return () => webEvents.client.removeMerchantListener(listener);
    }, [webEvents.client, auth.subMerchantId])

    return (
        <QueryClientProvider client={queryClient}>
            {props.children}
        </QueryClientProvider>
    )
}