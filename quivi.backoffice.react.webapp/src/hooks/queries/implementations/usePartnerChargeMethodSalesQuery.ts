import { useMemo } from "react";
import { useAuthenticatedUser } from "../../../context/AuthContext";
import { Entity, getEntityType } from "../../EntitiesName";
import { useQueryable } from "../useQueryable";
import { useReportingApi } from "../../api/useReportingApi";
import { PartnerChargeMethodSales } from "../../api/Dtos/reporting/PartnerChargeMethodSales";
import { GetPartnerChargeMethodSalesRequest } from "../../api/Dtos/reporting/GetPartnerChargeMethodSalesRequest";

export const usePartnerChargeMethodSalesQuery = (request: GetPartnerChargeMethodSalesRequest | undefined) => {
    const user = useAuthenticatedUser();
    const api = useReportingApi();

    const queryResult = useQueryable({
        queryName: "usePartnerChargeMethodSalesQuery",
        entityType: getEntityType(Entity.Sales),
        request: user.merchantId == undefined || request == undefined ? undefined : {
            ...request,
            merchant: user.merchantId,
            subMerchantId: user.subMerchantId,
        },
        getId: (e: PartnerChargeMethodSales) => `${e.chargeMethod}-${e.chargePartner}-${e.from}-${e.to}`,
        query: api.getPartnerChargeMethodSales,

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