import { OnChannelEvent } from "./Dtos/OnChannelEvent";
import { OnChannelProfileEvent } from "./Dtos/OnChannelProfileEvent";
import { OnCustomChargeMethodEvent } from "./Dtos/OnCustomChargeMethodEvent";
import { OnEmployeeEvent } from "./Dtos/OnEmployeeEvent";
import { OnItemCategoryEvent } from "./Dtos/OnItemCategoryEvent";
import { OnItemsModifierGroupEvent } from "./Dtos/OnItemsModifierGroupEvent";
import { OnLocalEvent } from "./Dtos/OnLocalEvent";
import { OnMenuItemEvent } from "./Dtos/OnMenuItemEvent";

export interface MerchantEventListener {
    readonly onChannelEvent?: (evt: OnChannelEvent) => any;
    readonly onChannelProfileEvent?: (evt: OnChannelProfileEvent) => any;
    readonly onItemCategoryEvent?: (evt: OnItemCategoryEvent) => any;
    readonly onLocalEvent?: (evt: OnLocalEvent) => any;
    readonly onMenuItemEvent?: (evt: OnMenuItemEvent) => any;
    readonly onEmployeeEvent?: (evt: OnEmployeeEvent) => any;
    readonly onItemsModifierGroupEvent?: (evt: OnItemsModifierGroupEvent) => any;
    readonly onCustomChargeMethodEvent?: (evt: OnCustomChargeMethodEvent) => any;
}