import { useMemo, useState } from "react";
import { format } from "date-fns";
import { SchedulerDialog, SchedulerDialogState } from "../../components/Ordering/SchedulerDialog";
import { useTranslation } from "react-i18next";
import { Page } from "../../layout/Page";
import { ButtonsSection } from "../../layout/ButtonsSection";
import Dialog from "../../components/Shared/Dialog";
import LoadingButton from "../../components/Buttons/LoadingButton";
import { toast } from "react-toastify";
import { Alert, Box, Checkbox, Chip, FormControl, Grid, Skeleton, TextField, Tooltip } from "@mui/material";
import React from "react";
import type { ICartItem } from "../../context/cart/ICartItem";
import { useCart } from "../../context/OrderingContextProvider";
import { useChannelContext } from "../../context/AppContextProvider";
import { ItemsHelper } from "../../helpers/ItemsHelper";
import { Formatter } from "../../helpers/formatter";
import { Link, Navigate, useNavigate } from "react-router";
import { MenuItemDetailDialog } from "../Menu/MenuItemDetailDialog";
import { AccessTimeIcon, CloseIcon, SuccessIcon } from "../../icons";
import { MenuItemComponent } from "../../components/Menu/MenuItemComponent";
import { useFormik } from "formik";
import * as Yup from "yup";
import { useOrderFieldsQuery } from "../../hooks/queries/implementations/useOrderFieldsQuery";
import { OrderFieldType } from "../../hooks/api/Dtos/orderFields/OrderFieldType";

