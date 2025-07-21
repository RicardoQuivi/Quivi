import { useMemo } from "react";
import { useAuthenticatedUser } from "../../context/AuthContext";
import { Entity, getEntityType } from "../EntitiesName";
import { useMutator } from "./useMutator";
import { Employee, EmployeeRestriction } from "../api/Dtos/employees/Employee";
import { CreateEmployeeRequest } from "../api/Dtos/employees/CreateEmployeeRequest";
import { useEmployeesApi } from "../api/useEmployeesApi";

interface PatchMutator {
    readonly name?: string;
    readonly inactivityLogoutTimeout?: string | null;
    readonly restrictions?: EmployeeRestriction[];
}
export const useEmployeeMutator = () => {
    const user = useAuthenticatedUser();
    const api = useEmployeesApi(user.token);
    
    const createMutator = useMutator({
        entityType: getEntityType(Entity.Employees),
        getKey: (e: Employee) => e.id,
        updateCall: async (request: CreateEmployeeRequest) => {
            const response = await api.create(request);
            return [response.data];
        }
    })

    const updateMutator = useMutator({
        entityType: getEntityType(Entity.Employees),
        getKey: (e: Employee) => e.id,
        updateCall: async (request: PatchMutator, entities: Employee[]) => {
            const result = [] as Employee[];

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
        entityType: getEntityType(Entity.Employees),
        getKey: (e: Employee) => e.id,
        updateCall: async (_: {}, entities: Employee[]) => {
            const result = [] as Employee[];
            
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

    const resetPinMutator = useMutator({
        entityType: getEntityType(Entity.Employees),
        getKey: (e: Employee) => e.id,
        updateCall: async (_: {}, entities: Employee[]) => {
            const result = [] as Employee[];

            //NOTE: Current implementation limits entities to an array of a single entry.
            // Nevertheless, the above implementation works even if we would allow several.
            for(const entity of entities) {
                const response = await api.resetPinCode({
                    id: entity.id,
                });
                
                if(response.data != undefined) {
                    result.push(response.data);
                }
            }
            return result;
        }
    })

    const result = useMemo(() => ({
        create: async (request: CreateEmployeeRequest) => {
            const result = await createMutator.mutate([], request);
            return result.response;
        },
        patch: async (e: Employee, mutator: PatchMutator) => {
            const result = await updateMutator.mutate([e], mutator);
            return result.response[0];
        },
        delete:  (e: Employee) => deleteMutator.mutate([e], {}),
        resetPin:  (e: Employee) => resetPinMutator.mutate([e], {}),
    }), [user, api]);

    return result;
}