import { useMemo } from "react";
import { Entity, getEntityType } from "../../EntitiesName";
import { PagedQueryResult } from "../QueryResult";
import { useQueryable } from "../useQueryable";
import { GetMenuCategoriesRequest } from "../../api/Dtos/menucategories/GetMenuCategoriesRequest";
import { MenuCategory } from "../../api/Dtos/menucategories/MenuCategory";
import { useMenuCategoriesApi } from "../../api/useMenuCategoriesApi";

export const useMenuCategoriesQuery = (request: GetMenuCategoriesRequest | undefined) : PagedQueryResult<MenuCategory> => {      
    const api = useMenuCategoriesApi();

    const innerQueryResult = useQueryable({
        queryName: "useInternalMenuCategoriesQuery",
        entityType: getEntityType(Entity.MenuCategories),
        request: {
            includeDeleted : true,
            page: 0,
        } as GetMenuCategoriesRequest,
        getId: (e: MenuCategory) => e.id,
        query: api.get,
        refreshOnAnyUpdate: true,
    })

    const categoriesWithItemsQueryResult = useQueryable({
        queryName: "useInternalMenuCategoriesQuery-withItems",
        entityType: getEntityType(Entity.MenuCategories),
        request: {
            hasItems: true,
            includeDeleted : true,
            page: 0,
        } as GetMenuCategoriesRequest,
        getId: (e: MenuCategory) => e.id,
        query: api.get,
        refreshOnAnyUpdate: true,
    })

    const categoriesWithItemsSet = useMemo(() => {
        if(categoriesWithItemsQueryResult.isFirstLoading) {
            return undefined;
        }

        const set = new Set<string>();
        for(const c of categoriesWithItemsQueryResult.data) {
            set.add(c.id);
        }
        return set;
    }, [categoriesWithItemsQueryResult.isFirstLoading, categoriesWithItemsQueryResult.data])

    const queryResult = useQueryable({
        queryName: "useMenuCategoriesQuery",
        entityType: `${getEntityType(Entity.MenuCategories)}-processed`,
        request: request == undefined || innerQueryResult.isFirstLoading || categoriesWithItemsSet == undefined ? undefined : {
            entities: innerQueryResult.data,
            isFirstLoading: innerQueryResult.isFirstLoading,
            isLoading: innerQueryResult.isLoading,

            request: request,

            categoriesWithItemsSet: categoriesWithItemsSet,
        },
        getId: (e: MenuCategory) => e.id,
        query: async r => {
            const request = r.request;

            const allData = r.entities.filter((d) => {
                if(request.ids != undefined && request.ids.includes(d.id) == false) {
                    return false;
                }

                if(request.hasItems != undefined) {
                    const hasItems = r.categoriesWithItemsSet.has(d.id);
                    if(request.hasItems != hasItems) {
                        return false;
                    }
                }

                return true;
            });

            let totalPages = 1;
            let resultData = allData;
            if(r.request.pageSize != undefined) {
                const start = r.request.page * r.request.pageSize;
                totalPages = Math.ceil(allData.length / r.request.pageSize);
                resultData = allData.splice(start, start + r.request.pageSize)
            }   
            return {
                data: resultData,
                isFirstLoading: r.isFirstLoading,
                isLoading: r.isLoading,
                totalItems: allData.length,
                totalPages: totalPages,
                page: r.request.page,
            }
        },

        refreshOnAnyUpdate: false,
    })
    
    const result = useMemo(() => ({
        isFirstLoading: queryResult.response?.isFirstLoading ?? queryResult.isFirstLoading,
        isLoading: queryResult.isLoading,
        data: queryResult.data,
        page: queryResult.response?.page ?? 0,
        totalPages: queryResult.response?.totalPages ?? 0,
        totalItems: queryResult.response?.totalItems ?? 0,
    }), [queryResult])

    return result;
}