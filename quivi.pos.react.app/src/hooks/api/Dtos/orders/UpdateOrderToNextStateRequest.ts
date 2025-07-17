import { OrderState } from "./OrderState";

export interface UpdateOrderToNextStateRequest {
    readonly id: string;
    readonly completeOrder?: boolean;
    readonly state?: OrderState;
}