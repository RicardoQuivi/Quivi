import { jwtDecode } from "jwt-decode";

export interface DecodedEmployeeToken {
    readonly aud: string;
    readonly exp: number;
    readonly email: string;
    readonly sub: string;
    readonly iss: string;
    readonly jti: string;
    readonly merchant_id: string;
    readonly sub_merchant_id: string;
    readonly employee_id: string;
    readonly activated_at?: number;
    readonly role: string[];
}

export class EmployeeTokenData {
    public readonly accessToken: string;
    public readonly refreshToken: string;
    
    public readonly jwtId: string;
    public readonly audience: string;
    public readonly issuer: string;
    public readonly expire: number;
    public readonly userId: string;
    public readonly email: string;
    public readonly merchantId: string;
    public readonly subMerchantId: string;
    public readonly employeeId: string;
    public readonly isAdmin: boolean;
    public readonly isActivated: boolean;

    constructor(accessToken: string, refreshToken: string) {
        this.accessToken = accessToken;
        this.refreshToken = refreshToken;

        const decoded = jwtDecode<DecodedEmployeeToken>(accessToken);

        this.jwtId = decoded.jti;
        this.audience = decoded.aud as string;
        this.expire = decoded.exp;
        this.userId = decoded.sub;
        this.email = decoded.email;
        this.issuer = decoded.iss;
        this.merchantId = decoded.merchant_id;
        this.subMerchantId = decoded.sub_merchant_id;
        this.employeeId = decoded.employee_id;
        this.isActivated = decoded.activated_at != undefined;
        this.isAdmin = decoded.role?.find(p => ["Admin", "SuperAdmin"].includes(p)) != undefined;
    }
}