import { Entity, getEntityType } from "../../EntitiesName";
import { useQueryable } from "../useQueryable";
import { useAuth } from "../../../context/AuthContext";
import { GetPrintersRequest } from "../../api/Dtos/printers/GetPrintersRequest";
import { Printer } from "../../api/Dtos/printers/Printer";
import { usePrintersApi } from "../../api/usePrintersApi";

export const usePrintersQuery = (request: GetPrintersRequest | undefined) => {
    const auth = useAuth();
    const api = usePrintersApi(auth.token);

    const queryResult = useQueryable({
        queryName: "usePrintersQuery",
        entityType: getEntityType(Entity.Printers),
        request: auth.token == undefined || auth.subMerchantId == undefined || request == undefined ? undefined : {
            ...request,
            subMerchantId: auth.subMerchantId,
        },
        getIdsFilter: r => r.ids,
        getId: (e: Printer) => e.id,
        query: request => api.get(request),

        refreshOnAnyUpdate: request?.ids == undefined,
        canUseOptimizedResponse: r => r.ids != undefined,
        getResponseFromEntities: (e) => ({
            data: e,
            page: 0,
            totalPages: 1,
            totalItems: 1,
        }),
    })

    return queryResult;
}