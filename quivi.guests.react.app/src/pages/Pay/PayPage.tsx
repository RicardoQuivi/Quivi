import { useFormik } from "formik";
import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { toast } from "react-toastify";
import * as Yup from "yup";
import Dialog from "../../components/Shared/Dialog";
import { Page } from "../../layout/Page";
import { useBrowserStorageService } from "../../hooks/useBrowserStorageService";
import { useQuiviTheme } from "../../hooks/theme/useQuiviTheme";
import { Calculations } from "../../helpers/calculations";
import { CloseIcon, InfoIcon } from "../../icons";
import { Formatter } from "../../helpers/formatter";
import { Alert, Grid } from "@mui/material";
import { useChannelContext } from "../../context/AppContextProvider";
import { useSessionsQuery } from "../../hooks/queries/implementations/useSessionsQuery";
import { Navigate, useNavigate } from "react-router";
import { useAuth } from "../../context/AuthContext";
import { LOW_AMOUNT_PAYMENT, SECOND_TIER_TIP, TipsOptions } from "../../helpers/tipsOptions";
import { Validator } from "../../helpers/validator";
import { payAtTheTable, type PaymentDetails } from "./paymentTypes";
import { PayTotal } from "./PayTotal";
import { ShareEqual } from "./ShareEqual";
import { ShareCustom } from "./ShareCustom";
import { ShareItems } from "./ShareItems";
import { TipSection } from "./TipSection/TipSection";

interface Props {
    readonly paymentSplit: "total" | "equal" | "custom" | "items" | "freepayment";
}

