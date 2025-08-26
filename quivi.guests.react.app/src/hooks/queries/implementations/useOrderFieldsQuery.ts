import { Entity, getEntityType } from "../../EntitiesName";
import type { QueryResult } from "../QueryResult";
import type { OrderField } from "../../api/Dtos/orderFields/OrderField";
import { useQueryable } from "../useQueryable";
import { useOrderFieldsApi } from "../../api/useOrderFieldsApi";

interface Request {
    readonly channelId: string;
    readonly languageIso: string;
}
export const useOrderFieldsQuery = (request: Request | undefined): QueryResult<OrderField[]> => {
    const api = useOrderFieldsApi();
    
    const query = useQueryable({
        queryName: "useOrderFieldsQuery",
        entityType: getEntityType(Entity.OrderFields),
        getId: (item: OrderField) => item.id,
        request: request,
        query: r => api.get(r),
        refreshOnAnyUpdate: true,
    })
    
    return query;
}