import { useMemo } from "react";
import { Entity, getEntityType } from "../../EntitiesName";
import { PagedQueryResult } from "../QueryResult";
import { useQueryable } from "../useQueryable";
import { GetPreparationGroupsRequest } from "../../api/Dtos/preparationgroups/GetPreparationGroupsRequest";
import { PreparationGroup } from "../../api/Dtos/preparationgroups/PreparationGroup";
import { usePreparationGroupsApi } from "../../api/usePreparationGroupsApi";

export const usePreparationGroupsQuery = (request: GetPreparationGroupsRequest | undefined) : PagedQueryResult<PreparationGroup> => {   
    const api = usePreparationGroupsApi();
        
    const queryResult = useQueryable({
        queryName: "usePreparationGroupsQuery",
        entityType: getEntityType(Entity.PreparationGroups),
        request: request,
        getId: (e: PreparationGroup) => e.id,
        query: r => api.get(r),

        refreshOnAnyUpdate: true,  
    })
    
    const result = useMemo(() => ({
        isFirstLoading: queryResult.isFirstLoading,
        isLoading: queryResult.isLoading,
        data: queryResult.data,
        page: queryResult.response?.page ?? request?.page ?? 1,
        totalPages: queryResult.response?.totalPages ?? 0,
        totalItems: queryResult.response?.totalItems ?? 0,
    }), [queryResult, request?.page ?? 1])
    
    return result;
}