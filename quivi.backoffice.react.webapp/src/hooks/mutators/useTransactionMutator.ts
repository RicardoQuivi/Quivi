import { useMemo } from "react";
import { useTransactionApi } from "../api/useTransactionsApi";
import { Transaction } from "../api/Dtos/transactions/Transaction";
import { useMutator } from "./useMutator";
import { Entity, getEntityType } from "../EntitiesName";

interface RefundMutator {
    readonly amount?: number;
    readonly cancelation: boolean;
}
export const useTransactionMutator = () => {
    const api = useTransactionApi();

    const updateMutator = useMutator({
        entityType: getEntityType(Entity.Transactions),
        getKey: (e: Transaction) => e.id,
        updateCall: async (request: RefundMutator, entities: Transaction[]) => {
            const result = [] as Transaction[];

            //NOTE: Current implementation limits entities to an array of a single entry.
            // Nevertheless, the above implementation works even if we would allow several.
            for(const entity of entities) {
                const response = await api.refund({
                    id: entity.id,
                    amount: request.amount,
                    cancelation: request.cancelation,
                });
                
                if(response.data != undefined) {
                    result.push(response.data);
                }
            }
            return result;
        }
    })

    const result = useMemo(() => ({
        refund: async (e: Transaction, mutator: RefundMutator) => {
            const result = await updateMutator.mutate([e], mutator);
            return result.response[0];
        },
    }), [api]);

    return result;
}