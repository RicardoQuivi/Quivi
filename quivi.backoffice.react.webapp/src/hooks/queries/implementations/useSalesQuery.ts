import { useMemo } from "react";
import { useAuthenticatedUser } from "../../../context/AuthContext";
import { Entity, getEntityType } from "../../EntitiesName";
import { useQueryable } from "../useQueryable";
import { useReportingApi } from "../../api/useReportingApi";
import { GetSalesRequest } from "../../api/Dtos/reporting/GetSalesRequest";
import { Sales } from "../../api/Dtos/reporting/Sales";

export const useSalesQuery = (request: GetSalesRequest | undefined) => {
    const user = useAuthenticatedUser();
    const api = useReportingApi();

    const queryResult = useQueryable({
        queryName: "useSalesQuery",
        entityType: getEntityType(Entity.Sales),
        request: user.merchantId == undefined || request == undefined ? undefined : {
            ...request,
            merchant: user.merchantId,
            subMerchantId: user.subMerchantId,
        },
        getId: (e: Sales) => `${e.from}-${e.to}`,
        query: api.getSales,

        refreshOnAnyUpdate: true,
    })

    const result = useMemo(() => ({
        isFirstLoading: queryResult.isFirstLoading,
        isLoading: queryResult.isLoading,
        data: queryResult.data,
        page: queryResult.response?.page ?? request?.page ?? 0,
        totalPages: queryResult.response?.totalPages ?? 0,
        totalItems: queryResult.response?.totalItems ?? 0,
    }), [queryResult, request?.page ?? 0])

    return result;
}