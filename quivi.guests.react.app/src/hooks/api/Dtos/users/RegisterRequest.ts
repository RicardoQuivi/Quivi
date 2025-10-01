export interface RegisterRequest {
    readonly name: string;
    readonly vatNumber?: string;
    readonly phoneNumber?: string;
    readonly email: string;
    readonly password: string;
}