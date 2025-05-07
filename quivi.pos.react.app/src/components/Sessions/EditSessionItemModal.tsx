import { useTranslation } from "react-i18next";
import { SessionItem } from "../../hooks/api/Dtos/sessions/SessionItem";
import { useToast } from "../../context/ToastProvider";
import { useEffect, useMemo, useState } from "react";
import { Currency } from "../../helpers/currencyHelper";
import CustomModal, { ModalSize } from "../Modals/CustomModal";
import { Box, Grid, Skeleton, Tab, Tabs } from "@mui/material";
import LoadingButton from "../Buttons/LoadingButton";
import HighlightMessage, { MessageType } from "../Messages/HighlightMessage";
import DecimalInput from "../Inputs/DecimalInput";
import { DiscountedItem } from "../Modals/DiscountsModal";
import { useMenuItemsQuery } from "../../hooks/queries/implementations/useMenuItemsQuery";
import BigNumber from "bignumber.js";

enum TabOption {
    Discount = 0,
    EditPrice = 1,
}

export interface EditItemPriceModel extends DiscountedItem<SessionItem> {
    readonly priceOverride?: number;
}

interface Props {
    readonly item?: SessionItem;
    readonly onClose: () => void;
    readonly onSubmit: (item: EditItemPriceModel) => Promise<void>;
}
export const EditSessionItemModal: React.FC<Props> = (props) => {
    const { t, i18n } = useTranslation();
    const toast = useToast();

    const [isSubmitAllowed, setIsSubmitAllowed] = useState(false);
    const [isLoading, setIsLoading] = useState(false);

    const [selectedTab, setSelectedTab] = useState(TabOption.Discount);
    const [absoluteDiscountTxt, setAbsoluteDiscountTxt] = useState("");
    const [percentageDiscountTxt, setPercentageDiscountTxt] = useState("");
    const [priceTxt, setPriceTxt] = useState("");
    const [absoluteDiscountErrorMsg, setAbsoluteDiscountErrorMsg] = useState<string>();
    const [percentageDiscountErrorMsg, setPercentageDiscountErrorMsg] = useState<string>();
    const [priceErrorMsg, setPriceErrorMsg] = useState<string>();

    const itemQuery = useMenuItemsQuery(props.item == undefined ? undefined : {
        ids: [props.item.menuItemId],
        page: 0,
    })
    const menuItem = useMemo(() => {
        if(itemQuery.data.length == 0) {
            return undefined;
        }
        return itemQuery.data[0];
    }, [itemQuery.data])
    

    const getOriginalPrice = (item: SessionItem) => item.originalPrice;
    const calculateAbsoluteDiscount = (item: SessionItem) => new BigNumber(item.originalPrice).minus(item.price).toNumber();
    const calculatePercentageDiscount = (item: SessionItem) => new BigNumber(100).minus(new BigNumber(item.price).dividedBy(item.originalPrice).times(100)).toNumber();
    const currencyFormat = (value?: number) => value == undefined ? "" : Currency.toCurrencyFormat({ value: value, culture: i18n.language });
    const getTitle = () => menuItem == undefined ? <Skeleton animation="wave" /> : `${menuItem.name} (${currencyFormat(props.item!.originalPrice)})`;
    const resetData = () => {
        setSelectedTab(TabOption.Discount);
        setPriceErrorMsg(undefined);
        setAbsoluteDiscountErrorMsg(undefined);
        setPercentageDiscountErrorMsg(undefined);
    }

    const onAbsoluteDiscountChange = (newAbsoluteDiscountStr: string) => {
        const originalPrice = getOriginalPrice(props.item!);
        const absoluteDiscount = Number(newAbsoluteDiscountStr);
        const percentageDiscount = new BigNumber(absoluteDiscount).dividedBy(originalPrice).times(100); 
        
        setAbsoluteDiscountTxt(absoluteDiscount.toString());
        setPercentageDiscountTxt(percentageDiscount.toString());
    }

    const onPercentageDiscountChange = (newPercentageDiscountStr: string) => {
        const originalPrice = getOriginalPrice(props.item!);
        const percentageDiscount = Number(newPercentageDiscountStr);
        const absoluteDiscount = new BigNumber(percentageDiscount).dividedBy(100).times(originalPrice);

        setAbsoluteDiscountTxt(absoluteDiscount.toString());
        setPercentageDiscountTxt(percentageDiscount.toString());
    }

    const onPriceChange = (newPriceStr: string) => setPriceTxt(newPriceStr);

    const onSubmit = async () => {
        setIsLoading(true);
        try {
            await props.onSubmit({
                item: props.item!,
                newDiscount: selectedTab == TabOption.Discount ? Number(percentageDiscountTxt) : 0,
                quantityToApply: props.item!.quantity,
                priceOverride: selectedTab == TabOption.EditPrice ? Number(priceTxt) : undefined,
            });
            props.onClose();
        } catch {
            toast.error(t('unexpectedErrorHasOccurred'));
        } finally {
            setIsLoading(false);
        }
    } 

    const onCancel = () => props.onClose();

    useEffect(() => {
        if (props.item == undefined) {
            return;
        }

        resetData();
        setAbsoluteDiscountTxt(() => {
            let absoluteDiscount = calculateAbsoluteDiscount(props.item!);
            return Number.isNaN(absoluteDiscount) ? "0" : absoluteDiscount.toString();
        });
        setPercentageDiscountTxt(() => {
            let percentageDiscount = calculatePercentageDiscount(props.item!);
            return Number.isNaN(percentageDiscount) ? "0" : percentageDiscount.toString();
        });
        setPriceTxt(() => {
            let originalPrice = getOriginalPrice(props.item!);
            return originalPrice.toString();
        });
    }, [props.item]);

    useEffect(() => {
        const originalPrice = props.item?.originalPrice ?? 0;
        const price = Number(absoluteDiscountTxt);
        if (price > originalPrice) {
            setAbsoluteDiscountErrorMsg(t("maxValueAllowed", { value: Currency.toCurrencyFormat({ value: originalPrice, culture: i18n.language }) })!);
        } else if (price < 0) {
            setAbsoluteDiscountErrorMsg(t("minValueAllowed", { value: "0€" })!);
        } else {
            setAbsoluteDiscountErrorMsg(undefined);
        }
    }, [absoluteDiscountTxt]);

    useEffect(() => {
        const discount = Number(percentageDiscountTxt);
        if (discount > 100) {
            setPercentageDiscountErrorMsg(t("maxValueAllowed", { value: "100%" })!);
        } else if (discount < 0) {
            setPercentageDiscountErrorMsg(t("minValueAllowed", { value: "0%" })!);
        } else {
            setPercentageDiscountErrorMsg(undefined);
        }
    }, [percentageDiscountTxt]);

    useEffect(() => {
        const price = Number(priceTxt);
        if (price < 0) {
            setPriceErrorMsg(t("minValueAllowed", { value: "0€" })!);
        } else {
            setPriceErrorMsg(undefined);
        }
    }, [priceTxt]);

    useEffect(() => {
        if (!props.item) {
            setIsSubmitAllowed(false);
            return;
        }
        
        if (!!absoluteDiscountErrorMsg) {
            setIsSubmitAllowed(false);
            return;
        }

        if (!!percentageDiscountErrorMsg) {
            setIsSubmitAllowed(false);
            return;
        }
        
        if (!!priceErrorMsg) {
            setIsSubmitAllowed(false);
            return;
        }

        if (selectedTab == TabOption.EditPrice) {
            const oldPrice = getOriginalPrice(props.item!);
            const newPrice = Number(priceTxt);
            if (newPrice != oldPrice) {
                setIsSubmitAllowed(true);
                return;
            }
        }

        if (selectedTab == TabOption.Discount) {
            const oldAbsoluteDiscount = calculateAbsoluteDiscount(props.item!);
            const newAbsoluteDiscount = Number(absoluteDiscountTxt);
            if (oldAbsoluteDiscount != newAbsoluteDiscount) {
                setIsSubmitAllowed(true);
                return;
            }
        }

        setIsSubmitAllowed(false);
    }, [priceTxt, percentageDiscountTxt, absoluteDiscountTxt, absoluteDiscountErrorMsg, percentageDiscountErrorMsg, priceErrorMsg]);

    const getFinalPrice = () => {
        if (!props.item) {
            return Number.NaN;
        }

        if (selectedTab == TabOption.Discount) {
            return new BigNumber(getOriginalPrice(props.item)).minus(Number(absoluteDiscountTxt)).toNumber();
        }

        if (selectedTab == TabOption.EditPrice) {
            return Number(priceTxt);
        }

        return Number.NaN;
    }

    if(props.item == undefined) {
        return <></>
    }

    return (
    <CustomModal isOpen={!!props.item} onClose={props.onClose} title={getTitle()} size={ModalSize.Small} disableCloseOutsideModal
        footer={
            <Grid container sx={{width: "100%", margin: "1rem 0.25rem"}} spacing={1}>
                <Grid size="grow">
                    <LoadingButton isLoading={false} onClick={onCancel} style={{width: "100%"}}>
                        {t("close")}
                    </LoadingButton>
                </Grid>
                <Grid size="grow">
                    <LoadingButton isLoading={isLoading} disabled={!isSubmitAllowed} onClick={onSubmit} primaryButton style={{width: "100%"}}>
                        {t("confirm")}
                    </LoadingButton>
                </Grid>
            </Grid>
        }>
        <Box sx={{textAlign: "center"}}>
            <HighlightMessage messageType={MessageType.information}>
                {`${t("finalPrice")}: ${currencyFormat(getFinalPrice())}`}
            </HighlightMessage>
        </Box>
        <Tabs sx={{p: 1}} variant="fullWidth" value={selectedTab} onChange={(_, value) => setSelectedTab(value)}>
            <Tab label={t("applyDiscount")} value={TabOption.Discount}/>
            <Tab label={t("editPrice")} value={TabOption.EditPrice} />
        </Tabs>
        <Box>
            <Box sx={{ display: "flex", gap: 2, justifyContent: "center", p: 2 }}>
                {
                    selectedTab == TabOption.Discount &&
                    <>
                        <DecimalInput
                            value={Number(absoluteDiscountTxt)}
                            label={t("absoluteDiscount")}
                            errorMsg={absoluteDiscountErrorMsg}
                            onChange={(newValue) => onAbsoluteDiscountChange(newValue.toString())}
                            endAdornment={<>€</>}
                        />
                        <DecimalInput
                            value={Number(percentageDiscountTxt)}
                            label={t("percentageDiscount")!}
                            errorMsg={percentageDiscountErrorMsg}
                            onChange={(newValue) => onPercentageDiscountChange(newValue.toString())}
                            endAdornment={<>%</>}
                        />
                    </>
                }
            {
                selectedTab == TabOption.EditPrice && 
                <DecimalInput 
                    value={Number(priceTxt)} 
                    label={t("newPrice")!}
                    errorMsg={priceErrorMsg}
                    onChange={(newValue) => onPriceChange(newValue.toString())}
                    endAdornment={<>€</>}
                />
            }
            </Box>
        </Box>
    </CustomModal>
    )
}