import { useMemo } from "react";
import { Entity, getEntityType } from "../../EntitiesName";
import { useQueryable } from "../useQueryable";
import { useAuthenticatedUser } from "../../../context/AuthContext";
import { AvailabilityChannelProfileAssociation } from "../../api/Dtos/availabilityChannelProfileAssociations/AvailabilityChannelProfileAssociation";
import { GetAvailabilityChannelProfileAssociationsRequest } from "../../api/Dtos/availabilityChannelProfileAssociations/GetAvailabilityChannelProfileAssociationsRequest";
import { useAvailabilityChannelProfileAssociationsApi } from "../../api/useAvailabilityChannelProfileAssociationsApi";

export const useAvailabilityChannelProfilesAssociationsQuery = (request: GetAvailabilityChannelProfileAssociationsRequest | undefined) => {
    const user = useAuthenticatedUser();
    const api = useAvailabilityChannelProfileAssociationsApi();

    const queryResult = useQueryable({
        queryName: "useAvailabilityChannelProfilesAssociationsQuery",
        entityType: getEntityType(Entity.AvailabilityChannelProfileAssociations),
        request: user.subMerchantId == undefined || request == undefined ? undefined : {
            ...request,
            subMerchantId: user.subMerchantId,
        },
        getId: (e: AvailabilityChannelProfileAssociation) => `${e.availabilityId}-${e.channelProfileId}`,
        query: api.get,

        refreshOnAnyUpdate: true,
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