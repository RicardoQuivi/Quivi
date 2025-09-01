export interface EndOfDayClosingRequest {
    readonly locationId?: string;
    readonly titleLabel: string;
    readonly printedByLabel: string;
    readonly locationLabel: string;
    readonly allLocationsLabel: string;
    readonly totalLabel: string;
    readonly amountLabel: string;
    readonly tipsLabel: string;
}