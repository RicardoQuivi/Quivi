import { useLoggedEmployee } from "../../../context/pos/LoggedEmployeeContextProvider";
import { Entity, getEntityType } from "../../EntitiesName";
import { QueryResult } from "../QueryResult";
import { useQueryable } from "../useQueryable";
import { BackgroundJob } from "../../api/Dtos/backgroundjobs/BackgroundJob";
import { useBackgroundJobsApi } from "../../api/useBackgroundJobsApi";

export const useBackgroundJobsQuery = (ids: string[] | undefined) : QueryResult<BackgroundJob[]> => {      
    const posContext = useLoggedEmployee();
    const api = useBackgroundJobsApi(posContext.token);

    const queryResult = useQueryable({
        queryName: "useBackgroundJobsQuery",
        entityType: getEntityType(Entity.BackgroundJobs),
        request: ids == undefined ? undefined : {
            ids: ids,
        },
        getId: (e: BackgroundJob) => e.id,
        query: async r => {
            if(r.ids.length == 0) {
                return {
                    data: [],
                };
            }

            const response = await api.get(r);
            return {
                data: response.data,
            }
        },
        refreshOnAnyUpdate: true,
    })

    return queryResult;
}