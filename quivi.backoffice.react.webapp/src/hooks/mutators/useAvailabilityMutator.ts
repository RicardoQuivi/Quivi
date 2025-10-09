import { useMemo } from "react";
import { Entity, getEntityType } from "../EntitiesName";
import { useMutator } from "./useMutator";
import { useAvailabilitiesApi } from "../api/useAvailabilitiesApi";
import { CreateAvailabilityRequest } from "../api/Dtos/availabilities/CreateAvailabilityRequest";
import { Availability } from "../api/Dtos/availabilities/Availability";
import { WeeklyAvailability } from "../api/Dtos/availabilities/WeeklyAvailability";

interface PatchMutator {
    readonly name?: string;
    readonly autoAddNewChannelProfiles?: boolean;
    readonly autoAddNewMenuItems?: boolean;
    readonly weeklyAvailabilities?: WeeklyAvailability[];
}
export const useAvailabilityMutator = () => {
    const api = useAvailabilitiesApi();
    
    const createMutator = useMutator({
        entityType: getEntityType(Entity.Availabilities),
        getKey: (e: Availability) => e.id,
        updateCall: async (request: CreateAvailabilityRequest) => {
            const response = await api.create(request);
            return [response.data];
        }
    })

    const updateMutator = useMutator({
        entityType: getEntityType(Entity.Availabilities),
        getKey: (e: Availability) => e.id,
        updateCall: async (request: PatchMutator, entities: Availability[]) => {
            const result = [] as Availability[];

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
        entityType: getEntityType(Entity.Availabilities),
        getKey: (e: Availability) => e.id,
        updateCall: async (_: {}, entities: Availability[]) => {
            const result = [] as Availability[];
            
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
        create: async (request: CreateAvailabilityRequest) => {
            const result = await createMutator.mutate([], request);
            return result.response;
        },
        patch: async (e: Availability, mutator: PatchMutator) => {
            const result = await updateMutator.mutate([e], mutator);
            return result.response[0];
        },
        delete:  (e: Availability) => deleteMutator.mutate([e], {}),
    }), [api]);

    return result;
}