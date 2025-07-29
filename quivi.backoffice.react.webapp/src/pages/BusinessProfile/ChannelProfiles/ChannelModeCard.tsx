import { Trans, useTranslation } from "react-i18next";
import { ChannelFeatures } from "../../../hooks/api/Dtos/channelProfiles/ChannelProfile";
import useChannelHelper, { ChannelMode } from "../../../utilities/useChannelHelper";
import { ReactNode, useEffect, useState } from "react";
import Checkbox from "../../../components/form/input/Checkbox";
import { CheckLineIcon, CloseIcon } from "../../../icons";
import Button from "../../../components/ui/button/Button";

interface Feature {
    readonly description: string;
    readonly isFixed: boolean;
    readonly isChecked: boolean;
    readonly action?: () => any;
    readonly children?: Feature[];
}

interface ChannelModeCardProps {
    readonly mode: ChannelMode;
    readonly features: ChannelFeatures;
    readonly onClick?: (features: ChannelFeatures) => any;
    readonly onChange?: (features: ChannelFeatures) => any;
}

export const ChannelModeCard = (props: ChannelModeCardProps) => {
    const { t } = useTranslation();
    const channelHelper = useChannelHelper();

    const [state, setState] = useState({
        features: ChannelFeatures.None,
        isActive: props.mode == channelHelper.getMode(props.features),
    })

    useEffect(() => setState({
        isActive: props.mode == ChannelMode.Other || props.mode == channelHelper.getMode(props.features),
        features: props.features,
    }), [props.features, props.mode, channelHelper])

    const setFeatures = (action: (f: ChannelFeatures) => ChannelFeatures) => {
        const result = action(state.features);
        props.onChange?.(result);
    }

    const toggleFeature = (feature: ChannelFeatures) => setFeatures(f => channelHelper.hasFlag(f, feature) ? (f & ~feature) : (f | feature));

    const getTitle = (): string => {
        switch(props.mode)
        {
            case ChannelMode.TPA: return t("common.channelModes.tpa.name");
            case ChannelMode.OnSite: return t("common.channelModes.onSite.name");
            case ChannelMode.Kiosk: return t("common.channelModes.kiosk.name");
            case ChannelMode.Online: return t("common.channelModes.online.name");
            case ChannelMode.Other: return t("common.channelModes.custom.name");
        }
    }

    const getTitleDescription = (): ReactNode => {
        let key = "";
        switch(props.mode)
        {
            case ChannelMode.TPA: key = "common.channelModes.tpa.description"; break;
            case ChannelMode.OnSite: key = "common.channelModes.onSite.description"; break;
            case ChannelMode.Kiosk: key = "common.channelModes.kiosk.description"; break;
            case ChannelMode.Online: key = "common.channelModes.online.description"; break;
            case ChannelMode.Other: key = "common.channelModes.custom.description"; break;
        }

        return <Trans
            t={t}
            i18nKey={key}
            shouldUnescape={true}
            components={{
                br: <br/>,
                b: <b/>,
            }}
        />
    }

    const getFeature = (feature: Feature) => {
        if(feature.children == undefined) {
            let checkbox = <Checkbox checked={feature.isChecked} disabled={feature.action == undefined} onChange={() => feature.action?.()}/>
            if(feature.isFixed) {
                checkbox = feature.isChecked ? <CheckLineIcon className="text-success-500"  /> : <CloseIcon className="text-gray-400" />
            }
            return (
                <li
                    key={feature.description}
                    className={`flex items-center gap-3 text-sm  ${
                        feature.isFixed == false || feature.isChecked
                        ? "text-gray-700 dark:text-gray-400"
                        : "text-gray-400"
                    }`}
                >
                    {checkbox}
                    {feature.description}
                </li>
            )
        }

        return (
        <li 
            key={feature.description}
            className="items-center gap-3 text-sm text-gray-500 dark:text-gray-400"
        >
            <ul>
            {
                getFeature({
                    description: feature.description,
                    isChecked: feature.isChecked,
                    action: feature.action,
                    isFixed: feature.isFixed,
                })
            }
            </ul>
            {
                feature.isChecked &&
                <ul className="pt-3 pl-6 space-y-3">
                    { feature.children.map((c) => getFeature(c)) }
                </ul>
            }
        </li>
        )
    }

    const getFeatures = (isActive: boolean): ReactNode[] => {
        const mode = props.mode;
        const defaultSettings = channelHelper.getDefaultFeatures(mode);
        const feats = isActive ? state.features : defaultSettings;

        const result = [
            getFeature({
                description: t("common.channelModes.features.visibleOnPos"), 
                isChecked: channelHelper.hasFlag(feats, ChannelFeatures.AllowsSessions),
                isFixed: true,
            }),
            getFeature({
                description: t("common.channelModes.features.allowsFreePayments"),
                isChecked: channelHelper.hasFlag(feats, ChannelFeatures.AllowsFreePayments),
                isFixed: [ChannelMode.Other].includes(mode) == false,
                action: isActive && [ChannelMode.Other].includes(mode)
                        ?
                            () => setFeatures(f => channelHelper.hasFlag(f, ChannelFeatures.AllowsFreePayments)
                                                    ? 
                                                        f & ~(ChannelFeatures.AllowsFreePayments | ChannelFeatures.FreePaymentsAsTipOnly)
                                                    :
                                                        f | ChannelFeatures.AllowsFreePayments
                                                    )
                        :
                        undefined,
                children: [
                    {
                        description: t("common.channelModes.features.freePaymentsAsTipOnly"),
                        isChecked: channelHelper.hasFlag(feats, ChannelFeatures.FreePaymentsAsTipOnly),
                        isFixed: false,
                        action: isActive && [ChannelMode.TPA, ChannelMode.Other].includes(mode)
                                ?
                                () => setFeatures(f => channelHelper.hasFlag(f, ChannelFeatures.FreePaymentsAsTipOnly)
                                                        ? 
                                                            f & ~ChannelFeatures.FreePaymentsAsTipOnly
                                                        :
                                                            f | ChannelFeatures.FreePaymentsAsTipOnly
                                                        )
                                :
                                undefined,
                    }
                ]
            }),
            getFeature({
                description: t("common.channelModes.features.sessions"), 
                isChecked: channelHelper.hasFlag(feats, ChannelFeatures.AllowsSessions),
                isFixed: [ChannelMode.Other].includes(mode) == false,
                action: isActive && [ChannelMode.Other].includes(mode) ? () => setFeatures(f => channelHelper.hasFlag(f, ChannelFeatures.AllowsSessions)
                                                                                                ? 
                                                                                                    f & ~(ChannelFeatures.AllowsSessions | ChannelFeatures.AllowsPayAtTheTable | ChannelFeatures.AllowsPostPaymentOrdering)
                                                                                                :
                                                                                                    f | ChannelFeatures.AllowsSessions
                                                                                                )  : undefined,
                children: [
                    {
                        description: t("common.channelModes.features.sessionChannelPayments"),
                        isChecked: channelHelper.hasFlag(feats, ChannelFeatures.AllowsPayAtTheTable), 
                        isFixed: [ChannelMode.Other].includes(mode) == false,
                        action: isActive && [ChannelMode.Other].includes(mode) ? () => toggleFeature(ChannelFeatures.AllowsPayAtTheTable) : undefined,
                    },
                    {
                        description: t("common.channelModes.features.postPaidOrdering"),
                        isChecked: channelHelper.hasFlag(feats, ChannelFeatures.AllowsPostPaymentOrdering), 
                        isFixed: [ChannelMode.Other, ChannelMode.OnSite].includes(mode) == false,
                        action: isActive && [ChannelMode.OnSite, ChannelMode.Other].includes(mode)
                                ? 
                                    () => setFeatures(f => channelHelper.hasFlag(f, ChannelFeatures.AllowsPostPaymentOrdering)
                                                            ?
                                                                (f & ~ChannelFeatures.AllowsPostPaymentOrdering)
                                                            :
                                                                (
                                                                    ChannelMode.Other == mode 
                                                                    ?
                                                                        f | ChannelFeatures.AllowsPostPaymentOrdering
                                                                    :
                                                                        (f & ~ChannelFeatures.AllowsOrderAndPay) | ChannelFeatures.AllowsPostPaymentOrdering
                                                                ) & ~ChannelFeatures.PostPaidOrderingAutoApproval & ~ChannelFeatures.PostPaidOrderingAutoComplete
                                                    )
                                : 
                                    undefined,
                        children: [
                            {
                                description: t("common.channelModes.features.autoApproval"),
                                isChecked: channelHelper.hasFlag(feats, ChannelFeatures.PostPaidOrderingAutoApproval), 
                                isFixed: false,
                                action: isActive && channelHelper.hasFlag(feats, ChannelFeatures.AllowsPostPaymentOrdering) && [ChannelMode.OnSite, ChannelMode.Kiosk, ChannelMode.Online, ChannelMode.Other].includes(mode) ? () => toggleFeature(ChannelFeatures.PostPaidOrderingAutoApproval) : undefined,
                            },
                            {
                                description: t("common.channelModes.features.autoComplete"),
                                isChecked: channelHelper.hasFlag(feats, ChannelFeatures.PostPaidOrderingAutoComplete), 
                                isFixed: false,
                                action: isActive && channelHelper.hasFlag(feats, ChannelFeatures.AllowsPostPaymentOrdering) && [ChannelMode.OnSite, ChannelMode.Kiosk, ChannelMode.Online, ChannelMode.Other].includes(mode) ? () => toggleFeature(ChannelFeatures.PostPaidOrderingAutoComplete) : undefined,
                            },
                        ]
                    }
                ]
            }),
            getFeature({
                description: t("common.channelModes.features.prePaidOrdering"),
                isChecked: channelHelper.hasFlag(feats, ChannelFeatures.AllowsOrderAndPay), 
                isFixed: [ChannelMode.Other, ChannelMode.OnSite].includes(mode) == false,
                action: isActive && [ChannelMode.OnSite, ChannelMode.Other].includes(mode)
                        ? 
                            () => setFeatures(f => channelHelper.hasFlag(f, ChannelFeatures.AllowsOrderAndPay)
                                                ?
                                                    (f & ~(ChannelFeatures.AllowsOrderAndPay | ChannelFeatures.RequiresEmailForOrderAndPay | ChannelFeatures.OrderScheduling | ChannelFeatures.OrderAndPayWithTracking))
                                                :
                                                    (
                                                        ChannelMode.Other == mode 
                                                        ?
                                                            f | ChannelFeatures.AllowsOrderAndPay
                                                        :
                                                            (f & ~ChannelFeatures.AllowsPostPaymentOrdering) | ChannelFeatures.AllowsOrderAndPay
                                                    ) & ~ChannelFeatures.PrePaidOrderingAutoApproval & ~ChannelFeatures.PrePaidOrderingAutoComplete
                                            ) 
                        : 
                            undefined,
                children: [
                    {
                        description: t("common.channelModes.features.autoApproval"),
                        isChecked: channelHelper.hasFlag(feats, ChannelFeatures.PrePaidOrderingAutoApproval), 
                        isFixed: false,
                        action: isActive && channelHelper.hasFlag(feats, ChannelFeatures.AllowsOrderAndPay) && [ChannelMode.OnSite, ChannelMode.Kiosk, ChannelMode.Online, ChannelMode.Other].includes(mode) ? () => toggleFeature(ChannelFeatures.PrePaidOrderingAutoApproval) : undefined,
                    },
                    {
                        description: t("common.channelModes.features.autoComplete"),
                        isChecked: channelHelper.hasFlag(feats, ChannelFeatures.PrePaidOrderingAutoComplete), 
                        isFixed: false,
                        action: isActive && channelHelper.hasFlag(feats, ChannelFeatures.AllowsOrderAndPay) && [ChannelMode.OnSite, ChannelMode.Kiosk, ChannelMode.Online, ChannelMode.Other].includes(mode) ? () => toggleFeature(ChannelFeatures.PrePaidOrderingAutoComplete) : undefined,
                    },
                    {
                        description: t("common.channelModes.features.emailRequired"),
                        isChecked: channelHelper.hasFlag(feats, ChannelFeatures.RequiresEmailForOrderAndPay), 
                        isFixed: [ChannelMode.Other].includes(mode) == false,
                        action: isActive && [ChannelMode.Other].includes(mode) ? () => toggleFeature(ChannelFeatures.RequiresEmailForOrderAndPay) : undefined,
                    },
                    {
                        description: t("common.channelModes.features.orderScheduling"),
                        isChecked: channelHelper.hasFlag(feats, ChannelFeatures.OrderScheduling), 
                        isFixed: [ChannelMode.Other].includes(mode) == false,
                        action: isActive && [ChannelMode.Other].includes(mode) ? () => toggleFeature(ChannelFeatures.OrderScheduling) : undefined,
                    },
                    {
                        description: t("common.channelModes.features.orderTracking"),
                        isChecked: channelHelper.hasFlag(feats, ChannelFeatures.OrderAndPayWithTracking), 
                        isFixed: [ChannelMode.Other].includes(mode) == false,
                        action: isActive && [ChannelMode.Other].includes(mode) ? () => toggleFeature(ChannelFeatures.OrderAndPayWithTracking) : undefined
                    }
                ]
            })
        ];

        if([ChannelMode.Other, ChannelMode.Kiosk].includes(mode)) {
            result.push(getFeature({
                description: t("common.channelModes.features.physicalKiosk"),
                isChecked: channelHelper.hasFlag(feats, ChannelFeatures.PhysicalKiosk),
                isFixed: false,
                action: isActive ? () => toggleFeature(ChannelFeatures.PhysicalKiosk) : undefined
            }))
        }
        return result;
    }

    const classes = state.isActive ? "border-2 border-brand-500 dark:border-brand-500 xl:p-8" : "border border-gray-200 dark:border-gray-800"
    return (
        <div className={`${classes} size-full flex flex-col rounded-2xl bg-white p-6 dark:bg-white/[0.03]`}>
            <div onClick={() => state.isActive == false && props.onClick?.(channelHelper.getDefaultFeatures(props.mode))} className="cursor-pointer">
                <span className="block mb-3 font-semibold text-gray-800 text-theme-xl dark:text-white/90">
                    {getTitle()}
                </span>
                <p className="text-sm text-gray-500 dark:text-gray-400">
                    {getTitleDescription()}
                </p>
            </div>

            <div className="w-full h-px my-6 bg-gray-200 dark:bg-gray-800"></div>

            <ul className="mb-8 space-y-3">
                {getFeatures(state.isActive)}
            </ul>

            {
                props.mode != ChannelMode.Other &&
                <Button
                    disabled={state.isActive}
                    onClick={() => state.isActive == false && props.onClick?.(channelHelper.getDefaultFeatures(props.mode))}
                    className="w-full mt-auto"
                    size="md"
                    variant="outline"
                >
                    {
                        state.isActive
                        ?
                        <>
                            <CheckLineIcon className="text-brand-500" />
                            {t("common.selected")}
                        </>
                        :
                        t("common.select")
                    }
                </Button>
            }
        </div>
    )
}