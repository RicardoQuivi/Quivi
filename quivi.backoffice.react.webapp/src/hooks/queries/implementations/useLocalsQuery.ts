import { Entity, getEntityType } from "../../EntitiesName";
import { useQueryable } from "../useQueryable";
import { useAuth } from "../../../context/AuthContext";
import { GetLocalsRequest } from "../../api/Dtos/locals/GetLocalsRequest";
import { useLocalsApi } from "../../api/useLocalsApi";
import { Local } from "../../api/Dtos/locals/Local";
import { useMemo } from "react";

export const useLocalsQuery = (request: GetLocalsRequest | undefined) => {
    const auth = useAuth();
    const api = useLocalsApi(auth.token);

    const queryResult = useQueryable({
        queryName: "useLocalsQuery",
        entityType: getEntityType(Entity.Locals),
        request: auth.token == undefined || auth.subMerchantId == undefined || request == undefined ? undefined : {
            ...request,
            subMerchantId: auth.subMerchantId,
        },
        getIdsFilter: r => r.ids,
        getId: (e: Local) => e.id,
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