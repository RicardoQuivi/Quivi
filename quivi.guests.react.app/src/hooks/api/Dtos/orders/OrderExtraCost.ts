import { ExtraCostType } from "./ExtraCostType";

export interface OrderExtraCost {
    readonly type: ExtraCostType;
    readonly amount: number;
}