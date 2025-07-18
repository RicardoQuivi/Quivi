import { useQueryable } from "../useQueryable";
import { useAuth } from "../../../context/AuthContext";
import { usePublicIdsApi } from "../../api/usePublicIdsApi";
import { useMemo } from "react";

export const usePublicIdQuery = (id: string | undefined) => {
    const auth = useAuth();
    const api = usePublicIdsApi(auth.token);

    const queryResult = useQueryable({
        queryName: "useLocalsQuery",
        entityType: "PublicId",
        request: auth.token == undefined || auth.isAdmin == false || id == undefined ? undefined : {
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