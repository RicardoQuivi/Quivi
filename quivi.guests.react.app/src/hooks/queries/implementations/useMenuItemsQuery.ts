import { useMemo } from "react";
import { Entity, getEntityType } from "../../EntitiesName";
import type { PagedQueryResult } from "../QueryResult";
import { useQueryable } from "../useQueryable";
import type { GetMenuItemsRequest } from "../../api/Dtos/menuItems/GetMenuItemsRequest";
import type { MenuItem } from "../../api/Dtos/menuItems/MenuItem";
import { useMenuItemsApi } from "../../api/useMenuItemsApi";
import { useTranslation } from "react-i18next";

export const useMenuItemsQuery = (request: GetMenuItemsRequest | undefined): PagedQueryResult<MenuItem> => {
    const { i18n } = useTranslation();
    const api = useMenuItemsApi();

    const query = useQueryable({
        queryName: "useMenuItemsQuery",
        entityType: getEntityType(Entity.MenuItems),
        getId: (item: MenuItem) => item.id,
        request: request == undefined ? undefined : {
            ...request,
            i18nLanguage: i18n.language,
        },
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