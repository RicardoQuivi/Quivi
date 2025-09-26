import { useMemo } from "react";
import { Entity, getEntityType } from "../../EntitiesName";
import { PagedQueryResult } from "../QueryResult";
import { useQueryable } from "../useQueryable";
import { GetMenuItemsRequest } from "../../api/Dtos/menuitems/GetMenuItemsRequest";
import { MenuItem } from "../../api/Dtos/menuitems/MenuItem";
import { useMenuItemsApi } from "../../api/useMenuItemsApi";

export const useMenuItemsQuery = (request: GetMenuItemsRequest | undefined) : PagedQueryResult<MenuItem> => {      
    const api = useMenuItemsApi();

    const innerQueryResult = useQueryable({
        queryName: "useInternalMenuItemsQuery",
        entityType: getEntityType(Entity.MenuItems),
        request: {
            includeDeleted : true,
            page: 0,
        } as GetMenuItemsRequest,
        getId: (e: MenuItem) => e.id,
        query: api.get,
        refreshOnAnyUpdate: true,
    })

    const queryResult = useQueryable({
        queryName: "useMenuItemsQuery",
        entityType: `${getEntityType(Entity.MenuItems)}-processed`,
        request: request == undefined || innerQueryResult.isFirstLoading ? undefined : {
            entities: innerQueryResult.data,
            isFirstLoading: innerQueryResult.isFirstLoading,
            isLoading: innerQueryResult.isLoading,

            request: request,
        },
        getId: (e: MenuItem) => e.id,
        query: async r => {
            const request = r.request;

            const allData = r.entities.filter((d) => {
                if(request.ids != undefined && request.ids.includes(d.id) == false) {
                    return false;
                }

                if(request.search != undefined) {
                    const matches = d.name.toLowerCase().includes(request.search.toLowerCase());
                    if(matches == false) {
                        return false;
                    }
                }

                if(request.menuCategoryId != undefined && d.categoryIds.includes(request.menuCategoryId) == false) {
                    return false;
                }

                if(request.includeDeleted != true && d.isDeleted) {
                    return false;
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