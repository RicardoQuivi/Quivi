import { Entity, getEntityType } from "../../EntitiesName";
import { useQueryable } from "../useQueryable";
import { useAuthenticatedUser } from "../../../context/AuthContext";
import { GetEmployeesRequest } from "../../api/Dtos/employees/GetEmployeesRequest";
import { useEmployeesApi } from "../../api/useEmployeesApi";
import { Employee } from "../../api/Dtos/employees/Employee";
import { useMemo } from "react";

export const useEmployeesQuery = (request: GetEmployeesRequest | undefined) => {
    const user = useAuthenticatedUser();
    const api = useEmployeesApi();

    const queryResult = useQueryable({
        queryName: "useEmployeesQuery",
        entityType: getEntityType(Entity.Employees),
        request: user.subMerchantId == undefined || request == undefined ? undefined : {
            ...request,
            subMerchantId: user.subMerchantId,
        },
        getIdsFilter: r => r.ids,
        getId: (e: Employee) => e.id,
        query: request => api.get(request),

        refreshOnAnyUpdate: true,
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