import type { ChannelFeature } from "./ChannelFeature";

export interface ChannelProfile {
    readonly id: string;
    readonly name: string;
    readonly features: ChannelFeature;
    readonly posIntegrationId: string;
    readonly prePaidOrderingMinimumAmount: number;
}