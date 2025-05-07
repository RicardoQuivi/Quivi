import { useMemo } from "react";
import { useLoggedEmployee } from "../../../context/pos/LoggedEmployeeContextProvider";
import { Entity, getEntityType } from "../../EntitiesName";
import { PagedQueryResult } from "../QueryResult";
import { useQueryable } from "../useQueryable";
import { GetMenuItemsRequest } from "../../api/Dtos/menuitems/GetMenuItemsRequest";
import { MenuItem } from "../../api/Dtos/menuitems/MenuItem";
import { useMenuItemsApi } from "../../api/useMenuItemsApi";

export const useMenuItemsQuery = (request: GetMenuItemsRequest | undefined) : PagedQueryResult<MenuItem> => {      
    const posContext = useLoggedEmployee();
    const api = useMenuItemsApi(posContext.token);

    const queryResult = useQueryable({
        queryName: "useMenuItemsQuery",
        entityType: getEntityType(Entity.MenuItems),
        request: request == undefined ? undefined : {
            ...request,
            ids: request.ids == undefined ? undefined : Array.from(new Set(request.ids)),
            search: !!request.search ? request.search : undefined,
        },
        getId: (e: MenuItem) => e.id,
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