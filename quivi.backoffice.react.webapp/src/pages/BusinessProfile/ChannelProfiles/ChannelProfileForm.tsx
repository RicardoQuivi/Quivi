import { useTranslation } from "react-i18next";
import { ChannelFeatures, ChannelProfile } from "../../../hooks/api/Dtos/channelProfiles/ChannelProfile";
import { usePosIntegrationsQuery } from "../../../hooks/queries/implementations/usePosIntegrationsQuery";
import { useEffect, useMemo, useState } from "react";
import { PosIntegration } from "../../../hooks/api/Dtos/posIntegrations/PosIntegration";
import { fromDateToTimespan, fromStringToTimespan, fromTimespanToDate, fromTimespanToString } from "../../../utilities/timespanHelpers";
import useChannelHelper, { ChannelMode } from "../../../utilities/useChannelHelper";
import { useIntegrationHelper } from "../../../utilities/useIntegrationHelper";
import Label from "../../../components/form/Label";
import Input from "../../../components/form/input/InputField";
import { Skeleton } from "../../../components/ui/skeleton/Skeleton";
import Radio from "../../../components/form/input/Radio";
import { ChannelModeCard } from "./ChannelModeCard";
import { useQuiviForm } from "../../../hooks/api/exceptions/useQuiviForm";
import * as yup from 'yup';
import Button from "../../../components/ui/button/Button";
import { SingleSelect } from "../../../components/inputs/SingleSelect";
import { useToast } from "../../../layout/ToastProvider";
import { TextField } from "../../../components/inputs/TextField";
import { Spinner } from "../../../components/spinners/Spinner";

const schema = yup.object<QrCodeProfileFormState>({
    name: yup.string().required(),
    posIntegration: yup.object().required(),
});

export interface QrCodeProfileFormState {
    readonly name: string;
    readonly features: ChannelFeatures;
    readonly minimumPrePaidOrderAmount: number;
    readonly sendToPreparationTimer: string | null | undefined;
    readonly posIntegrationId: string;
}

