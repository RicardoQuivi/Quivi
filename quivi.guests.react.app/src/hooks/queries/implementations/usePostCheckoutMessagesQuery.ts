import { Entity, getEntityType } from "../../EntitiesName";
import { useQueryable } from "../useQueryable";
import type { QueryResult } from "../QueryResult";
import type { PostCheckoutMessage } from "../../api/Dtos/postcheckoutmessages/PostCheckoutMessage";

interface Props {
    readonly merchantId: string;
}
export const usePostCheckoutMessagesQuery = (id: Props | undefined): QueryResult<PostCheckoutMessage[]> => {
    const query = useQueryable({
        queryName: "usePostCheckoutMessagesQuery",
        entityType: getEntityType(Entity.PostCheckoutMessages),
        getId: (item: PostCheckoutMessage) => item.id,
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