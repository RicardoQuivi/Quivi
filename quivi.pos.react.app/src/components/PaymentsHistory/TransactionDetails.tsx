import { useMemo, useState } from "react";
import { Trans, useTranslation } from "react-i18next";
import { Accordion, AccordionDetails, AccordionSummary, Grid, Skeleton, Stack, Tooltip, Typography } from "@mui/material";
import { Transaction } from "../../hooks/api/Dtos/transactions/Transaction";
import { useToast } from "../../context/ToastProvider";
import { useChannelsQuery } from "../../hooks/queries/implementations/useChannelsQuery";
import { useChannelProfilesQuery } from "../../hooks/queries/implementations/useChannelProfilesQuery";
import { usePosIntegrationsQuery } from "../../hooks/queries/implementations/usePosIntegrationsQuery";
import { DownloadIcon, ExpandIcon } from "../../icons";
import { SummaryBox } from "../common/SummaryBox";
import CurrencySpan from "../Currency/CurrencySpan";
import HighlightMessage, { MessageType } from "../Messages/HighlightMessage";
import { useDateHelper } from "../../helpers/dateHelper";
import { useEmployeesQuery } from "../../hooks/queries/implementations/useEmployeesQuery";
import { TransactionItemsPage } from "./TransactionItemsPage";
import { useTransactionDocumentsQuery } from "../../hooks/queries/implementations/useTransactionDocumentsQuery";
import { useAuth } from "../../context/AuthContextProvider";
import { TransactionInvoiceType } from "../../hooks/api/Dtos/transactionDocuments/TransactionInvoiceType";
import React from "react";

