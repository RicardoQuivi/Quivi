import { useMemo } from "react";
import { useAuthenticatedUser } from "../../../context/AuthContext";
import { Entity, getEntityType } from "../../EntitiesName";
import { useQueryable } from "../useQueryable";
import { useReportingApi } from "../../api/useReportingApi";
import { GetChargeMethodSalesRequest } from "../../api/Dtos/reporting/GetChargeMethodSalesRequest";
import { ChargeMethodSales } from "../../api/Dtos/reporting/ChargeMethodSales";

export const useChargeMethodSalesQuery = (request: GetChargeMethodSalesRequest | undefined) => {
    const user = useAuthenticatedUser();
    const api = useReportingApi();

    const queryResult = useQueryable({
        queryName: "useChargeMethodSalesQuery",
        entityType: getEntityType(Entity.Sales),
        request: user.merchantId == undefined || request == undefined ? undefined : {
            ...request,
            merchant: user.merchantId,
            subMerchantId: user.subMerchantId,
        },
        getId: (e: ChargeMethodSales) => `ChargeMethod-${e.customChargeMethodId ?? "quivi"}-${e.from}-${e.to}`,
        query: api.getChargeMethodSales,

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