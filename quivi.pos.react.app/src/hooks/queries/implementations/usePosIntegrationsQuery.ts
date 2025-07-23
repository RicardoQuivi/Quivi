import { GetPosIntegrationsRequest } from "../../api/Dtos/posintegrations/GetPosIntegrationsRequest";
import { PosIntegration } from "../../api/Dtos/posintegrations/PosIntegration";
import { usePosIntegrationsApi } from "../../api/usePosIntegrationsApi";
import { Entity, getEntityType } from "../../EntitiesName";
import { QueryResult } from "../QueryResult";
import { useQueryable } from "../useQueryable";

export const usePosIntegrationsQuery = (request: GetPosIntegrationsRequest | undefined) : QueryResult<PosIntegration[]> => {
    const api = usePosIntegrationsApi();

    const queryResult = useQueryable({
        queryName: "usePosIntegrationsQuery",
        entityType: getEntityType(Entity.PosIntegrations),
        request: request == undefined ? undefined : {
            ...request,
        },
        getId: (e: PosIntegration) => e.id,
        query: r => api.get(r),

        refreshOnAnyUpdate: request?.ids == undefined,
        canUseOptimizedResponse: r => r.ids != undefined,
        getResponseFromEntities: (e) => ({
            data: e,
            page: 0,
            totalPages: 1,
            totalItems: 1,
        }),       
    })
    
    return queryResult;
}