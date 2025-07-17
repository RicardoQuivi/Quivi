import { useMemo } from "react";
import { Entity, getEntityType } from "../EntitiesName";
import { useMutator } from "./useMutator";
import { useReviewsApi } from "../api/useReviewsApi";
import type { PatchReviewRequest } from "../api/Dtos/reviews/PatchReviewRequest";
import type { Review } from "../api/Dtos/reviews/Review";

export const useReviewMutator = () => {
    const api = useReviewsApi();

    const updateMutator = useMutator({
        entityType: getEntityType(Entity.Reviews),
        getKey: (e: Review) => e.id,
        updateCall: async (request: PatchReviewRequest, _) => {
            const result = [] as Review[];
            const response = await api.patch(request);
            result.push(response.data);
            return result;
        },
    })

    const result = useMemo(() => ({
        update: async (mutator: PatchReviewRequest) => {
            const result = await updateMutator.mutate([], mutator);
            return result.response[0];
        },
    }), [api]);
    
    return result;
}