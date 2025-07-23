import { useMemo } from "react";
import { Entity, getEntityType } from "../../EntitiesName";
import { PagedQueryResult } from "../QueryResult";
import { useQueryable } from "../useQueryable";
import { GetMenuCategoriesRequest } from "../../api/Dtos/menucategories/GetMenuCategoriesRequest";
import { MenuCategory } from "../../api/Dtos/menucategories/MenuCategory";
import { useMenuCategoriesApi } from "../../api/useMenuCategoriesApi";

export const useMenuCategoriesQuery = (request: GetMenuCategoriesRequest | undefined) : PagedQueryResult<MenuCategory> => {      
    const api = useMenuCategoriesApi();

    const queryResult = useQueryable({
        queryName: "useMenuCategoriesQuery",
        entityType: getEntityType(Entity.MenuCategories),
        request: request == undefined ? undefined : {
            ...request,
        },
        getId: (e: MenuCategory) => e.id,
        query: r => api.get(r),

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