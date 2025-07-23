import { useMemo } from "react";
import { Entity, getEntityType } from "../EntitiesName";
import { useMutator } from "./useMutator";
import { Local } from "../api/Dtos/locals/Local";
import { CreateLocalRequest } from "../api/Dtos/locals/CreateLocalRequest";
import { useLocalsApi } from "../api/useLocalsApi";

interface PatchMutator {
    readonly name?: string;
}
export const useLocalMutator = () => {
    const api = useLocalsApi();
    
    const createMutator = useMutator({
        entityType: getEntityType(Entity.Locals),
        getKey: (e: Local) => e.id,
        updateCall: async (request: CreateLocalRequest) => {
            const response = await api.create(request);
            return [response.data];
        }
    })

    const updateMutator = useMutator({
        entityType: getEntityType(Entity.Locals),
        getKey: (e: Local) => e.id,
        updateCall: async (request: PatchMutator, entities: Local[]) => {
            const result = [] as Local[];

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
        entityType: getEntityType(Entity.Locals),
        getKey: (e: Local) => e.id,
        updateCall: async (_: {}, entities: Local[]) => {
            const result = [] as Local[];
            
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
        create: async (request: CreateLocalRequest) => {
            const result = await createMutator.mutate([], request);
            return result.response;
        },
        patch: async (e: Local, mutator: PatchMutator) => {
            const result = await updateMutator.mutate([e], mutator);
            return result.response[0];
        },
        delete:  (e: Local) => deleteMutator.mutate([e], {}),
    }), [api]);

    return result;
}