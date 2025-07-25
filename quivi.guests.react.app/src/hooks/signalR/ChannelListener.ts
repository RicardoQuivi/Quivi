import type { OnOrderOperationEvent } from "./dtos/OnOrderOperationEvent";
import type { OnPosChargeOperationEvent } from "./dtos/OnPosChargeOperationEvent";
import type { OnSessionUpdatedEvent } from "./dtos/OnSessionUpdatedEvent";

export interface ChannelListener {
    readonly channelId: string;
    readonly onSessionUpdatedEvent?: (evt: OnSessionUpdatedEvent) => any;
    readonly onOrderOperationEvent?: (evt: OnOrderOperationEvent) => any;
    readonly onPosChargeOperation?: (evt: OnPosChargeOperationEvent) => any;
}