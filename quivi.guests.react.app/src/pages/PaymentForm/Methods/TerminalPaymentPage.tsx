import { useTranslation } from "react-i18next";
import { GenericPaymentPage } from "../GenericPaymentPage";
import { Box, CircularProgress, Typography } from "@mui/material";
import type { Transaction } from "../../../hooks/api/Dtos/transactions/Transaction";
import { ChargeMethod } from "../../../hooks/api/Dtos/ChargeMethod";
import { TransactionStatus } from "../../../hooks/api/Dtos/transactions/TransactionStatus";
import { PaymentResume } from "../PaymentResume";
import { ChargeMethodIcon } from "../../../icons/ChargeMethodIcon";

interface Props {
    readonly transaction: Transaction;
    readonly nextTransaction?: Transaction;
}
const TerminalPaymentPage : React.FC<Props> = ({
    transaction,
    nextTransaction,
}) => {   
    const { t } = useTranslation();

    if(transaction.method != ChargeMethod.PaymentTerminal)
        return <></>

    return (
        <GenericPaymentPage
            transaction={transaction}
            nextTransaction={nextTransaction}
        >
            <PaymentResume
                transaction={transaction}
                nextTransaction={nextTransaction}
            />
            <Box
                className="tutorial__header"
            >
                <ChargeMethodIcon
                    chargeMethod={ChargeMethod.PaymentTerminal}
                    style={{
                        maxHeight: "5vh",
                        marginBottom: "0.75rem",
                    }}
                />
                <h2>{t("paymentMethods.paymentTerminal.title")}</h2>
                <p>{t("paymentMethods.paymentTerminal.description")}</p>
            </Box>
            {
                transaction.status != TransactionStatus.Processing &&
                <Box className="mt-5">
                    <Box
                        sx={{
                            position: "relative",
                            height: "100px",
                            width: "100px",
                            margin: "0 auto",
                        }}
                    >
                        <Box
                            position="relative"
                            display="inline-flex"
                            sx={{
                                '& .MuiCircularProgress-circle': {
                                    color: t => t.palette.primary.main,
                                },
                                '& .MuiCircularProgress-root': {
                                    width: "100% !important",
                                    height: "100% !important"
                                },
                                '& .MuiTypography-root': {
                                    fontSize: "1rem",
                                    fontWeight: "bolder",
                                },
                                width: "100%",
                                height: "100%",
                            }}
                        >
                            <CircularProgress color="primary" />
                        </Box>
                    </Box>
                    <Typography className="mt-5" variant="caption" component="div" color="textSecondary" textAlign="center">{t("paymentMethods.paymentTerminal.sendingPayment")}</Typography>
                </Box>
            }
        </GenericPaymentPage>
    )
}
export default TerminalPaymentPage;