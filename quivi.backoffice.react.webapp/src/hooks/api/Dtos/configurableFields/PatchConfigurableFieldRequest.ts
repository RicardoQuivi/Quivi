import { Language } from "../Language";
import { AssignedOn } from "./AssignedOn";
import { ConfigurableFieldType } from "./ConfigurableFieldType";
import { PrintedOn } from "./PrintedOn";

export interface PatchConfigurableFieldRequest {
    readonly id: string;
    readonly name?: string;
    readonly type?: ConfigurableFieldType;
    readonly isRequired?: boolean;
    readonly isAutoFill?: boolean;
    readonly printedOn?: PrintedOn;
    readonly assignedOn?: AssignedOn;
    readonly defaultValue?: string;
    readonly translations?: Record<Language, PatchConfigurableFieldTranslation | undefined>;
}

export interface PatchConfigurableFieldTranslation {
    readonly name?: string;
    readonly description?: string;
}