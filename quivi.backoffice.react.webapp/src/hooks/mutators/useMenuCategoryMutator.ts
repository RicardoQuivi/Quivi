import { useMemo } from "react";
import { useAuthenticatedUser } from "../../context/AuthContext";
import { CreateMenuCategoryRequest } from "../api/Dtos/menuCategories/CreateMenuCategoryRequest";
import { MenuCategory } from "../api/Dtos/menuCategories/MenuCategory";
import { useMenuCategoriesApi } from "../api/useMenuCategoriesApi";
import { Entity, getEntityType } from "../EntitiesName";
import { useMutator } from "./useMutator";
import { Language } from "../api/Dtos/Language";
import { PatchMenuCategoryTranslation } from "../api/Dtos/menuCategories/PatchMenuCategoryRequest";

interface PatchMutator {
    readonly name?: string;
    readonly imageUrl?: string | null;
    readonly translations?: Record<Language, PatchMenuCategoryTranslation | undefined>;
}
export const useMenuCategoryMutator = () => {
    const user = useAuthenticatedUser();
    const api = useMenuCategoriesApi(user.token);
    
    const createMutator = useMutator({
        entityType: getEntityType(Entity.MenuCategories),
        getKey: (e: MenuCategory) => e.id,
        updateCall: async (request: CreateMenuCategoryRequest) => {
            const response = await api.create(request);
            return [response.data];
        }
    })

    const updateMutator = useMutator({
        entityType: getEntityType(Entity.MenuCategories),
        getKey: (e: MenuCategory) => e.id,
        updateCall: async (request: PatchMutator, entities: MenuCategory[]) => {
            const result = [] as MenuCategory[];

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
        entityType: getEntityType(Entity.MenuCategories),
        getKey: (e: MenuCategory) => e.id,
        updateCall: async (_: {}, entities: MenuCategory[]) => {
            const result = [] as MenuCategory[];
            
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

    const sortMutator = useMutator({
        entityType: getEntityType(Entity.MenuCategories),
        getKey: (e: MenuCategory) => e.id,
        updateCall: async (_: {}, entities: MenuCategory[]) => {
            const response = await api.sort({
                items: entities.map((item, index) => ({
                    id: item.id,
                    sortIndex: index,
                }))
            });
            return response.data;
        }
    })
    
    const result = useMemo(() => ({
        create: async (request: CreateMenuCategoryRequest) => {
            const result = await createMutator.mutate([], request);
            return result.response[0];
        },
        patch: async (e: MenuCategory, mutator: PatchMutator) => {
            const result = await updateMutator.mutate([e], mutator);
            return result.response[0];
        },
        delete:  (e: MenuCategory) => deleteMutator.mutate([e], {}),
        sort:  (entities: MenuCategory[]) => sortMutator.mutate(entities, {}),
    }), [user, api]);

    return result;
}