import { useLoggedEmployee } from "../../../context/pos/LoggedEmployeeContextProvider";
import { CustomChargeMethod } from "../../api/Dtos/customchargemethods/CustomChargeMethod";
import { GetCustomChargeMethodsRequest } from "../../api/Dtos/customchargemethods/GetCustomChargeMethodsRequest";
import { useCustomChargeMethodsApi } from "../../api/useCustomChargeMethodsApi";
import { Entity, getEntityType } from "../../EntitiesName";
import { QueryResult } from "../QueryResult";
import { useQueryable } from "../useQueryable";

export const useCustomChargeMethodsQuery = (request: GetCustomChargeMethodsRequest | undefined) : QueryResult<CustomChargeMethod[]> => {
    const posContext = useLoggedEmployee();
    const api = useCustomChargeMethodsApi(posContext.token);
    
    const queryResult = useQueryable({
        queryName: "useCustomChargeMethodsQuery",
        entityType: getEntityType(Entity.CustomChargeMethods),
        request: request == undefined ? undefined : {
            ...request,
            ids: request?.ids == undefined ? undefined : Array.from(new Set(request.ids)),
        },
        getId: (e: CustomChargeMethod) => e.id,
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

    return queryResult;
}