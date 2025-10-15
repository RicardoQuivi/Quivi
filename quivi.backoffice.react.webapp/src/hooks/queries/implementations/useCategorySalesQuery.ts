import { useMemo } from "react";
import { useAuthenticatedUser } from "../../../context/AuthContext";
import { Entity, getEntityType } from "../../EntitiesName";
import { useQueryable } from "../useQueryable";
import { useReportingApi } from "../../api/useReportingApi";
import { GetCategorySalesRequest } from "../../api/Dtos/reporting/GetCategorySalesRequest";
import { CategorySales } from "../../api/Dtos/reporting/CategorySales";

export const useCategorySalesQuery = (request: GetCategorySalesRequest | undefined) => {
    const user = useAuthenticatedUser();
    const api = useReportingApi();

    const queryResult = useQueryable({
        queryName: "useCategorySalesQuery",
        entityType: getEntityType(Entity.Sales),
        request: user.merchantId == undefined || request == undefined ? undefined : {
            ...request,
            merchant: user.merchantId,
            subMerchantId: user.subMerchantId,
        },
        getId: (e: CategorySales) => `Category-${e.menuCategoryId}-${e.from}-${e.to}`,
        query: api.getCategorySales,

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