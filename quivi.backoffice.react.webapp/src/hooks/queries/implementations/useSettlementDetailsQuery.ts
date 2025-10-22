import { Entity, getEntityType } from "../../EntitiesName";
import { useQueryable } from "../useQueryable";
import { useAuthenticatedUser } from "../../../context/AuthContext";
import { useMemo } from "react";
import { useSettlementsApi } from "../../api/useSettlementsApi";
import { SettlementDetail } from "../../api/Dtos/settlements/SettlementDetail";
import { GetSettlementDetailsRequest } from "../../api/Dtos/settlements/GetSettlementDetailsRequest";

export const useSettlementDetailsQuery = (request: GetSettlementDetailsRequest | undefined) => {
    const user = useAuthenticatedUser();
    const api = useSettlementsApi();

    const queryResult = useQueryable({
        queryName: "useSettlementDetailsQuery",
        entityType: getEntityType(Entity.SettlementDetails),
        request: user.subMerchantId == undefined || request == undefined ? undefined : {
            ...request,
            subMerchantId: user.subMerchantId,
        },
        getId: (e: SettlementDetail) => e.id,
        query: api.getDetails,

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