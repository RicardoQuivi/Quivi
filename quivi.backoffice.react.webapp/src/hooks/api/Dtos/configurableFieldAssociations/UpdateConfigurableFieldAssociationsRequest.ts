export interface UpdateConfigurableFieldAssociationsRequest {
    readonly channelProfileId?: string;
    readonly configurableFieldId?: string;
    readonly associations: UpdateConfigurableFieldAssociation[];
}

export interface UpdateConfigurableFieldAssociation {
    readonly id: string;
    readonly active: boolean;
}