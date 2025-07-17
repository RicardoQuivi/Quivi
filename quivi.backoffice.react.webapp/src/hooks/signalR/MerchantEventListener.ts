import { OnAcquirerConfigurationEvent } from "./Dtos/OnAcquirerConfigurationEvent";
import { OnChannelEvent } from "./Dtos/OnChannelEvent";
import { OnChannelProfileEvent } from "./Dtos/OnChannelProfileEvent";
import { OnCustomChargeMethodEvent } from "./Dtos/OnCustomChargeMethodEvent";
import { OnEmployeeEvent } from "./Dtos/OnEmployeeEvent";
import { OnItemCategoryEvent } from "./Dtos/OnItemCategoryEvent";
import { OnItemsModifierGroupEvent } from "./Dtos/OnItemsModifierGroupEvent";
import { OnLocalEvent } from "./Dtos/OnLocalEvent";
import { OnMenuItemEvent } from "./Dtos/OnMenuItemEvent";
import { OnPrinterEvent } from "./Dtos/OnPrinterEvent";
import { OnPrinterMessageEvent } from "./Dtos/OnPrinterMessageEvent";
import { OnPrinterWorkerEvent } from "./Dtos/OnPrinterWorkerEvent";

export interface MerchantEventListener {
    readonly onChannelEvent?: (evt: OnChannelEvent) => any;
    readonly onChannelProfileEvent?: (evt: OnChannelProfileEvent) => any;
    readonly onItemCategoryEvent?: (evt: OnItemCategoryEvent) => any;
    readonly onLocalEvent?: (evt: OnLocalEvent) => any;
    readonly onMenuItemEvent?: (evt: OnMenuItemEvent) => any;
    readonly onEmployeeEvent?: (evt: OnEmployeeEvent) => any;
    readonly onItemsModifierGroupEvent?: (evt: OnItemsModifierGroupEvent) => any;
    readonly onCustomChargeMethodEvent?: (evt: OnCustomChargeMethodEvent) => any;
    readonly onPrinterWorkerEvent?: (evt: OnPrinterWorkerEvent) => any;
    readonly onPrinterEvent?: (evt: OnPrinterEvent) => any;
    readonly onPrinterMessageOperation?: (evt: OnPrinterMessageEvent) => any;
    readonly onAcquirerConfigurationOperation?: (evt: OnAcquirerConfigurationEvent) => any;
}