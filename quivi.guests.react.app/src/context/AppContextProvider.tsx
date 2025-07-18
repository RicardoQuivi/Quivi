import { createContext, useContext, useEffect, useMemo } from "react";
import { useBrowserStorageService } from "../hooks/useBrowserStorageService";
import SplashScreen from "../pages/SplashScreen";
import { useChannelsQuery } from "../hooks/queries/implementations/useChannelsQuery";
import { useChannelProfilesQuery } from "../hooks/queries/implementations/useChannelProfilesQuery";
import { useMerchantsQuery } from "../hooks/queries/implementations/useMerchantsQuery";
import { usePosIntegrationsQuery } from "../hooks/queries/implementations/usePosIntegrationsQuery";
import { ChannelFeature } from "../hooks/api/Dtos/channelProfiles/ChannelFeature";
import type { PosIntegration } from "../hooks/api/Dtos/posIntegrations/PosIntegration";
import type { Channel } from "../hooks/api/Dtos/channels/Channel";
import type { ChannelProfile } from "../hooks/api/Dtos/channelProfiles/ChannelProfile";
import type { Merchant } from "../hooks/api/Dtos/merchants/Merchant";
import type { ChannelListener } from "../hooks/signalR/ChannelListener";
import { useWebEvents } from "../hooks/signalR/useWebEvents";
import type { OnSessionUpdatedEvent } from "../hooks/signalR/dtos/OnSessionUpdatedEvent";
import { useInvalidator } from "./QueryContextProvider";
import { Entity } from "../hooks/EntitiesName";
import type { OnOrderOperationEvent } from "../hooks/signalR/dtos/OnOrderOperationEvent";
import type { OnPosChargeOperationEvent } from "../hooks/signalR/dtos/OnPosChargeOperationEvent";
import { useParams } from "react-router";

interface FreePaymentsFeatures {
    readonly isActive: boolean;
    readonly isTipOnly: boolean;
}

interface OrderingFeatures {
    readonly isActive: boolean,
    readonly allowsPostPaidOrdering: boolean,
    readonly allowsPrePaidOrdering: boolean,
    readonly invoiceIsDownloadable: boolean,
    readonly allowsTracking: boolean,
    readonly enforceTip: boolean;
    readonly mandatoryUserEmailForTakeawayPayment: boolean;
    readonly allowScheduling: boolean;
    readonly minimumPrePaidOrderAmount: number;
}

interface PayAtTheTableFeatures {
    readonly isActive: boolean,
    readonly allowsInvoiceDownloads: boolean,
    readonly freePayment: boolean,
    readonly itemSelectionPayment: boolean,
    readonly splitBillPayment: boolean,
    readonly allowsAddingItemsToSession: boolean,
    readonly allowsRemovingItemsFromSession: boolean,
    readonly allowsIgnoreBill: boolean,
    readonly enforceTip: boolean;
}

interface Features {
    readonly payAtTheTable: PayAtTheTableFeatures;
    readonly ordering: OrderingFeatures;
    readonly freePayments: FreePaymentsFeatures;
    readonly allowsSessions: boolean;
    readonly physicalKiosk: boolean;
    readonly showPaymentNote: boolean;
}

interface AppContextType {
    readonly context: ChannelContextType;
    readonly integration: PosIntegration;
    readonly channel: Channel;
    readonly profile: ChannelProfile;
    readonly merchant: Merchant;
}

interface ChannelContextType {
    readonly channelId: string;
    readonly merchantId: string;
    readonly logoUrl: string;
    readonly merchantName: string;
    readonly channelFullName: string;
    readonly features: Features;
    readonly surchargeFeesMayApply: boolean;
    readonly inactive: boolean;
    readonly posIntegrationId: string;
}

const AppContext = createContext<AppContextType | undefined>(undefined);

const hasFlag = (features: ChannelFeature, f: ChannelFeature): boolean => (features & f) == f;

