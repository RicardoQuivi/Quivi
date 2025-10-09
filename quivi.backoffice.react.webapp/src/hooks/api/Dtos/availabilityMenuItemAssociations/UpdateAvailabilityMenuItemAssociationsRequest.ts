export interface UpdateAvailabilityMenuItemAssociationsRequest {
    readonly availabilityId?: string;
    readonly menuItemId?: string;
    readonly associations: UpdateAvailabilityMenuItemAssociation[];
}

export interface UpdateAvailabilityMenuItemAssociation {
    readonly id: string;
    readonly active: boolean;
}