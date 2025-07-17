import { useMemo } from "react";
import { Entity, getEntityType } from "../../EntitiesName";
import { useQueryable } from "../useQueryable";
import type { QueryResult } from "../QueryResult";
import type { Review } from "../../api/Dtos/reviews/Review";
import { useReviewsApi } from "../../api/useReviewsApi";

export const useReviewsQuery = (id: string | undefined): QueryResult<Review | undefined> => {
    const api = useReviewsApi();

    const query = useQueryable({
        queryName: "useReviewsQuery",
        entityType: getEntityType(Entity.Invoices),
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
    
    const result = useMemo(() => ({
        isFirstLoading: query.isFirstLoading,
        isLoading: query.isLoading,
        data: query.data.length == 0 ? undefined : query.data[0],
    }), [query])
    
    return result;
}