import { useMemo } from "react";
import { Entity, getEntityType } from "../EntitiesName";
import { useMutator } from "./useMutator";
import { useConfigurableFieldsApi } from "../api/useConfigurableFieldsApi";
import { ConfigurableField } from "../api/Dtos/configurableFields/ConfigurableField";
import { CreateConfigurableFieldRequest } from "../api/Dtos/configurableFields/CreateConfigurableFieldRequest";
import { ConfigurableFieldType } from "../api/Dtos/configurableFields/ConfigurableFieldType";
import { PrintedOn } from "../api/Dtos/configurableFields/PrintedOn";
import { AssignedOn } from "../api/Dtos/configurableFields/AssignedOn";
import { Language } from "../api/Dtos/Language";
import { PatchConfigurableFieldTranslation } from "../api/Dtos/configurableFields/PatchConfigurableFieldRequest";

interface PatchMutator {
    readonly name?: string;
    readonly type?: ConfigurableFieldType;
    readonly isRequired?: boolean;
    readonly isAutoFill?: boolean;
    readonly printedOn?: PrintedOn;
    readonly assignedOn?: AssignedOn;
    readonly defaultValue?: string;
    readonly translations?: Record<Language, PatchConfigurableFieldTranslation | undefined>;
}
export const useConfigurableFieldMutator = () => {
    const api = useConfigurableFieldsApi();
    
    const createMutator = useMutator({
        entityType: getEntityType(Entity.ConfigurableFields),
        getKey: (e: ConfigurableField) => e.id,
        updateCall: async (request: CreateConfigurableFieldRequest) => {
            const result = [] as ConfigurableField[];
            const response = await api.create(request);
            result.push(response.data);
            return result;
        }
    })

    const updateMutator = useMutator({
        entityType: getEntityType(Entity.ConfigurableFields),
        getKey: (e: ConfigurableField) => e.id,
        updateCall: async (request: PatchMutator, entities: ConfigurableField[]) => {
            const result = [] as ConfigurableField[];
            
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
        entityType: getEntityType(Entity.ConfigurableFields),
        getKey: (e: ConfigurableField) => e.id,
        updateCall: async (_: {}, entities: ConfigurableField[]) => {
            const result = [] as ConfigurableField[];
            
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
        create: async (request: CreateConfigurableFieldRequest) => {
            const result = await createMutator.mutate([], request);
            return result.response[0];
        },
        patch: async (e: ConfigurableField, mutator: PatchMutator) => {
            const result = await updateMutator.mutate([e], mutator);
            return result.response[0];
        },
        delete:  (e: ConfigurableField) => deleteMutator.mutate([e], {}),
    }), [api]);

    return result;
}