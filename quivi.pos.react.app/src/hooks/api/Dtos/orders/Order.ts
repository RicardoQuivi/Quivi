import { SessionItem } from "../sessions/SessionItem";
import { OrderState } from "./OrderState";

export interface OrderFieldValue {
    readonly id: string;
    readonly value: string;
}

export interface Order {
    readonly id: string;
    readonly channelId: string;
    readonly sequenceNumber: string;
    readonly state: OrderState;
    readonly isTakeAway: boolean;
    readonly items: SessionItem[];
    readonly fields: OrderFieldValue[];
    readonly scheduledTo?: string;
    readonly lastModified: string;
}