export const CartPage = () => {
    const { i18n, t } = useTranslation(); 

    const channelContext = useChannelContext();
    const navigate = useNavigate();
    const cart = useCart();
    const {
        features,
        channelId,
    } = channelContext;
    const orderFieldsQuery = useOrderFieldsQuery({
        languageIso: i18n.language,
        channelId: channelId,
    });

    const [selectedItem, setSelectedItem] = useState<ICartItem | null>()
    const [schedulerOpen, setSchedulerOpen] = useState(false);
    const [isPayLaterDialogOpen, setIsPayLaterDialogOpen] = useState(false);
    const [isSubmitting, setIsSubmitting] = useState(false);

    const formik = useFormik({
        enableReinitialize: true,
        initialValues: orderFieldsQuery.data.reduce((result, field) => {
            result[field.id] = field.id in cart.fields ? cart.fields[field.id] : (field.defaultValue ?? (field.type == OrderFieldType.Number ? "1" : ""));

            if(field.type == OrderFieldType.Check) {
                result[field.id] = ![undefined, false, "", "0", "false"].includes(result[field.id]);
            }

            return result;
        }, {} as any),
        validationSchema: Yup.object(orderFieldsQuery.data.reduce((result, field) => {
            let validation = field.type == OrderFieldType.Number 
                                ? 
                                    Yup.number() 
                                :
                                (
                                    field.type == OrderFieldType.Check
                                    ?
                                        Yup.boolean().transform(value => ![undefined, false, "", "0", "false"].includes(value))
                                                    .oneOf(field.isRequired ? [true] : [false, true], t("form.requiredField"))
                                    :
                                        Yup.string()
                                );

            if(field.isRequired) {
                validation = validation.required(t("form.requiredField"));
            }
            
            result[field.id] = validation;
            return result;
        }, {} as any)),
        onSubmit: (values) => cart.editFields(values),
        initialStatus: false,
    });

    const submit = async () => {
        try {
            setIsSubmitting(true);
            await cart.submit(true);
            toast.info(t("digitalMenu.receivedOrderMsg"), {
                icon: <SuccessIcon />,
            });
            navigate(`/c/${channelContext.channelId}/session/summary`);
        } finally {
            setIsSubmitting(false)
        }
    }

    const getDate = (scheduledDate: Date | undefined) => {
        if(scheduledDate == undefined) {
            return t("orderScheduling.asSoonAsPossible");
        }

        const day = scheduledDate.getDate();
        const month = t(`calendar.months.${scheduledDate.getMonth() + 1}`);
        const time = format(scheduledDate, "HH:mm");

        return t("calendar.dateFormat", { day: day, month: month, time: time});
    }

    const getPrePaidButton = () => {
        const prePaidTotal = ItemsHelper.getItemsPrice(cart.items);
        const remainingAmount = features.ordering.minimumPrePaidOrderAmount - prePaidTotal;
        const isDisabled = remainingAmount > 0;
        return <LoadingButton
            key="prepay"
            className="primary-button w-100"
            isLoading={orderFieldsQuery.isFirstLoading || formik.isSubmitting}
            onClick={async () => {
                formik.setStatus(true);
                const result = await formik.validateForm();
                if(Object.keys(result).length > 0) {
                    return;
                }
                await formik.handleSubmit();
                navigate(`/c/${channelContext.channelId}/pay/checkout`);
            }}
            disabled={formik.isValid == false || isDisabled}
        >
            {
                isDisabled
                ?
                <Tooltip
                    title={t("cart.amountNotEligible", { 
                        amount: Formatter.price(features.ordering.minimumPrePaidOrderAmount), 
                        remaining: Formatter.price(remainingAmount),
                    })}
                    placement="top" 
                    arrow
                    open
                    slotProps={{
                        popper: {
                            className: "MuiTooltip-popper MuiTooltip-popperArrow"
                        }
                    }}
                >
                    <span>
                        {t("pay.payNow")}
                    </span>
                </Tooltip>
                :
                t("pay.payNow")
            }
        </LoadingButton>
    }

    const getFooter = () => {
        const buttons: React.ReactNode[] = [];
        if(cart.totalItems == 0) {
            buttons.push(<Link key="see-menu" className="primary-button w-100" to={`/c/${channelContext.channelId}/menu`}>
                {t("cart.seeMenu")}
            </Link>)
            return <ButtonsSection children={buttons} />
        } 

        const payLaterButton = <LoadingButton
                                    key="pay-later"
                                    className="primary-button w-100"
                                    onClick={(async () => {
                                        formik.setStatus(true);
                                        const result = await formik.validateForm();
                                        if(Object.keys(result).length > 0) {
                                            return;
                                        }
                                        await formik.handleSubmit();
                                        formik.setSubmitting(false);
                                        setIsPayLaterDialogOpen(true);
                                    })}
                                    disabled={formik.isValid == false}
                                    isLoading={orderFieldsQuery.isFirstLoading}
                                >
                                    { t("pay.payLater") }
                                </LoadingButton>;
        const prePayButton = getPrePaidButton();
        if(features.ordering.allowsPostPaidOrdering) {
            buttons.push(payLaterButton)
        }
        if(features.ordering.allowsPrePaidOrdering) {
            buttons.push(prePayButton)
        }
        return <ButtonsSection children={buttons} />
    }

    if(features.ordering.isActive == false) {
        return <Navigate to={`/c/${channelContext.channelId}`} replace />
    }

    return <Page
        title={t("cart.title")}
        headerProps={{hideCart: true}}
        footer={getFooter()}
    >
        { 
            cart.items.length > 0 
            ?
            <>
                <CartResume items={cart.items} onItemSelected={setSelectedItem} />
                {
                    orderFieldsQuery.data.length > 0 &&
                    <Box
                        sx={{
                            margin: "30px 0",
                        }}
                    >
                    {
                        orderFieldsQuery.data.map(f => (
                            <FormControl 
                                    key={f.id} sx={{
                                        mb: "0.5rem",
                                        display: "flex",
                                        flexDirection: "column"
                                    }}
                            >
                                <Box
                                    sx={
                                        f.type == OrderFieldType.Check ?
                                        {
                                            display: "flex",
                                            flexDirection: "row",
                                            justifyContent: "space-between",
                                        }
                                        :
                                        undefined
                                    }
                                >
                                    <label
                                        htmlFor={f.id}
                                        style={
                                            f.type == OrderFieldType.Check
                                            ?
                                            {
                                                textAlign: "center",
                                                alignContent: "center",
                                            }
                                            : 
                                            {}
                                        }
                                    >
                                        {f.name} ({f.isRequired ? t("form.mandatory") : t("optional")})
                                    </label>
                                    {
                                        cart.isInitializing
                                        ?
                                        <Skeleton animation="wave" width="100%" /> 
                                        :
                                        (
                                            f.type == OrderFieldType.Check
                                                ?
                                                <Checkbox
                                                    id={f.id}
                                                    name={f.id}
                                                    checked={!!formik.values[f.id]}
                                                    value={!!formik.values[f.id]}
                                                    onChange={formik.handleChange}
                                                    onBlur={formik.handleBlur}
                                                    color="primary"
                                                />
                                                :
                                                <TextField
                                                    id={f.id}
                                                    name={f.id}
                                                    value={formik.values[f.id]}
                                                    onChange={formik.handleChange}
                                                    onBlur={formik.handleBlur}

                                                    variant="outlined"
                                                    color="primary"
                                                    autoComplete="off"
                                                    type={f.type ? "number" : "text"}
                                                    required={f.isRequired}
                                                    multiline={f.type == OrderFieldType.LongText}
                                                    minRows={f.type == OrderFieldType.LongText ? 3 : undefined}
                                                    slotProps={{
                                                        htmlInput: {
                                                            maxLength: 1000,
                                                        },
                                                    }}
                                                />
                                        )
                                    }
                                </Box>
                                {
                                    (formik.touched[f.id] || formik.status == true) && formik.errors[f.id] && 
                                    <Alert severity="warning" icon={false} className="mt-2">
                                        {formik.errors[f.id]?.toString() ?? ""}
                                    </Alert>
                                }
                            </FormControl>
                        ))
                    }
                    </Box>
                }
                <div className="mt-5">
                    {
                        features.ordering.isActive && features.ordering.allowScheduling &&
                        <Chip
                            onClick={() => setSchedulerOpen(true)}
                            avatar={<AccessTimeIcon />}
                            label={<p>{getDate(cart.scheduledDate)}</p>} 
                            variant="outlined"
                            size="medium"
                            style={{width: "100%", cursor: "pointer", fontFamily: "Montserrat, sans-serif"}}
                        />                     
                    }
                </div>
            </>
            :
            <div className="flex flex-fd-c flex-ai-c flex-jc-c mt-10">
                <h2 className="mb-4">{t("cart.cartIsEmpty")}</h2>
                <p className="ta-c">{t("cart.cartIsEmptyDesc")}</p>
            </div>
        }
        <MenuItemDetailDialog
            menuItem={selectedItem ?? null}
            onClose={() => setSelectedItem(undefined)}
        />
        <SchedulerDialog
            date={cart.scheduledDate}
            isOpen={schedulerOpen}
            onDialogChange={(s) => s == SchedulerDialogState.Closed && setSchedulerOpen(false)}
            onDateSelected={() => setSchedulerOpen(false)}
        />
        <Dialog
            isOpen={isPayLaterDialogOpen || isSubmitting}
            onClose={() => setIsPayLaterDialogOpen(false)}
        >
            <Box
                className="container"
                sx={{
                    paddingTop: "1.75rem",
                    paddingBottom: "1.75rem",
                }}
            >
                <Box className="modal__header">
                    <h3>{t("cart.confirmSubmissionTitle")}</h3>
                    <Box className="close-icon" onClick={() => setIsPayLaterDialogOpen(false)}>
                        <CloseIcon />
                    </Box>
                </Box>

                <p className="mb-5">{t("cart.confirmSubmissionDescription")}</p>

                <ButtonsSection>
                    <LoadingButton
                        isLoading={false}
                        className="secondary-button w-100"
                        onClick={() => setIsPayLaterDialogOpen(false)}
                    >
                        {t("cart.payLater.change")}
                    </LoadingButton>
                    <LoadingButton
                        isLoading={isSubmitting}
                        className="primary-button w-100"
                        onClick={submit}
                    >
                        {t("cart.payLater.submit")}
                    </LoadingButton>
                </ButtonsSection>
            </Box>
        </Dialog>
    </Page>
}

