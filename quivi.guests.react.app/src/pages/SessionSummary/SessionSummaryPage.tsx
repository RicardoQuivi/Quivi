import { useEffect, useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import Dialog from "../../components/Shared/Dialog";
import LoadingButton from "../../components/Buttons/LoadingButton";
import { toast } from "react-toastify";
import { Page } from "../../layout/Page";
import { ButtonsSection } from "../../layout/ButtonsSection";
import { useQuiviTheme } from "../../hooks/theme/useQuiviTheme";
import { useBrowserStorageService } from "../../hooks/useBrowserStorageService";
import { useChannelContext, useCurrentPosIntegration } from "../../context/AppContextProvider";
import { useOrdersQuery } from "../../hooks/queries/implementations/useOrdersQuery";
import type { ReceiptLine } from "../../components/Receipt/ReceiptLine";
import type { ReceiptSubTotalLine } from "../../components/Receipt/ReceiptSubTotalLine";
import { Calculations } from "../../helpers/calculations";
import { CloseIcon, InfoIcon } from "../../icons";
import { MenuSelector } from "../../components/Menu/MenuSelector";
import { Link, Navigate, useNavigate } from "react-router";
import Receipt from "../../components/Receipt/Receipt";
import { PaymentSplitter } from "../../helpers/paymentSplitter";
import { useSessionsQuery } from "../../hooks/queries/implementations/useSessionsQuery";
import { PosIntegrationState } from "../../hooks/api/Dtos/posIntegrations/PosIntegrationState";
import { useMenuItemsQuery } from "../../hooks/queries/implementations/useMenuItemsQuery";
import type { MenuItem } from "../../hooks/api/Dtos/menuItems/MenuItem";
import { useTransactionsQuery } from "../../hooks/queries/implementations/useTransactionsQuery";
import { useShareEqualSettings } from "../../hooks/useShareEqualSettings";
import type { ReceiptTotalLine } from "../../components/Receipt/ReceiptTotalLine";
import { OrdersHelper } from "../../helpers/ordersHelper";
import BigNumber from "bignumber.js";

export const SessionSummaryPage = () => {
    const browserStorageService = useBrowserStorageService();
    const { t } = useTranslation();
    const theme = useQuiviTheme();
    const navigate = useNavigate();

    const channelContext = useChannelContext();
    const integration = useCurrentPosIntegration();
    const {
        channelId,
        features,
    } = channelContext;

    const sessionQuery = useSessionsQuery({
        channelId: channelId,
    });
    const ordersQuery = useOrdersQuery({
        channelIds: [channelId],
        page: 0,
    })

    const menuItemIds = useMemo(() => {
        if(sessionQuery.data == undefined) {
            return [];
        }

        const result = new Set<string>();
        for(const item of sessionQuery.data.items) {
            result.add(item.menuItemId);
            for(const extra of item.extras) {
                result.add(extra.menuItemId);
            }
        }
        return Array.from(result.values());
    }, [sessionQuery.data])

    const menuItemsQuery = useMenuItemsQuery(menuItemIds.length == 0 ? undefined : {
        channelId: channelId,
        ignoreCalendarAvailability: true,
        ids: menuItemIds,
        page: 0,
    })
    const menuItemsMap = useMemo(() => {
        const result = new Map<string, MenuItem>();
        for(const item of menuItemsQuery.data) {
            result.set(item.id, item);
        }
        return result;
    }, [menuItemsQuery.data])

    const session = sessionQuery.data;
    const transactionsQuery = useTransactionsQuery(session == undefined ? undefined : {
        sessionId: session.id,
        page: 0,
    });
    const shareEqualSettings = useShareEqualSettings(channelId);
    // const webApi10 = useWeb10API();
    // const [webClient] = useWebEvents();

    const [paySplitModalIsOpen, setPaySplitModalIsOpen] = useState(false);
    const [notMyBillModalIsOpen, setNotMyBillModalIsOpen] = useState(false);
    const [isClearingSession, _setIsClearingSession] = useState(false);

    const hasPaymentDivision = features.payAtTheTable.freePayment || features.payAtTheTable.itemSelectionPayment || features.payAtTheTable.splitBillPayment;
    const isLoading = sessionQuery.isFirstLoading || ordersQuery.isFirstLoading;

    const isEmpty = () => {
        const hasOrders = ordersQuery.isFirstLoading == false && ordersQuery.data.length > 0;
        if(hasOrders) {
            return false;
        }

        //This rule applied when merchant only allows the full table to be paid:
        //If a previous payment already exists then no more payments are allowed.
        //If the table is not fully paid then it has to be paid at the POS.
        const forceTableEmpty = !hasPaymentDivision && transactionsQuery.data.length > 0;
        return forceTableEmpty || session == undefined || session.items.length === 0;
    }

    const resume = useMemo(() => {
        let lines: ReceiptLine[] | undefined = undefined;
        let pendingOrdersTotal = BigNumber(0);

        if(isLoading == false) {
            lines = [];

            for(const i of sessionQuery.data?.items ?? []) {
                lines.push({
                    id: i.menuItemId,
                    discount: i.discountPercentage,
                    isStroke: i.isPaid,
                    info: undefined,
                    name: menuItemsMap.get(i.menuItemId)?.name,
                    amount: i.price,
                    quantity: i.quantity,
                    subItems: i.extras.map(s => ({
                        id: s.menuItemId,
                        discount: i.discountPercentage,
                        isStroke: false,
                        name: menuItemsMap.get(s.menuItemId)?.name ?? "",
                        amount: s.price,
                        quantity: s.quantity,
                    }))
                })
            }

            if(ordersQuery.isFirstLoading == false) {
                for(const order of ordersQuery.data) {
                    const orderTotal = OrdersHelper.getTotal(order);
                    pendingOrdersTotal.plus(orderTotal);

                    lines.push({
                        id: order.id,
                        discount: 0,
                        isStroke: false,
                        info: t("pay.requiringApproval"),
                        name: `${t("orders.order")} ${order.sequenceNumber}`,
                        amount: orderTotal,
                    })
                }
            }
        }

        const subTotals: ReceiptSubTotalLine[] = [];
        if(session != undefined && session.discount > 0) {
            subTotals.push({
                amount: session.discount,
                name: t("pay.discounts"),
            });
        }

        let pendingTotal = pendingOrdersTotal.toNumber();
        if(pendingTotal > 0) {
            subTotals.push({
                amount: pendingTotal,
                name: t("pay.requiringApproval"),
            });
        }
        
        subTotals.push({
            amount: session?.total ?? 0,
            name: t("pay.sessionTotal"),
        });

        if(session != undefined && session.paid > 0) {
            subTotals.push({
                amount: session.paid,
                name: t("pay.paidFor"),
            });
        }

        const total: ReceiptTotalLine = {
            amount: isLoading ? undefined : Calculations.roundUp(session?.unpaid ?? 0),
            name: t("pay.paymentPending")
        };
        
        return {
            lines: lines,
            subTotals: subTotals,
            total: total,
        };
    }, [sessionQuery.isFirstLoading, ordersQuery.isFirstLoading, menuItemsMap, session, ordersQuery.data, t])

    const clearSession = async () => {
        //TODO: clear session
        // setIsClearingSession(true);
        // try {
        //     const response = await webApi10.session.Delete(channelId);
        //     const jobId = response.data;
        //     if(jobId != undefined) {
        //         //TODO: await promise
        //         // const promise = new BackgroundJobPromise(jobId, webClient, webApi10);
        //         // await promise;
        //     }
        // } catch {
        //     toast.error(t("unexpectedError"), {
        //         icon: <ErrorIcon />,
        //     });
        // } finally {
        //     setIsClearingSession(false);
        //     setNotMyBillModalIsOpen(false);
        // }
    }

    useEffect(() => {
        browserStorageService.savePaymentDivision(null);
        browserStorageService.savePaymentDetails(null);
    }, [])

    const getFooter = () => {
        if(isLoading) {
            return;
        }

        if(features.payAtTheTable.isActive == false) {
            return;
        }
        const hasConsumptions = features.payAtTheTable.isActive && isEmpty() == false;
        return <ButtonsSection>
            {
                hasConsumptions && 
                <button
                    className="primary-button"
                    onClick={() => {
                        if(session != undefined && session.unpaid == 0) {
                            toast.info(t("pay.onlyRequiringApprovalAmountAvailable"), {
                                icon: <InfoIcon color={theme.primaryColor.hex} />,
                            });
                            return;
                        }
                        navigate(`/c/${channelContext.channelId}/session/pay/total`)
                    }}
                >
                    {t("pay.payTotalBill")}
                </button>
            }
            {
                hasConsumptions &&
                hasPaymentDivision &&
                <button
                    className="secondary-button"
                    type="button"
                    onClick={() => {
                        if(session != undefined && session.unpaid == 0) {
                            toast.info(t("pay.onlyRequiringApprovalAmountAvailable"), {
                                icon: <InfoIcon color={theme.primaryColor.hex} />,
                            });
                            return;
                        }
                        setPaySplitModalIsOpen(prevState => !prevState);
                    }}
                >
                    {t("pay.splitBill")}
                </button>
            }
            <MenuSelector />
            {
                features.payAtTheTable.allowsRemovingItemsFromSession == true && 
                features.payAtTheTable.allowsIgnoreBill == true &&
                isEmpty() == false &&
                <button className="secondary-button" type="button" onClick={() => setNotMyBillModalIsOpen(true)}>
                    {t("pay.notMyBill")}
                </button>
            }
        </ButtonsSection>
    }

    if(features.allowsSessions == false) {
        return <Navigate to="/" replace />
    }

    return <Page title={t("pay.title")} footer={getFooter()}>
        {
            integration.state == PosIntegrationState.Error 
            ?
            <div className="home__failed">
                <img src="/assets/illustrations/table-sync-failed.jpg" />
                <p>{t("pay.syncError")}</p>
            </div>
            :
            (
                isEmpty()
                ?
                <div className="flex flex-fd-c flex-ai-c flex-jc-c mt-10">
                    <h2 className="mb-4">{t("pay.emptySessionTitle")}</h2>
                    <p className="ta-c">{t("pay.emptySessionDescription")}</p>
                    <p className="ta-c">&nbsp;</p>
                </div>
                :
                <Receipt
                    header={t("pay.yourSession")}
                    items={resume.lines}
                    subTotals={resume.subTotals}
                    total={resume.total}
                />
            )
        }
        <Dialog
            onClose={() => setPaySplitModalIsOpen(prevState => !prevState)}
            isOpen={paySplitModalIsOpen}
        >
            {
                shareEqualSettings.isShareable && features.payAtTheTable.splitBillPayment &&
                <Link to={`/c/${channelContext.channelId}/session/pay/equal`} className="secondary-button mb-4">{t("pay.shareEqual")}</Link>
            }
            {
                session != undefined && PaymentSplitter.isShareItemsAvailable(session.items ?? []) && features.payAtTheTable.itemSelectionPayment &&
                <Link to={`/c/${channelContext.channelId}/session/pay/items`} className="secondary-button mb-4">{t("pay.chooseItems")}</Link>
            }
            {
                features.payAtTheTable.freePayment &&
                <Link to={`/c/${channelContext.channelId}/session/pay/custom`} className="secondary-button">{t("pay.payCustomAmount")}</Link>
            }
        </Dialog>
        <Dialog isOpen={notMyBillModalIsOpen} onClose={() => setNotMyBillModalIsOpen(false)} >
            <div className="container" style={{ paddingTop: "1.75rem", paddingBottom: "1.75rem" }}>
                <div className="modal__header">
                    <h3>{t("pay.notMyBillModal.header")}</h3>
                    <div className="close-icon" onClick={() => setNotMyBillModalIsOpen(false)}>
                        <CloseIcon />
                    </div>
                </div>
                <p className="mb-5">{t("pay.notMyBillModal.description")}</p>
                <div className="mt-5" style={{display: "flex", flexDirection: "row", columnGap: "0.875rem"}}>
                    <LoadingButton onClick={clearSession} primaryButton={true} isLoading={isClearingSession}>
                        {t("pay.notMyBillModal.ok")}
                    </LoadingButton>
                    <button type="button" className="secondary-button" onClick={() => setNotMyBillModalIsOpen(false)}>
                        {t("cancel")}
                    </button>
                </div>
            </div>
        </Dialog>
    </Page>
}