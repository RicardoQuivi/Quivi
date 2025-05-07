import { useMemo } from "react";
import { useLoggedEmployee } from "../../../context/pos/LoggedEmployeeContextProvider";
import { Entity, getEntityType } from "../../EntitiesName";
import { PagedQueryResult } from "../QueryResult";
import { useQueryable } from "../useQueryable";
import { GetChannelProfilesRequest } from "../../api/Dtos/channelProfiles/GetChannelProfilesRequest";
import { ChannelProfile } from "../../api/Dtos/channelProfiles/ChannelProfile";
import { useChannelProfilesApi } from "../../api/useChannelProfilesApi";
import { ChannelFeatures } from "../../api/Dtos/channelProfiles/ChannelFeatures";

export const useChannelProfilesQuery = (request: GetChannelProfilesRequest | undefined) : PagedQueryResult<ChannelProfile> => {      
    const innerQueryResult = useInternalChannelProfilesQuery();

    const result = useMemo<PagedQueryResult<ChannelProfile>>(() => {
        if(request == undefined) {
            return {
                data: [],
                page: 0,
                totalItems: 0,
                totalPages: 0,
                isFirstLoading: true,
                isLoading: true,
            }
        }
        
        let allData = innerQueryResult.data.filter((d) => {
            if(request.allowsSessionsOnly != undefined && (d.features&ChannelFeatures.AllowsSessions) != ChannelFeatures.AllowsSessions) {
                return false;
            }

            if(request.ids != undefined && request.ids.includes(d.id) == false) {
                return false;
            }

            return true;
        });

        let totalPages = 1;
        if(request.pageSize != undefined) {
            const start = request.page * request.pageSize;
            totalPages = Math.ceil(allData.length / request.pageSize);
            allData = allData.splice(start, start + request.pageSize)
        }   
        return {
            data: allData,
            isFirstLoading: innerQueryResult.isFirstLoading,
            isLoading: innerQueryResult.isLoading,
            totalItems: allData.length,
            totalPages: totalPages,
            page: request.page,
        }
    }, [JSON.stringify(request), innerQueryResult])

    return result;
}

export const useInternalChannelProfilesQuery = () => {      
    const posContext = useLoggedEmployee();
    const api = useChannelProfilesApi(posContext.token);

    const queryResult = useQueryable({
        queryName: "useChannelProfilesQuery",
        entityType: getEntityType(Entity.ChannelProfiles),
        request: {
            page: 0,
        } as GetChannelProfilesRequest,
        getId: (e: ChannelProfile) => e.id,
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