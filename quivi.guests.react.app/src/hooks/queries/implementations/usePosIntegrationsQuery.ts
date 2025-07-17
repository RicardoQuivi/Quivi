import type { PosIntegration } from "../../api/Dtos/posIntegrations/PosIntegration";
import { usePosIntegrationsApi } from "../../api/usePosIntegrationsApi";
import { Entity, getEntityType } from "../../EntitiesName";
import type { QueryResult } from "../QueryResult";
import { useQueryable } from "../useQueryable";

export const usePosIntegrationsQuery = (posIntegrationId: string | undefined): QueryResult<PosIntegration[]> => {
    const api = usePosIntegrationsApi();

    const query = useQueryable({
        queryName: "usePosIntegrationsQuery",
        entityType: getEntityType(Entity.PosIntegrations),
        getId: (item: PosIntegration) => item.id,
        request: posIntegrationId == undefined ? undefined : {
            posIntegrationId: posIntegrationId,
        },
        query: async request => {
            const response = await api.get(request.posIntegrationId);
            return {
                data: [response.data],
            }
        },
        getIdsFilter: (r) => [r.posIntegrationId],

        refreshOnAnyUpdate: false,
        canUseOptimizedResponse: _ => true,
        getResponseFromEntities: (e) => ({
            data: e,
            page: 0,
            totalPages: 1,
            totalItems: 1,
        }),   
    })
    
    return query;
}