import { GetLocalsRequest } from "../../api/Dtos/locals/GetLocalsRequest";
import { Local } from "../../api/Dtos/locals/Local";
import { useLocalsApi } from "../../api/useLocalsApi";
import { Entity, getEntityType } from "../../EntitiesName";
import { QueryResult } from "../QueryResult";
import { useQueryable } from "../useQueryable";

export const useLocalsQuery = (request: GetLocalsRequest | undefined) : QueryResult<Local[]> => {       
    const api = useLocalsApi();

    const queryResult = useQueryable({
        queryName: "useLocalsQuery",
        entityType: getEntityType(Entity.Locals),
        request: request,
        getId: (e: Local) => e.id,
        query: async r => {
            const response = await api.get(r);
            return response;
        },
        refreshOnAnyUpdate: true,
    })
    
    return queryResult;
}