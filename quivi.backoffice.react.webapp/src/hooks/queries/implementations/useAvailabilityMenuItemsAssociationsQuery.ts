import { useMemo } from "react";
import { Entity, getEntityType } from "../../EntitiesName";
import { useQueryable } from "../useQueryable";
import { useAuthenticatedUser } from "../../../context/AuthContext";
import { AvailabilityMenuItemAssociation } from "../../api/Dtos/availabilityMenuItemAssociations/AvailabilityMenuItemAssociation";
import { GetAvailabilityMenuItemAssociationsRequest } from "../../api/Dtos/availabilityMenuItemAssociations/GetAvailabilityMenuItemAssociationsRequest";
import { useAvailabilityMenuItemAssociationsApi } from "../../api/useAvailabilityMenuItemAssociationsApi";

export const useAvailabilityMenuItemsAssociationsQuery = (request: GetAvailabilityMenuItemAssociationsRequest | undefined) => {
    const user = useAuthenticatedUser();
    const api = useAvailabilityMenuItemAssociationsApi();

    const queryResult = useQueryable({
        queryName: "useAvailabilityMenuItemsAssociationsQuery",
        entityType: getEntityType(Entity.AvailabilityMenuItemAssociations),
        request: user.subMerchantId == undefined || request == undefined ? undefined : {
            ...request,
            subMerchantId: user.subMerchantId,
        },
        getId: (e: AvailabilityMenuItemAssociation) => `${e.availabilityId}-${e.menuItemId}`,
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