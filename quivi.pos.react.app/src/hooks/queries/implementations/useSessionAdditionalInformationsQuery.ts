import { Entity, getEntityType } from "../../EntitiesName";
import { QueryResult } from "../QueryResult";
import { useQueryable } from "../useQueryable";
import { SessionAdditionalInformation } from "../../api/Dtos/sessionAdditionalInformations/SessionAdditionalInformation";
import { GetSessionAdditionalInformationsRequest } from "../../api/Dtos/sessionAdditionalInformations/GetSessionAdditionalInformationsRequest";
import { useSessionAdditionalInformationsApi } from "../../api/useSessionAdditionalInformationsApi";

export const useSessionAdditionalInformationsQuery = (request: GetSessionAdditionalInformationsRequest | undefined) : QueryResult<SessionAdditionalInformation[]> => {
    const api = useSessionAdditionalInformationsApi();
    
    const queryResult = useQueryable({
        queryName: "useSessionAdditionalInformationsQuery",
        entityType: getEntityType(Entity.SessionAdditionalInformations),
        request: request,
        getId: (e: SessionAdditionalInformation) => `${e.orderId}/${e.id}`,
        query: r => api.get(r),

        refreshOnAnyUpdate: true,    
    })

    return queryResult;
}