import { Entity, getEntityType } from "../../EntitiesName";
import { useQueryable } from "../useQueryable";
import type { QueryResult } from "../QueryResult";
import type { PostCheckoutLink } from "../../api/Dtos/postcheckoutlinks/PostCheckoutLink";

interface Props {
    readonly merchantId: string;
}
export const usePostCheckoutLinksQuery = (id: Props | undefined): QueryResult<PostCheckoutLink[]> => {
    const query = useQueryable({
        queryName: "usePostCheckoutLinksQuery",
        entityType: getEntityType(Entity.PostCheckoutLinks),
        getId: (item: PostCheckoutLink) => item.id,
        request: id == undefined ? undefined : {
            transactionId: id,
        },
        query: async _ => ({
            data: [],
        }),

        refreshOnAnyUpdate: true,
    })
    
    return query;
}