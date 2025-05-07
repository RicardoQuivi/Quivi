import { QueryClient, QueryKey, QueryMeta, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from "react";
import { QueryResult } from './QueryResult';
import { EntityType, getKey } from './QueryKeys';

const emptyResponse: unknown[] = [];
interface IQueryableResult<TEntity, TResponse extends IResponse<TEntity>> extends QueryResult<TEntity[]> {
    readonly response: TResponse | undefined;
}

export interface IResponse<TEntity> {
    readonly data: TEntity[];
}

interface Props<TRequest, TEntity, TResponse extends IResponse<TEntity>> {
    /** The unique name for the query. This is usefull when you need to use the same entityType for more than one query. */
    readonly queryName: string;

    // The request object. If undefined is pass, the query will return no data and 
    //will be with both "firstLoading" and "loading".
    readonly request: TRequest | undefined;

    //The entity this query relies on. This query will (potentially) be re-fetche whenever
    //a related event of the provided entity type happens.
    readonly entityType: EntityType;

    //How to obtain the key of the related entity. The query will bound to the specified Id
    //of the specified entity type. For instance, if entityType is set with "DigitalMenuItem"
    //and this query has fetched an entity with Id "1234", then this query will be refreshed
    //If any event (Added, Changed, Deleted, etc) related to DigitalMenuItem "1234" happens,
    //then this query will re-fetch
    readonly getId?: (r: TEntity) => string | undefined;

    //The function on how to retrieve the data with the provided request.
    readonly query: (r: TRequest) => Promise<TResponse>;

    //A function to retrieve the Ids (if applicable) related to the entityType passed.
    //For instance, if a filter asks some data for the entityType DigitalMenuItem "1234",
    //Even this entity does not return that entity, it will still be bound to the events
    //related to DigitalMenuItem "1234".
    readonly getIdsFilter?: (r: TRequest) => string[] | undefined;

    //A boolean stating whether this query shoudl re-fetch on any kind of event related 
    //to entityType. If true, even if the query isn't specified to the event Entity Id,
    //it will still be fetched. For instance, if the query is not bound to DigitalMenuItem "1234"
    //and an evenmt related to that specific Id happens, then this query will re-fetch
    readonly refreshOnAnyUpdate?: boolean;

    //Used for constructing responses. Only useful in case canUseOptimizedResponse is also set.
    //This method is called whenever it needs to construct responses for caching optimizations.
    //For instance, if a query retrieves entity with ids 2, 3 and 6, this method will be called
    //to cache response for any request that requests either entity with id 2, 3 or 6, or any combination
    //between the three values.
    readonly getResponseFromEntities?: (entities: TEntity[]) => TResponse;

    //A function returning true or false for a particular request. This function will be used
    //to know if, for a particular request, it can use individual entities previously cached.
    //For isntance, if we are retrieving entities 2, 3 and 6, we can probably avoid a call to the
    //API if we already have all those entities in memory. If canUseOptimizedResponse return true,
    //then we will check if those entities were already returned in other queries and use that as
    //result instead of calling API. If it return false, then it will procceed as normally.
    readonly canUseOptimizedResponse?: (request: TRequest) => boolean;
}

const optimizedKey = "optimization-enabled";
const undefinedRequest = null; //tanStack doesn't allow undefined in query key, so we use null instead.
interface QueryContext<TRequest> {
    readonly queryKey: (string | TRequest | undefined)[];
    readonly meta: QueryMeta | undefined;
}
const getFreshQueryData = <T>(queryClient: QueryClient, key: QueryKey): T | undefined => {
    const query = queryClient.getQueryCache().find({
        queryKey: key,
    });
    return query?.isStale() === false ? query.state.data as T : undefined;
}

export const useQueryable = <TRequest, TEntity, TResponse extends IResponse<TEntity>>(props: Props<TRequest, TEntity, TResponse>): IQueryableResult<TEntity, TResponse> => {
    const queryClient = useQueryClient();

    const queryKey = [getKey(props.entityType), props.request ?? undefinedRequest, props.queryName];

    const getOptimizedQueryKey = (id: string) => [getKey(props.entityType), props.queryName, getKey(props.entityType, id), optimizedKey];

    const getMeta = (refreshOnAnyUpdate?: boolean) => ({
        entityType: props.entityType,
        ids: new Set<string>(),
        refreshOnAnyUpdate: refreshOnAnyUpdate,
    })

    const queryFn = async (c: QueryContext<TRequest | null>, getData: (r: TRequest) => Promise<TResponse>) => {
        if (props.request == undefined) {
            return null;
        }
        
        const idsIndexer = c.meta!["ids"] as Set<string>;
        const isOptimizedResponse = c.queryKey.includes(optimizedKey) == true;
        const ids = props.getIdsFilter?.(props.request);
        if(ids != undefined && isOptimizedResponse == false) {
            for(const id of ids) {
                idsIndexer.add(getKey(props.entityType, id))
            }
        }

        const canUseOptimizedResponse = props.getResponseFromEntities != undefined && (props.canUseOptimizedResponse?.(props.request) ?? false);
        let isUsingOptimizedResponse = false;
        let response: TResponse | undefined = undefined;
        if(isOptimizedResponse == false && canUseOptimizedResponse && ids != undefined) {
            const optimizedResults = [] as TEntity[];
            let canUse = true;
            for(const id of ids) {
                const optimizedKey = getOptimizedQueryKey(id);
                const optimizedResult = getFreshQueryData<TResponse>(queryClient, optimizedKey);
                if(optimizedResult == undefined) {
                    canUse = false;
                    break;
                }
                for(const r of optimizedResult.data) {
                    optimizedResults.push(r);
                }
            }
            if(canUse) {
                console.debug("Using cached entities ", optimizedResults, " for request ", props.request)
                response = props.getResponseFromEntities(optimizedResults);
                isUsingOptimizedResponse = true;
            }
        }
        
        if(response == undefined) {
            response = await getData(props.request);
        }

        if(isUsingOptimizedResponse == false && props.getId != undefined) {
            for(const item of response.data) {
                const id = props.getId(item);
                if(id == undefined) {
                    continue;
                }

                idsIndexer.add(getKey(props.entityType, id));
                
                if(isOptimizedResponse) {
                    continue;
                }
                
                if(props.getResponseFromEntities == undefined) {
                    continue;
                }
                
                const optimizedResponse = props.getResponseFromEntities([item]);
                const optimizedQueryKey = getOptimizedQueryKey(id);
                queryClient.fetchQuery({
                    queryKey: optimizedQueryKey,
                    queryFn: c => queryFn(c, async () => optimizedResponse),
                    meta: getMeta(false),
                })
            }
        }

        return response;
    }

    const { isLoading, isFetching, data: response, } = useQuery({
        queryKey: queryKey,
        queryFn: c => queryFn(c, props.query),
        meta: getMeta(props.refreshOnAnyUpdate),
    })
    
    const [result, setResult] = useState(() => ({
        isFirstLoading: response === undefined || props.request == undefined,
        isLoading: isLoading || isFetching,
        data: response?.data ?? (emptyResponse as TEntity[]),
        response: response === null ? undefined : response,
    }))
    
    useEffect(() => setResult(r => {
        const firstLoading = response === undefined || props.request == undefined;
        const loading = isLoading || isFetching;
        const data = response?.data ?? (emptyResponse as TEntity[]);
        const dataResponse = response === null ? undefined : response;

        let isDifferent = false;
        if(r.isFirstLoading != firstLoading) {
            isDifferent = true;
        }
        if(r.isLoading != loading) {
            isDifferent = true;
        }
        if(r.data != data) {
            isDifferent = true;
        }
        if(r.response != dataResponse) {
            isDifferent = true;
        }

        if(isDifferent == false) {
            return r;
        }

        return {
            isFirstLoading: firstLoading,
            isLoading: loading,
            data: data,
            response: dataResponse,
        }
    }), [isLoading, isFetching, response, props.entityType, props.request == undefined, props.queryName]);

    return result;
}