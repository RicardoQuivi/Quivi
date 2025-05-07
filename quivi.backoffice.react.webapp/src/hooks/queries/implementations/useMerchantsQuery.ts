import { useMemo } from "react";
import { GetMerchantsRequest } from "../../api/Dtos/merchants/GetMerchantsRequest";
import { Merchant } from "../../api/Dtos/merchants/Merchant";
import { useMerchantsApi } from "../../api/useMerchantsApi"
import { Entity, getEntityType } from "../../EntitiesName";
import { useQueryable } from "../useQueryable";
import { useAuth } from "../../../context/AuthContext";

export const useMerchantsQuery = (request: GetMerchantsRequest | undefined) => {
    const auth = useAuth();
    const api = useMerchantsApi(auth.token);

    const queryResult = useQueryable({
        queryName: "useMerchantsQuery",
        entityType: getEntityType(Entity.Merchants),
        request: auth.token == undefined || request == undefined ? undefined : {
            ...request,
            authMerchantId: auth.merchantId,
            authSubMerchantId: auth.subMerchantId,
        },
        getId: (e: Merchant) => e.id,
        query: request => api.get(request),
        refreshOnAnyUpdate: true,
    })

    const result = useMemo(() => ({
        isFirstLoading: queryResult.isFirstLoading,
        isLoading: queryResult.isLoading,
        data: queryResult.data,
        page: queryResult.response?.page ?? 1,
        totalPages: queryResult.response?.totalPages ?? 0,
        totalItems: queryResult.response?.totalItems ?? 0,
    }), [queryResult])

    return result;
}