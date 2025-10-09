import { useMemo } from "react";
import { Entity, getEntityType } from "../../EntitiesName";
import { useQueryable } from "../useQueryable";
import { useAuthenticatedUser } from "../../../context/AuthContext";
import { GetConfigurableFieldAssociationsRequest } from "../../api/Dtos/configurableFieldAssociations/GetConfigurableFieldAssociationsRequest";
import { useConfigurableFieldAssociationsApi } from "../../api/useConfigurableFieldAssociationsApi";
import { ConfigurableFieldAssociation } from "../../api/Dtos/configurableFieldAssociations/ConfigurableFieldAssociation";

export const useConfigurableFieldAssociationsQuery = (request: GetConfigurableFieldAssociationsRequest | undefined) => {
    const user = useAuthenticatedUser();
    const api = useConfigurableFieldAssociationsApi();

    const queryResult = useQueryable({
        queryName: "useConfigurableFieldAssociationsQuery",
        entityType: getEntityType(Entity.ConfigurableFieldAssociations),
        request: user.subMerchantId == undefined || request == undefined ? undefined : {
            ...request,
            subMerchantId: user.subMerchantId,
        },
        getId: (e: ConfigurableFieldAssociation) => `${e.channelProfileId}-${e.configurableFieldId}`,
        query: api.get,

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