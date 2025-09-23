import { Entity, getEntityType } from "../../EntitiesName";
import { useQueryable } from "../useQueryable";
import { useAuthenticatedUser } from "../../../context/AuthContext";
import { useMenuCategoriesApi } from "../../api/useMenuCategoriesApi";
import { GetMenuCategoriesRequest } from "../../api/Dtos/menuCategories/GetMenuCategoriesRequest";
import { MenuCategory } from "../../api/Dtos/menuCategories/MenuCategory";

export const useMenuCategoriesQuery = (request: GetMenuCategoriesRequest | undefined) => {
    const user = useAuthenticatedUser();
    const api = useMenuCategoriesApi();

    const queryResult = useQueryable({
        queryName: "useMenuCategoriesQuery",
        entityType: getEntityType(Entity.MenuCategories),
        request: user.subMerchantId == undefined || request == undefined ? undefined : {
            ...request,
            subMerchantId: user.subMerchantId,
        },
        getIdsFilter: r => r.ids,
        getId: (e: MenuCategory) => e.id,
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

    return queryResult;
}