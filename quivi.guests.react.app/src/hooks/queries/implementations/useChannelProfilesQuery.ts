import type { ChannelProfile } from "../../api/Dtos/channelProfiles/ChannelProfile";
import { useChannelProfilesApi } from "../../api/useChannelProfilesApi";
import { Entity, getEntityType } from "../../EntitiesName";
import type { QueryResult } from "../QueryResult";
import { useQueryable } from "../useQueryable";

export const useChannelProfilesQuery = (channelProfileId: string | undefined): QueryResult<ChannelProfile[]> => {
    const api = useChannelProfilesApi();

    const query = useQueryable({
        queryName: "useChannelProfilesQuery",
        entityType: getEntityType(Entity.Channels),
        getId: (item: ChannelProfile) => item.id,
        request: channelProfileId == undefined ? undefined : {
            channelProfileId: channelProfileId,
        },
        query: async request => {
            const response = await api.get(request.channelProfileId);
            return {
                data: [response.data],
            }
        },
        getIdsFilter: (r) => [r.channelProfileId],

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