import { useMemo } from "react";
import { Entity, getEntityType } from "../EntitiesName";
import { useMutator } from "./useMutator";
import { Language } from "../api/Dtos/Language";
import { useModifierGroupsApi } from "../api/useModifierGroupsApi";
import { CreateModifierGroupRequest } from "../api/Dtos/modifierGroups/CreateModifierGroupRequest";
import { ModifierGroup } from "../api/Dtos/modifierGroups/ModifierGroup";
import { PatchModifierGroupTranslation, PatchModifierItem } from "../api/Dtos/modifierGroups/PatchModifierGroupRequest";

interface PatchMutator {
    readonly name?: string;
    readonly minSelection?: number;
    readonly maxSelection?: number;
    readonly items?: Record<string, PatchModifierItem | undefined>;
    readonly translations?: Record<Language, PatchModifierGroupTranslation> | undefined;
}
export const useModifierGroupMutator = () => {
    const api = useModifierGroupsApi();
    
    const createMutator = useMutator({
        entityType: getEntityType(Entity.ModifierGroups),
        getKey: (e: ModifierGroup) => e.id,
        updateCall: async (request: CreateModifierGroupRequest) => {
            const response = await api.create(request);
            return [response.data];
        }
    })

    const updateMutator = useMutator({
        entityType: getEntityType(Entity.ModifierGroups),
        getKey: (e: ModifierGroup) => e.id,
        updateCall: async (request: PatchMutator, entities: ModifierGroup[]) => {
            const result = [] as ModifierGroup[];

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
        entityType: getEntityType(Entity.ModifierGroups),
        getKey: (e: ModifierGroup) => e.id,
        updateCall: async (_: {}, entities: ModifierGroup[]) => {
            const result = [] as ModifierGroup[];
            
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
        create: async (request: CreateModifierGroupRequest) => {
            const result = await createMutator.mutate([], request);
            return result.response[0];
        },
        patch: async (e: ModifierGroup, mutator: PatchMutator) => {
            const result = await updateMutator.mutate([e], mutator);
            return result.response[0];
        },
        delete:  (e: ModifierGroup) => deleteMutator.mutate([e], {}),
    }), [api]);

    return result;
}