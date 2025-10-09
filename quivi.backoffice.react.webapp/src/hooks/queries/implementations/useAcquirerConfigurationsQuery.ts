import { Entity, getEntityType } from "../../EntitiesName";
import { useQueryable } from "../useQueryable";
import { useAuthenticatedUser } from "../../../context/AuthContext";
import { useMemo } from "react";
import { GetAcquirerConfigurationsRequest } from "../../api/Dtos/acquirerconfigurations/GetAcquirerConfigurationsRequest";
import { useAcquirerConfigurationsApi } from "../../api/useAcquirerConfigurationsApi";
import { AcquirerConfiguration } from "../../api/Dtos/acquirerconfigurations/AcquirerConfiguration";

export const useAcquirerConfigurationsQuery = (request: GetAcquirerConfigurationsRequest | undefined) => {
    const user = useAuthenticatedUser();
    const api = useAcquirerConfigurationsApi();

    const queryResult = useQueryable({
        queryName: "useAcquirerConfigurationsQuery",
        entityType: getEntityType(Entity.AcquirerConfigurations),
        request: user.subMerchantId == undefined || request == undefined ? undefined : {
            ...request,
            subMerchantId: user.subMerchantId,
        },
        getIdsFilter: r => r.ids,
        getId: (e: AcquirerConfiguration) => e.id,
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