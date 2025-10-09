export interface UpdateAvailabilityChannelProfileAssociationsRequest {
    readonly availabilityId?: string;
    readonly channelProfileId?: string;
    readonly associations: UpdateAvailabilityChannelProfileAssociation[];
}

export interface UpdateAvailabilityChannelProfileAssociation {
    readonly id: string;
    readonly active: boolean;
}