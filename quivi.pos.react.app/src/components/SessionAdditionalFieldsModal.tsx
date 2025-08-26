import { Box, Checkbox, Grid, Skeleton, TextField } from "@mui/material";
import { ConfigurableField, ConfigurableFieldType } from "../hooks/api/Dtos/configurablefields/ConfigurableField";
import { NumberInputField } from "./Inputs/NumberInput";
import { JSX, useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useChannelsQuery } from "../hooks/queries/implementations/useChannelsQuery";
import CustomModal, { ModalSize } from "./Modals/CustomModal";
import LoadingButton from "./Buttons/LoadingButton";
import { usePosSession } from "../context/pos/PosSessionContextProvider";
import { useFormik } from "formik";
import * as Yup from "yup";
import { useChannelProfilesQuery } from "../hooks/queries/implementations/useChannelProfilesQuery";
import { SessionAdditionalInformation } from "../hooks/api/Dtos/sessionAdditionalInformations/SessionAdditionalInformation";
import { useSessionAdditionalInformationsMutator } from "../hooks/mutators/useSessionAdditionalInformationsMutator";
import { useToast } from "../context/ToastProvider";

interface Props {
    readonly isOpen: boolean;
    readonly additionalInfo: SessionAdditionalInformation[];
    readonly fields: ConfigurableField[];
    readonly onClose: () => any;
}
export const SessionAdditionalFieldsModal = (props: Props) => {
    const { t } = useTranslation();
    const pos = usePosSession();
    const toast = useToast();

    const channelsQuery = useChannelsQuery(!pos.cartSession.channelId ? undefined : {
        ids: [pos.cartSession.channelId],
        allowsSessionsOnly: true,
        includeDeleted: true,
        page: 0,
        pageSize: 1,
    })
    const channel = useMemo(() => channelsQuery.data.length == 0 ? undefined : channelsQuery.data[0], [channelsQuery.data])

    const profileQuery = useChannelProfilesQuery(channel == undefined ? undefined : {
        ids: [channel.channelProfileId],
        page: 0,
    })
    const profile = useMemo(() => profileQuery.data.length == 0 ? undefined : profileQuery.data[0], [profileQuery.data])

    const mutator = useSessionAdditionalInformationsMutator();

    const [state, setState] = useState({
        isLoading: false,
    })

    const additionalInfoMap = useMemo(() => {
        const map = new Map<string, SessionAdditionalInformation>();
        for(const info of props.additionalInfo) {
            map.set(info.id, info);
        }
        return map;
    }, [props.additionalInfo])

    const formik = useFormik({
        enableReinitialize: true,
        initialValues: props.fields.reduce((result, field) => {
            const info = additionalInfoMap.get(field.id);
            if(info) {
                result[field.id] = info.value;
            } else {
                const value = field.defaultValue ?? (field.type == ConfigurableFieldType.Number ? "1" : "");
                result[field.id] = value;
            }

            if(field.type == ConfigurableFieldType.Check) {
                result[field.id] = ![undefined, false, "", "0", "false"].includes(result[field.id]);
            }

            return result;
        }, {} as any),
        validationSchema: Yup.object(props.fields.reduce((result, field) => {
            let validation = field.type == ConfigurableFieldType.Number 
                                ? 
                                    Yup.number() 
                                :
                                (
                                    field.type == ConfigurableFieldType.Check
                                    ?
                                        Yup.boolean().transform(value => ![undefined, false, "", "0", "false"].includes(value))
                                                    .oneOf(field.isRequired ? [true] : [false, true], t("required")!)
                                    :
                                        Yup.string()
                                );

            if(field.isRequired) {
                validation = validation.required(t("required")!);
            }
            
            result[field.id] = validation;
            return result;
        }, {} as any)),
        onSubmit: async (values) => {
            if(!!pos.cartSession.sessionId) {
                setState(s => ({...s, isLoading: true}));
                try {
                    await mutator.upsert(pos.cartSession.sessionId, values);
                    toast.success(t("sessionInformationUpdated"));
                } catch {
                    toast.error(t('unexpectedErrorHasOccurred'));
                } finally {
                    setState(s => ({...s, isLoading: false}));
                }
            }
            props.onClose();
        },
        initialStatus: false,
    });

    const getTitle = () => {
        return <Box sx={{display: "flex", justifyContent: "center"}}>
            {
                channel == undefined || profile == undefined
                ?
                <>
                    <Skeleton animation="wave" sx={{ width: 80, display: "inline-block" }} />&nbsp; - {t("sessionInformation")}
                </>
                :
                <>
                    {profile.name} {channel.name} - {t("sessionInformation")}
                </>
            }
        </Box>
    }

    return (
        <CustomModal 
            isOpen={props.isOpen}
            title={getTitle()}
            onClose={props.onClose}
            hideClose={props.additionalInfo.length == 0}
            size={ModalSize.Small}
            footer={
                <Grid container sx={{width: "100%", margin: "1rem 0.25rem"}} spacing={1}>
                    <Grid size="grow">
                        <LoadingButton
                            isLoading={state.isLoading}
                            onClick={() => formik.handleSubmit()}
                            primaryButton
                            style={{width: "100%"}}
                            disabled={formik.isValid == false}
                        >
                            {t("confirm")}
                        </LoadingButton>
                    </Grid>
                </Grid>
            }
        >
            <Grid container spacing={2} justifyContent="center">
            {
                props.fields.map(f => (
                    <Grid key={f.id} size="grow">
                        <ConfigurableFieldInput
                            field={f}
                            onChange={(v) => {
                                formik.setFieldValue(f.id, v, true);
                                formik.setFieldTouched(f.id, true, false);
                            }}
                            errorMessage={(formik.touched[f.id] || formik.status == true) && formik.errors[f.id] ? formik.errors[f.id]?.toString() : undefined}
                            onBlur={formik.handleBlur}
                            value={formik.values[f.id]}
                        />
                    </Grid>
                ))
            }
            </Grid>
        </CustomModal>
    )
}

