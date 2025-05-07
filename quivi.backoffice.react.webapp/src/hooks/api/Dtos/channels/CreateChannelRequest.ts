interface ChannelToAdd {
    readonly name: string;
}

export interface CreateChannelsRequest {
    readonly channelProfileId: string;
    readonly data: ChannelToAdd[];
}