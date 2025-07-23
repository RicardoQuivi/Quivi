import { useQueryable } from "../useQueryable";
import { useAuthenticatedUser } from "../../../context/AuthContext";
import { usePublicIdsApi } from "../../api/usePublicIdsApi";
import { useMemo } from "react";

export const usePublicIdQuery = (id: string | undefined) => {
    const user = useAuthenticatedUser();
    const api = usePublicIdsApi();

    const queryResult = useQueryable({
        queryName: "useLocalsQuery",
        entityType: "PublicId",
        request: user.isAdmin == false || id == undefined ? undefined : {
            id: id,
        },
        getId: (e: number) => e.toString(),
        query: async request => {
            const result = [] as number[];
            try  {
                const r = await api.get(request.id);
                result.push(r.data);
            } catch {
            }

            return {
                data: result,
            };
        },
        refreshOnAnyUpdate: false,     
    })

    const result = useMemo(() => ({
        isFirstLoading: queryResult.isFirstLoading,
        isLoading: queryResult.isLoading,
        data: queryResult.data.length == 0 ? undefined : queryResult.data[0],
    }), [queryResult])

    return result;
}