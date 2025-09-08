import { useMemo } from "react";
import { Entity, getEntityType } from "../EntitiesName";
import { useMutator } from "./useMutator";
import { useMenuItemsApi } from "../api/useMenuItemsApi"
import { MenuItem } from "../api/Dtos/menuitems/MenuItem";

interface Request {
    readonly itemsWithStock: MenuItem[];
    readonly itemsWithoutStock: MenuItem[];
}
export const useMenuItemMutator = () => {
    const api = useMenuItemsApi();

    const stockMutator = useMutator({
        entityType: getEntityType(Entity.MenuItems),
        getKey: (r: MenuItem) => r.id,
        updateCall: async (request: Request) => {
            const stockMap: Record<string, boolean> = {};

            for(const item of request.itemsWithStock) {
                stockMap[item.id] = true;
            }

            for(const item of request.itemsWithoutStock) {
                stockMap[item.id] = false;
            }

            const response = await api.patchStock({
                stockMap: stockMap,
            })

            return response.data;
        },
    })

    const result = useMemo(() => ({
        updateStock: (itemsWithStock: MenuItem[], itemsWithoutStock: MenuItem[]) => {
            return stockMutator.mutate([...itemsWithStock, ...itemsWithoutStock], {
                itemsWithStock,
                itemsWithoutStock,
            } as Request)
        },
    }), [api]);

    return result;
}