export const AppContextProvider = (props: {
    readonly children: React.ReactNode;
}) => {
    const { id: channelId } = useParams<{ id: string}>();

    const browserStorageService = useBrowserStorageService();
    const webEvents = useWebEvents();
    const invalidator = useInvalidator();

    const channelsQuery = useChannelsQuery(!channelId ? undefined : channelId);
    const channel = useMemo(() => channelsQuery.data.length == 0 ? undefined : channelsQuery.data[0], [channelsQuery.data])

    const merchantQuery = useMerchantsQuery(channel == undefined ? undefined : channel.merchantId);
    const merchant = useMemo(() => merchantQuery.data.length == 0 ? undefined : merchantQuery.data[0], [merchantQuery.data])

    const profilesQuery = useChannelProfilesQuery(channel == undefined ? undefined : channel.channelProfileId);
    const profile = useMemo(() => profilesQuery.data.length == 0 ? undefined : profilesQuery.data[0], [profilesQuery.data])

    const integrationQuery = usePosIntegrationsQuery(profile == undefined ? undefined : profile.posIntegrationId);
    const integration = useMemo(() => integrationQuery.data.length == 0 ? undefined : integrationQuery.data[0], [integrationQuery.data])

    useEffect(() => {
        if(channel == undefined) {
            return;
        }

        browserStorageService.saveChannelId(channel.id);
    }, [channel])

    useEffect(() => {
        init();
    }, [])

    const init = async () => {
        const emailConfirmed = new URLSearchParams(location.search.toLowerCase()).has('register');
        if (emailConfirmed) {
            window.location.replace("/login?confirmed=true");
            return;
        }
    }

    const result = useMemo<AppContextType | undefined | null>(() => {
        if(channelId == undefined) {
            return null;
        }
        
        if(channel == undefined || profile == undefined || merchant == undefined || integration == undefined) {
            return undefined
        }

        let prePaidOrderingActive = hasFlag(profile.features, ChannelFeature.AllowsOrderAndPay) && integration.allowsAddingItemsToSession;
        let postPaidOrderingActive = hasFlag(profile.features, ChannelFeature.AllowsPostPaymentOrdering) && integration.allowsAddingItemsToSession;
        let isActive = prePaidOrderingActive || postPaidOrderingActive;

        return {
            context: {
                channelId: channel.id,
                merchantId: merchant.id,
                merchantName: merchant.name,
                channelFullName: `${profile.name} ${channel.name}`,
                logoUrl: merchant.logoUrl,
                surchargeFeesMayApply: merchant.surchargeFeesMayApply,
                inactive: merchant.inactive,
                features: {
                    allowsSessions: integration.isActive && hasFlag(profile.features, ChannelFeature.AllowsSessions),
                    physicalKiosk: hasFlag(profile.features, ChannelFeature.PhysicalKiosk),
                    showPaymentNote: merchant.showPaymentNotes,
                    freePayments: {
                        isActive: hasFlag(profile.features, ChannelFeature.AllowsFreePayments),
                        isTipOnly: hasFlag(profile.features, ChannelFeature.FreePaymentsAsTipOnly),
                    },
                    payAtTheTable: {
                        isActive: integration.isActive && integration.allowsPayments,

                        freePayment: merchant.freePayment,
                        itemSelectionPayment: merchant.itemSelectionPayment,
                        splitBillPayment: merchant.splitBillPayment,
                        enforceTip: merchant.enforceTip,

                        allowsInvoiceDownloads: integration.allowsInvoiceDownloads,
                        allowsAddingItemsToSession: hasFlag(profile.features, ChannelFeature.AllowsPostPaymentOrdering) ? integration.allowsAddingItemsToSession : false,
                        allowsRemovingItemsFromSession: hasFlag(profile.features, ChannelFeature.AllowsPostPaymentOrdering) ? integration.allowsRemovingItemsFromSession : false,
                        allowsIgnoreBill: merchant.allowsIgnoreBill,
                    },
                    ordering: {
                        isActive: isActive,
                        allowsPostPaidOrdering: postPaidOrderingActive,
                        allowsPrePaidOrdering: prePaidOrderingActive,
                        invoiceIsDownloadable: isActive && integration.allowsInvoiceDownloads,
                        allowsTracking: hasFlag(profile.features, ChannelFeature.OrderAndPayWithTracking),
                        enforceTip: merchant.enforceTip,
                        mandatoryUserEmailForTakeawayPayment: hasFlag(profile.features, ChannelFeature.RequiresEmailForOrderAndPay),
                        allowScheduling: hasFlag(profile.features, ChannelFeature.OrderScheduling),
                        minimumPrePaidOrderAmount: profile.prePaidOrderingMinimumAmount,
                    },
                },
                posIntegrationId: integration.id,
            },
            channel: channel,
            integration: integration,
            profile: profile,
            merchant: merchant,
        }
    }, [channelId, channel, profile, merchant, integration]);
    

    useEffect(() => {
        if(result === undefined || result === null) {
            return;
        }

        const listener: ChannelListener = {
            channelId: result.channel.id,

            onSessionUpdatedEvent: (evt: OnSessionUpdatedEvent) => invalidator.invalidate(Entity.Sessions, evt.id),
            onOrderOperationEvent: (evt: OnOrderOperationEvent) => invalidator.invalidate(Entity.Orders, evt.id),
            onPosChargeOperation: (evt: OnPosChargeOperationEvent) => invalidator.invalidate(Entity.Transactions, evt.id),
        }
        webEvents.client.addChannelListener(listener);
        return () => webEvents.client.removeChannelListener(listener);
    }, [webEvents.client, result?.channel.id])

    if(result == undefined) {
        return <SplashScreen />;
    }

    return (
        <AppContext.Provider value={result}>
            {props.children}
        </AppContext.Provider>
    );
}

export const useAppContext = (): ChannelContextType | null => {
    const context = useContext(AppContext);
    if(context === undefined) {
        throw Error("useAppContext can only be used inside AppContextProvider");
    }
    return context.context;
};

export const useChannelContext = (): ChannelContextType => {
    const context = useContext(AppContext);
    if(context === undefined) {
        throw Error("useChannelContext requires a channel");
    }
    return context.context;
}

export const useCurrentMerchant = (): Merchant => {
    const context = useContext(AppContext);
    if(context === undefined) {
        throw Error("useCurrentMerchant requires a channel");
    }
    return context.merchant;
}

export const useCurrentPosIntegration = (): PosIntegration => {
    const context = useContext(AppContext);
    if(context === undefined) {
        throw Error("useCurrentPosIntegration requires a channel");
    }
    return context.integration;
}