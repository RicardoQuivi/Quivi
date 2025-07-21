import { useMemo } from "react";
import { useAuthenticatedUser } from "../../context/AuthContext";
import { Entity, getEntityType } from "../EntitiesName";
import { useMutator } from "./useMutator";
import { CustomChargeMethod } from "../api/Dtos/customchargemethods/CustomChargeMethod";
import { useCustomChargeMethodsApi } from "../api/useCustomChargeMethodsApi";
import { CreateCustomChargeMethodRequest } from "../api/Dtos/customchargemethods/CreateCustomChargeMethodRequest";

interface PatchMutator {
    readonly name?: string;
    readonly logoUrl?: string | null;
}
export const useCustomChargeMethodMutator = () => {
    const user = useAuthenticatedUser();
    const api = useCustomChargeMethodsApi(user.token);
    
    const createMutator = useMutator({
        entityType: getEntityType(Entity.CustomChargeMethods),
        getKey: (e: CustomChargeMethod) => e.id,
        updateCall: async (request: CreateCustomChargeMethodRequest) => {
            const response = await api.create(request);
            return [response.data];
        }
    })

    const updateMutator = useMutator({
        entityType: getEntityType(Entity.CustomChargeMethods),
        getKey: (e: CustomChargeMethod) => e.id,
        updateCall: async (request: PatchMutator, entities: CustomChargeMethod[]) => {
            const result = [] as CustomChargeMethod[];

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
        entityType: getEntityType(Entity.CustomChargeMethods),
        getKey: (e: CustomChargeMethod) => e.id,
        updateCall: async (_: {}, entities: CustomChargeMethod[]) => {
            const result = [] as CustomChargeMethod[];
            
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
        create: async (request: CreateCustomChargeMethodRequest) => {
            const result = await createMutator.mutate([], request);
            return result.response;
        },
        patch: async (e: CustomChargeMethod, mutator: PatchMutator) => {
            const result = await updateMutator.mutate([e], mutator);
            return result.response[0];
        },
        delete:  (e: CustomChargeMethod) => deleteMutator.mutate([e], {}),
    }), [user, api]);

    return result;
}