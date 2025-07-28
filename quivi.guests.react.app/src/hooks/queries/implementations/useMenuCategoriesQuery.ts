import { useMemo } from "react";
import type { GetMenuCategoriesRequest } from "../../api/Dtos/menuCategories/GetMenuCategoriesRequest";
import type { MenuCategory } from "../../api/Dtos/menuCategories/MenuCategory";
import { useMenuCategoriesApi } from "../../api/useMenuCategoriesApi";
import { Entity, getEntityType } from "../../EntitiesName";
import type { PagedQueryResult } from "../QueryResult";
import { useQueryable } from "../useQueryable";
import { useTranslation } from "react-i18next";

export const useMenuCategoriesQuery = (request: GetMenuCategoriesRequest | undefined): PagedQueryResult<MenuCategory> => {
    const { i18n } = useTranslation();
    const api = useMenuCategoriesApi();

    const query = useQueryable({
        queryName: "useMenuCategoriesQuery",
        entityType: getEntityType(Entity.MenuCategories),
        getId: (item: MenuCategory) => item.id,
        request: request == undefined ? undefined : {
            ...request,
            i18nLanguage: i18n.language,
        },
        query: request => api.get(request),
        
        refreshOnAnyUpdate: false,
        canUseOptimizedResponse: _ => false,
        getResponseFromEntities: (e) => ({
            data: e,
            page: 0,
            totalPages: 1,
            totalItems: 1,
        }),  
    })
    
    const result = useMemo(() => ({
        isFirstLoading: query.isFirstLoading,
        isLoading: query.isLoading,
        data: query.data,
        page: query.response?.page ?? 0,
        totalPages: query.response?.totalPages ?? 0,
        totalItems: query.response?.totalItems ?? 0,
    }), [query])
    
    return result;
}