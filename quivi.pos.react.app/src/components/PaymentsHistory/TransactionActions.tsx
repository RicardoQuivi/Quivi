import { useEffect, useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { Divider, FormControl, FormGroup, Grid, Skeleton, Stack, TextField, Tooltip, Typography } from "@mui/material";
import { useToast } from "../../context/ToastProvider";
import { Transaction } from "../../hooks/api/Dtos/transactions/Transaction";
import { useTransactionMutator } from "../../hooks/mutators/useTransactionMutator";
import { Currency } from "../../helpers/currencyHelper";
import { SummaryBox } from "../common/SummaryBox";
import CurrencySpan from "../Currency/CurrencySpan";
import DecimalInput from "../Inputs/DecimalInput";
import ConfirmButton from "../Buttons/ConfirmButton";
import { usePosIntegrationsQuery } from "../../hooks/queries/implementations/usePosIntegrationsQuery";
import { useChannelsQuery } from "../../hooks/queries/implementations/useChannelsQuery";
import { useChannelProfilesQuery } from "../../hooks/queries/implementations/useChannelProfilesQuery";

const transactionFullAmount = (item: Transaction) => item.payment + item.tip;

interface Props {
    readonly transaction: Transaction;
    readonly onRefundSuccess: (amount: number) => any;
}
export const TransactionActions = (props: Props) => {
    const { i18n, t } = useTranslation();
    const transactionMutator = useTransactionMutator();
    const toast = useToast();
    
    const channelQuery = useChannelsQuery({
        ids: [props.transaction.channelId],
        page: 0,
    })
    const channel = useMemo(() => channelQuery.data.length == 0 ? undefined : channelQuery.data[0], [channelQuery.data]);

    const profileQuery = useChannelProfilesQuery(channel == undefined ? undefined :{
        ids: [channel.id],
        page: 0,
    })
    const profile = useMemo(() => profileQuery.data.length == 0 ? undefined : profileQuery.data[0], [profileQuery.data]);

    const posIntegrationQuery = usePosIntegrationsQuery(profile == undefined ? undefined : {
        ids: [profile.posIntegrationId],
        page: 0,
        pageSize: 1,
    })
    const posIntegration = useMemo(() => posIntegrationQuery.data.length == 0 ? undefined : posIntegrationQuery.data[0], [posIntegrationQuery.data]);

    const [state, setState] = useState(() => ({
        amount: 0,
        refundReason: "",
        bypassAcquirerErrors: false,
        bypassAcquirerVisible: false,
    }))

    useEffect(() => setState({
        amount: transactionFullAmount(props.transaction),
        refundReason: "",
        bypassAcquirerErrors: false,
        bypassAcquirerVisible: false,
    }), [props.transaction])
    
    const amountErrorMsg = (valueTxt: string): string | null => {
        const value = Number(valueTxt);

        if (!valueTxt?.length)
            return t("WebDashboard.AmountError_Required");

        if (value !== 0 && !value)
            return t("WebDashboard.AmountError_Invalid");

        if (value <= 0)
            return t("WebDashboard.AmountError_MinValue", { min: Currency.toCurrencyFormat({ value: 0.01, culture: i18n.language}) });

        const maxAmount = transactionFullAmount(props.transaction);
        if (value > maxAmount) {
            return t("WebDashboard.AmountError_MaxValue", { max: Currency.toCurrencyFormat({ value: maxAmount, culture: i18n.language}) });
        }
        return null;
    }

    const processRefund = async (item: Transaction, isCancellation: boolean) => {
        await transactionMutator.refund(item, {
            amount: Number(state.amount),
            ignoreAcquireRefundErrors: state.bypassAcquirerErrors ?? false,
            isCancellation: isCancellation,
            refundReason: state.refundReason,
        });
        toast.success(t('WebDashboard.RefundPerformedSuccessfully'));
        props.onRefundSuccess(state.amount);
    }

    const getRefundButton = (isCancelation: boolean) => {
        const isRefundAvailable = posIntegration?.allowsRefunds != false;
        const baseButton = (
            <ConfirmButton
                primaryButton={!isCancelation}
                confirmText={t('confirm')}
                onAction={() => processRefund(props.transaction, isCancelation)}
                disabled={isRefundAvailable == false || props.transaction.refundedAmount > 0}
                style={{
                    width: "100%",
                }}
            >
            {
                posIntegration == undefined
                ?
                <Skeleton animation="wave" />
                :
                (
                    isCancelation
                    ?
                    t("cancellation")
                    :
                    t("refund")
                )
            }
            </ConfirmButton>
        )

        if(isRefundAvailable) {
            if((props.transaction.refundedAmount ?? 0) > 0) {
                return <Tooltip
                    title={t("alreadyRefunded")}
                >
                    {baseButton}
                </Tooltip>
            }
            return baseButton;
        }

        return <Tooltip
            title={t("refundNotSupported")}
        >
            {baseButton}
        </Tooltip>
    }

    return <>
        <SummaryBox
            items={[
                {
                    label: t("total"),
                    content: <CurrencySpan value={props.transaction.payment + props.transaction.tip} />
                },
                {
                    label: t("amount"),
                    content: <CurrencySpan value={props.transaction.payment} />
                },
                {
                    label: t("tip"),
                    content: <CurrencySpan value={props.transaction.tip} />
                },
            ]}
        />
        <Divider>
            <Typography
                variant="h5"
                textAlign="center"
            >
                {t("paymentHistory.refund")}
            </Typography>
        </Divider>
        <Stack direction="column" spacing={2}>
            <FormControl component="fieldset" variant="standard">
                {/* <ValidationMessage errorMessages={apiErrors} propertyPath={""} /> */}
                <FormGroup>
                    <FormControl fullWidth>
                        <DecimalInput
                            label={t("amount")}
                            textFieldProps={{
                                variant: "outlined",
                            }}
                            value={Number(state.amount)}
                            errorMsg={amountErrorMsg(state.amount.toString())}
                            endAdornment={<Typography variant="h6">€</Typography>}
                            style={{
                                fontSize: "1.5rem",
                                textAlign: "center"
                            }}
                            onChange={(newValue) => setState(s => ({ ...s, amount: newValue }))} />
                    </FormControl>
                    {/* <ValidationMessage errorMessages={apiErrors} propertyPath={"Amount"} /> */}
                </FormGroup>
                
                <FormGroup sx={{mt: "0.5rem"}}>
                    <FormControl fullWidth>
                        <TextField
                            label={t("observations")}
                            slotProps={{
                                htmlInput: {
                                    maxLength: 256,
                                }
                            }}
                            multiline
                            rows={4}
                            onChange={(e) => setState(s => ({ ...s, refundReason: e.target.value }))}
                        />
                    </FormControl>
                    {/* <ValidationMessage errorMessages={apiErrors} propertyPath={"RefundReason"} /> */}
                </FormGroup>
            </FormControl>

            <Grid
                container
                spacing={2}
            >
                <Grid
                    size={{
                        xs: 12,
                        sm: 6,
                    }}
                >
                    {getRefundButton(true)}
                </Grid>
                <Grid
                    size={{
                        xs: 12,
                        sm: 6,
                    }}
                >
                    {getRefundButton(false)}
                </Grid>
            </Grid>
        </Stack>
    </>
}