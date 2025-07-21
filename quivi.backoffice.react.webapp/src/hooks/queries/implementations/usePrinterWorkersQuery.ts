import { Entity, getEntityType } from "../../EntitiesName";
import { useQueryable } from "../useQueryable";
import { useAuthenticatedUser } from "../../../context/AuthContext";
import { usePrinterWorkersApi } from "../../api/usePrinterWorkersApi";
import { GetPrinterWorkersRequest } from "../../api/Dtos/printerWorkers/GetPrinterWorkersRequest";
import { PrinterWorker } from "../../api/Dtos/printerWorkers/PrinterWorker";

export const usePrinterWorkersQuery = (request: GetPrinterWorkersRequest | undefined) => {
    const user = useAuthenticatedUser();
    const api = usePrinterWorkersApi(user.token);

    const queryResult = useQueryable({
        queryName: "usePrinterWorkersQuery",
        entityType: getEntityType(Entity.PrinterWorkers),
        request: user.subMerchantId == undefined || request == undefined ? undefined : {
            ...request,
            subMerchantId: user.subMerchantId,
        },
        getIdsFilter: r => r.ids,
        getId: (e: PrinterWorker) => e.id,
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