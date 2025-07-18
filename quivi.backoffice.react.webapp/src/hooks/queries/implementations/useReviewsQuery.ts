import { useMemo } from "react";
import { useAuth } from "../../../context/AuthContext";
import { GetReviewsRequest } from "../../api/Dtos/reviews/GetReviewsRequest";
import { Review } from "../../api/Dtos/reviews/Review";
import { useReviewsApi } from "../../api/useReviewsApi";
import { Entity, getEntityType } from "../../EntitiesName";
import { useQueryable } from "../useQueryable";

export const useReviewsQuery = (request: GetReviewsRequest | undefined) => {
    const auth = useAuth();
    const api = useReviewsApi(auth.token);

    const queryResult = useQueryable({
        queryName: "useReviewsQuery",
        entityType: getEntityType(Entity.Reviews),
        request: auth.token == undefined || auth.merchantId == undefined || request == undefined ? undefined : {
            ...request,
            merchant: auth.merchantId,
            subMerchantId: auth.subMerchantId,
        },
        getIdsFilter: r => r.ids,
        getId: (e: Review) => e.id,
        query: request => api.get(request),

        refreshOnAnyUpdate: true,
        canUseOptimizedResponse: r => r.ids != undefined,
        getResponseFromEntities: (e) => ({
            data: e,
            page: 0,
            totalPages: 1,
            totalItems: 1,
        }),       
    })

    const result = useMemo(() => ({
        isFirstLoading: queryResult.isFirstLoading,
        isLoading: queryResult.isLoading,
        data: queryResult.data,
        page: queryResult.response?.page ?? 0,
        totalPages: queryResult.response?.totalPages ?? 0,
        totalItems: queryResult.response?.totalItems ?? 0,
    }), [queryResult])

    return result;
}