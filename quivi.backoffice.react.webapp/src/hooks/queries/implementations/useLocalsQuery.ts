import { Entity, getEntityType } from "../../EntitiesName";
import { useQueryable } from "../useQueryable";
import { useAuthenticatedUser } from "../../../context/AuthContext";
import { GetLocalsRequest } from "../../api/Dtos/locals/GetLocalsRequest";
import { useLocalsApi } from "../../api/useLocalsApi";
import { Local } from "../../api/Dtos/locals/Local";
import { useMemo } from "react";

export const useLocalsQuery = (request: GetLocalsRequest | undefined) => {
    const user = useAuthenticatedUser();
    const api = useLocalsApi();

    const queryResult = useQueryable({
        queryName: "useLocalsQuery",
        entityType: getEntityType(Entity.Locals),
        request: user.subMerchantId == undefined || request == undefined ? undefined : {
            ...request,
            subMerchantId: user.subMerchantId,
        },
        getIdsFilter: r => r.ids,
        getId: (e: Local) => e.id,
        query: api.get,

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
        page: queryResult.response?.page ?? request?.page ?? 0,
        totalPages: queryResult.response?.totalPages ?? 0,
        totalItems: queryResult.response?.totalItems ?? 0,
    }), [queryResult, request?.page ?? 0])

    return result;
}