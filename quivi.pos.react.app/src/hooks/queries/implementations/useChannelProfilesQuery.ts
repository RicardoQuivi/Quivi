import { useMemo } from "react";
import { Entity, getEntityType } from "../../EntitiesName";
import { PagedQueryResult } from "../QueryResult";
import { useQueryable } from "../useQueryable";
import { GetChannelProfilesRequest } from "../../api/Dtos/channelProfiles/GetChannelProfilesRequest";
import { ChannelProfile } from "../../api/Dtos/channelProfiles/ChannelProfile";
import { useChannelProfilesApi } from "../../api/useChannelProfilesApi";
import { ChannelFeatures } from "../../api/Dtos/channelProfiles/ChannelFeatures";

export const useChannelProfilesQuery = (request: GetChannelProfilesRequest | undefined) : PagedQueryResult<ChannelProfile> => {      
    const api = useChannelProfilesApi();

    const innerQueryResult = useQueryable({
        queryName: "useInternalChannelProfilesQuery",
        entityType: getEntityType(Entity.ChannelProfiles),
        request: {
            page: 0,
        } as GetChannelProfilesRequest,
        getId: (e: ChannelProfile) => e.id,
        query: api.get,
        refreshOnAnyUpdate: true,
    })

    const queryResult = useQueryable({
        queryName: "useChannelProfilesQuery",
        entityType: `${getEntityType(Entity.ChannelProfiles)}-processed`,
        request: request == undefined ? undefined : {
            entities: innerQueryResult.data,
            isFirstLoading: innerQueryResult.isFirstLoading,
            isLoading: innerQueryResult.isLoading,

            request: request,
        },
        getId: (e: ChannelProfile) => e.id,
        query: async r => {
            const request = r.request;
            
            const allData = r.entities.filter((d) => {
                if(request.allowsSessionsOnly != undefined && (d.features&ChannelFeatures.AllowsSessions) != ChannelFeatures.AllowsSessions) {
                    return false;
                }

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

    const result = useMemo(() => ({
        isFirstLoading: queryResult.isFirstLoading,
        isLoading: queryResult.isLoading,
        data: queryResult.data,
        page: queryResult.response?.page ?? 0,
        totalPages: queryResult.response?.totalPages ?? 0,
        totalItems: queryResult.response?.totalItems ?? 0,
    }), [queryResult]);

    return result;
}