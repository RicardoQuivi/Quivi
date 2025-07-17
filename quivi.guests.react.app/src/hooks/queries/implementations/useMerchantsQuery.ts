import type { Merchant } from "../../api/Dtos/merchants/Merchant";
import { useMerchantsApi } from "../../api/useMerchantsApi";
import { Entity, getEntityType } from "../../EntitiesName";
import type { QueryResult } from "../QueryResult";
import { useQueryable } from "../useQueryable";

export const useMerchantsQuery = (merchantId: string | undefined): QueryResult<Merchant[]> => {
    const api = useMerchantsApi();

    const query = useQueryable({
        queryName: "useMerchantsQuery",
        entityType: getEntityType(Entity.Merchants),
        getId: (item: Merchant) => item.id,
        request: merchantId == undefined ? undefined : {
            merchantId: merchantId,
        },
        query: async request => {
            const response = await api.get(request.merchantId);
            return {
                data: [response.data],
            }
        },
        getIdsFilter: (r) => [r.merchantId],
        
        refreshOnAnyUpdate: false,
        canUseOptimizedResponse: _ => true,
        getResponseFromEntities: (e) => ({
            data: e,
            page: 0,
            totalPages: 1,
            totalItems: 1,
        }),   
    })
    
    return query;
}