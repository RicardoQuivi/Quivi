import type { Channel } from "../../api/Dtos/channels/Channel";
import { useChannelsApi } from "../../api/useChannelsApi";
import { Entity, getEntityType } from "../../EntitiesName";
import type { QueryResult } from "../QueryResult";
import { useQueryable } from "../useQueryable";

export const useChannelsQuery = (channelId: string | undefined): QueryResult<Channel[]> => {
    const api = useChannelsApi();

    const query = useQueryable({
        queryName: "useChannelsQuery",
        entityType: getEntityType(Entity.Channels),
        getId: (item: Channel) => item.id,
        request: channelId == undefined ? undefined : {
            channelId: channelId,
        },
        query: async request => {
            const response = await api.get(request.channelId);
            return {
                data: [response.data],
            }
        },
        getIdsFilter: (r) => [r.channelId],

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