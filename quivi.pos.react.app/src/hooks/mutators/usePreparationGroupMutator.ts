import { useMemo } from "react";
import { Entity, getEntityType } from "../EntitiesName";
import { useMutator } from "./useMutator";
import { usePreparationGroupsApi } from "../api/usePreparationGroupsApi";
import { PreparationGroup } from "../api/Dtos/preparationgroups/PreparationGroup";

interface CommitMutator {
    readonly isPrepared?: boolean;
    readonly note?: string;
    readonly itemsToCommit?: Record<string, number>;
    readonly locationId?: string;
}

interface PatchMutator {
    readonly items: Record<string, number>;
}

interface PrintMutator {
    readonly locationId?: string;
}

export const usePreparationGroupMutator = () => {
    const api = usePreparationGroupsApi();

    const patchMutator = useMutator({
        entityType: getEntityType(Entity.PreparationGroups),
        getKey: (r: PreparationGroup) => r.id,
        updateCall: async (request: PatchMutator, entities: PreparationGroup[]) => {
            const result = [] as PreparationGroup[];
            //NOTE: Current implementation limits entities to an array of a single entry.
            for(const entity of entities) {
                const response = await api.patch({
                    ...request,
                    id: entity.id,
                    items: request.items,
                });
                
                result.push(response.data);
            }
            return result;
        },
        keepDataAsOutdated: true,
    })

    const result = useMemo(() => ({
        commit: async (entity: PreparationGroup, request: CommitMutator) => {
            const response = await api.commit({
                ...request,
                id: entity.id,
            });
            return response.jobId;
        },
        patch: (e: PreparationGroup, mutator: PatchMutator) => patchMutator.mutate([e], mutator),
        print: async (entity: PreparationGroup, request: PrintMutator) => {
            await api.print({
                id: entity.id,
                locationId: request.locationId,
            });
        },
    }), [api]);

    return result;
}