interface ConfigurableFieldInputProps  {
    readonly field: ConfigurableField;
    readonly value: any;
    readonly errorMessage?: string;
    readonly onChange: (value: any) => any;
    readonly onBlur: React.FocusEventHandler<HTMLInputElement | HTMLTextAreaElement | HTMLButtonElement>;
}
const ConfigurableFieldInput = (props: ConfigurableFieldInputProps): JSX.Element => {
    switch(props.field.type)
    {
        case ConfigurableFieldType.Check: return <CheckConfigurableField {...props} />;
        case ConfigurableFieldType.LongText: return <TextConfigurableField {...props} />;
        case ConfigurableFieldType.Text: return <TextConfigurableField {...props} />;
        case ConfigurableFieldType.Number: return <NumberConfigurableField {...props} />;
    }
}

const CheckConfigurableField = (props: ConfigurableFieldInputProps) => {
    const value = !!props.value;
    return (
        <Checkbox
            id={props.field.id}
            name={props.field.id}
            checked={value}
            value={value}
            onChange={(e) => props.onChange(!value)}
            onBlur={props.onBlur}
            color="primary"
        />
    )
}

const TextConfigurableField = (props: ConfigurableFieldInputProps) => {
    return (
        <TextField
            id={props.field.id}
            name={props.field.id}
            value={props.value}
            onChange={(e) => props.onChange(e.target.value)}
            onBlur={props.onBlur}
            label={props.field.name}

            fullWidth
            error={!!props.errorMessage}
            helperText={props.errorMessage}
            variant="outlined"
            color="primary"
            autoComplete="off"
            type="text"
            required={props.field.isRequired}
            multiline={props.field.type == ConfigurableFieldType.LongText}
            minRows={props.field.type == ConfigurableFieldType.LongText ? 3 : undefined}
            slotProps={{
                htmlInput: {
                    maxLength: 1000
                }
            }}
        />
    )
}

const NumberConfigurableField = (props: ConfigurableFieldInputProps) => {
    return <NumberInputField
        name={props.field.id}
        value={props.value}
        errorMessage={props.errorMessage}
        label={props.field.name}
        onChange={props.onChange}
        onBlur={props.onBlur}

        minValue={0}
    />
}