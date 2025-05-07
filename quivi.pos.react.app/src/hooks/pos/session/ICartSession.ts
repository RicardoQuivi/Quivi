import { MenuItem } from "../../api/Dtos/menuitems/MenuItem";
import { SessionItem } from "../../api/Dtos/sessions/SessionItem";

export interface ICartSession {
    readonly sessionId?: string;
    readonly channelId: string;
    readonly items: SessionItem[];
    readonly isSyncing: boolean;
    readonly closedAt?: Date;
    
    addItem(item: MenuItem | SessionItem, quantity: number): void;
    removeItem(item: SessionItem, quantity: number, discount?: number): void;
    applyDiscount(item: SessionItem, quantity: number, discount: number, priceOverride?: number): void;
    transferSession(toChannelId: string, transferItems?: Map<SessionItem, number>): void
    forceSync(): Promise<void>;
}