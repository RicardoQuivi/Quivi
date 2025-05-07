import { Entity, getEntityType } from "../../EntitiesName";
import { useQueryable } from "../useQueryable";
import { useAuth } from "../../../context/AuthContext";
import { useMenuCategoriesApi } from "../../api/useMenuCategoriesApi";
import { GetMenuCategoriesRequest } from "../../api/Dtos/menuCategories/GetMenuCategoriesRequest";
import { MenuCategory } from "../../api/Dtos/menuCategories/MenuCategory";

export const useMenuCategoriesQuery = (request: GetMenuCategoriesRequest | undefined) => {
    const auth = useAuth();
    const api = useMenuCategoriesApi(auth.token);

    const queryResult = useQueryable({
        queryName: "useMenuCategoriesQuery",
        entityType: getEntityType(Entity.MenuCategories),
        request: auth.token == undefined || auth.subMerchantId == undefined || request == undefined ? undefined : {
            ...request,
            subMerchantId: auth.subMerchantId,
        },
        getIdsFilter: r => r.ids,
        getId: (e: MenuCategory) => e.id,
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