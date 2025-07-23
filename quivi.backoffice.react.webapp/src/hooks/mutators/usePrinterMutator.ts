import { useMemo } from "react";
import { Entity, getEntityType } from "../EntitiesName";
import { useMutator } from "./useMutator";
import { Printer } from "../api/Dtos/printers/Printer";
import { usePrintersApi } from "../api/usePrintersApi";
import { CreatePrinterRequest } from "../api/Dtos/printers/CreatePrinterRequest";
import { NotificationType } from "../api/Dtos/notifications/NotificationType";

interface PatchMutator {
    readonly name?: string;
    readonly address?: string;
    readonly printerWorkerId?: string;
    readonly locationId?: string | null;
    readonly notifications?: NotificationType[];
}

export const usePrinterMutator = () => {
    const api = usePrintersApi();
    
    const createMutator = useMutator({
        entityType: getEntityType(Entity.Printers),
        getKey: (e: Printer) => e.id,
        updateCall: async (request: CreatePrinterRequest) => {
            const response = await api.create(request);
            return [response.data];
        }
    })

    const updateMutator = useMutator({
        entityType: getEntityType(Entity.Printers),
        getKey: (e: Printer) => e.id,
        updateCall: async (request: PatchMutator, entities: Printer[]) => {
            const result = [] as Printer[];

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
        entityType: getEntityType(Entity.Printers),
        getKey: (e: Printer) => e.id,
        updateCall: async (_: {}, entities: Printer[]) => {
            const result = [] as Printer[];
            
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
        create: async (request: CreatePrinterRequest) => {
            const result = await createMutator.mutate([], request);
            return result.response;
        },
        patch: async (e: Printer, mutator: PatchMutator) => {
            const result = await updateMutator.mutate([e], mutator);
            return result.response[0];
        },
        delete:  (e: Printer) => deleteMutator.mutate([e], {}),
    }), [api]);

    return result;
}