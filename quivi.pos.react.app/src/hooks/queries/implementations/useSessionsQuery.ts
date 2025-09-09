import { useMemo } from "react";
import { GetSessionsRequest } from "../../api/Dtos/sessions/GetSessionsRequest";
import { Session } from "../../api/Dtos/sessions/Session";
import { useSessionsApi } from "../../api/useSessionsApi";
import { Entity, getEntityType } from "../../EntitiesName";
import { QueryResult } from "../QueryResult";
import { useQueryable } from "../useQueryable";

export const useSessionsQuery = (request: GetSessionsRequest | undefined) : QueryResult<Session[]> => {
    const api = useSessionsApi();
    
    const innerQueryResult = useQueryable({
        queryName: "useInternalSessionsQuery",
        entityType: getEntityType(Entity.Sessions),
        request: {
            page: 0,
            includeDeleted: true,
        } as GetSessionsRequest,
        getId: (e: Session) => e.id,
        query: api.get,
        refreshOnAnyUpdate: true,
    })

    const queryResult = useQueryable({
        queryName: "useSessionsQuery",
        entityType: `${getEntityType(Entity.Sessions)}-processed`,
        request: request == undefined ? undefined : {
            entities: innerQueryResult.data,
            isFirstLoading: innerQueryResult.isFirstLoading,
            isLoading: innerQueryResult.isLoading,

            request: request,
        },
        getId: (e: Session) => e.id,
        query: async r => {
            const request = r.request;

                const allData = r.entities.filter((d) => {
                if(request.channelIds != undefined && request.channelIds.includes(d.channelId) == false) {
                    return false;
                }

                if(request.ids != undefined && request.ids.includes(d.id) == false) {
                    return false;
                }

                if(request.includeDeleted != true && d.isDeleted) {
                    return false;
                }

                if(request.isOpen != undefined) {
                    if(request.isOpen == true && d.closedDate != undefined) {
                        return false;
                    }

                    if(request.isOpen == false && d.closedDate == undefined) {
                        return false;
                    }
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

    const result = useMemo(() => ({
        isLoading: queryResult.isLoading,
        isFirstLoading: queryResult.response?.isFirstLoading ?? queryResult.isFirstLoading,
        data: queryResult.data,
    }), [queryResult])
    
    return result;
}