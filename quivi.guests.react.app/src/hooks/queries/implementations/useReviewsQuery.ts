import { useEffect, useMemo } from "react";
import { Entity, getEntityType } from "../../EntitiesName";
import { useQueryable } from "../useQueryable";
import type { QueryResult } from "../QueryResult";
import type { Review } from "../../api/Dtos/reviews/Review";
import { useReviewsApi } from "../../api/useReviewsApi";
import { useWebEvents } from "../../signalR/useWebEvents";
import type { TransactionListener } from "../../signalR/TransactionListener";
import { useInvalidator } from "../../../context/QueryContextProvider";

export const useReviewsQuery = (id: string | undefined): QueryResult<Review | undefined> => {
    const api = useReviewsApi();
    const invalidator = useInvalidator();
    const webEvents = useWebEvents();

    const query = useQueryable({
        queryName: "useReviewsQuery",
        entityType: getEntityType(Entity.Reviews),
        getId: (item: Review) => item.id,
        request: id == undefined ? undefined : {
            transactionId: id,
        },
        query: async request => {
            const response = await api.get(request);
            return {
                data: response.data == undefined ? [] : [response.data],
            }
        },

        refreshOnAnyUpdate: true,
    })
    
    useEffect(() => {
        if(id == undefined) {
            return;
        }

        const listener: TransactionListener = {
            transactionId: id,
            onReviewOperation: evt => invalidator.invalidate(Entity.Reviews, evt.id),
        }
        webEvents.client.addTransactionListener(listener);
        return () => webEvents.client.removeTransactionListener(listener);
    }, [id])

    const result = useMemo(() => ({
        isFirstLoading: query.isFirstLoading,
        isLoading: query.isLoading,
        data: query.data.length == 0 ? undefined : query.data[0],
    }), [query])
    
    return result;
}