import { Badge, Box, Button, Grid, SpeedDial, SpeedDialAction, SpeedDialIcon, Tooltip } from "@mui/material"
import { useEffect, useMemo, useState } from "react"
import { useTranslation } from "react-i18next";
import { useToast } from "../../context/ToastProvider";
import { usePosSession } from "../../context/pos/PosSessionContextProvider";
import { useChannelsQuery } from "../../hooks/queries/implementations/useChannelsQuery";
import { useLocalsQuery } from "../../hooks/queries/implementations/useLocalsQuery";
import { Local } from "../../hooks/api/Dtos/locals/Local";
import { usePosIntegrationsQuery } from "../../hooks/queries/implementations/usePosIntegrationsQuery";
import { useChannelProfilesQuery } from "../../hooks/queries/implementations/useChannelProfilesQuery";
import { usePrintersQuery } from "../../hooks/queries/implementations/usePrintersQuery";
import { usePreparationGroupsQuery } from "../../hooks/queries/implementations/usePreparationGroupsQuery";
import { PreparationGroup } from "../../hooks/api/Dtos/preparationgroups/PreparationGroup";
import SplitButton from "../Buttons/SplitButton";
import { CashDrawerIcon, EuroBadgeIcon, MicrowaveIcon, PercentIcon, ReceiptIcon } from "../../icons";
import { DiscountedItem, DiscountsModal } from "../Modals/DiscountsModal";
import { SessionItem } from "../../hooks/api/Dtos/sessions/SessionItem";
import { PaymentData, PaymentsModal } from "../Payments/PaymentsModal";
import { useTransactionMutator } from "../../hooks/mutators/useTransactionMutator";
import { PaymentAmountType } from "../../hooks/api/Dtos/payments/PaymentAmountType";
import { PreparationGroupDetailModal } from "../Orders/groups/PreparationGroupDetailModal";
import { PaymentHistoryModal } from "../PaymentsHistory/PaymentHistoryModal";
import { createPortal } from 'react-dom';
import { useActionAwaiter } from "../../hooks/useActionAwaiter";

const getCheckedItems = (group: PreparationGroup | undefined) =>  {
    if(group == undefined) {
        return { };
    }

    const result = {} as Record<string, boolean>;
    for(const item of group.items) {
        result[item.id] = true;
        for(const extra of item.extras) {
            result[extra.id] = true;
        }
    }
    return result;
}

