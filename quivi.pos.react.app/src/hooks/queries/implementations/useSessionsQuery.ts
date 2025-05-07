import { useMemo } from "react";
import { useLoggedEmployee } from "../../../context/pos/LoggedEmployeeContextProvider";
import { GetSessionsRequest } from "../../api/Dtos/sessions/GetSessionsRequest";
import { Session } from "../../api/Dtos/sessions/Session";
import { useSessionsApi } from "../../api/useSessionsApi";
import { Entity, getEntityType } from "../../EntitiesName";
import { QueryResult } from "../QueryResult";
import { useQueryable } from "../useQueryable";

export const useSessionsQuery = (request: GetSessionsRequest | undefined) : QueryResult<Session[]> => {
    const innerQueryResult = useInternalSessionsQuery();

    const result = useMemo<QueryResult<Session[]>>(() => {
        if(request == undefined) {
            return {
                data: [],
                isFirstLoading: true,
                isLoading: true,
            }
        }
        
        let allData = innerQueryResult.data.filter((d) => {
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

        if(request.pageSize != undefined) {
            const start = request.page * request.pageSize;
            allData = allData.splice(start, start + request.pageSize)
        }

        return {
            data: allData,
            isFirstLoading: innerQueryResult.isFirstLoading,
            isLoading: innerQueryResult.isLoading,
        }
    }, [JSON.stringify(request), innerQueryResult])

    return result;
}

const useInternalSessionsQuery = () : QueryResult<Session[]> => {
    const posContext = useLoggedEmployee();
    const api = useSessionsApi(posContext.token);
    
    const queryResult = useQueryable({
        queryName: "useSessionsQuery",
        entityType: getEntityType(Entity.Sessions),
        request: {
            page: 0,
            includeDeleted: true,
        } as GetSessionsRequest,
        getId: (e: Session) => e.id,
        query: r => api.get(r),

        refreshOnAnyUpdate: true,
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