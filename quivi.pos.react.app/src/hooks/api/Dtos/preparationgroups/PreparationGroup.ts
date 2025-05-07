export interface PreparationGroup {
    readonly id: string;
    readonly sessionId: string;
    readonly isCommited: boolean;
    readonly note?: string;
    readonly orderIds: string[];
    readonly createdDate: string;
    readonly lastModified: string;
    readonly items: PreparationGroupItem[];
}

export interface BasePreparationGroupItem {
    readonly id: string;
    readonly menuItemId: string;
    readonly remainingQuantity: number;
    readonly quantity: number;
    readonly locationId?: string;
}

export interface PreparationGroupItem extends BasePreparationGroupItem {
    readonly extras: BasePreparationGroupItem[];
}