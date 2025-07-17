import { useFormik } from "formik";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import * as Yup from "yup";
import Dialog from "../../components/Shared/Dialog";
import { Page } from "../../layout/Page";
import { ButtonsSection } from "../../layout/ButtonsSection";
import { useBrowserStorageService } from "../../hooks/useBrowserStorageService";
import { TipSection } from "../Pay/TipSection/TipSection";
import { Alert } from "@mui/material";
import { Validator } from "../../helpers/validator";
import { CloseIcon } from "../../icons";
import { useChannelContext } from "../../context/AppContextProvider";
import { useCart } from "../../context/OrderingContextProvider";
import { useAuth } from "../../context/AuthContext";
import { Calculations } from "../../helpers/calculations";
import { LOW_AMOUNT_PAYMENT, SECOND_TIER_TIP, TipsOptions } from "../../helpers/tipsOptions";
import type { PaymentDetails } from "../Pay/paymentTypes";
import { Formatter } from "../../helpers/formatter";
import { useNavigate } from "react-router";
import { PayTotalCart } from "./PayTotalCart";
import LoadingButton from "../../components/Buttons/LoadingButton";

export const OrderCheckoutPage = () => {
    const browserStorageService = useBrowserStorageService();
    const { t } = useTranslation();
    const navigate = useNavigate();

    const channelContext = useChannelContext();
    
    const cartSession = useCart();
    const auth = useAuth();
    
    const paymentDetails = browserStorageService.getPaymentDetails();
    const paymentDivision = browserStorageService.getPaymentDivision();
    
    const userEmailIsRequired = channelContext.features.ordering.mandatoryUserEmailForTakeawayPayment;

    const [isLoading, setIsLoading] = useState(false);
    const [vatIsValid, setVatIsValid] = useState(true);
    const [userBill, setUserBill] = useState(0); // Amount of the bill the user wants to pay
    const [tipValue, setTipValue] = useState(paymentDetails?.tip ?? 0);
    const [selectedTipOption, setSelectedTipOption] = useState(paymentDetails?.selectedTip ?? TipsOptions.empty());
    const [tipModalIsOpen, setTipModalIsOpen] = useState(false);

    const userTotal = Calculations.total(Calculations.roundUp(userBill), tipValue);

    const handleAmountResult = (userBillAmount: number) => {
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
            email: userEmailIsRequired ? 
                Yup.string()
                    .email(t("form.emailValidation"))
                    .required(t("form.requiredField"))
                :
                Yup.string()
                    .email(t("form.emailValidation")),
        }),
        onSubmit: async (values) => {
            setIsLoading(true)
            try {
                const order = await cartSession.submit();
                const newPaymentDetails: PaymentDetails = {
                    total: userTotal,
                    amount: Calculations.roundUp(userBill),
                    tip: tipValue,
                    email: values.email,
                    vatNumber: Formatter.cleanString(values.vat),
                    additionalData: {
                        orderId: order.id,
                        scheduledDate: cartSession.scheduledDate,
                    },
                    selectedTip: selectedTipOption,
                };
                browserStorageService.savePaymentDivision(paymentDivision);
                browserStorageService.savePaymentDetails(newPaymentDetails);

                navigate(`/c/${channelContext.channelId}/session/pay/methods`);
            } finally {
                setIsLoading(false);
            }
        }
    });

    const onPayClick = () => {
        if(tipValue > 0 || channelContext.features.ordering.enforceTip == false) {
            formik.handleSubmit();
            return;
        }

        setTipModalIsOpen(true);
    }

    const getFooter = () => {
        return <ButtonsSection>
            {
                <LoadingButton
                    isLoading={isLoading}
                    onClick={onPayClick}
                    disabled={ userBill === 0 || !!formik.errors.email || !!formik.errors.vat || (userEmailIsRequired && !formik.values.email?.length)}
                >
                {
                    userBill > 0
                    ? 
                        `${t("pay.pay")} ${Formatter.price(userTotal, "€")}`
                    : 
                        t("pay.pay")
                }
                </LoadingButton>
            }
            {undefined}
        </ButtonsSection>
    }

    return <Page title={t("pay.title")} footer={getFooter()}>
        <section className="pay" style={{marginBottom: 0}}>
            <form>
                <PayTotalCart loadUserBill={handleAmountResult} />

                <TipSection
                    userBill={userBill}
                    setTipResult={setTipValue}
                    setTipChoice={setSelectedTipOption}
                    selectedTip={selectedTipOption}
                />
                <div className="pay__receipt">
                    <div className="pay__email">
                        <label htmlFor="email">{t("form.invoiceEmail")} ({userEmailIsRequired ? t("form.mandatory") : t("optional")})</label>
                        <input type="email" id="email" name="email" placeholder={t("form.emailPlaceholder")} value={formik.values.email} onChange={formik.handleChange} onBlur={formik.handleBlur} />
                        {formik.touched.email && formik.errors.email && <Alert severity="warning" icon={false} className="mt-2">{formik.errors.email}</Alert>}
                    </div>
                    <div className="pay__nif mt-5">
                        <label htmlFor="vat">{t("form.vat")} ({t("optional")})</label>
                        <input 
                            type="tel"
                            id="vat"
                            name="vat"
                            placeholder={t("form.vatPlaceholder")}
                            value={formik.values.vat}
                            onChange={formik.handleChange}
                            onBlur={(e: React.FocusEvent) => {
                                setVatIsValid(Validator.vatNumber(formik.values.vat));
                                formik.handleBlur(e);
                            }} 
                        />
                        {formik.touched.vat && !vatIsValid && <Alert severity="warning" icon={false} className="mt-2">{t("form.vatValidation")}</Alert>}
                    </div>
                </div>
            </form>
        </section>
        <Dialog
            onClose={() => setTipModalIsOpen(false)}
            isOpen={tipModalIsOpen && !isLoading}
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
                    onClick={() => formik.handleSubmit()}
                >
                    {t("paymentMethods.continue")}
                </button>
            </div>
        </Dialog>
    </Page>
}