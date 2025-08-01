import type { ChargeMethod } from "../ChargeMethod";

export interface ProcessPaybyrdChargeRequest {
    readonly method: ChargeMethod;
    readonly tokenId: string;
    readonly redirectUrl?: string;
}