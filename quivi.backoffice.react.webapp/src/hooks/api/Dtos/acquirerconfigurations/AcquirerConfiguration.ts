import { ChargeMethod } from "../ChargeMethod";
import { ChargePartner } from "./ChargePartner";

export interface AcquirerConfiguration {
    readonly id: string;
    readonly method: ChargeMethod;
    readonly partner: ChargePartner;
    readonly isActive: boolean;
    readonly settings: Record<string, Record<string, any>>;
}