import { GetPosIntegrationsRequest } from "../../api/Dtos/posintegrations/GetPosIntegrationsRequest";
import { PosIntegration } from "../../api/Dtos/posintegrations/PosIntegration";
import { usePosIntegrationsApi } from "../../api/usePosIntegrationsApi";
import { Entity, getEntityType } from "../../EntitiesName";
import { QueryResult } from "../QueryResult";
import { useQueryable } from "../useQueryable";

export const usePosIntegrationsQuery = (request: GetPosIntegrationsRequest | undefined) : QueryResult<PosIntegration[]> => {
    const innerQueryResult = useInternalPosIntegrationsQuery();

    const queryResult = useQueryable({
        queryName: "usePosIntegrationsQuery",
        entityType: `${getEntityType(Entity.PosIntegrations)}-processed`,
        request: request == undefined ? undefined : {
            entities: innerQueryResult.data,
            isFirstLoading: innerQueryResult.isFirstLoading,
            isLoading: innerQueryResult.isLoading,

            request: request,
        },
        getId: (e: PosIntegration) => e.id,
        query: async r => {
            const request = r.request;
            
            const allData = r.entities.filter((d) => {
                if(request.ids != undefined && request.ids.includes(d.id) == false) {
                    return false;
                }

                return true;
            });

            let totalPages = 1;
            let resultData = allData;
            if(request.pageSize != undefined) {
                const start = request.page * request.pageSize;
                totalPages = Math.ceil(allData.length / request.pageSize);
                resultData = allData.splice(start, start + request.pageSize)
            }

            return {
                data: resultData,
                isFirstLoading: r.isFirstLoading,
                isLoading: r.isLoading,
                totalItems: allData.length,
                totalPages: totalPages,
                page: request.page,
            }
        },
        refreshOnAnyUpdate: false,
    })
    return queryResult;
}

const useInternalPosIntegrationsQuery = (): QueryResult<PosIntegration[]> => {
    const api = usePosIntegrationsApi();
    
    const queryResult = useQueryable({
        queryName: "useInternalPosIntegrationsQuery",
        entityType: getEntityType(Entity.PosIntegrations),
        request: {
            page: 0,
        } as GetPosIntegrationsRequest,
        getId: (e: PosIntegration) => e.id,
        query: api.get,
        refreshOnAnyUpdate: true,
    })

    return queryResult;
}