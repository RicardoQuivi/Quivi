import { ChannelFeatures } from "./ChannelFeatures";

export interface ChannelProfile {
    readonly id: string;
    readonly name: string;
    readonly posIntegrationId: string;
    readonly features: ChannelFeatures;
}