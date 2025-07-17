import { Entity, getEntityType } from "../../EntitiesName";
import { useQueryable } from "../useQueryable";
import { useAuth } from "../../../context/AuthContext";
import { useMemo } from "react";
import { GetAcquirerConfigurationsRequest } from "../../api/Dtos/acquirerconfigurations/GetAcquirerConfigurationsRequest";
import { useAcquirerConfigurationsApi } from "../../api/useAcquirerConfigurationsApi";
import { AcquirerConfiguration } from "../../api/Dtos/acquirerconfigurations/AcquirerConfiguration";

export const useAcquirerConfigurationsQuery = (request: GetAcquirerConfigurationsRequest | undefined) => {
    const auth = useAuth();
    const api = useAcquirerConfigurationsApi(auth.token);

    const queryResult = useQueryable({
        queryName: "useAcquirerConfigurationsQuery",
        entityType: getEntityType(Entity.AcquirerConfigurations),
        request: auth.token == undefined || auth.subMerchantId == undefined || request == undefined ? undefined : {
            ...request,
            subMerchantId: auth.subMerchantId,
        },
        getIdsFilter: r => r.ids,
        getId: (e: AcquirerConfiguration) => e.id,
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