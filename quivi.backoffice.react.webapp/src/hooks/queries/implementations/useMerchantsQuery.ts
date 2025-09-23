import { useMemo } from "react";
import { GetMerchantsRequest } from "../../api/Dtos/merchants/GetMerchantsRequest";
import { Merchant } from "../../api/Dtos/merchants/Merchant";
import { useMerchantsApi } from "../../api/useMerchantsApi"
import { Entity, getEntityType } from "../../EntitiesName";
import { useQueryable } from "../useQueryable";
import { useAuthenticatedUser } from "../../../context/AuthContext";

export const useMerchantsQuery = (request: GetMerchantsRequest | undefined) => {
    const user = useAuthenticatedUser();
    const api = useMerchantsApi();

    const queryResult = useQueryable({
        queryName: "useMerchantsQuery",
        entityType: getEntityType(Entity.Merchants),
        request: request == undefined ? undefined : {
            ...request,
            authMerchantId: user.merchantId,
            authSubMerchantId: user.subMerchantId,
        },
        getId: (e: Merchant) => e.id,
        query: api.get,
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