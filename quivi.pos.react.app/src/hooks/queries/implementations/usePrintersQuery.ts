import { useMemo } from "react";
import { Entity, getEntityType } from "../../EntitiesName";
import { PagedQueryResult } from "../QueryResult";
import { useQueryable } from "../useQueryable";
import { GetPrintersRequest } from "../../api/Dtos/printers/GetPrintersRequest";
import { Printer } from "../../api/Dtos/printers/Printer";
import { usePrinterApi } from "../../api/usePrinterApi";
import { useLoggedEmployee } from "../../../context/pos/LoggedEmployeeContextProvider";

export const usePrintersQuery = (request: GetPrintersRequest | undefined) : PagedQueryResult<Printer> => {
    const posContext = useLoggedEmployee();
    const api = usePrinterApi(posContext.token);

    const queryResult = useQueryable({
        queryName: "usePrintersQuery",
        entityType: getEntityType(Entity.Printers),
        request: request == undefined ? undefined : {
            ...request,
        },
        getId: (e: Printer) => e.id,
        query: r => api.get(r),

        refreshOnAnyUpdate: true,
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