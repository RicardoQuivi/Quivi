import { useMemo, useState } from "react"
import { Accordion, AccordionSummary, Chip, Skeleton, Stack, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Tooltip } from "@mui/material"
import { useTranslation } from "react-i18next"
import { useChannelsQuery } from "../../hooks/queries/implementations/useChannelsQuery";
import { useEmployeesQuery } from "../../hooks/queries/implementations/useEmployeesQuery";
import CurrencySpan from "../Currency/CurrencySpan";
import { PaginationFooter } from "../Pagination/PaginationFooter";
import { useTransactionsQuery } from "../../hooks/queries/implementations/useTransactionsQuery";
import { Channel } from "../../hooks/api/Dtos/channels/Channel";
import { Employee } from "../../hooks/api/Dtos/employees/Employee";
import { useCustomChargeMethodsQuery } from "../../hooks/queries/implementations/useCustomChargeMethodsQuery";
import { Transaction } from "../../hooks/api/Dtos/transactions/Transaction";
import { CustomChargeMethod } from "../../hooks/api/Dtos/customchargemethods/CustomChargeMethod";
import { useDateHelper } from "../../helpers/dateHelper";
import { useChannelProfilesQuery } from "../../hooks/queries/implementations/useChannelProfilesQuery";
import { QuiviIcon, RefundIcon } from "../../icons";
import { AGetTransactionsRequest } from "../../hooks/api/Dtos/transactions/AGetTransactionsRequest";
import { useTransactionsResumeQuery } from "../../hooks/queries/implementations/useTransactionsResumeQuery";
import { SummaryBox } from "../common/SummaryBox";

const dateToString = (d: Date) => {
    const year = d.getFullYear();
    const month = d.getMonth() + 1;
    const day = d.getDate();
    const hour = d.getHours();
    const minute = d.getMinutes();
    const format = (n: number) => n.toLocaleString(undefined, {minimumIntegerDigits: 2});
    
    const result = `${year}-${format(month)}-${format(day)} ${format(hour)}:${format(minute)}`;
    return result;
}

interface Props extends AGetTransactionsRequest {
    readonly onTransactionDetailsClicked?: (transactionId: string) => any;
    readonly hideChannel?: boolean;
}
export const TransactionsTable = ({
    onTransactionDetailsClicked,
    hideChannel,
    ...baseRequest
}: Props) => {
    const { t } = useTranslation();
   
    const [state, setState] = useState({
        currentPage: 0,
    })
    
    const transactionsResumeQuery = useTransactionsResumeQuery(baseRequest);
    const transactionsQuery = useTransactionsQuery({
        ...baseRequest,
        page: state.currentPage,
        pageSize: 10,
    });

    const channelsQuery = useChannelsQuery(hideChannel == true || transactionsQuery.isFirstLoading ? undefined : {
        allowsSessionsOnly: false,
        ids: transactionsQuery.data.map(t => t.channelId),
        page: 0,
        includeDeleted: true,
    });
    const channelsMap = useMemo(() => {
        const result = new Map<string, Channel>();
        for(const d of channelsQuery.data) {
            result.set(d.id, d);
        }
        return result;
    }, [channelsQuery.data])

    const employeesQuery = useEmployeesQuery(transactionsQuery.isFirstLoading ? undefined : {
        ids: transactionsQuery.data.filter(t => t.employeeId != undefined).map(t => t.employeeId!),
        includeDeleted: true,
        page: 0,
    });
    const employeesMap = useMemo(() => {
        const result = new Map<string, Employee>();
        for(const d of employeesQuery.data) {
            result.set(d.id, d);
        }
        return result;
    }, [employeesQuery.data])

    const chargeMethodsQuery = useCustomChargeMethodsQuery({
        page: 0,
    });
    const chargeMethodsMap = useMemo(() => {
        const result = new Map<string, CustomChargeMethod>();
        for(const d of chargeMethodsQuery.data) {
            result.set(d.id, d);
        }
        return result;
    }, [chargeMethodsQuery.data])

    return (
        <>
            <Accordion square sx={{backgroundColor: "#F7F7F8", width: "100%"}} expanded>
                <AccordionSummary>
                    <SummaryBox 
                        isLoading={transactionsResumeQuery.isFirstLoading}
                        items={[
                            {
                                label: t("total"),
                                content: <CurrencySpan value={transactionsResumeQuery.data == undefined ? 0 : (transactionsResumeQuery.data.payment + transactionsResumeQuery.data.tip)} />
                            },
                            {
                                label: t("amount"),
                                content: <CurrencySpan value={transactionsResumeQuery.data == undefined ? 0 : transactionsResumeQuery.data.payment} />
                            },
                            {
                                label: t("tips"),
                                content: <CurrencySpan value={transactionsResumeQuery.data == undefined ? 0 : transactionsResumeQuery.data.tip} />
                            },
                        ]}
                    />
                </AccordionSummary>
            </Accordion>

            <TableContainer component={"div"}>
                <Table>
                    <TableHead>
                        <TableRow>
                            <TableCell />
                            <TableCell>{t("date")}</TableCell>
                            {
                                hideChannel != true &&
                                <TableCell align="right">{t("channel")}</TableCell>
                            }
                            <TableCell align="right">{t("total")}</TableCell>
                            <TableCell align="right">{t("amount")}</TableCell>
                            <TableCell align="right">{t("tip")}</TableCell>
                            <TableCell align="right">{t("employee")}</TableCell>
                            <TableCell align="right">{t("actions")}</TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {
                            transactionsQuery.isFirstLoading
                            ?
                            [1, 2, 3, 4, 5].map(r => <TransactionRow key={`loading-${r}`} row={undefined} channelsMap={channelsMap} chargeMethodsMap={chargeMethodsMap} employeesMap={employeesMap}/>)
                            :
                            transactionsQuery.data.map(r => <TransactionRow key={r.id} row={r} channelsMap={hideChannel == true ? undefined : channelsMap} chargeMethodsMap={chargeMethodsMap} employeesMap={employeesMap} onTransactionDetailsClicked={onTransactionDetailsClicked}/>)
                        }
                    </TableBody>
                </Table>
            </TableContainer>
    
            <PaginationFooter 
                currentPage={transactionsQuery.page}
                numberOfPages={transactionsQuery.totalPages}
                onPageChanged={p => setState(s => ({
                    ...s,
                    currentPage: p,
                }))}
            />
        </>
    )
}

