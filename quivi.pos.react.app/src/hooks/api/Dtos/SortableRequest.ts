export interface SortableRequest {
    readonly sortDirection: SortDirection;
}

export enum SortDirection {
    Asc = 0,
    Desc = 1
}