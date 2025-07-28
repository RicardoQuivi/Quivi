export enum OrderState {
    ScheduledRequested = -3,
    Draft = -2,
    PendingApproval = -1,
    Accepted = 0,
    Rejected = 1,
    Processing = 2,
    Completed = 3,
    Scheduled = 4,
}