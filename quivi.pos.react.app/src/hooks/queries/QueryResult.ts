export interface QueryResult<T> {
    readonly isLoading: boolean;
    readonly isFirstLoading: boolean;
    readonly data: T;
}

export interface PagedQueryResult<T> extends QueryResult<T[]> {
    readonly page: number;
    readonly totalPages: number;
    readonly totalItems: number;
}