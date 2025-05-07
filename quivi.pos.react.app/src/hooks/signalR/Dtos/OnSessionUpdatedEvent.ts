export interface OnSessionUpdatedEvent {
    readonly merchantId: string;
    readonly channelId: string;
    readonly id: string;
}