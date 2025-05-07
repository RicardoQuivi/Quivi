import { Box, Card, CardActionArea, CardMedia, Divider, Grid, Skeleton, TextField, Typography, styled } from "@mui/material";
import React, { useEffect, useMemo, useState } from "react"
import { Html } from "../../helpers/htmlHelper";
import { useLocalsQuery } from "../../hooks/queries/implementations/useLocalsQuery";
import { Local } from "../../hooks/api/Dtos/locals/Local";
import { useCustomChargeMethodsQuery } from "../../hooks/queries/implementations/useCustomChargeMethodsQuery";
import { useTranslation } from "react-i18next";
import DecimalInput from "../Inputs/DecimalInput";
import { SingleSelect } from "../Inputs/SingleSelect";
import { Enumerable } from "../../helpers/arrayHelper";
import { CustomChargeMethod } from "../../hooks/api/Dtos/customchargemethods/CustomChargeMethod";
import useValidatorService from "../../hooks/useValidator";

const StyleCard = styled(Card)(({ theme }) => ({
    "&.active": {
        borderColor: theme.palette.primary.main,
        backgroundColor: Html.hexToRgbaColor(theme.palette.primary.main, 0.08),
    }
}));

interface Props {
    readonly localId: string | undefined;
    readonly onLocalChanged?: (id: string) => any;

    readonly total: number;
    
    readonly tip: number;
    readonly onTipChanged: (v: number) => void;

    readonly vatNumber: string;
    readonly onVatNumberChange: (v: string) => void;

    readonly email: string;
    readonly onEmailChanged: (v: string) => void;

    readonly observations: string;
    readonly onObservationsChanged: (v: string) => void;

    readonly paymentMethodId?: string;

    onPaymentMethodChanged: (id: string) => void;
    onIsValidChanged: (isValid: boolean) => void;
}

export const PaymentCustomerOptions: React.FC<Props> = (props) => {
    const { t } = useTranslation();

    const validator = useValidatorService();
    const chargeMethodsQuery = useCustomChargeMethodsQuery({
        page: 0,
    });
    const localsQuery = useLocalsQuery({})
    const localsMap = useMemo(() => {
        const map = new Map<string, Local>();
        for(const l of localsQuery.data) {
            map.set(l.id, l);
        }
        return map;
    }, [localsQuery.data])

    const [errorVatMsg, setErrorVatMsg] = useState<string>();
    const [errorEmailMsg, setErrorEmailMsg] = useState<string>();

    const [selectedLocal, setSelectedLocal] = useState<Local>();

    useEffect(() => {
        if(props.localId == undefined) {
            setSelectedLocal(undefined);

            const local = localsMap.values() .next().value;
            if(local != undefined) {
                props.onLocalChanged?.(local.id);
            }
            return;
        }

        const local = localsMap.get(props.localId);
        if(local != undefined) {
            setSelectedLocal(local);
            return;
        }
    }, [localsMap, props.localId])

    const getItemsRatio = (): number => {
        const itemsPerRow = 4;
        const paymentsCount = chargeMethodsQuery.data.length;
        if (paymentsCount < itemsPerRow) {
            return 12 / itemsPerRow;
        }

        const rest = Math.max(paymentsCount, itemsPerRow) % itemsPerRow;
        return Math.min(12 / itemsPerRow + Math.ceil(rest/2), 12);
    }

    const onVatNumberChanged = (value: string) => {
        if (!!value && !validator.validatePortugueseVat(value)) {
            setErrorVatMsg(t("invalidNifMessage")!);
        } else {
            setErrorVatMsg(undefined);
        }

        props.onVatNumberChange(value);
    }

    const onEmailChanged = (value: string) => {
        if (!!value && !validator.validateEmail(value)) {
            setErrorEmailMsg(t("invalidEmailMessage")!);
        } else {
            setErrorEmailMsg(undefined);
        }
        props.onEmailChanged(value);
    }

    useEffect(() => {
        if (!props.paymentMethodId) {
            props.onIsValidChanged(false);
        } else if (!!errorVatMsg) {
            props.onIsValidChanged(false);
        } else if (!!errorEmailMsg) {
            props.onIsValidChanged(false);
        } else {
            props.onIsValidChanged(true);
        }
    }, [props.paymentMethodId, errorVatMsg, errorEmailMsg]);

    return (
        <Grid container spacing={3} justifyContent="center">
            {
                selectedLocal != undefined &&
                <Grid size={12}>
                    <Divider sx={{mb: 2}}>
                        <Typography variant="overline">{t("local")}</Typography>
                    </Divider>
                    <SingleSelect
                        label={t("local")}
                        value={selectedLocal} 
                        options={localsQuery.data}
                        getId={l => l.id}
                        render={l => l.name}
                        onChange={l => props.onLocalChanged?.(l.id)}
                    />
                </Grid>
            }
            <Grid size={12}>
                <Divider sx={{mb: 2}}>
                    <Typography variant="overline">{t("amount")}</Typography>
                </Divider>
                <Grid container spacing={2}>
                    <Grid size={{xs: 12, md: 6}}>
                        <DecimalInput 
                            label= {t("total")!}
                            endAdornment={<Typography variant="body1" sx={{ mb: 0, color: "rgba(0, 0, 0, 0.38)"}}>€</Typography>}
                            textFieldProps={{
                                variant:"outlined",
                                fullWidth: true,
                                disabled : true,
                            }}
                            value={props.total} 
                        />
                    </Grid>
                    <Grid size={{xs: 12, md: 6}}>
                        <DecimalInput 
                            label={t("tip")!}
                            endAdornment={<Typography variant="body1" sx={{ mb: 0 }}>€</Typography>}
                            textFieldProps={{
                                variant: "outlined",
                                fullWidth: true,
                            }}
                            value={props.tip} 
                            onChange={props.onTipChanged}
                        />
                    </Grid>
                </Grid>
            </Grid>
            <Grid size={12}>
                <Divider sx={{mb: 2}}>
                    <Typography variant="overline">{t("customerInformation")}</Typography>
                </Divider>
                <Grid container spacing={2}>
                    <Grid size={{xs: 12, md: 6}}>
                        <TextField
                            type="number"
                            slotProps={{
                                htmlInput: {
                                    step: 1,
                                }
                            }}
                            error={!!errorVatMsg}
                            helperText={errorVatMsg}
                            label={t("vatNumber")}
                            fullWidth
                            variant="outlined"
                            value={props.vatNumber}
                            onChange={(e) => onVatNumberChanged(e.target.value)}
                        />
                    </Grid>
                    <Grid size={{xs: 12, md: 6}}>
                        <TextField
                            label={t("email")}
                            error={!!errorEmailMsg}
                            helperText={errorEmailMsg}
                            fullWidth
                            autoComplete="email"
                            variant="outlined"
                            value={props.email}
                            onChange={(e) => onEmailChanged(e.target.value)}
                        />
                    </Grid>
                    <Grid size={12}>
                        <TextField
                            label={t("observations")}
                            multiline
                            fullWidth
                            rows={3}
                            value={props.observations}
                            onChange={e => props.onObservationsChanged(e.target.value)}
                        />
                    </Grid>
                </Grid>
            </Grid>
            <Grid size={12}>
                <Divider sx={{mb: 2}}>
                    <Typography variant="overline">{t("paymentMethods")}</Typography>
                </Divider>
                <Grid container spacing={2} justifyContent="center">
                {
                    chargeMethodsQuery.isFirstLoading
                    ?
                    Enumerable.range(4).map(i => (
                        <Grid key={i} size={{xs: 6, md: getItemsRatio()}}>
                            <PaymentMethodCard
                                selected={false}
                            />
                        </Grid>
                    ))
                    :
                    chargeMethodsQuery.data.map((item) => (
                        <Grid size={{xs: 6, md: getItemsRatio()}} display="flex" key={item.id}>
                            <PaymentMethodCard
                                item={item}
                                selected={props.paymentMethodId == item.id}
                                onClick={() => props.onPaymentMethodChanged(item.id)}
                            />
                        </Grid>
                    ))
                }
                </Grid>
            </Grid>
        </Grid>
    );
}

