import { Entity, getEntityType } from "../../EntitiesName";
import { useQueryable } from "../useQueryable";
import { useAuth } from "../../../context/AuthContext";
import { useMenuItemsApi } from "../../api/useMenuItemsApi";
import { GetMenuItemsRequest } from "../../api/Dtos/menuItems/GetMenuItemsRequest";
import { MenuItem } from "../../api/Dtos/menuItems/MenuItem";

export const useMenuItemsQuery = (request: GetMenuItemsRequest | undefined) => {
    const auth = useAuth();
    const api = useMenuItemsApi(auth.token);

    const queryResult = useQueryable({
        queryName: "useMenuItemsQuery",
        entityType: getEntityType(Entity.MenuItems),
        request: auth.token == undefined || auth.subMerchantId == undefined || request == undefined ? undefined : {
            ...request,
            subMerchantId: auth.subMerchantId,
        },
        getIdsFilter: r => r.ids,
        getId: (e: MenuItem) => e.id,
        query: request => api.get(request),

        refreshOnAnyUpdate: request?.ids == undefined,
        canUseOptimizedResponse: r => r.ids != undefined,
        getResponseFromEntities: (e) => ({
            data: e,
            page: 0,
            totalPages: 1,
            totalItems: 1,
        }),
    })

    return queryResult;
}