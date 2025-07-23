import { Entity, getEntityType } from "../../EntitiesName";
import { useQueryable } from "../useQueryable";
import { useAuthenticatedUser } from "../../../context/AuthContext";
import { useMemo } from "react";
import { useCustomChargeMethodsApi } from "../../api/useCustomChargeMethodsApi";
import { GetCustomChargeMethodsRequest } from "../../api/Dtos/customchargemethods/GetCustomChargeMethodsRequest";
import { CustomChargeMethod } from "../../api/Dtos/customchargemethods/CustomChargeMethod";

export const useCustomChargeMethodsQuery = (request: GetCustomChargeMethodsRequest | undefined) => {
    const user = useAuthenticatedUser();
    const api = useCustomChargeMethodsApi();

    const queryResult = useQueryable({
        queryName: "useCustomChargeMethodsQuery",
        entityType: getEntityType(Entity.CustomChargeMethods),
        request: user.subMerchantId == undefined || request == undefined ? undefined : {
            ...request,
            subMerchantId: user.subMerchantId,
        },
        getIdsFilter: r => r.ids,
        getId: (e: CustomChargeMethod) => e.id,
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