interface CartResumeProps {
    readonly items: ICartItem[];
    readonly onItemSelected: (item: ICartItem) => any;
    readonly totalDescription?: string;
}
const CartResume = (props: CartResumeProps) => {
    const { t } = useTranslation(); 
    const total = useMemo(() => ItemsHelper.getItemsPrice(props.items), [props.items]);

    const getItemId = (item: ICartItem) => {
        if(!!item.modifiers && item.modifiers.length > 0) {
            const allSelectedOptions = item.modifiers.map(m => m.selectedOptions).reduce((r, o) => [...r, ...o], []);
            return allSelectedOptions.map(m => {
                let result = m.id;
                for(let i = 0; i < m.quantity; ++i) {
                    result = `${result}-${m.id}`;
                }
                return result;
            }).reduce((r, id) => `${r}-${id}`, item.id);
        }
        return item.id;
    }
    
    return <>
        <Grid container spacing={2}>
        {
            props.items.map(item => <Grid key={getItemId(item)} style={{width: "100%"}}>
                <MenuItemComponent menuItem={item} quickCartAlwaysOpened exactItemMatch onItemSelected={() => props.onItemSelected(item)}/>
            </Grid>)
        }
        </Grid>

        <div className="cart__total-price mt-4">
            <div>
                <h2 className="title">{t("cart.totalPrice")}</h2>
                { props.totalDescription != undefined && <h4 className="title">{props.totalDescription}</h4> }
            </div>
            <h2 className="amount">{Formatter.price(total, "€")}</h2>
        </div>
    </>
}