interface SessionButtonsProps {
    readonly canPay: boolean;
    readonly canAddItems: boolean;
    readonly canRemoveItems: boolean;
    readonly canApplyDiscounts: boolean;
    readonly allowsInvoiceEscPosPrinting: boolean;
    readonly localId: string | undefined;
}
export const SessionButtons = ({
    canPay,
    canAddItems,
    canRemoveItems,
    canApplyDiscounts,
    allowsInvoiceEscPosPrinting,
    localId,
}: SessionButtonsProps) => {
    const { t } = useTranslation();
    const toast = useToast();
    const pos = usePosSession();
    const awaiter = useActionAwaiter();
    const transactionMutator = useTransactionMutator();

    const printersQuery = usePrintersQuery({
        page: 0,
    });

    const currentChannelQuery = useChannelsQuery({
        ids: [pos.cartSession.channelId],
        page: 0,
    })
    const currentChannel = useMemo(() => {
        if(currentChannelQuery.data.length == 0) {
            return undefined;
        }
        return currentChannelQuery.data[0];
    }, [currentChannelQuery.data])

    const currentProfileQuery = useChannelProfilesQuery(currentChannel == undefined ? undefined : {
        ids: [currentChannel.channelProfileId],
        page: 0,
    })
    const currentProfile = useMemo(() => {
        if(currentProfileQuery.data.length == 0) {
            return undefined;
        }
        return currentProfileQuery.data[0];
    }, [currentProfileQuery.data])

    const integrationQuery = usePosIntegrationsQuery(currentProfile?.posIntegrationId == undefined ? undefined : {
        ids: [currentProfile.posIntegrationId],
        page: 0,
    })

    const localsQuery = useLocalsQuery({})
    const localsMap = useMemo(() => localsQuery.data.reduce((r, l) => {
        r.set(l.id, l);
        return r;
    }, new Map<string, Local>()), [localsQuery.data])

    const preparationGroupQuery = usePreparationGroupsQuery(!pos.cartSession.sessionId ? undefined : {
        sessionIds: [pos.cartSession.sessionId],
        isCommited: false,
        page: 0,
    });
    const preparationGroup = useMemo(() => {
        if(preparationGroupQuery.data.length == 0) {
            return undefined;
        }
        return preparationGroupQuery.data[0];
    }, [preparationGroupQuery.data])
    

    const [printingConsumerBill, setPrintingConsumerBill] = useState(false);
    const [isOpeningCashDrawer, setIsOpeningCashDrawer] = useState(false);
    const [printingEndOfDayTotals, setPrintingEndOfDayTotals] = useState(false);
    const [isPrepareModalOpen, setIsPrepareModalOpen] = useState(false);
    const [discountsModalOpen, setDiscountsModalOpen] = useState(false);
    const [paymentsModalOpen, setPaymentsModalOpen] = useState(false);
    const [paymentHistoryModalOpen, setPaymentHistoryModalOpen] = useState(false);
    const [isItemCheckedMap, setIsItemCheckedMap] = useState<Record<string, boolean>>(() => getCheckedItems(preparationGroup))

    const paymentsDisabled = pos.cartSession.isSyncing || pos.cartSession.items.filter(m => !m.isPaid).length == 0;
    const kitchenOrdersEnabled = !pos.cartSession.isSyncing && ((preparationGroup != undefined && preparationGroup.items.length > 0) || !paymentsDisabled);

    const printerSettings = useMemo(() => {
        let canOpenCashDrawer = false;
        let canPrintBill = false;
        let canPrintEndOfDay = false;

        for(const p of printersQuery.data) {
            if(p.canOpenCashDrawer) {
                canOpenCashDrawer = true;
            }

            if(p.canPrintCloseDayTotals) {
                canPrintEndOfDay = true;
            }

            if(p.printConsumerBill) {
                canPrintBill = true;
            }

            if(canPrintBill && canOpenCashDrawer && canPrintEndOfDay) {
                break;
            }
        }

        if(integrationQuery.data.length == 0 || integrationQuery.data[0].isOnline != true || integrationQuery.data[0].allowsEscPosInvoices == false) {
            canPrintBill = false;
        }

        return {
            canOpenCashDrawer: canOpenCashDrawer,
            canPrintBill: canPrintBill,
            canPrintEndOfDay: canPrintEndOfDay,
        };
    }, [printersQuery.data, integrationQuery.data])

    const totalPendingItems = useMemo(() => preparationGroup?.items.reduce((r, i) => r + Math.abs(i.remainingQuantity), 0) ?? 0, [preparationGroup]);

    useEffect(() => setIsItemCheckedMap(getCheckedItems(preparationGroup)), [preparationGroup])

    const applyDiscount = async (selectableItems: DiscountedItem<SessionItem>[]) => {
        for(const {item, newDiscount, quantityToApply} of selectableItems) {
            if(item.discountPercentage == 0) {
                pos.cartSession.applyDiscount(item, quantityToApply, newDiscount);
            } else {
                pos.cartSession.applyDiscount(item, quantityToApply, 0);
            }
        }
    }

    const printBill = async () => {
        if(!allowsInvoiceEscPosPrinting){
            toast.error(t('invoiceEscPosPrintingNotSupported'));
            return;
        }

        if(printerSettings.canPrintBill == false){
            toast.error(t('printerMissing'));
            return;
        }

        try {
            setPrintingConsumerBill(true);
            await pos.printConsumerBill(localId);
            toast.success(t("printingBill"))
        } catch {
            toast.error(t("printingBillError"))
        } finally {
            setPrintingConsumerBill(false);
        }
    }
    
    const onClickOpenCashDrawer = async () => {
        try {
            setIsOpeningCashDrawer(true);
            await pos.openCashDrawer(localId);
            toast.success(t("openingCashDrawerMsg"));
        } catch {
            toast.error(t('unexpectedErrorHasOccurred'));
        } finally {
            setIsOpeningCashDrawer(false);
        }
    }

    const onEndDayTotals = async () => {
        try {
            setPrintingEndOfDayTotals(true);
            await pos.endOfDayClosing(localId)
            toast.success(t("endOfDayTotalsMsg"));
        } catch {
            toast.error(t('unexpectedErrorHasOccurred'));
        } finally {
            setPrintingEndOfDayTotals(false);
        }
    }
    
    const submitPayment = async (data: PaymentData) => {
        const response = await transactionMutator.create({
            channelId: pos.cartSession.channelId,
            customChargeMethodId: data.paymentMethodId,
            locationId: data.localId,

            email: data.email,
            vatNumber: data.vatNumber,
            observations: data.observations,
            amount: data.amount ?? 0,
            items: data.amountType == PaymentAmountType.Price ? undefined : data.selectedItems,
            tip: data.tip,
        });
        await awaiter.syncedTransaction(response.id);
    }

    if(canAddItems == false && canRemoveItems == false) {
        return <></>
    }
    
    return <>
        <Grid
            container
            spacing={2}
            sx={{
                flex: "0 0 auto",
                display: {
                    xs: 'none',
                    sm: 'none',
                    md: 'flex',
                    lg: "flex",
                    xl: "flex",
                },
            }}
        >
            <Grid 
                size={{
                    xs: 12,
                    sm: "grow"
                }}
            >
                <Badge
                    sx={{
                        width: "100%",
                        height: "3rem",
                        "& .MuiBadge-badge": {
                            backgroundColor: t => t.palette.primary.main,
                            color: t => t.palette.primary.light,
                            fontSize: '0.875rem',
                            minHeight: '1rem',
                        }
                    }}
                    invisible={preparationGroupQuery.isFirstLoading || !kitchenOrdersEnabled}
                    badgeContent={totalPendingItems}
                    anchorOrigin={{
                        vertical: 'top',
                        horizontal: 'left',
                    }}
                >
                    <Button
                        variant="outlined"
                        sx={{
                            width: "100%",
                            height: "3rem",
                        }}
                        loading={!!pos.cartSession.sessionId && preparationGroupQuery.isFirstLoading} 
                        disabled={!kitchenOrdersEnabled || totalPendingItems == 0} 
                        onClick={() => setIsPrepareModalOpen(true)}
                    >
                        {t("sendToPreparation")}
                    </Button>
                </Badge>
            </Grid>
            <Grid
                size={{
                    xs: 12,
                    sm: "grow"
                }}
            >
                <SplitButton
                    style={{
                        width: "100%",
                        height: "3rem",
                    }}
                    onClick={printBill}
                    isDisabled={paymentsDisabled}
                    isLoading={pos.cartSession.isSyncing || printingConsumerBill}
                    options={
                        canApplyDiscounts
                        ? 
                            [
                                {
                                    onClick: () => setDiscountsModalOpen(true),
                                    children: t("applyDiscount"),
                                }
                            ] 
                        : 
                            []
                        }
                    >
                    {t("bill")}
                </SplitButton>
            </Grid>
            <Grid
                size={{ 
                    xs: 12,
                }}
            >
                <Box
                    sx={{
                        display: "flex",
                        gap: 1,

                        "& .MuiButtonBase-root": {
                            height: "3rem",

                            "& svg": {
                                height: "80%",
                                width: "auto",
                            }
                        }
                    }}
                >
                    <SplitButton
                        variant="contained"
                        style={{
                            flexGrow: 1,
                            height: "3rem",
                        }} 
                        onClick={() => {
                            if(paymentsDisabled) {
                                toast.info(t("session.empty"))
                                return;
                            }

                            if(canPay == false) {
                                toast.info(t("integrationDoesNotAllowPayments"))
                                return;
                            }
                            setPaymentsModalOpen(true);
                        }}
                        isLoading={pos.cartSession.isSyncing}
                        options={
                            [
                                {
                                    onClick: () => setPaymentHistoryModalOpen(true),
                                    children: t("history"),
                                }
                            ]}
                        >
                        {t("payment")}
                    </SplitButton>
                    {
                        printerSettings.canOpenCashDrawer &&
                        <Tooltip title={t("openCashDrawer")}>
                            <Button 
                                variant="outlined"
                                onClick={onClickOpenCashDrawer}
                                loading={isOpeningCashDrawer}
                            >
                                <CashDrawerIcon />
                            </Button>
                        </Tooltip>
                    }
                    {
                        printerSettings.canPrintEndOfDay &&
                        <Tooltip title={t("printEndOfDayTotals")}>
                            <Button
                                variant="outlined"
                                onClick={onEndDayTotals}
                                loading={printingEndOfDayTotals}
                            >
                                <ReceiptIcon />
                            </Button>
                        </Tooltip>
                    }
                </Box>
            </Grid>
        </Grid>

        {
            createPortal(<Box
                sx={{
                    position: 'absolute',
                    bottom: 55,
                    right: 15,
                    display: {
                        xs: 'block',
                        sm: 'block',
                        md: 'none',
                        lg: "none",
                        xl: "none"
                    },

                    "& svg": {
                        width: 25,
                        height: "auto",
                    }
                }}
            >
                <SpeedDial
                    ariaLabel="session actions"
                    icon={<SpeedDialIcon />}
                    direction="up"
                >
                    <SpeedDialAction
                        icon={<MicrowaveIcon />}
                        slotProps={{
                            tooltip: {
                                title: t("sendToPreparation"),
                                open: true,
                            },
                        }}
                        onClick={() => {
                            if(kitchenOrdersEnabled == false || totalPendingItems == 0) {
                                toast.info(t("session.nothingToPrepare"))
                                return;
                            }
                            setIsPrepareModalOpen(true);
                        }}
                    />

                    {
                        canApplyDiscounts &&
                        <SpeedDialAction
                            icon={<PercentIcon />}
                            slotProps={{
                                tooltip: {
                                    title: t("applyDiscount"),
                                    open: true,
                                },
                            }}
                            onClick={() => {
                                if(paymentsDisabled) {
                                    toast.info(t("session.empty"))
                                    return;
                                }
                                setDiscountsModalOpen(true);
                            }}
                        />
                    }

                    <SpeedDialAction
                        icon={<EuroBadgeIcon />}
                        slotProps={{
                            tooltip: {
                                title: t("payment"),
                                open: true,
                            },
                        }}
                        onClick={() => {
                            if(paymentsDisabled) {
                                toast.info(t("session.empty"))
                                return;
                            }

                            if(canPay == false) {
                                toast.info(t("session.paymentNotAllowed"))
                                return;
                            }

                            setPaymentsModalOpen(true);
                        }}
                    />
                </SpeedDial>
            </Box>, document.body)
        }

        <PaymentsModal
            isOpen={paymentsModalOpen}
            channelId={pos.cartSession.channelId}
            localId={localId}
            items={pos.cartSession.items}
            onClose={() => setPaymentsModalOpen(false)} 
            onComplete={submitPayment}
        />
        <DiscountsModal
            isOpen={discountsModalOpen}
            onClose={() => setDiscountsModalOpen(false)}
            items={pos.cartSession.items}
            onApplyDiscount={applyDiscount}
        />
        <PreparationGroupDetailModal 
            group={isPrepareModalOpen ? preparationGroup : undefined} 
            onClose={() => setIsPrepareModalOpen(false)}
            localsMap={localsMap}
            currentLocalId={localId}

            checkedItems={isItemCheckedMap}
            onCheckedItemsChanged={setIsItemCheckedMap}
        />

        <PaymentHistoryModal
            isOpen={paymentHistoryModalOpen}
            onClose={() => setPaymentHistoryModalOpen(false)}
        />
    </>
}