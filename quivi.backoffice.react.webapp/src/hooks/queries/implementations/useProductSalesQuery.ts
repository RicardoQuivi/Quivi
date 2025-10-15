import { useMemo } from "react";
import { useAuthenticatedUser } from "../../../context/AuthContext";
import { Entity, getEntityType } from "../../EntitiesName";
import { useQueryable } from "../useQueryable";
import { useReportingApi } from "../../api/useReportingApi";
import { GetProductSalesRequest } from "../../api/Dtos/reporting/GetProductSalesRequest";
import { ProductSales } from "../../api/Dtos/reporting/ProductSales";

export const useProductSalesQuery = (request: GetProductSalesRequest | undefined) => {
    const user = useAuthenticatedUser();
    const api = useReportingApi();

    const queryResult = useQueryable({
        queryName: "useProductSalesQuery",
        entityType: getEntityType(Entity.Sales),
        request: user.merchantId == undefined || request == undefined ? undefined : {
            ...request,
            merchant: user.merchantId,
            subMerchantId: user.subMerchantId,
        },
        getId: (e: ProductSales) => `Product-${e.menuItemId}-${e.from}-${e.to}`,
        query: api.getProductSales,

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