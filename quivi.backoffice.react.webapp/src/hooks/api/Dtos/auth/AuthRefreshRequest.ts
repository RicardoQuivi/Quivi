export interface AuthRefreshRequest {
    readonly merchantId?: string;
    readonly refreshToken: string;
}