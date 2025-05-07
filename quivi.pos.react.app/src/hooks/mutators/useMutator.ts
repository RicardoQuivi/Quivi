import { Query, QueryKey, useMutation, useQueryClient } from "@tanstack/react-query";
import { EntityType } from "../queries/QueryKeys";

interface MutatorProps<TEntity, TRequest,> {
    readonly entityType: EntityType;
    readonly getKey: (r: TEntity) => string;
    readonly updateCall: (request: TRequest, entities: TEntity[]) => Promise<TEntity[]>;
    readonly keepDataAsOutdated?: boolean;
}

interface Aux<TEntity, TRequest,> {
    readonly entities: TEntity[];
    readonly request: TRequest;
    response: TEntity[];
}
export const useMutator = <TEntity, TRequest,>(props: MutatorProps<TEntity, TRequest>) => {
    const queryClient = useQueryClient();

    const mutation = useMutation({
        mutationFn: async (aux: Aux<TEntity, TRequest>) => aux,
        onSuccess: async (aux: Aux<TEntity, TRequest>) => {
            const request = aux.request;
            const result = await props.updateCall(request, aux.entities);
            aux.response = result;

            const resultMap = new Map<string, TEntity>();
            for(const e of result) {
                resultMap.set(props.getKey(e), e);
            }
            const entryMap = new Map<string, TEntity>();
            for(const e of aux.entities) {
                entryMap.set(props.getKey(e), e);
            }

            const updateQueriesResult = queryClient.setQueriesData({
                predicate: (q: Query) => {
                    if(q.meta == undefined) {
                        return false;
                    }

                    const entityType = q.meta["entityType"] as (EntityType | undefined);
                    if(entityType == undefined) {
                        return false;
                    }
                    return entityType == props.entityType;
                },
            }, (oldData: any) => {
                if(oldData != undefined && 'data' in oldData) {
                    try {
                        const oldEntities = oldData.data as TEntity[]; 
                        const newEntities: TEntity[] = [];
                        for(const e of oldEntities) {
                            const foundInEntry = entryMap.get(props.getKey(e));
                            const foundInResult = resultMap.get(props.getKey(e));
                            if(foundInEntry != undefined && foundInResult == undefined) {
                                //the entry was deleted, do not push it into the result
                                continue;
                            }

                            if(foundInResult != undefined) {
                                newEntities.push(foundInResult);
                                continue;
                            }

                            newEntities.push(e);
                        }
                        return {
                            ...oldData,
                            data: newEntities,
                        };
                    }
                    catch {
                        return oldData;
                    }
                }
                return oldData;
            });

            if(props.keepDataAsOutdated != true) {
                return;
            }
            
            const affectedKeysMap = updateQueriesResult.reduce((r, e) => {
                r.add(e[0]);
                return r;
            }, new Set<QueryKey>())

            await queryClient.invalidateQueries({
                predicate: q => affectedKeysMap.has(q.queryKey),
            })
        }
    })

    return {
        mutate: (e: TEntity[], request: TRequest) => mutation.mutateAsync({
            entities: e,
            request: request,
            response: [],
        } as Aux<TEntity, TRequest>),
    }
}