import { useTranslation } from "react-i18next";
import { useMemo } from "react";
import { ChannelFeatures } from "../hooks/api/Dtos/channelProfiles/ChannelProfile";

export enum ChannelMode {
    Other = 0,
    TPA = 1,
    OnSite = 2,
    Kiosk = 3,
    Online = 4,
}

const useQrCodeHelper = () => {
    const { t } = useTranslation();
        
    const hasFlag = (features: ChannelFeatures, flag: ChannelFeatures) => (features & flag) == flag;
    const isTpa = (features: ChannelFeatures): boolean => {
        const allowedValues = [ChannelFeatures.AllowsFreePayments, ChannelFeatures.AllowsFreePayments | ChannelFeatures.FreePaymentsAsTipOnly];
        return allowedValues.includes(features);
    }
    const isOnSite = (features: ChannelFeatures): boolean => {
        return hasFlag(features, ChannelFeatures.AllowsSessions) == true &&
                hasFlag(features, ChannelFeatures.AllowsPayAtTheTable) == true &&
                hasFlag(features, ChannelFeatures.OrderScheduling) == false &&
                hasFlag(features, ChannelFeatures.OrderAndPayWithTracking) == false &&
                hasFlag(features, ChannelFeatures.RequiresEmailForOrderAndPay) == false &&
                (
                    hasFlag(features, ChannelFeatures.AllowsOrderAndPay) == true &&
                    hasFlag(features, ChannelFeatures.AllowsPostPaymentOrdering) == true
                ) == false
    }
    const isKiosk = (features: ChannelFeatures): boolean => {
        return hasFlag(features, ChannelFeatures.AllowsOrderAndPay) == true &&
                hasFlag(features, ChannelFeatures.AllowsSessions) == false &&
                hasFlag(features, ChannelFeatures.AllowsPayAtTheTable) == false &&
                hasFlag(features, ChannelFeatures.OrderScheduling) == false &&
                hasFlag(features, ChannelFeatures.OrderAndPayWithTracking) == false &&
                hasFlag(features, ChannelFeatures.RequiresEmailForOrderAndPay) == false;
    }
    const isOnline = (features: ChannelFeatures): boolean => {
        return hasFlag(features, ChannelFeatures.AllowsOrderAndPay) == true &&
                hasFlag(features, ChannelFeatures.AllowsSessions) == false &&
                hasFlag(features, ChannelFeatures.AllowsPayAtTheTable) == false &&
                hasFlag(features, ChannelFeatures.OrderScheduling) == true &&
                hasFlag(features, ChannelFeatures.OrderAndPayWithTracking) == true &&
                hasFlag(features, ChannelFeatures.RequiresEmailForOrderAndPay) == true;
    }
    const getMode = (features: ChannelFeatures): ChannelMode => {
        let isTPA = isTpa(features);
        let onSite = isOnSite(features);
        let kiosk = isKiosk(features);
        let online = isOnline(features);
    
        if (isTPA && !onSite && !kiosk && !online)
            return ChannelMode.TPA;
        if (!isTPA && onSite && !kiosk && !online)
            return ChannelMode.OnSite;
        if (!isTPA && !onSite && kiosk && !online)
            return ChannelMode.Kiosk;
        if (!isTPA && !onSite && !kiosk && online)
            return ChannelMode.Online;
        return ChannelMode.Other;
    }
    const getDefaultFeatures = (mode: ChannelMode) => {
        let features: ChannelFeatures = ChannelFeatures.None;
        switch(mode)
        {
            case ChannelMode.TPA:
                features |= ChannelFeatures.AllowsFreePayments;
            break;
            case ChannelMode.OnSite: 
                features |= ChannelFeatures.AllowsSessions;
                features |= ChannelFeatures.AllowsPayAtTheTable;
            break;
            case ChannelMode.Kiosk: 
                features |= ChannelFeatures.AllowsOrderAndPay;
                break;
            case ChannelMode.Online: 
                features |= ChannelFeatures.RequiresEmailForOrderAndPay;
                features |= ChannelFeatures.AllowsOrderAndPay;
                features |= ChannelFeatures.OrderScheduling;
                features |= ChannelFeatures.OrderAndPayWithTracking;
                break;
            case ChannelMode.Other: 
                break;
        }
        return features;
    }
    // const sort = <T extends Channel>(qrCodes: T[]): T[] => qrCodes.sort((a, b) => {
    //     if (a.category > b.category) {
    //         return 1;
    //     }
    //     if (a.category < b.category) { 
    //         return -1;
    //     }
    
    //     const numberA = parseInt(a.identifier);
    //     const numberB = parseInt(b.identifier);

    //     if(Number.isNaN(numberA) == false && Number.isNaN(numberB) == false) {
    //         return numberA - numberB;
    //     }
    //     return a.identifier.localeCompare(b.identifier);
    // })

    const helper = useMemo(() => ({
        getMode,
        hasFlag,
        getDefaultFeatures,
        //sort,
    }), [t])
    
    return helper;
}
export default useQrCodeHelper;