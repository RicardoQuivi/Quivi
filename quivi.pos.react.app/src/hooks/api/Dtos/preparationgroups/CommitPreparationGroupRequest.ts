export interface CommitPreparationGroupRequest {
    readonly id: string;
    readonly isPrepared?: boolean;
    readonly note?: string;
    readonly itemsToCommit?: Record<string, number>;
    readonly locationId?: string;
}