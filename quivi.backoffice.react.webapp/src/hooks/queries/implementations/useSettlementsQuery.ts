import { Entity, getEntityType } from "../../EntitiesName";
import { useQueryable } from "../useQueryable";
import { useAuthenticatedUser } from "../../../context/AuthContext";
import { useMemo } from "react";
import { GetSettlementsRequest } from "../../api/Dtos/settlements/GetSettlementsRequest";
import { useSettlementsApi } from "../../api/useSettlementsApi";
import { Settlement } from "../../api/Dtos/settlements/Settlement";

export const useSettlementsQuery = (request: GetSettlementsRequest | undefined) => {
    const user = useAuthenticatedUser();
    const api = useSettlementsApi();

    const queryResult = useQueryable({
        queryName: "useSettlementsQuery",
        entityType: getEntityType(Entity.Settlements),
        request: user.subMerchantId == undefined || request == undefined ? undefined : {
            ...request,
            subMerchantId: user.subMerchantId,
        },
        getId: (e: Settlement) => e.id,
        query: api.get,

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