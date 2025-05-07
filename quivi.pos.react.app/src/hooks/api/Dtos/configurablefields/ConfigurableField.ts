export interface ConfigurableField {
    readonly id: string;
    readonly name: string;
    readonly defaultValue?: string;
    readonly isRequired: boolean;
    readonly posOnly: boolean;
    readonly type: ConfigurableFieldType;
}

export enum ConfigurableFieldType {
    Text = 0,
    LongText = 1,
    Check = 2,
    Number = 3,
}