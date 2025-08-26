import { Language } from "../Language";
import { AssignedOn } from "./AssignedOn";
import { ConfigurableFieldType } from "./ConfigurableFieldType";
import { PrintedOn } from "./PrintedOn";

export interface ConfigurableField {
    readonly id: string;
    readonly name: string;
    readonly isRequired: boolean;
    readonly isAutoFill: boolean;
    readonly defaultValue?: string;
    readonly printedOn: PrintedOn;
    readonly assignedOn: AssignedOn;
    readonly type: ConfigurableFieldType;
    readonly translations: Record<Language, ConfigurableFieldTranslation>;
}

export interface ConfigurableFieldTranslation {
    readonly name: string;
}