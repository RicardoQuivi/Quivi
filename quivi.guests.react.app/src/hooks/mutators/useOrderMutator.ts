import { useMemo } from "react";
import type { Order } from "../api/Dtos/orders/Order";
import { useOrdersApi } from "../api/useOrdersApi";
import { Entity, getEntityType } from "../EntitiesName";
import { useMutator } from "./useMutator";
import type { CreateOrderRequest } from "../api/Dtos/orders/CreateOrderRequest";
import type { UpdateOrderRequest } from "../api/Dtos/orders/UpdateOrderRequest";

interface SubmittedOrder extends Order {
    readonly jobId: string;
}
export const useOrderMutator = () => {
    const api = useOrdersApi();

    const createMutator = useMutator({
        entityType: getEntityType(Entity.Orders),
        getKey: (e: Order) => e.id,
        updateCall: async (request: CreateOrderRequest) => {
            const response = await api.create(request);
            return [response.data];
        }
    })

    const updateMutator = useMutator({
        entityType: getEntityType(Entity.Orders),
        getKey: (e: Order) => e.id,
        updateCall: async (request: UpdateOrderRequest, _) => {

            const result = [] as Order[];
            const response = await api.update(request);
            if(response.data != undefined) {
                result.push(response.data);
            }

            return result;
        }
    })

    const submitMutator = useMutator({
        entityType: getEntityType(Entity.Orders),
        getKey: (e: Order) => e.id,
        updateCall: async (orderId: string, _) => {
            const response = await api.submit({
                id: orderId,
            });
            return [{
                ...response.data,
                jobId: response.jobId,
            }];
        }
    })

    const result = useMemo(() => ({
        create: async (request: CreateOrderRequest) => {
            const result = await createMutator.mutate([], request);
            return result.response[0];
        },
        update: async (mutator: UpdateOrderRequest) => {
            const result = await updateMutator.mutate([], mutator);
            return result.response[0];
        },
        submit: async (orderId: string) => {
            const result = await submitMutator.mutate([], orderId);
            return {
                jobId: (result.response[0] as SubmittedOrder).jobId,
                order: result.response[0],
            }
        },
    }), [api]);
    
    return result;
}