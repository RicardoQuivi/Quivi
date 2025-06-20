import { Entity, getEntityType } from "../../EntitiesName";
import { useQueryable } from "../useQueryable";
import { useAuth } from "../../../context/AuthContext";
import { usePrinterWorkersApi } from "../../api/usePrinterWorkersApi";
import { GetPrinterWorkersRequest } from "../../api/Dtos/printerWorkers/GetPrinterWorkersRequest";
import { PrinterWorker } from "../../api/Dtos/printerWorkers/PrinterWorker";

export const usePrinterWorkersQuery = (request: GetPrinterWorkersRequest | undefined) => {
    const auth = useAuth();
    const api = usePrinterWorkersApi(auth.token);

    const queryResult = useQueryable({
        queryName: "usePrinterWorkersQuery",
        entityType: getEntityType(Entity.PrinterWorkers),
        request: auth.token == undefined || auth.subMerchantId == undefined || request == undefined ? undefined : {
            ...request,
            subMerchantId: auth.subMerchantId,
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