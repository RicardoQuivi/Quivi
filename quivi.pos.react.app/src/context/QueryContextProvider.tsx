import { keepPreviousData, Query, QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { useEffect } from "react";
import { useWebEvents } from "../hooks/signalR/useWebEvents";
import { EntityType, getKey } from "../hooks/queries/QueryKeys";
import { useAuth } from "./AuthContextProvider";
import { MerchantEventListener } from "../hooks/signalR/MerchantEventListener";
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
        if(auth.principal == undefined) {
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
            onSessionUpdatedEvent: (evt) => invalidateQuery(queryClient, getEntityType(Entity.Sessions), evt.id),
            onBackgroundJobChangedEvent: (evt) => invalidateQuery(queryClient, getEntityType(Entity.BackgroundJobs), evt.id),
            onPosChargeEvent: (evt) => invalidateQuery(queryClient, getEntityType(Entity.Transactions), evt.id),
            onPosChargeSyncAttemptEvent: (evt) => invalidateQuery(queryClient, getEntityType(Entity.Transactions), evt.posChargeId),
            onOrderOperationEvent: (evt) => invalidateQuery(queryClient, getEntityType(Entity.Orders), evt.id),
            onPreparationGroupOperationEvent: (evt) => invalidateQuery(queryClient, getEntityType(Entity.PreparationGroups), evt.id),
        }
        webEvents.client.addMerchantListener(listener);
        return () => webEvents.client.removeMerchantListener(listener);
    }, [webEvents.client, auth.principal])

    return (
        <QueryClientProvider client={queryClient}>
            {props.children}
        </QueryClientProvider>
    )
}