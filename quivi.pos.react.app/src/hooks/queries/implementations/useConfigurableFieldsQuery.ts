import { useMemo } from "react";
import { Entity, getEntityType } from "../../EntitiesName";
import { PagedQueryResult } from "../QueryResult";
import { useQueryable } from "../useQueryable";
import { GetConfigurableFieldsRequest } from "../../api/Dtos/configurablefields/GetConfigurableFieldsRequest";
import { ConfigurableField } from "../../api/Dtos/configurablefields/ConfigurableField";

export const useConfigurableFieldsQuery = (request: GetConfigurableFieldsRequest | undefined) : PagedQueryResult<ConfigurableField> => {      
    //const posContext = useLoggedEmployee();

    const queryResult = useQueryable({
        queryName: "useConfigurableFieldsQuery",
        entityType: getEntityType(Entity.ConfigurableFields),
        request: request == undefined ? undefined : {
            ...request,
        },
        getId: (e: ConfigurableField) => e.id,
        query: async () => {
            return {
                data: [],
                page: 0,
                totalPages: 0,
                totalItems: 0,
            }
        },

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