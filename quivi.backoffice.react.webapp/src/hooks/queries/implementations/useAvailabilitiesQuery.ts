import { Entity, getEntityType } from "../../EntitiesName";
import { useQueryable } from "../useQueryable";
import { useAuthenticatedUser } from "../../../context/AuthContext";
import { useMemo } from "react";
import { GetAvailabilitiesRequest } from "../../api/Dtos/availabilities/GetAvailabilitiesRequest";
import { Availability } from "../../api/Dtos/availabilities/Availability";
import { useAvailabilitiesApi } from "../../api/useAvailabilitiesApi";

export const useAvailabilitiesQuery = (request: GetAvailabilitiesRequest | undefined) => {
    const user = useAuthenticatedUser();
    const api = useAvailabilitiesApi();

    const queryResult = useQueryable({
        queryName: "useAvailabilitiesQuery",
        entityType: getEntityType(Entity.Availabilities),
        request: user.subMerchantId == undefined || request == undefined ? undefined : {
            ...request,
            subMerchantId: user.subMerchantId,
        },
        getIdsFilter: r => r.ids,
        getId: (e: Availability) => e.id,
        query: api.get,

        refreshOnAnyUpdate: true,
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