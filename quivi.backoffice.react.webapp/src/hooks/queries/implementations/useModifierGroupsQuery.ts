import { Entity, getEntityType } from "../../EntitiesName";
import { useQueryable } from "../useQueryable";
import { useAuthenticatedUser } from "../../../context/AuthContext";
import { useMemo } from "react";
import { GetModifierGroupsRequest } from "../../api/Dtos/modifierGroups/GetModifierGroupsRequest";
import { useModifierGroupsApi } from "../../api/useModifierGroupsApi";
import { ModifierGroup } from "../../api/Dtos/modifierGroups/ModifierGroup";

export const useModifierGroupsQuery = (request: GetModifierGroupsRequest | undefined) => {
    const user = useAuthenticatedUser();
    const api = useModifierGroupsApi();

    const queryResult = useQueryable({
        queryName: "useModifierGroupsQuery",
        entityType: getEntityType(Entity.ModifierGroups),
        request: user.subMerchantId == undefined || request == undefined ? undefined : {
            ...request,
            subMerchantId: user.subMerchantId,
        },
        getIdsFilter: r => r.ids,
        getId: (e: ModifierGroup) => e.id,
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