export const PayPage: React.FC<Props> = ({ paymentSplit }) => {
    const browserStorageService = useBrowserStorageService();
    const { t } = useTranslation();
    const theme = useQuiviTheme();
    const navigate = useNavigate();
    
    const channelContext = useChannelContext();
    const { features, channelId } = channelContext;
    
    const sessionQuery = useSessionsQuery({
        channelId: channelId,
    });
    const auth = useAuth();
    const paymentDetails = browserStorageService.getPaymentDetails();
    const paymentDivision = browserStorageService.getPaymentDivision();

    const [sessionPending, setSessionPending] = useState<number | null>(sessionQuery.isFirstLoading ? null : Calculations.roundUp(sessionQuery.data?.unpaid ?? 0));
    const [vatIsValid, setVatIsValid] = useState(true);
    const [userBill, setUserBill] = useState(0); // Amount of the bill the user wants to pay
    const [tipValue, setTipValue] = useState(paymentDetails?.tip ?? 0);
    const [selectedTipOption, setSelectedTipOption] = useState(paymentDetails?.selectedTip ?? TipsOptions.empty());
    const [selectedItems, setSelectedItems] = useState(paymentDetails == undefined ? [] : payAtTheTable(paymentDetails.additionalData)?.items || []);
    const [tipModalIsOpen, setTipModalIsOpen] = useState(false);

    const isTipOnly = paymentSplit == "freepayment" && features.freePayments.isTipOnly;
    const isOverPaying = () => paymentSplit != "freepayment" && (userBill > (sessionPending ?? 0));
    const getUserTotal = () => Calculations.total(Calculations.roundUp(userBill), tipValue);

    const handleAmountResult = (userBillAmount: number) => {
        if(isTipOnly) {
            setUserBill(0);
            setTipValue(userBillAmount);
            return;
        }
        
        setUserBill(userBillAmount);
        let tip: number;
        if (!userBillAmount || selectedTipOption.id === TipsOptions.empty().id) {
            tip = 0;
        } else if (selectedTipOption.id === TipsOptions.otherButton().id) {
            tip = tipValue; 
        } else if (userBillAmount < SECOND_TIER_TIP) {
            tip = selectedTipOption.fisrtTierFixedTip;
            setTipValue(tip);
        } else if (userBillAmount >= SECOND_TIER_TIP && userBillAmount < LOW_AMOUNT_PAYMENT) {
            tip = selectedTipOption.secondTierFixedTip;
            setTipValue(tip);
        } else {
            tip = Calculations.getTip(userBillAmount, selectedTipOption.percentage);
            setTipValue(tip);
        }
    };

    const formik = useFormik({
        initialValues: {
            email: auth.user?.email ?? paymentDetails?.email ?? "",
            vat: auth.user?.vatNumber ?? paymentDetails?.vatNumber ?? "",
        },
        validationSchema: Yup.object({
            email: Yup.string()
                .email(t("form.emailValidation")),
        }),
        onSubmit: async (values) => {
            const newPaymentDetails: PaymentDetails = {
                total: getUserTotal(),
                amount: Calculations.roundUp(userBill),
                tip: tipValue,
                email: values.email,
                vatNumber: Formatter.cleanString(values.vat),
                additionalData: {
                    items: selectedItems,
                },
                selectedTip: selectedTipOption,
            };

            browserStorageService.savePaymentDivision(paymentDivision);
            browserStorageService.savePaymentDetails(newPaymentDetails);

            setSessionPending(0);
            if(paymentSplit == "freepayment") {
                navigate(`/c/${channelContext.channelId}/session/pay/methods/free`)
            } else {
                navigate(`/c/${channelContext.channelId}/session/pay/method`)
            }
            return;
        }
    });

    useEffect(() => {
        if(sessionQuery.isFirstLoading) {
            return;
        }

        if(sessionPending == null) {
            setSessionPending(Calculations.roundUp(sessionQuery.data?.unpaid ?? 0))
        }

    }, [sessionQuery.isFirstLoading, sessionQuery.data]);

    const submitForm = () => {
        if (paymentSplit != "freepayment" && isAmountPayable(sessionQuery.data?.unpaid ?? 0, userBill) != true) {
            navigate(`/c/${channelContext.channelId}/session/summary`)
            toast.info(t("pay.sessionHasUpdates"), {
                icon: <InfoIcon color={theme.primaryColor.hex} />,
            });
            return;
        }
        formik.handleSubmit();
    }

    const onPayClick = () => {
        if(tipValue > 0 || features.payAtTheTable.enforceTip == false) {
            submitForm();
            return;
        }

        setTipModalIsOpen(true);
    }

    const getFooter = () => {
        const disabled = isTipOnly ? tipValue == 0 : userBill === 0 || isOverPaying();
        return <input
            type="button"
            onClick={onPayClick}
            value={userBill > 0? `${t("pay.pay")} ${Formatter.price(getUserTotal(), "€")}` : t("pay.pay")}
            className={`primary-button ${disabled ? "disabled" : ""}`}
        />
    }

    if(sessionQuery.data == undefined && ["custom", "freepayment"].includes(paymentSplit) == false){
        return <Navigate to={`/c/${channelId}`} replace />
    }

    return <Page title={t("pay.title")} footer={getFooter()}>
        <section className="pay" style={{marginBottom: 0}}>
            <form>
                {
                    paymentSplit === "total" &&
                    <PayTotal loadUserBill={handleAmountResult} sessionPending={sessionPending} />
                }
                {
                    paymentSplit === "equal" &&
                    <ShareEqual onChangeAmount={handleAmountResult} sessionPending={sessionPending} />
                }
                {
                    paymentSplit === "custom" &&
                    <ShareCustom onChangeAmount={handleAmountResult} sessionPending={sessionPending} />
                }
                {
                    paymentSplit === "freepayment" &&
                    <ShareCustom onChangeAmount={handleAmountResult} />
                }
                {
                    paymentSplit === "items" &&
                    <ShareItems onChangeAmount={handleAmountResult} sessionPending={sessionPending} onChangeItems={setSelectedItems} />
                }
                {
                    isTipOnly == false &&
                    <TipSection
                        userBill={userBill}
                        setTipResult={setTipValue}
                        setTipChoice={setSelectedTipOption}
                        selectedTip={selectedTipOption}
                    />
                }

                <div className="pay__receipt mb-6">
                    <Grid container spacing={1} style={{justifyContent: "center"}}>
                        <Grid size={{xs: 12, sm: 12, md: 6, lg: 6, xl: 6}} className="pay__email">
                            <div className="pay__email">
                                <label htmlFor="email">{t("form.invoiceEmail")}</label>
                                <input type="email" id="email" name="email" placeholder={t("form.emailPlaceholder")} value={formik.values.email} onChange={formik.handleChange} onBlur={formik.handleBlur} />
                                {formik.touched.email && formik.errors.email && <Alert severity="warning" icon={false} className="mt-2">{formik.errors.email}</Alert>}
                            </div>
                        </Grid>
                        <Grid size={{xs: 12, sm: 12, md: 6, lg: 6, xl: 6}}  className="pay__nif">
                            <label htmlFor="vat">{t("form.vat")} ({t("optional")})</label>
                            <input type="tel" id="vat" name="vat" placeholder={t("form.vatPlaceholder")}
                                value={formik.values.vat} onChange={formik.handleChange}
                                onBlur={(e: React.FocusEvent) => {
                                    setVatIsValid(Validator.vatNumber(formik.values.vat));
                                    formik.handleBlur(e);
                                }} 
                            />
                            {
                                formik.touched.vat &&
                                !vatIsValid &&
                                <Alert severity="warning" icon={false} className="mt-2">{t("form.vatValidation")}</Alert>
                            }
                        </Grid>
                    </Grid>
                </div>
            </form>
        </section>
        <Dialog
            onClose={() => setTipModalIsOpen(false)}
            isOpen={tipModalIsOpen}
        >
            <div className="container" style={{ paddingTop: "1.75rem", paddingBottom: "1.75rem" }}>
                <div className="modal__header">
                    <h3>{t("paymentMethods.tipRecall")}</h3>
                    <div className="close-icon" onClick={() => setTipModalIsOpen(false)}>
                        <CloseIcon />
                    </div>
                </div>

                <p className="mb-5">{t("paymentMethods.tipRecallDescription")}</p>

                {
                    tipValue == 0 &&
                    <button type="button" className="secondary-button" onClick={() => formik.handleSubmit()}>{t("paymentMethods.noTip")}</button>
                }
                <br/>
                <TipSection
                    userBill={userBill}
                    setTipResult={setTipValue}
                    setTipChoice={setSelectedTipOption}
                    selectedTip={selectedTipOption}
                />
                <button
                    type="button"
                    className={`primary-button mt-5 ${tipValue == 0 && "primary-button--inactive"}`}
                    onClick={submitForm}
                >
                    {t("paymentMethods.continue")}
                </button>
            </div>
        </Dialog>
    </Page>
}

const isAmountPayable = (unpaidAmount: number, amount: number): boolean => {
    return Calculations.roundUp(unpaidAmount) >= Calculations.roundUp(amount);
}