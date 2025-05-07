import { useTranslation } from "react-i18next";
import { ChannelFeatures } from "../../hooks/api/Dtos/channelProfiles/ChannelProfile";
import useChannelHelper, { ChannelMode } from "../../utilities/useChannelHelper";

interface Props {
    readonly features: ChannelFeatures;
}
export const ChannelModeName = (props: Props) => {
    const { t } = useTranslation();
    const helper = useChannelHelper();
    
    const getTitle = (): string => {
        switch(helper.getMode(props.features))
        {
            case ChannelMode.TPA: return t("common.channelModes.tpa.name");
            case ChannelMode.OnSite: return t("common.channelModes.onSite.name");
            case ChannelMode.Kiosk: return t("common.channelModes.kiosk.name");
            case ChannelMode.Online: return t("common.channelModes.online.name");
            case ChannelMode.Other: return t("common.channelModes.custom.name");
        }
    }

    return <>{getTitle()}</>
}