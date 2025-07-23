import { useMemo } from "react";
import { Entity, getEntityType } from "../../EntitiesName";
import { useQueryable } from "../useQueryable";
import { useAuthenticatedUser } from "../../../context/AuthContext";
import { usePosIntegrationsApi } from "../../api/usePosIntegrationsApi";
import { GetPosIntegrationsRequest } from "../../api/Dtos/posIntegrations/GetPosIntegrationsRequest";
import { PosIntegration } from "../../api/Dtos/posIntegrations/PosIntegration";

export const usePosIntegrationsQuery = (request: GetPosIntegrationsRequest | undefined) => {
    const user = useAuthenticatedUser();
    const api = usePosIntegrationsApi();

    const queryResult = useQueryable({
        queryName: "usePosIntegrationsQuery",
        entityType: getEntityType(Entity.PosIntegrations),
        request: user.subMerchantId == undefined || request == undefined ? undefined : {
            ...request,
            subMerchantId: user.subMerchantId,
        },
        getIdsFilter: r => r.ids,
        getId: (e: PosIntegration) => e.id,
        query: request => api.get(request),

        refreshOnAnyUpdate: request?.ids == undefined,
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