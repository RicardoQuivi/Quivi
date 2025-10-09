import type { OnMenuItemAvailabilityChanged } from "./dtos/OnMenuItemAvailabilityChanged";

export interface ChannelProfileListener {
    readonly channelProfileId: string;

    readonly onMenuItemAvailabilityChanged?: (evt: OnMenuItemAvailabilityChanged) => any;
}