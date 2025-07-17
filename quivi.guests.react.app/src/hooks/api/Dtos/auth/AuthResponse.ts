export interface AuthResponse {
    readonly access_token: string;
    readonly token_type: string;
    readonly expires_in: number;
    readonly scope: string;
    readonly id_token: string;
    readonly refresh_token: string;
}