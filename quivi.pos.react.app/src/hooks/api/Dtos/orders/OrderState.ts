export enum OrderState
{
    Draft = -1,
    Requested = 0,
    Rejected = 1,
    Processing = 2,
    Completed = 3,
    ScheduledRequested = -2,
    Scheduled = 4,
}