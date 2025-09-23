import { Entity, getEntityType } from "../../EntitiesName";
import { useQueryable } from "../useQueryable";
import { useAuthenticatedUser } from "../../../context/AuthContext";
import { GetPrintersRequest } from "../../api/Dtos/printers/GetPrintersRequest";
import { Printer } from "../../api/Dtos/printers/Printer";
import { usePrintersApi } from "../../api/usePrintersApi";

export const usePrintersQuery = (request: GetPrintersRequest | undefined) => {
    const user = useAuthenticatedUser();
    const api = usePrintersApi();

    const queryResult = useQueryable({
        queryName: "usePrintersQuery",
        entityType: getEntityType(Entity.Printers),
        request: user.subMerchantId == undefined || request == undefined ? undefined : {
            ...request,
            subMerchantId: user.subMerchantId,
        },
        getIdsFilter: r => r.ids,
        getId: (e: Printer) => e.id,
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

    return queryResult;
}