import { Badge, Box, Button, Grid, Tooltip } from "@mui/material"
import { useEffect, useMemo, useState } from "react"
import { useTranslation } from "react-i18next";
import { useToast } from "../../context/ToastProvider";
import { usePosSession } from "../../context/pos/PosSessionContextProvider";
import { useChannelsQuery } from "../../hooks/queries/implementations/useChannelsQuery";
import { useLocalsQuery } from "../../hooks/queries/implementations/useLocalsQuery";
import { Local } from "../../hooks/api/Dtos/locals/Local";
import { usePosIntegrationsQuery } from "../../hooks/queries/implementations/usePosIntegrationsQuery";
import { useChannelProfilesQuery } from "../../hooks/queries/implementations/useChannelProfilesQuery";
import { useWebEvents } from "../../hooks/signalR/useWebEvents";
import { usePrintersQuery } from "../../hooks/queries/implementations/usePrintersQuery";
import { usePreparationGroupsQuery } from "../../hooks/queries/implementations/usePreparationGroupsQuery";
import { PreparationGroup } from "../../hooks/api/Dtos/preparationgroups/PreparationGroup";
import SplitButton from "../Buttons/SplitButton";
import { CashDrawerIcon, ReceiptIcon } from "../../icons";
import { DiscountedItem, DiscountsModal } from "../Modals/DiscountsModal";
import { SessionItem } from "../../hooks/api/Dtos/sessions/SessionItem";
import { PaymentData, PaymentsModal } from "../Payments/PaymentsModal";
import { useTransactionMutator } from "../../hooks/mutators/useTransactionMutator";
import { PaymentAmountType } from "../../hooks/api/Dtos/payments/PaymentAmountType";
import { TransactionSyncedPromise } from "../../hooks/signalR/promises/TransactionSyncedPromise";
import { useTransactionsApi } from "../../hooks/api/useTransactionsApi";

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
    const webEvents = useWebEvents();
    const pos = usePosSession();
    const transactionApi = useTransactionsApi();
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
    

    const [isPrepareModalOpen, setIsPrepareModalOpen] = useState(false);
    const [discountsModalOpen, setDiscountsModalOpen] = useState(false);
    const [paymentsModalOpen, setPaymentsModalOpen] = useState(false);
    const [paymentHistoryModalOpen, setPaymentHistoryModalOpen] = useState(false);
    const [isItemCheckedMap, setIsItemCheckedMap] = useState<Record<string, boolean>>(getCheckedItems(preparationGroup))

    const paymentsDisabled = pos.cartSession.isSyncing || pos.cartSession.items.filter(m => !m.isPaid).length == 0;
    const kitchenOrdersEnabled = !pos.cartSession.isSyncing && ((preparationGroup != undefined && preparationGroup.items.length > 0) || !paymentsDisabled);

    const {
        canOpenCashDrawer,
        canPrintBill,
        canPrintEndOfDay
    } = useMemo(() => {
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

    const totalPendingItems = useMemo(() => preparationGroup?.items.reduce((r, i) => r + Math.abs(i.remainingQuantity), 0) ?? 0, [preparationGroupQuery]);

    useEffect(() => setIsItemCheckedMap(getCheckedItems(preparationGroup)), [preparationGroupQuery.isFirstLoading, preparationGroupQuery.data])

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

        if(canPrintBill == false){
            toast.error(t('printerMissing'));
            return;
        }

        try {
            await pos.printConsumerBill(localId);
            toast.success(t("printingBill"))
        } catch {
            toast.error(t("printingBillError"))
        }
    }
    
    const onClickOpenCashDrawer = async () => {
        try {
            await pos.openCashDrawer(localId);
            toast.success(t("openingCashDrawerMsg"));
        } catch {
            toast.error(t('unexpectedErrorHasOccurred'));
        }
    }

    const onEndDayTotals = async () => {
        try {
            // await posApi.tools.EndOfDayClosing({
            //     accessToken: context.merchant.token,
            //     employeeToken: context.user.employeeToken ?? "",
            //     locationId: localId,
            // })
            toast.success(t("endOfDayTotalsMsg"));
        } catch {
            toast.error(t('unexpectedErrorHasOccurred'));
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
        await new TransactionSyncedPromise(response.id, webEvents.client, async () => {
            const result = await transactionApi.get({
                ids: [response.id],
                page: 0,
                pageSize: 1,
            });

            if(result.data.length == 0) {
                return undefined;
            }
            return result.data[0];
        });
    }

    if(canAddItems == false && canRemoveItems == false) {
        return <></>
    }
    
    return <>
        <Grid container gap={1} style={{flex: "0 0 auto",}}>
            <Grid size={{xs: 12, sm: "grow"}}>
                <Badge
                    sx={{
                        "& .MuiBadge-badge": {
                            backgroundColor: '#6c757d',
                            color: '#fff',
                            fontSize: '0.875rem',
                            minHeight: '1rem',
                        }
                    }}
                    invisible={preparationGroupQuery.isFirstLoading || !kitchenOrdersEnabled}
                    badgeContent={totalPendingItems}
                    style={{ width: "100%", height: "3rem" }}
                    anchorOrigin={{
                        vertical: 'top',
                        horizontal: 'left',
                    }}
                >
                    <Button
                        variant="contained"
                        sx={{
                            width: "100%",
                            height: "3rem"
                        }}
                        loading={!!pos.cartSession.sessionId && preparationGroupQuery.isFirstLoading} 
                        disabled={!kitchenOrdersEnabled || totalPendingItems == 0} 
                        onClick={() => setIsPrepareModalOpen(true)}
                    >
                        {t("sendToPreparation")}
                    </Button>
                </Badge>
            </Grid>
            <Grid size={{xs: 12, sm: "grow"}}>
                <SplitButton
                    style={{
                        width: "100%",
                        height: "3rem"
                    }}
                    onClick={printBill}
                    isDisabled={paymentsDisabled}
                    isLoading={pos.cartSession.isSyncing}
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
            <Grid size={{xs: 12 }}>
                <Box sx={{display: "flex", gap: 1}}>
                    <SplitButton
                        style={{ flexGrow: 1, height: "3rem" }} 
                        onClick={() => {
                            if(paymentsDisabled) {
                                toast.info(t("emptySession"))
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
                        canOpenCashDrawer &&
                        <Tooltip title={t("openCashDrawer")}>
                            <Button variant="contained" sx={{height: "3rem", width: "3rem"}} onClick={onClickOpenCashDrawer}>
                                <CashDrawerIcon />
                            </Button>
                        </Tooltip>
                    }
                    {
                        canPrintEndOfDay &&
                        <Tooltip title={t("printEndOfDayTotals")}>
                            <Button variant="contained" sx={{height: "3rem", width: "3rem"}} onClick={onEndDayTotals}>
                                <ReceiptIcon />
                            </Button>
                        </Tooltip>
                    }
                </Box>
            </Grid>
        </Grid>

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
        {/*
        <PreparationGroupDetailModal 
            group={isPrepareModalOpen ? preparationGroup : undefined} 
            onClose={() => setIsPrepareModalOpen(false)}
            locationsMap={localsMap}
            currentLocationId={localId}

            checkedItems={isItemCheckedMap}
            onCheckedItemsChanged={setIsItemCheckedMap}
        />
        <PaymentHistoryModal
            isOpen={paymentHistoryModalOpen}
            onClose={() => setPaymentHistoryModalOpen(false)}
        /> */}
    </>
}