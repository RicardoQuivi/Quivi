export interface UpsertSessionAdditionalInformationsRequest {
    readonly sessionId: string;
    readonly fields: Record<string, string>;
}