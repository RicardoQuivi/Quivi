import type { ChargeMethod } from "../ChargeMethod";

export interface PaymentMethod {
    readonly id: string;
    readonly method: ChargeMethod;
}