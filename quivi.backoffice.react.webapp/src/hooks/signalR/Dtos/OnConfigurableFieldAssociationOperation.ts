import { Operation } from "./Operation";

export interface OnConfigurableFieldAssociationOperation {
    readonly operation: Operation;
    readonly channelProfileId: string;
    readonly configurableFieldId: string;
    readonly merchantId: string;
}