interface Props {
    readonly model?: ChannelProfile;
    readonly onSubmit: (state: QrCodeProfileFormState) => Promise<any>;
    readonly submitText: string;
}
export const ChannelProfileForm = (props: Props) => {
    const { t } = useTranslation();
    const channelHelper = useChannelHelper();
    const integrationHelper = useIntegrationHelper();
    const toast = useToast();
    
    const integrationsQuery = usePosIntegrationsQuery({
        page: 0,
        pageSize: undefined,
    });

    const {
        integrationsMap,
        defaultIntegration
     } = useMemo(() => {
        const result = new Map<string, PosIntegration>();
        for(const integration of integrationsQuery.data) {
            result.set(integration.id, integration);
        }
        return {
            integrationsMap: result,
            defaultIntegration: integrationsQuery.data.length == 0 ? undefined : integrationsQuery.data[0]
        };
    }, [integrationsQuery.data])

    const [state, setState] = useState({
        name: props.model?.name ?? "",
        features: props.model?.features ?? channelHelper.getDefaultFeatures(ChannelMode.OnSite),
        minimumPrePaidOrderAmount: props.model?.minimumPrePaidOrderAmount ?? 0,
        originalSendToPreparationTimer: props.model?.sendToPreparationTimer == undefined ? null : fromTimespanToDate(fromStringToTimespan(props.model.sendToPreparationTimer)),
        sendToPreparationTimer: props.model?.sendToPreparationTimer == undefined ? null : fromTimespanToDate(fromStringToTimespan(props.model.sendToPreparationTimer)),
        posIntegration: undefined as PosIntegration | undefined,

        mode: "default" as "custom" | "default",
        apiErrors: [],
    })
    const form = useQuiviForm(state, schema);

    const resetData = () => setState(s => ({
        ...s,
        name: props.model?.name ?? "",
        features: props.model?.features ?? channelHelper.getDefaultFeatures(ChannelMode.OnSite),
        minimumPrePaidOrderAmount: props.model?.minimumPrePaidOrderAmount ?? 0,
        originalSendToPreparationTimer: props.model?.sendToPreparationTimer == undefined ? null : fromTimespanToDate(fromStringToTimespan(props.model.sendToPreparationTimer)),
        sendToPreparationTimer: props.model?.sendToPreparationTimer == undefined ? null : fromTimespanToDate(fromStringToTimespan(props.model.sendToPreparationTimer)),
        posIntegration: props.model?.posIntegrationId == undefined ? undefined : (integrationsMap.get(props.model?.posIntegrationId) ?? defaultIntegration),

        mode: props.model != undefined && channelHelper.getMode(props.model.features) == ChannelMode.Other ? "custom" : "default",
        apiErrors: [],
    }))

    const setFeatures = (features: ChannelFeatures) => setState(s => {
        let mode = s.mode;
        if([ChannelMode.Kiosk, ChannelMode.OnSite, ChannelMode.Online, ChannelMode.TPA].includes(channelHelper.getMode(state.features)) == false) {
            mode = "custom";
        }
        return {
            ...s, 
            features: features,
            mode: mode,
        }
    })

    useEffect(() => resetData(), [props.model]);
    useEffect(() => {
        const modelIntegration = props.model == undefined ? defaultIntegration : integrationsMap.get(props.model.posIntegrationId) 
        const integration = state.posIntegration;
        setState(s => ({...s, posIntegration: integration ?? modelIntegration }))
    }, [integrationsMap, defaultIntegration])

    const save = () => form.submit(async () => {
        const sendToPreparationTimer = state.sendToPreparationTimer == state.originalSendToPreparationTimer 
        ? 
            undefined 
        : 
        (
            state.sendToPreparationTimer === null
            ?
            null
            :
            fromTimespanToString(fromDateToTimespan(state.sendToPreparationTimer))
        );

        await props.onSubmit({
            name: state.name,
            minimumPrePaidOrderAmount: state.minimumPrePaidOrderAmount,
            sendToPreparationTimer: sendToPreparationTimer,
            features: state.features,
            posIntegrationId: state.posIntegration?.id ?? defaultIntegration?.id ?? "",
        })
    }, () => toast.error(t("common.operations.failure.generic")))

    return <>
        <div className="grid grid-cols-1 sm:grid-cols-1 md:grid-cols-2 lg:grid-cols-2 gap-4">
            <TextField
                label={t("common.name")}
                placeholder={t("common.name")}
                value={state.name}
                onChange={v => setState(s => ({...s, name: v }))}
                errorMessage={form.touchedErrors.get("name")?.message}
            />
            <div>
                {
                    integrationsQuery.isFirstLoading
                    ?
                    <Skeleton className="w-full h-full"/>
                    :
                    <SingleSelect
                        label={t("common.entities.posIntegration")}
                        options={integrationsQuery.data}
                        getId={r => r?.id ?? ""}
                        render={r => r == undefined ? <Skeleton /> : integrationHelper.getStrategyName(r.type)}
                        placeholder={t("common.entities.posIntegration")}
                        onChange={e => setState(s => ({ ...s, integration: e }))}
                        value={state.posIntegration ?? defaultIntegration}
                    />
                }
            </div>
            <div>
                <Label htmlFor="minPrePaidOrderAmount">{t("pages.channelProfiles.minimumPrePaidOrderAmount")}</Label>
                <div className="relative">
                    <Input
                        type="text"
                        placeholder={t("pages.channelProfiles.minimumPrePaidOrderAmount")}
                        id="minPrePaidOrderAmount"
                        value={state.minimumPrePaidOrderAmount}
                        onChange={(e) => setState(s => ({...s, minimumPrePaidOrderAmount: +e.target.value}))}
                    />
                    <span className="absolute text-gray-500 -translate-y-1/2 pointer-events-none right-4 top-1/2 dark:text-gray-400">
                        €
                    </span>
                </div>
            </div>
            <div className="flex items-center gap-3">
                <Label className="mb-0">{t("pages.channelProfiles.preparationTimer")}</Label>
                <div className="flex flex-wrap items-center gap-4">
                    <Radio
                        label={t("common.active")}
                        checked={state.sendToPreparationTimer != null}
                        onChange={() => setState(s => ({
                                                ...s, 
                                                sendToPreparationTimer: s.sendToPreparationTimer == null ? fromTimespanToDate({
                                                    hours: 0,
                                                    minutes: 2,
                                                    seconds: 0,
                                                }) : null
                                            }))
                        }
                    />
                    <Radio
                        label={t("common.inactive")}
                        checked={state.sendToPreparationTimer == null}
                        onChange={() => setState(s => ({
                                                ...s, 
                                                sendToPreparationTimer: s.sendToPreparationTimer == null ? fromTimespanToDate({
                                                    hours: 0,
                                                    minutes: 2,
                                                    seconds: 0,
                                                }) : null
                                            }))
                        }
                    />
                </div>
            </div>
        </div>
        <div className="p-3 border border-gray-200 rounded-t-xl dark:border-gray-800">
            <nav className="flex overflow-x-auto rounded-lg bg-gray-100 p-1 dark:bg-gray-900 [&::-webkit-scrollbar]:h-1.5 [&::-webkit-scrollbar-track]:bg-white dark:[&::-webkit-scrollbar-track]:bg-transparent [&::-webkit-scrollbar-thumb]:rounded-full [&::-webkit-scrollbar-thumb]:bg-gray-200 dark:[&::-webkit-scrollbar-thumb]:bg-gray-600">
                {
                    ["default", "custom"].map(s => (
                    <button
                        key={s}
                        onClick={() => setState(st => ({...st, mode: s as "custom" | "default"}))}
                        className={`inline-flex items-center rounded-md px-3 py-2 text-sm font-medium transition-colors duration-200 ease-in-out ${
                            state.mode === s
                            ? "bg-white text-gray-900 shadow-theme-xs dark:bg-white/[0.03] dark:text-white"
                            : "bg-transparent text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200"
                        }`}
                    >
                        {t(`common.channelModes.${s}.name`)}
                    </button>))
                }
            </nav>
        </div>
        <div className="flex flex-wrap justify-center gap-2">
             {
                state.mode == "custom"
                ?
                <div className="grow">
                    <ChannelModeCard
                        mode={ChannelMode.Other}
                        features={state.features}
                        onChange={setFeatures}
                        onClick={setFeatures}
                    />
                </div>
                :
                <>
                    <div className="grow">
                        <ChannelModeCard
                            mode={ChannelMode.TPA}
                            features={state.features}
                            onChange={setFeatures}
                            onClick={setFeatures}
                        />
                    </div>
                    <div className="grow">
                        <ChannelModeCard
                            mode={ChannelMode.OnSite}
                            features={state.features}
                            onChange={setFeatures}
                            onClick={setFeatures}
                        />
                    </div>
                    <div className="grow">
                        <ChannelModeCard
                            mode={ChannelMode.Kiosk}
                            features={state.features}
                            onChange={setFeatures}
                            onClick={setFeatures}
                        />
                    </div>
                    <div className="grow">
                        <ChannelModeCard
                            mode={ChannelMode.Online}
                            features={state.features}
                            onChange={setFeatures}
                            onClick={setFeatures}
                        />
                    </div>
                </>
            }
        </div>

        <Button
            size="md"
            onClick={save}
            disabled={form.isValid == false}
            variant="primary"
        >
            {
                form.isSubmitting
                ?
                <Spinner />
                :
                props.submitText
            }
        </Button>
    </>
}