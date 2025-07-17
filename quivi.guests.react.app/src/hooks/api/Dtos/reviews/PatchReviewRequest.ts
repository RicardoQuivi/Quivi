export interface PatchReviewRequest {
    readonly transactionId: string;
    readonly stars: number;
    readonly comment?: string;
}