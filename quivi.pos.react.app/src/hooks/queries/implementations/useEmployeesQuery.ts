import { useMemo } from "react";
import { Employee } from "../../api/Dtos/employees/Employee";
import { GetEmployeesRequest } from "../../api/Dtos/employees/GetEmployeesRequest";
import { useEmployeesApi } from "../../api/useEmployeesApi";
import { Entity, getEntityType } from "../../EntitiesName";
import { useQueryable } from "../useQueryable";
import { QueryResult } from "../QueryResult";

export const useEmployeesQuery = (request: GetEmployeesRequest | undefined) => {
    const api = useEmployeesApi();
    
    const innerQueryResult = useQueryable({
        queryName: "useInternalEmployeesQuery",
        entityType: getEntityType(Entity.Employees),
        request: {
            page: 0,
            includeDeleted: true,
        } as GetEmployeesRequest,
        getId: (e: Employee) => e.id,
        query: api.get,
        refreshOnAnyUpdate: true,
    })

    const queryResult = useQueryable({
        queryName: "useEmployeesQuery",
        entityType: `${getEntityType(Entity.Employees)}-processed`,
        request: request == undefined ? undefined : {
            entities: innerQueryResult.data,
            isFirstLoading: innerQueryResult.isFirstLoading,
            isLoading: innerQueryResult.isLoading,

            request: request,
        },
        getId: (e: Employee) => e.id,
        query: async r => {
            const request = r.request;
            
            const allData = r.entities.filter((d) => {
                if(request.ids != undefined && request.ids.includes(d.id) == false) {
                    return false;
                }

                if(request.includeDeleted != true && d.isDeleted) {
                    return false;
                }

                return true;
            });

            let totalPages = 1;
            let resultData = allData;
            if(request.pageSize != undefined) {
                const start = request.page * request.pageSize;
                totalPages = Math.ceil(allData.length / request.pageSize);
                resultData = allData.splice(start, start + request.pageSize)
            }

            return {
                data: resultData,
                isFirstLoading: r.isFirstLoading,
                isLoading: r.isLoading,
                totalItems: allData.length,
                totalPages: totalPages,
                page: request.page,
            }
        },
        refreshOnAnyUpdate: false,
    })

    const result = useMemo(() => ({
        isFirstLoading: queryResult.isFirstLoading,
        isLoading: queryResult.isLoading,
        data: queryResult.data,
        page: queryResult.response?.page ?? 0,
        totalPages: queryResult.response?.totalPages ?? 0,
        totalItems: queryResult.response?.totalItems ?? 0,
    }), [queryResult]);

    return result;
}