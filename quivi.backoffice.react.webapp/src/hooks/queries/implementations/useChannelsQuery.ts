import { useMemo } from "react";
import { Entity, getEntityType } from "../../EntitiesName";
import { useQueryable } from "../useQueryable";
import { useAuthenticatedUser } from "../../../context/AuthContext";
import { useChannelsApi } from "../../api/useChannelsApi";
import { GetChannelsRequest } from "../../api/Dtos/channels/GetChannelsRequest";
import { Channel } from "../../api/Dtos/channels/Channel";

export const useChannelsQuery = (request: GetChannelsRequest | undefined) => {
    const user = useAuthenticatedUser();
    const api = useChannelsApi();

    const queryResult = useQueryable({
        queryName: "useChannelsQuery",
        entityType: getEntityType(Entity.Channels),
        request: user.merchantId == undefined || request == undefined ? undefined : {
            ...request,
            merchantId: user.merchantId,
            subMerchantId: user.subMerchantId,
        },
        getIdsFilter: r => r.ids,
        getId: (e: Channel) => e.id,
        query: api.get,

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
        page: queryResult.response?.page ?? request?.page ?? 0,
        totalPages: queryResult.response?.totalPages ?? 0,
        totalItems: queryResult.response?.totalItems ?? 0,
    }), [queryResult, request?.page ?? 0])

    return result;
}