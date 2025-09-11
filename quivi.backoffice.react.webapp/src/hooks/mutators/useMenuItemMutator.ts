import { useMemo } from "react";
import { Entity, getEntityType } from "../EntitiesName";
import { useMutator } from "./useMutator";
import { useMenuItemsApi } from "../api/useMenuItemsApi";
import { CreateMenuItemRequest } from "../api/Dtos/menuItems/CreateMenuItemRequest";
import { MenuItem } from "../api/Dtos/menuItems/MenuItem";
import { PriceType } from "../api/Dtos/menuItems/PriceType";
import { Language } from "../api/Dtos/Language";
import { PatchMenuItemTranslation } from "../api/Dtos/menuItems/PatchMenuItemRequest";

interface PatchMutator {
    readonly name?: string;
    readonly description?: string | null;
    readonly imageUrl?: string | null;
    readonly price?: number;
    readonly priceType?: PriceType;
    readonly vatRate?: number;
    readonly locationId?: string | null;
    readonly translations?: Record<Language, PatchMenuItemTranslation | undefined>;
    readonly menuCategoryIds?: string[];
    readonly modifierGroupIds?: string[];
}
export const useMenuItemMutator = () => {
    const api = useMenuItemsApi();
    
    const createMutator = useMutator({
        entityType: getEntityType(Entity.MenuItems),
        getKey: (e: MenuItem) => e.id,
        updateCall: async (request: CreateMenuItemRequest) => {
            const response = await api.create(request);
            return [response.data];
        }
    })

    const updateMutator = useMutator({
        entityType: getEntityType(Entity.MenuItems),
        getKey: (e: MenuItem) => e.id,
        updateCall: async (request: PatchMutator, entities: MenuItem[]) => {
            const result = [] as MenuItem[];

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
        entityType: getEntityType(Entity.MenuItems),
        getKey: (e: MenuItem) => e.id,
        updateCall: async (_: {}, entities: MenuItem[]) => {
            const result = [] as MenuItem[];
            
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
        create: async (request: CreateMenuItemRequest) => {
            const result = await createMutator.mutate([], request);
            return result.response;
        },
        patch: async (e: MenuItem, mutator: PatchMutator) => {
            const result = await updateMutator.mutate([e], mutator);
            return result.response[0];
        },
        delete:  (e: MenuItem) => deleteMutator.mutate([e], {}),
    }), [api]);

    return result;
}