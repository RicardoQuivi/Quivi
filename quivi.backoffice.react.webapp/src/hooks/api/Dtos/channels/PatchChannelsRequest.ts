export interface PatchChannelsRequest {
    readonly ids: string[];
    readonly channelProfileId?: string;
    readonly name?: string;
}