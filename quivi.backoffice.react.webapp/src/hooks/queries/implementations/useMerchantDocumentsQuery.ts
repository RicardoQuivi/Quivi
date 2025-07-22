import { useMemo } from "react";
import { useAuthenticatedUser } from "../../../context/AuthContext";
import { Entity, getEntityType } from "../../EntitiesName";
import { useQueryable } from "../useQueryable";
import { GetMerchantDocumentsRequest } from "../../api/Dtos/merchantdocuments/GetMerchantDocumentsRequest";
import { useMerchantDocumentsApi } from "../../api/useMerchantDocumentsApi";
import { MerchantDocument } from "../../api/Dtos/merchantdocuments/MerchantDocument";

export const useMerchantDocumentsQuery = (request: GetMerchantDocumentsRequest | undefined) => {
    const user = useAuthenticatedUser();
    const api = useMerchantDocumentsApi(user.token);

    const queryResult = useQueryable({
        queryName: "useMerchantDocumentsQuery",
        entityType: getEntityType(Entity.MerchantDocuments),
        request: user.merchantId == undefined || request == undefined ? undefined : {
            ...request,
            merchant: user.merchantId,
            subMerchantId: user.subMerchantId,
        },
        getId: (e: MerchantDocument) => e.id,
        query: request => api.get(request),

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