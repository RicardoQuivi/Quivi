import type { OnConfigurableFieldAssociationOperation } from "./dtos/OnConfigurableFieldAssociationOperation";
import type { OnConfigurableFieldOperation } from "./dtos/OnConfigurableFieldOperation";

export interface MerchantListener {
    readonly merchantId: string;
    
    readonly onConfigurableFieldOperation ?: (evt: OnConfigurableFieldOperation) => any;
    readonly onConfigurableFieldAssociationOperation ?: (evt: OnConfigurableFieldAssociationOperation) => any;
}