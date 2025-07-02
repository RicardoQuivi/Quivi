import { Entity, getEntityType } from "../../EntitiesName";
import { useQueryable } from "../useQueryable";
import { useAuth } from "../../../context/AuthContext";
import { GetPrinterMessagesRequest } from "../../api/Dtos/printerMessages/GetPrinterMessagesRequest";
import { usePrinterMessagesApi } from "../../api/usePrinterMessagesApi";
import { PrinterMessage } from "../../api/Dtos/printerMessages/PrinterMessage";
import { useMemo } from "react";

export const usePrinterMessagesQuery = (request: GetPrinterMessagesRequest | undefined) => {
    const auth = useAuth();
    const api = usePrinterMessagesApi(auth.token);

    const queryResult = useQueryable({
        queryName: "usePrintersQuery",
        entityType: getEntityType(Entity.PrinterMessages),
        request: auth.token == undefined || auth.subMerchantId == undefined || request == undefined ? undefined : {
            ...request,
            subMerchantId: auth.subMerchantId,
        },
        getId: (e: PrinterMessage) => `${e.printerId}-${e.messageId}`,
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