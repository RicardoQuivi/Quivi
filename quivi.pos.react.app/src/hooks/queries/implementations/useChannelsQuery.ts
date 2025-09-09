import { useMemo } from "react";
import { Channel } from "../../api/Dtos/channels/Channel";
import { GetChannelsRequest } from "../../api/Dtos/channels/GetChannelsRequest";
import { useChannelsApi } from "../../api/useChannelsApi";
import { Entity, getEntityType } from "../../EntitiesName";
import { PagedQueryResult } from "../QueryResult";
import { useQueryable } from "../useQueryable";
import { useChannelProfilesQuery } from "./useChannelProfilesQuery";
import { ChannelFeatures } from "../../api/Dtos/channelProfiles/ChannelFeatures";
import { useSessionsQuery } from "./useSessionsQuery";
import { CollectionFunctions } from "../../../helpers/collectionsHelper";

export const useChannelsQuery = (request: GetChannelsRequest | undefined) : PagedQueryResult<Channel> => {
    const api = useChannelsApi();

    const innerQueryResult = useQueryable({
        queryName: "useInternalChannelsQuery",
        entityType: getEntityType(Entity.Channels),
        request: {
            includeDeleted : true,
            page: 0,
        } as GetChannelsRequest,
        getId: (e: Channel) => e.id,
        query: api.get,
        refreshOnAnyUpdate: true,
    })

    const channelProfilesQuery = useChannelProfilesQuery({
        page: 0,
    });
    const channelProfilesMap = useMemo(() => {
        if(channelProfilesQuery.isFirstLoading) {
            return undefined;
        }

        return CollectionFunctions.toMap(channelProfilesQuery.data, p => p.id);
    }, [channelProfilesQuery.isFirstLoading, channelProfilesQuery.data]);

    const sessionsQuery = useSessionsQuery({
        page: 0,
    });
    const sessionsMap = useMemo(() => {
        if(sessionsQuery.isFirstLoading) {
            return undefined;
        }
        return CollectionFunctions.toMap(sessionsQuery.data, p => p.channelId);
    }, [sessionsQuery.isFirstLoading, sessionsQuery.data]);

    
    const queryResult = useQueryable({
        queryName: "useChannelsQuery",
        entityType: `${getEntityType(Entity.Channels)}-processed`,
        request: request == undefined || innerQueryResult.isFirstLoading || channelProfilesMap == undefined || sessionsMap == undefined ? undefined : {
            entities: innerQueryResult.data,
            isFirstLoading: innerQueryResult.isFirstLoading,
            isLoading: innerQueryResult.isLoading,

            request: request,

            profilesMap: channelProfilesMap,
            sessionsMap: sessionsMap,
        },
        getId: (e: Channel) => e.id,
        query: async r => {
            const request = r.request;

            const allData = r.entities.filter((d) => {
                if(request.ids != undefined && request.ids.includes(d.id) == false) {
                    return false;
                }

                if(request.search != undefined && d.name.includes(request.search) == false) {
                    return false;
                }
                
                if(request.includeDeleted != true && d.isDeleted) {
                    return false;
                }

                if(request.allowsPrePaidOrderingOnly != undefined) {
                    const profile = r.profilesMap.get(d.channelProfileId)!;
                    if((profile.features & ChannelFeatures.AllowsOrderAndPay) != ChannelFeatures.AllowsOrderAndPay) {
                        return false;
                    }
                }

                if(request.allowsPostPaidOrderingOnly != undefined) {
                    const profile = r.profilesMap.get(d.channelProfileId)!;
                    if((profile.features & ChannelFeatures.AllowsPostPaymentOrdering) != ChannelFeatures.AllowsPostPaymentOrdering) {
                        return false;
                    }
                }

                if(request.channelProfileId != undefined && request.channelProfileId != d.channelProfileId) {
                    return false;
                }
                
                const session = r.sessionsMap.get(d.id);
                if(request.hasOpenSession != undefined) {
                    if(request.hasOpenSession == true && session == undefined) {
                        return false;
                    }
                    if(request.hasOpenSession == false && session != undefined) {
                        return false;
                    }
                }

                if(request.sessionIds != undefined) {
                    if(session == undefined) {
                        return false;
                    }

                    if(request.sessionIds.includes(session.id) == false) {
                        return false;
                    }
                }

                return true;
            });

            let totalPages = 1;
            let resultData = allData;
            if(r.request.pageSize != undefined) {
                const start = r.request.page * r.request.pageSize;
                totalPages = Math.ceil(allData.length / r.request.pageSize);
                resultData = allData.splice(start, start + r.request.pageSize)
            }   
            return {
                data: resultData,
                isFirstLoading: r.isFirstLoading,
                isLoading: r.isLoading,
                totalItems: allData.length,
                totalPages: totalPages,
                page: r.request.page,
                request: r.request,
            }
        },
        refreshOnAnyUpdate: false,
    })

    const result = useMemo(() => ({
        isFirstLoading: queryResult.response?.isFirstLoading ?? queryResult.isFirstLoading,
        isLoading: queryResult.isLoading,
        data: queryResult.data,
        page: queryResult.response?.page ?? 0,
        totalPages: queryResult.response?.totalPages ?? 0,
        totalItems: queryResult.response?.totalItems ?? 0,
        request: queryResult.response?.request,
    }), [queryResult]);

    return result;
}