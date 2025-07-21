import { useMemo } from "react";
import { useAuthenticatedUser } from "../../context/AuthContext";
import { Entity, getEntityType } from "../EntitiesName";
import { useMutator } from "./useMutator";
import { usePrinterWorkersApi } from "../api/usePrinterWorkersApi";
import { PrinterWorker } from "../api/Dtos/printerWorkers/PrinterWorker";
import { CreatePrinterWorkerRequest } from "../api/Dtos/printerWorkers/CreatePrinterWorkerRequest";

interface PatchMutator {
    readonly name?: string;
}
export const usePrinterWorkerMutator = () => {
    const user = useAuthenticatedUser();
    const api = usePrinterWorkersApi(user.token);
    
    const createMutator = useMutator({
        entityType: getEntityType(Entity.PrinterWorkers),
        getKey: (e: PrinterWorker) => e.id,
        updateCall: async (request: CreatePrinterWorkerRequest) => {
            const response = await api.create(request);
            return [response.data];
        }
    })

    const updateMutator = useMutator({
        entityType: getEntityType(Entity.PrinterWorkers),
        getKey: (e: PrinterWorker) => e.id,
        updateCall: async (request: PatchMutator, entities: PrinterWorker[]) => {
            const result = [] as PrinterWorker[];

            //NOTE: Current implementation limits entities to an array of a single entry.
            // Nevertheless, the above implementation works even if we would allow several.
            for(const entity of entities) {
                const response = await api.patch({
                    ...request,
                    id: entity.id,
                });
                
                if(response.data != undefined) {
                    result.push(response.data);
                }
            }
            return result;
        }
    })

    const deleteMutator = useMutator({
        entityType: getEntityType(Entity.PrinterWorkers),
        getKey: (e: PrinterWorker) => e.id,
        updateCall: async (_: {}, entities: PrinterWorker[]) => {
            const result = [] as PrinterWorker[];
            
            //NOTE: Current implementation limits entities to an array of a single entry.
            // Nevertheless, the above implementation works even if we would allow several.
            for(const entity of entities) {
                await api.delete({
                    id: entity.id,
                });
            }
            return result;
        }
    })

    const result = useMemo(() => ({
        create: async (request: CreatePrinterWorkerRequest) => {
            const result = await createMutator.mutate([], request);
            return result.response[0];
        },
        patch: async (e: PrinterWorker, mutator: PatchMutator) => {
            const result = await updateMutator.mutate([e], mutator);
            return result.response[0];
        },
        delete:  (e: PrinterWorker) => deleteMutator.mutate([e], {}),
    }), [user, api]);

    return result;
}