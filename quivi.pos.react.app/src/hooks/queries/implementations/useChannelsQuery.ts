import { useMemo } from "react";
import { useLoggedEmployee } from "../../../context/pos/LoggedEmployeeContextProvider";
import { Channel } from "../../api/Dtos/channels/Channel";
import { GetChannelsRequest } from "../../api/Dtos/channels/GetChannelsRequest";
import { useChannelsApi } from "../../api/useChannelsApi";
import { Entity, getEntityType } from "../../EntitiesName";
import { PagedQueryResult } from "../QueryResult";
import { useQueryable } from "../useQueryable";

export const useChannelsQuery = (request: GetChannelsRequest | undefined) : PagedQueryResult<Channel> => {      
    const posContext = useLoggedEmployee();
    const api = useChannelsApi(posContext.token);

    const queryResult = useQueryable({
        queryName: "useChannelsQuery",
        entityType: getEntityType(Entity.Channels),
        request: request,
        getId: (e: Channel) => e.id,
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