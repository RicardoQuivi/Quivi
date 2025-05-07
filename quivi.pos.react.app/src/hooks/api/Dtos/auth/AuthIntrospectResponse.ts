export interface AuthIntrospectResponse {
    readonly active: boolean;
    readonly iss: string;
    readonly sub: string;
    readonly jti: string;
    readonly token_type: string;
    readonly client_id: string;
    readonly iat: number;
    readonly nbf: number;
    readonly exp: number;

    readonly merchant_id?: string;
    readonly sub_merchant_id?: string;
}