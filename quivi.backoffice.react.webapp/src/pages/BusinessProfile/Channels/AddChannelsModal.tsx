import { useEffect, useMemo, useState } from "react";
import { Trans, useTranslation } from "react-i18next";
import { ChannelProfile } from "../../../hooks/api/Dtos/channelProfiles/ChannelProfile";
import { Modal, ModalSize } from "../../../components/ui/modal/Modal";
import { ModalButtonsFooter } from "../../../components/ui/modal/ModalButtonsFooter";
import { Skeleton } from "../../../components/ui/skeleton/Skeleton";
import { useChannelProfilesQuery } from "../../../hooks/queries/implementations/useChannelProfilesQuery";
import { SingleSelect } from "../../../components/inputs/SingleSelect";
import { InfoIcon } from "../../../icons";
import { Tooltip } from "../../../components/ui/tooltip/Tooltip";
import Button from "../../../components/ui/button/Button";
import { useChannelMutator } from "../../../hooks/mutators/useChannelMutator";
import { useToast } from "../../../layout/ToastProvider";
import { TextField } from "../../../components/inputs/TextField";

interface NewChannel {
    name: string;
}

interface Props {
    readonly isOpen: boolean,
    readonly onClose: () => void,
}

export const AddChannelsModal = (props: Props) => {
    const { t } = useTranslation();
    const mutator = useChannelMutator();
    const toast = useToast();

    const profilesQuery = useChannelProfilesQuery({
        page: 0,
        pageSize: undefined,
    });
    const defaultProfile = useMemo(() => {
        if(profilesQuery.data.length == 0) {
            return undefined;
        }
        return profilesQuery.data[0];
    }, [profilesQuery.data])

    const [isSubmitting, setIsSubmitting] = useState(false);
    const [profile, setProfile] = useState<ChannelProfile | undefined>(defaultProfile);
    const [startValue, setStartValue] = useState("");
    const [endValue, setEndValue] = useState("");
    const [channelsToAdd, setNewQrCodes] = useState<NewChannel[]>([]);

    const reset = () => {
        setStartValue("");
        setEndValue("");
        setNewQrCodes([]);
        setProfile(defaultProfile);
        setIsSubmitting(false);
    }

    const identifierParser = (currentIdentifier: string) => {
        const separator = ':)';
        const numericPart = currentIdentifier.match(/\d+/);
        const template = currentIdentifier.replace(/\d+/, separator);
        
        if (!numericPart) {
            return null;
        }

        return {
            numeric: parseInt(numericPart.toString()),
            template: template,
            placeholder: separator,
        };
    }

    const getIdentifiersRange = (startValue: string, endValue: string) => {
        if (!startValue || !endValue) {
            //toast.warning({title: t("WebDashboard.ValidationError")!, message: t("WebDashboard.SetRangeValues")});
            return null;
        }
        
        const startValueParsed = identifierParser(startValue);
        const endValueParsed = identifierParser(endValue);

        if (startValueParsed == null || endValueParsed == null) {
            //toast.warning({title: t("WebDashboard.ValidationError")!, message: t("WebDashboard.QrCodeRangeErrorMsg")});
            return null;
        }

        if (startValueParsed.template != endValueParsed.template) {
            //toast.warning({title: t("WebDashboard.ValidationError")!, message: t("WebDashboard.QrCodeRangeSequenceError")});
            return null;
        }

        if (startValueParsed.numeric > endValueParsed.numeric) {
            //toast.warning({title: t("WebDashboard.ValidationError")!, message: t("WebDashboard.StartEndValueOutOfRange")});
            return null;
        }

        const template = startValueParsed.template;
        const placeholder = startValueParsed.placeholder;
        
        const result: string[] = [];
        for (let i = startValueParsed.numeric; i <= endValueParsed.numeric; i++) {
            const value = template.replace(placeholder, i.toString());
            result.push(value);
        }

        return result;
    }

    const onClickGenerateBtn = () => {
        const identifiers = getIdentifiersRange(startValue, endValue);
        if (!identifiers)
            return;

        setNewQrCodes(identifiers.map(item => ({ name: item })));
    }

    const onChangeIdentifier = (item: NewChannel, newValue: string) => {
        item.name = newValue;
        setNewQrCodes(prev => [...prev]);
    }

    const onClickConfirmBtn = async () => {
        if (channelsToAdd.some(item => !item.name)){
            //toast.warning({title: t("WebDashboard.ValidationError")!, message: t("WebDashboard.SomeQrCodesIdentifiersNotFilled")});
            return;
        }

        if(profile == undefined) {
            toast.error(t("common.operations.failure.generic"));
            return;
        }

        try {
            setIsSubmitting(true)
            await mutator.create({
                data: channelsToAdd.map(s => ({
                    name: s.name,
                })),
                channelProfileId: profile.id,
            })
            toast.success(t("common.operations.success.new"));
        } catch {
            toast.error(t("common.operations.failure.generic"));
        } finally {
            setIsSubmitting(false)
            props.onClose();
        }
    }

    useEffect(() => setProfile(p => p ?? defaultProfile), [defaultProfile])
    useEffect(() => {
        if (!props.isOpen) {
            return;
        }

        reset();
    }, [props.isOpen]);


    return <Modal
        isOpen={props.isOpen}
        onClose={() => props.onClose()}
        size={ModalSize.Medium}
        title={t('common.operations.new', {
            name: t("common.entities.channels"),
        })}
        footer={(
            <ModalButtonsFooter 
                primaryButton={{
                    content: t("common.confirm"),
                    disabled: channelsToAdd.length == 0,
                    onClick: onClickConfirmBtn,
                    isLoading: isSubmitting,
                }}
                secondaryButton={{
                    content: t("common.close"),
                    onClick: () => props.onClose(),
                }}
            />
        )}
    >
        <div className="grid grid-cols-1 sm:grid-cols-1 md:grid-cols-1 lg:grid-cols-1 gap-4">
            <div>
                {
                    profilesQuery.isFirstLoading
                    ?
                    <Skeleton className="w-full h-full"/>
                    :
                    <SingleSelect
                        options={profilesQuery.data}
                        value={profile}
                        getId={e => e?.id ?? ""}
                        onChange={setProfile}
                        render={e => e?.name ?? ""}
                        label={t(`common.entities.channelProfile`)}
                    />
                }
            </div>
            
            <div className="h-px w-full bg-gray-300 my-4" />

            <div className="text-sm text-gray-500 dark:text-gray-400 inline-flex">
                <p>
                    <Trans
                        i18nKey="pages.channels.defineCreateInterval"
                        components={{
                            br: <br/>,
                        }}
                    />
                </p>
                &nbsp;
                <Tooltip message={t("pages.channels.defineCreateIntervalDescription")}>
                    <InfoIcon />
                </Tooltip>
            </div>

            <div className="grid grid-cols-2 sm:grid-cols-2 gap-4">
                <TextField
                    type="text"
                    label={t("common.from")}
                    placeholder="Ex: 1"
                    onKeyUp={(e) => e.key === "Enter" && onClickGenerateBtn()}
                    onChange={setStartValue}
                    className="w-full"
                    value={startValue}
                />
                <TextField
                    type="text"
                    label={t("common.to")}
                    placeholder="Ex: 10"
                    onKeyUp={(e) => e.key === "Enter" && onClickGenerateBtn()}
                    onChange={setEndValue}
                    value={endValue}
                    className="w-full"
                />
            </div>
            <Button
                onClick={onClickGenerateBtn}
                disabled={!startValue || !endValue}
            >
                {t("pages.channels.generate")}
            </Button>

            {
                channelsToAdd.length > 0 &&
                <>
                    <div className="h-px w-full bg-gray-300 my-4" />
                    <div className="grid grid-cols-2 gap-4">
                        {channelsToAdd.map((c, index) => 
                        <TextField
                            key={index}
                            label={`${t("common.entities.channel")} ${index + 1}`}
                            type="text"
                            className="w-full"
                            value={c.name}
                            onChange={(e) => onChangeIdentifier(c, e)}
                        />)}
                    </div>
                </>
            }
        </div>
    </Modal>
}