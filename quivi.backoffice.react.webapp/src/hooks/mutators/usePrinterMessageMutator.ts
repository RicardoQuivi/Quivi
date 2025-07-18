import { useMemo } from "react";
import { useAuth } from "../../context/AuthContext";
import { usePrinterMessagesApi } from "../api/usePrinterMessagesApi";
import { CreatePrinterMessageRequest } from "../api/Dtos/printerMessages/CreatePrinterMessageRequest";
import { useMutator } from "./useMutator";
import { Entity, getEntityType } from "../EntitiesName";
import { PrinterMessage } from "../api/Dtos/printerMessages/PrinterMessage";

export const usePrinterMessageMutator = () => {
    const auth = useAuth();
    const api = usePrinterMessagesApi(auth.token);
    
    const createMutator = useMutator({
        entityType: getEntityType(Entity.PrinterMessages),
        getKey: (e: PrinterMessage) => `${e.printerId}-${e.messageId}`,
        updateCall: async (request: CreatePrinterMessageRequest) => {
            const response = await api.create(request);
            return [response.data];
        }
    })
        
    const result = useMemo(() => ({
        create: async (request: CreatePrinterMessageRequest) => {
            const result = await createMutator.mutate([], request);
            return result.response;
        },
    }), [auth, api]);

    return result;
}