interface RowProps {
    readonly row?: Transaction;
    readonly channelsMap?: Map<string, Channel>;
    readonly employeesMap: Map<string, Employee>;
    readonly chargeMethodsMap: Map<string, CustomChargeMethod>;
    readonly onTransactionDetailsClicked?: (transactionId: string) => any;
}
const TransactionRow = (props: RowProps) => {
    const { t } = useTranslation();
    const dateHelpers = useDateHelper();
    //const printersApi = usePrintersApi();
    // const printersQuery = usePrintersQuery({
    //     page: 0,
    // });

    const currentChannelQuery = useChannelsQuery(props.row == undefined ? undefined : {
        ids: [props.row.channelId],
        page: 0,
    })
    const currentChannel = useMemo(() => currentChannelQuery.data.length == 0 ? undefined : currentChannelQuery.data[0], [currentChannelQuery]);

    const channelProfileQuery = useChannelProfilesQuery(currentChannel == undefined ? undefined : {
        ids: [currentChannel.channelProfileId],
        page: 0,
    })
    const channelProfile = useMemo(() => channelProfileQuery.data.length == 0 ? undefined : channelProfileQuery.data[0], [channelProfileQuery]);

    // const integrationQuery = usePosIntegrationsQuery(channelProfile == undefined ? undefined : {
    //     ids: [channelProfile.posIntegrationId],
    //     page: 0,
    // })
    // const integration = useMemo(() => integrationQuery.data.length == 0 ? undefined : integrationQuery.data[0], [integrationQuery]);

    // const [state, setState] = useState({
    //     isPrinting: false,
    // })

    // const setIsPrinting = (b: boolean) => setState(s => ({...s, isPrinting: b}));
    // const Print = async (transaction: Transaction) => {
    //     setIsPrinting(true);

    //     try {
    //         //await printersApi.Print(transaction.id);
    //         toast.info(`${t('printing')}...`);
    //     } catch {
    //         toast.error(t('unexpectedErrorHasOccurred'));
    //     } finally {            
    //         setIsPrinting(false);
    //     }
    // }

    const row = props.row;
    const channel = row != undefined && props.channelsMap != undefined ? props.channelsMap.get(row.channelId) : undefined;

    const hasEmployee = row != undefined && row.employeeId != undefined;
    const employee = hasEmployee ? props.employeesMap.get(row.employeeId) : undefined;

    const hasChargeMethod = row != undefined && row.customChargeMethodId != undefined;
    const chargeMethod = hasChargeMethod ? props.chargeMethodsMap.get(row.customChargeMethodId) : undefined;

    return (
    <TableRow
        onClick={() => row != undefined && props.onTransactionDetailsClicked?.(row.id)}
        style={{
            cursor: [row, props.onTransactionDetailsClicked].every(p => p != undefined) ? "pointer" : undefined,
            backgroundColor: row != undefined && row.refundedAmount > 0 ? "rgba(255, 0, 0, 0.15)" : undefined,
        }}
    >
        <TableCell component="th" scope="row" style={{ display: "flex", justifyContent: "center" }}>
        {
            row == undefined
            ?
                <Skeleton animation="wave" />
            :
            (
                hasChargeMethod
                ?
                (
                    chargeMethod == undefined
                    ?
                        <Skeleton animation="wave" />
                    :
                        <img style={{ height: "30px", width: "auto" }} src={chargeMethod.logoUrl}/>
                )
                :
                    <QuiviIcon style={{ height: "30px", width: "auto" }} />
            )
        }
        </TableCell>
        <TableCell component="th" scope="row">
        {
            row == undefined
            ?
                <Skeleton animation="wave" />
            :
                dateToString(dateHelpers.toDate(row.capturedDate))
        }
        </TableCell>
        {
            props.channelsMap != undefined &&
            <TableCell align="right">
            {
                row == undefined || channel == undefined || channelProfile == undefined
                ?
                    <Skeleton animation="wave" />
                :
                    `${channelProfile.name} ${channel.name}`
            }
            </TableCell>
        }
        <TableCell align="right">
        {
            row == undefined
            ?
                <Skeleton animation="wave" />
            :
                <Stack direction="row" gap={1} justifyContent="flex-end" alignItems="center">
                    <CurrencySpan value={row.payment + row.tip} />
                    {
                        row.refundedAmount > 0 &&
                        <Tooltip title={t("refunded")}>
                            <Chip
                                label={<CurrencySpan value={-1 * row.refundedAmount} />}
                                variant="outlined"
                                icon={<RefundIcon style={{padding: "0.5rem"}}/>} 
                            />
                        </Tooltip>
                    }
                </Stack>
        }
        </TableCell>
        <TableCell align="right">
        {
            row == undefined
            ?
                <Skeleton animation="wave" />
            :
                <CurrencySpan value={row.payment} />
        }
        </TableCell>
        <TableCell align="right">
        {
            row == undefined
            ?
                <Skeleton animation="wave" />
            :
                <CurrencySpan value={row.tip} />
        }
        </TableCell>
        <TableCell align="right">
        {
            row == undefined
            ?
                <Skeleton animation="wave" />
            :
            (
                hasEmployee
                ?
                (
                    employee == undefined
                    ?
                        <Skeleton animation="wave" />
                    :
                        employee.name
                )
                :
                    <b>N/A</b>
            )
        }
        </TableCell>
        <TableCell align="right">
            <Stack
                direction="row"
                spacing={2}
                sx={{flexDirection: "row-reverse"}}
            >
            {/* {
                row == undefined
                ?
                    <Skeleton animation="wave" />
                :
                (
                    row.payment + row.tip == 0 || isFreePayment
                    ?
                    <></>
                    :
                    <>
                        {
                            printersQuery.isFirstLoading ||
                            state.isPrinting ||
                            currentChannel != undefined &&
                            channelProfile != undefined &&
                            (channelProfile.posIntegrationId != undefined && integrationQuery.isFirstLoading)
                            ?
                                <div className="spinner-border" role="status" style={{height: "16px", width: "16px"}}>
                                    <span className="sr-only">{t("loading")}...</span>
                                </div>
                            :
                            (
                                printersQuery.data.some(p => p.printConsumerInvoice) == true &&
                                integration != undefined &&
                                integration.isOnline &&
                                integration.allowsEscPosInvoices &&
                                <>
                                    <IconButton onClick={async (e) => { e.stopPropagation(); await Print(row); }}>
                                        <PrinterIcon style={{height: "16px", width: "16px"}} />
                                    </IconButton>
                                </>
                            )
                        }
                    </>
                )
            } */}
            </Stack>
        </TableCell>
    </TableRow>
    )
}