import React, { useEffect, useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { MobileStepper, Skeleton, Stack } from '@mui/material';
import { SessionItem } from '../../hooks/api/Dtos/sessions/SessionItem';
import { useToast } from '../../context/ToastProvider';
import { useChannelProfilesQuery } from '../../hooks/queries/implementations/useChannelProfilesQuery';
import { QuantifiedItem } from '../Pickers/QuantifiedItemPicker';
import { Channel } from '../../hooks/api/Dtos/channels/Channel';
import { PaymentAmountType } from '../../hooks/api/Dtos/payments/PaymentAmountType';
import { Items } from '../../helpers/itemsHelpers';
import { Currency } from '../../helpers/currencyHelper';
import CustomModal, { ModalSize } from '../Modals/CustomModal';
import LoadingButton from '../Buttons/LoadingButton';
import { useChannelsQuery } from '../../hooks/queries/implementations/useChannelsQuery';
import { ChannelProfile } from '../../hooks/api/Dtos/channelProfiles/ChannelProfile';
import StepperContextProvider, { StepperContext } from '../../context/stepperContextProvider';
import { PaymentAmount } from './PaymentAmount';
import { PaymentCustomerOptions } from './PaymentCustomerOptions';

export enum PaymentStep {
    selectAmountType = 0,
    selectPaymentMethod = 1,
    customerData = 2,
    paymentSummary = 3,
}

export interface PaymentData {
    readonly vatNumber?: string;
    readonly email?: string;
    readonly observations?: string;
    readonly amountType: PaymentAmountType;
    readonly amount?: number;
    readonly selectedItems?: SessionItem[];
    readonly paymentMethodId: string;
    readonly tip: number;
    readonly localId: string | undefined;
}

interface Props {
    readonly isOpen: boolean;
    readonly items: SessionItem[];
    readonly channelId: string;
    readonly localId: string | undefined;
    readonly onClose: () => any;
    readonly onComplete: (data: PaymentData) => Promise<any>;
}

export const PaymentsModal: React.FC<Props> = ({
    isOpen,
    items,
    channelId,
    localId,
    onClose,
    onComplete
}) => {
    const { t, i18n } = useTranslation();
    const toast = useToast();

    const channelQuery = useChannelsQuery({
        ids: [channelId],
        page: 0,
        pageSize: 1,
        allowsSessionsOnly: true,
        includeDeleted: true,
    })
    const channel = useMemo(() => channelQuery.data.length == 0 ? undefined : channelQuery.data[0], [channelQuery.data])
    
    const profileQuery = useChannelProfilesQuery(channel == undefined ? undefined : {
        ids: [channel.channelProfileId],
        page: 0,
    })
    const channelProfile = useMemo(() => profileQuery.data.length == 0 ? undefined : profileQuery.data[0], [profileQuery.data])

    const [state, setState] = useState({
        activeStep: 0,
        isLoading: false,
        amountType: PaymentAmountType.Price,
        selectedItems: [] as QuantifiedItem<SessionItem>[],
        partialAmount: undefined as number | undefined,
        vatNumber: "",
        email: "",
        observations: "",
        paymentMethodId: "",
        tip: 0,
        localId: localId,
    })

    const getModalTitle = (profile: ChannelProfile, channel: Channel) => `${profile.name} ${channel.name} - ${state.activeStep >= steps.length ? steps[state.activeStep - 1].label : steps[state.activeStep].label}`;

    const bill = useMemo(() => !items ? 0 : Items.getTotalPrice(items.filter(item => item.isPaid == false)), [items]);
    const amountToPay = useMemo(() => {
        switch (state.amountType) {
            case PaymentAmountType.ItemsSelection:
                return Items.getTotalPrice(state.selectedItems.map(s => ({...s.item, quantity: s.quantity})));
            case PaymentAmountType.Price:
            default:
                return state.partialAmount ?? bill;
        }
    }, [state.amountType, state.partialAmount, state.selectedItems]);

    const steps = [
        {
            step: PaymentStep.selectAmountType,
            label: t("amountToPay"),
            component: 
                <StepperContext.Consumer>
                {
                    provided => (
                        <PaymentAmount
                            bill={bill}
                            unpaidItems={items.filter(item => item.isPaid == false)}
                            selectedItems={state.selectedItems}
                            partialAmount={state.partialAmount}
                            onPayAmountChanged={v => setState(s => ({ ...s, partialAmount: v }))}
                            amountType={state.amountType}
                            onAmountTypeChanged={v => setState(s => ({ ...s, amountType: v }))}
                            onSelectedItemsChanged={items => setState(s => ({ ...s, selectedItems: items }))}
                            onIsValidChanged={v => { 
                                if (provided.isInitialized == false) {
                                    return;
                                }
                                provided.setIsNextStepAllowed(v);
                            }}
                        />
                    )
                }
                </StepperContext.Consumer>
        },
        {
            step: PaymentStep.selectPaymentMethod,
            label: Currency.toCurrencyFormat({value: amountToPay, culture: i18n.language, currencyIso: "EUR" }) ,
            component: 
                <StepperContext.Consumer>
                {
                    provided => (
                        <PaymentCustomerOptions
                            localId={state.localId}
                            onLocalChanged={l => setState(s => ({...s, localId: l}))}

                            total={amountToPay}

                            tip={state.tip}
                            onTipChanged={v => setState(s => ({...s, tip: v}))}

                            email={state.email} 
                            onEmailChanged={(e) => setState(s => ({...s, email: e}))} 

                            vatNumber={state.vatNumber} 
                            onVatNumberChange={(v) => setState(s => ({...s, vatNumber: v}))}

                            observations={state.observations}
                            onObservationsChanged={(o) => setState(s => ({...s, observations: o}))}

                            paymentMethodId={state.paymentMethodId}
                            onPaymentMethodChanged={id => setState(s => ({...s, paymentMethodId: id}))} 

                            onIsValidChanged={v => provided.setIsNextStepAllowed(v)}
                        />
                    )
                }
                </StepperContext.Consumer>
        },
    ];

    const resetData = () => setState({
        activeStep: 0,
        amountType: PaymentAmountType.Price,
        partialAmount: undefined,
        vatNumber: "",
        email: "",
        tip: 0,
        observations: "",
        selectedItems: [],
        paymentMethodId: "",
        isLoading: false,
        localId: localId,
    })

    useEffect(() => {
        if(state.activeStep < steps.length) {
            return;
        }

        const successCallback = () => {
            onClose();
            resetData();
            toast.success(t("paymentSuccess"));
        };

        setState(s => ({
            ...s,
            isLoading: true,
        }))
        onComplete({
            vatNumber: state.vatNumber,
            email: state.email,
            observations: state.observations,
            amountType: state.amountType,
            amount: amountToPay,
            selectedItems: state.selectedItems?.map(i => ({
                ...i.item,
                quantity: i.quantity,
            })),
            paymentMethodId: state.paymentMethodId,
            tip: state.tip,
            localId: state.localId,
        })
        .then(successCallback)
        .catch(() => toast.error(t("paymentError")))
        .finally(() => setState(s => ({
            ...s,
            activeStep: steps.length -1,
            isLoading: false,
        })));
    }, [state.activeStep])

    useEffect(() => resetData(), [isOpen]);

    return (
    <StepperContextProvider>
        <StepperContext.Consumer>
        {
            provided => (
                <CustomModal
                    isOpen={isOpen} 
                    title={channel != undefined && channelProfile != undefined ? getModalTitle(channelProfile, channel) : <Skeleton variant="rounded" animation="wave"/>} 
                    size={ModalSize.Medium} 
                    onClose={onClose} 
                    disableCloseOutsideModal
                    footer={
                        <Stack
                            direction="row"
                            sx={{
                                width: "100%",
                                marginY: "0.25rem",
                            }}
                            spacing={2}
                        >
                            <MobileStepper
                                variant="dots"
                                steps={steps.length}
                                position="static"
                                activeStep={state.activeStep}
                                sx={{width: "100%", mb: -2}}
                                backButton={<LoadingButton 
                                    onClick={() => setState(s => ({...s, activeStep: s.activeStep - 1}))}
                                    disabled={state.activeStep === 0}
                                    style={{width: "100%", marginRight: "1rem"}}
                                >
                                    {t("back")}
                                </LoadingButton>}
                                nextButton={<LoadingButton 
                                    isLoading={state.isLoading} 
                                    disabled={!provided.isNextStepAllowed}
                                    onClick={() => setState(s => ({...s, activeStep: s.activeStep + 1}))}
                                    primaryButton 
                                    style={{width: "100%", marginLeft: "1rem"}}>
                                    {state.activeStep === steps.length - 1 ? t("confirm") : t("next")}
                                </LoadingButton>}
                            />
                        </Stack>
                    }
                >
                    { state.activeStep >= steps.length ? steps[steps.length - 1].component : steps[state.activeStep].component }
                </CustomModal>
            )
        }
        </StepperContext.Consumer>
    </StepperContextProvider>
    )
}