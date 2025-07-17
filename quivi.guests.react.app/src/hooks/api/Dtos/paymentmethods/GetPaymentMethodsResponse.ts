import type { PagedResponse } from "../PagedResponse";
import type { PaymentMethod } from "./PaymentMethod";

export interface GetPaymentMethodsResponse extends PagedResponse<PaymentMethod> {
    
}