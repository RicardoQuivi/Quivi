import { useMemo } from "react";
import { Entity, getEntityType } from "../../EntitiesName";
import { useQueryable } from "../useQueryable";
import { useAuthenticatedUser } from "../../../context/AuthContext";
import { GetConfigurableFieldRequest } from "../../api/Dtos/configurableFields/GetConfigurableFieldsRequest";
import { useConfigurableFieldsApi } from "../../api/useConfigurableFieldsApi";
import { ConfigurableField } from "../../api/Dtos/configurableFields/ConfigurableField";

export const useConfigurableFieldsQuery = (request: GetConfigurableFieldRequest | undefined) => {
    const user = useAuthenticatedUser();
    const api = useConfigurableFieldsApi();

    const queryResult = useQueryable({
        queryName: "useConfigurableFieldsQuery",
        entityType: getEntityType(Entity.ConfigurableFields),
        request: user.subMerchantId == undefined || request == undefined ? undefined : {
            ...request,
            subMerchantId: user.subMerchantId,
        },
        getIdsFilter: r => r.ids,
        getId: (e: ConfigurableField) => e.id,
        query: api.get,

        refreshOnAnyUpdate: request?.ids == undefined,
        canUseOptimizedResponse: r => r.ids != undefined,
        getResponseFromEntities: (e) => ({
            data: e,
            page: 0,
            totalPages: 1,
            totalItems: 1,
        }),
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