interface PaymentMethodCardProps {
    readonly selected: boolean;
    readonly item?: CustomChargeMethod;

    readonly onClick?: () => any;
}
const PaymentMethodCard = ({
    item,
    selected,
    onClick
}: PaymentMethodCardProps) => {
    const [imageLoaded, setImageLoaded] = useState(false);
    
    useEffect(() => {
        setImageLoaded(false);
        if(item == undefined) {
            return;
        }

        const img = new Image();
        img.src = item.logoUrl;
        img.onload = () => setImageLoaded(true);
    }, [item])

    return (
        <StyleCard
            variant="outlined"
            className={selected ? "active" : ""}
            onClick={onClick}
            sx={{
                display: "flex",
                flexGrow: 1,
                width: "100%",
                aspectRatio: "4/3",
            }}
        >
            <CardActionArea
                sx={{
                    textAlign: "center",
                    display: "flex",
                    flexDirection: "column",
                    justifyContent: "space-evenly",
                    width: "100%",
                    height: "100%",
                }}
                disabled={item == undefined}
            >
                <Box
                    sx={{
                        flex: "1 1 auto",
                        padding: "0.75rem 0.25rem",
                        width: "100%",
                    }}
                >
                    {
                        item == undefined || imageLoaded == false
                        ?
                        <Skeleton
                            animation="wave"
                            variant="rounded"
                            sx={{
                                width: "100%",
                                height: "100%",
                            }}
                        />
                        :
                        <CardMedia 
                            component="div"
                            sx={{
                                width: "100%",
                                height: "100%",

                                backgroundImage: `url("${item.logoUrl}")`,
                                backgroundRepeat: "no-repeat",
                                backgroundPosition: "center",
                                backgroundSize: "contain",
                            }}
                        />
                    }
                </Box>
                <Typography
                    gutterBottom
                    variant="subtitle2"
                    textTransform="capitalize"
                    sx={{
                        width: "100%",
                        display: "flex",
                        justifyContent: "center"
                    }}
                >
                    {
                        item == undefined
                        ?
                        <Skeleton variant="text" animation="wave" width="80%" />
                        :
                        item.name
                    }
                </Typography>
            </CardActionArea>
        </StyleCard>
    )
}