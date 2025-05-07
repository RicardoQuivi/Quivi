import { ChannelFeatures } from "./ChannelProfile";

export interface PatchChannelProfileRequest {
    readonly id: string;
    readonly name?: string;
    readonly minimumPrePaidOrderAmount?: number;
    readonly features?: ChannelFeatures;
    readonly sendToPreparationTimer?: string | undefined | null;
    readonly posIntegrationId?: string | undefined;
}