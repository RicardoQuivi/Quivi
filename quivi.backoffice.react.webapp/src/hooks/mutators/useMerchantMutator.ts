import { useMemo } from "react";
import { useAuthenticatedUser } from "../../context/AuthContext";
import { useMerchantsApi } from "../api/useMerchantsApi";
import { CreateMerchantRequest } from "../api/Dtos/merchants/CreateMerchantRequest";
import { Merchant } from "../api/Dtos/merchants/Merchant";
import { Entity, getEntityType } from "../EntitiesName";
import { useMutator } from "./useMutator";

interface PatchMutator {
    readonly name?: string;
    readonly iban?: string;
    readonly vatNumber?: string;
    readonly vatRate?: string;
    readonly postalCode?: string;
    readonly logoUrl?: string;
    readonly transactionFee?: number;
    readonly acceptTermsAndConditions?: boolean;
    readonly isDemo?: boolean;
    readonly inactive?: boolean;
}
export const useMerchantMutator = () => {
    const user = useAuthenticatedUser();
    const api = useMerchantsApi(user.token);
    
    const createMutator = useMutator({
        entityType: getEntityType(Entity.Merchants),
        getKey: (e: Merchant) => e.id,
        updateCall: async (request: CreateMerchantRequest) => {
            const result = [] as Merchant[];
            const response = await api.create(request);
            result.push(response.data);
            return result;
        }
    })

    const updateMutator = useMutator({
        entityType: getEntityType(Entity.Merchants),
        getKey: (e: Merchant) => e.id,
        updateCall: async (request: PatchMutator, entities: Merchant[]) => {
            const result = [] as Merchant[];
            
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

    const result = useMemo(() => ({
        create: async (request: CreateMerchantRequest) => {
            const result = await createMutator.mutate([], request);
            return result.response[0];
        },
        patch: async (e: Merchant, mutator: PatchMutator) => {
            const result = await updateMutator.mutate([e], mutator);
            return result.response[0];
        },
    }), [user, api]);

    return result;
}