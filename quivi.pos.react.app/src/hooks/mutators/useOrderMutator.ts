import { useMemo } from "react";
import { Entity, getEntityType } from "../EntitiesName";
import { useMutator } from "./useMutator";
import { useOrdersApi } from "../api/useOrdersApi";
import { Order } from "../api/Dtos/orders/Order";

interface ProcessProps {
    readonly completeOrder?: boolean;
}

interface DeclineProps {
    readonly reason?: string;
}

interface ExtendedOrder extends Order {
    readonly jobId: string;
}
export const useOrderMutator = () => {
    const api = useOrdersApi();

    const processMutator = useMutator<Order, ProcessProps>({
        entityType: getEntityType(Entity.Orders),
        getKey: (e: Order) => e.id,
        updateCall: async (request: ProcessProps, orders: Order[]) => {
            const result = [] as ExtendedOrder[];
            for(const o of orders) {
                const response = await api.processTo({
                    id: o.id,
                    state: o.state,
                    ...request,
                });
                result.push({
                    ...o,
                    jobId: response.data,
                })
            }
            return result;
        }
    })

    const declineMutator = useMutator<Order, DeclineProps>({
        entityType: getEntityType(Entity.Orders),
        getKey: (e: Order) => e.id,
        updateCall: async (request: DeclineProps, orders: Order[]) => {
            const result = [] as ExtendedOrder[];
            for(const o of orders) {
                const response = await api.decline({
                    id: o.id,
                    ...request,
                });
                result.push({
                    ...o,
                    jobId: response.data,
                })
            }
            return result;
        }
    })

    const result = useMemo(() => ({
        process: async (order: Order, request: ProcessProps) => {
            const result = await processMutator.mutate([order], request);
            return (result.response[0] as ExtendedOrder).jobId;
        },
        decline: async (order: Order, request: DeclineProps) => {
            const result = await declineMutator.mutate([order], request);
            return (result.response[0] as ExtendedOrder).jobId;
        },
    }), [api]);

    return result;
}