interface Props {
    readonly transaction?: Transaction;
}
export const TransactionDetails = ({
    transaction
}: Props) => {
    const { t } = useTranslation();
    const auth = useAuth();
    //const api = usePrintersApi();
    const toast = useToast();
    // const printersQuery = usePrintersQuery({
    //     page: 0,
    // });

    const channelQuery = useChannelsQuery(transaction == undefined ? undefined : {
        ids: [transaction.channelId],
        page: 0,
        includeDeleted: true,
    })
    const channel = useMemo(() => channelQuery.data.length == 0 ? undefined : channelQuery.data[0], [channelQuery.data]);

    const channelProfileQuery = useChannelProfilesQuery(channel == undefined ? undefined : {
        ids: [channel.channelProfileId],
        page: 0,
    })
    const channelProfile = useMemo(() => channelProfileQuery.data.length == 0 ? undefined : channelProfileQuery.data[0], [channelProfileQuery.data]);

    const integrationQuery = usePosIntegrationsQuery(channelProfile == undefined ? undefined : {
        ids: [channelProfile.posIntegrationId],
        page: 0,
    })
    const integration = useMemo(() => integrationQuery.data.length == 0 ? undefined : integrationQuery.data[0], [integrationQuery.data]);

    const documentsQuery = useTransactionDocumentsQuery(transaction == undefined ? undefined : {
        transactionId: transaction.id,
        page: 0,
    })

    const [isPrinting, setIsPrinting] = useState(false);
    const [isExpanded, setIsExpanded] = useState(false);

    const Print = async (transaction: Transaction) => {
        setIsPrinting(true);
        
        try {
            //await api.Print(transaction.id);
            toast.info(t('savedChanges'));
        } catch {
            toast.error(t('unexpectedErrorHasOccurred'));
        } finally {            
            setIsPrinting(false);
        }
    }

    return <Stack direction="column" spacing={2}>
        <Tooltip title={t("paymentHistory.clickToSeeItems")} style={{width: "100%"}}>
            <Accordion square sx={{backgroundColor: "#F7F7F8", width: "100%"}} expanded={isExpanded} onChange={() => setIsExpanded(p => !p)}>
                <AccordionSummary expandIcon={<ExpandIcon />}>
                    <SummaryBox style={{padding: 0, margin: 0}} items={[
                        {
                            label: t("total"),
                            content: transaction == undefined 
                                        ? 
                                        <Skeleton animation="wave"/> 
                                        :
                                        <CurrencySpan value={transaction.payment + transaction.tip} />
                        },
                        {
                            label: t("amount"),
                            content: transaction == undefined 
                                        ? 
                                        <Skeleton animation="wave"/> 
                                        :
                                        <CurrencySpan value={transaction.payment} />
                        },
                        {
                            label: t("tip"),
                            content: transaction == undefined 
                                        ? 
                                        <Skeleton animation="wave"/> 
                                        :
                                        <CurrencySpan value={transaction.tip} />
                        },
                    ]} />
                </AccordionSummary>
                {
                    transaction != undefined &&
                    <AccordionDetails>
                        <TransactionItemsPage transaction={transaction} canLoadItems={isExpanded} />
                    </AccordionDetails>
                }
            </Accordion>
        </Tooltip>
    
        {
            transaction != undefined && 
            transaction.refundedAmount > 0 &&
            <HighlightMessage messageType={MessageType.warning}>
                <Trans
                    t={t}
                    i18nKey="paymentHistory.transactionWasRefundedWarning"
                    shouldUnescape={true}
                    components={{
                        amount: <CurrencySpan value={transaction.refundedAmount} />,
                        description: <RefundDescription transaction={transaction} />
                    }}
                />
            </HighlightMessage>
        }
        <Grid container style={{ marginTop: "1rem" }}>
            <Grid size={6} flexDirection="column" alignContent="start" flexWrap="wrap" display="flex">
                <Typography variant="caption" component="label" gutterBottom fontWeight="bold">
                    {t("email")}
                </Typography>

                <Typography variant="body2" component="label" gutterBottom>
                {
                    transaction == undefined
                    ?
                    <Skeleton animation="wave" />
                    :
                    (transaction.email != undefined ? transaction.email : t("notAvailable"))
                }
                </Typography>
            </Grid>
            <Grid size={6} flexDirection="column" alignContent="end" flexWrap="wrap" display="flex">
                <Typography variant="caption" component="label" gutterBottom fontWeight="bold">
                    {t("vatNumber")}
                </Typography>

                <Typography variant="body2" component="label" gutterBottom>
                {
                        transaction == undefined
                        ?
                        <Skeleton animation="wave" />
                        :
                        (!!transaction.vatNumber ? transaction.vatNumber : t("notAvailable"))
                }
                </Typography>
            </Grid>
        </Grid>

        <Grid container>
            <Grid size={6} flexDirection="column" alignContent="start" flexWrap="wrap" display="flex">
                <Typography variant="caption" component="label" gutterBottom fontWeight="bold">
                    {t("documents")}
                </Typography>

                <Typography variant="body2" component="label" gutterBottom>
                {
                    transaction == undefined
                    ?
                    <Skeleton animation="wave" />
                    :
                    (
                        documentsQuery.data.length == 0
                        ?
                        t("notAvailable")
                        :
                        documentsQuery.data.filter(p => auth.principal?.isAdmin == true || p.type != TransactionInvoiceType.Surcharge).map(s =>
                            <React.Fragment key={s.id}>
                                {s.name}
                                &nbsp;
                                <a href={s.url} target="_blank" rel="noopener noreferrer">
                                    <DownloadIcon
                                        fontSize="inherit"
                                        style={{
                                            verticalAlign: "middle",
                                        }}
                                    />
                                </a>
                                <br/>
                            </React.Fragment>
                        )
                    )
                }
                </Typography>
            </Grid>
            <Grid size={6} flexDirection="column" alignContent="end" flexWrap="wrap" display="flex" />
        </Grid>
    </Stack>
}

interface RefundDescriptionProps {
    readonly transaction: Transaction;
}
const RefundDescription = (props: RefundDescriptionProps) => {
    const { t } = useTranslation();
    const dateHelper = useDateHelper();

    const refundEmployeeQuery = useEmployeesQuery(props.transaction.refundEmployeeId == undefined ? undefined : {
        ids: [props.transaction.refundEmployeeId],
        includeDeleted: true,
        page: 0,
    });
    const refundEmployee = refundEmployeeQuery.isFirstLoading || refundEmployeeQuery.data.length == 0 ? undefined : refundEmployeeQuery.data[0];
   
    const getEmployeeDescription = (name: string) => {
        const string = t("byEmployee", {
            name: name,
        });

        return string[0].toLowerCase() + string.slice(1);
    }

    return (
        <span>
            {dateHelper.toLocalString(props.transaction.lastModified, `D MMM YYYY`)}
            <span style={{textTransform: "lowercase"}}>&nbsp;{t("dateHelper.at")}&nbsp;</span>
            {dateHelper.toLocalString(props.transaction.lastModified, `HH:mm`)}
            {
                props.transaction.refundEmployeeId != undefined &&
                <span style={{textTransform: "-moz-initial"}}>
                    &nbsp;
                    {
                        refundEmployee == undefined
                        ?
                        <Skeleton animation="wave" width="30" />
                        :
                        getEmployeeDescription(refundEmployee.name)
                    }
                </span>
            }
        </span>
    )
}