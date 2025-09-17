export interface OnPosChargeSyncAttemptEvent {
    readonly merchantId: string;
    readonly id: string;
    readonly posChargeId: string;
}