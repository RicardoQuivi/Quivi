import { Entity, getEntityType } from "../../EntitiesName";
import { useQueryable } from "../useQueryable";
import { useAuthenticatedUser } from "../../../context/AuthContext";
import { useMenuItemsApi } from "../../api/useMenuItemsApi";
import { GetMenuItemsRequest } from "../../api/Dtos/menuItems/GetMenuItemsRequest";
import { MenuItem } from "../../api/Dtos/menuItems/MenuItem";
import { useMemo } from "react";

export const useMenuItemsQuery = (request: GetMenuItemsRequest | undefined) => {
    const user = useAuthenticatedUser();
    const api = useMenuItemsApi();

    const queryResult = useQueryable({
        queryName: "useMenuItemsQuery",
        entityType: getEntityType(Entity.MenuItems),
        request: user.subMerchantId == undefined || request == undefined ? undefined : {
            ...request,
            subMerchantId: user.subMerchantId,
        },
        getIdsFilter: r => r.ids,
        getId: (e: MenuItem) => e.id,
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
        page: queryResult.response?.page ?? 0,
        totalPages: queryResult.response?.totalPages ?? 0,
        totalItems: queryResult.response?.totalItems ?? 0,
    }), [queryResult])

    return result;
}