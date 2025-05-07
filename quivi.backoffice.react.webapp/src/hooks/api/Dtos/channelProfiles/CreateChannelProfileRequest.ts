import { ChannelFeatures } from "./ChannelProfile";

export interface CreateChannelProfileRequest {
    readonly name: string;
    readonly minimumPrePaidOrderAmount: number;
    readonly features: ChannelFeatures;
    readonly sendToPreparationTimer?: string;
    readonly posIntegrationId: string;
}