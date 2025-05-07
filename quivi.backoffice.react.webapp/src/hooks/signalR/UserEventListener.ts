import { OnMerchantAssociatedEvent } from "./Dtos/OnMerchantAssociatedEvent";

export interface UserEventListener {
    readonly onMerchantAssociatedEvent?: (evt: OnMerchantAssociatedEvent) => any;
}