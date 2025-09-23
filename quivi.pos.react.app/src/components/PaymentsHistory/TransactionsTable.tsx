import { useMemo, useState } from "react"
import { Accordion, AccordionSummary, Chip, Skeleton, Stack, Tooltip, Typography } from "@mui/material"
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
import { CollectionFunctions } from "../../helpers/collectionsHelper";
import { ResponsiveTable } from "../Tables/ResponsiveTable";
import { ChannelProfile } from "../../hooks/api/Dtos/channelProfiles/ChannelProfile";

interface ExtendedTransaction {
    readonly data: Transaction;
    readonly channel?: Channel;
    readonly profile?: ChannelProfile;
    readonly employee?: Employee;
    readonly chargeMethod?: CustomChargeMethod;
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
    const dateHelper = useDateHelper();

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
    const channelsMap = useMemo(() => CollectionFunctions.toMap(channelsQuery.data, c => c.id), [channelsQuery.data])
    const profileIds = useMemo(() => {
        const ids = new Set<string>();
        for(const channel of channelsQuery.data) {
            ids.add(channel.channelProfileId);
        }
        return Array.from(ids);
    }, [channelsQuery.data])

    const profilesQuery = useChannelProfilesQuery(profileIds.length == 0 ? undefined : {
        ids: profileIds,
        page: 0,
    })
    const channelProfilesMap = useMemo(() => CollectionFunctions.toMap(profilesQuery.data, c => c.id), [profilesQuery.data])

    const employeesQuery = useEmployeesQuery(transactionsQuery.isFirstLoading ? undefined : {
        ids: transactionsQuery.data.filter(t => t.employeeId != undefined).map(t => t.employeeId!),
        includeDeleted: true,
        page: 0,
    });
    const employeesMap = useMemo(() => CollectionFunctions.toMap(employeesQuery.data, e => e.id), [employeesQuery.data])

    const chargeMethodsQuery = useCustomChargeMethodsQuery({
        page: 0,
    });
    const chargeMethodsMap = useMemo(() => CollectionFunctions.toMap(chargeMethodsQuery.data, c => c.id), [chargeMethodsQuery.data])

    const transactions = useMemo(() => {
        const result = [] as ExtendedTransaction[];
        for(const t of transactionsQuery.data) {
            const channel = channelsMap.get(t.channelId);
            result.push({
                data: t,
                channel: channel,
                profile: channel == undefined ? undefined : channelProfilesMap.get(channel.channelProfileId),
                employee: t.employeeId == undefined ? undefined : employeesMap.get(t.employeeId),
                chargeMethod: t.customChargeMethodId == undefined ? undefined : chargeMethodsMap.get(t.customChargeMethodId),
            })
        }
        return result;
    }, [transactionsQuery.data, chargeMethodsMap, employeesMap, channelsMap, channelProfilesMap])

    return (
        <>
            <Accordion
                square
                sx={{
                    backgroundColor: t => t.palette.background.paper,
                    width: "100%",
                }}
                expanded
            >
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

            <ResponsiveTable
                data={transactions}
                isLoading={transactionsQuery.isFirstLoading}
                getKey={t => t.data.id}
                columns={[
                    {
                        key: "logo",
                        label: "",
                        render: row => row.chargeMethod == undefined ? <QuiviIcon style={{ height: "30px", width: "auto" }} /> : <img style={{ height: "30px", width: "auto" }} src={row.chargeMethod.logoUrl} />
                    },
                    {
                        key: "date",
                        label: t("date"),
                        render: row => dateHelper.toLocalString(row.data.capturedDate, "YYYY-MM-DD HH:mm"),
                    },
                    ...(
                        hideChannel == true
                        ?
                        []
                        :
                        [
                            {
                                key: "channel",
                                label: t("channel"),
                                render: (row: ExtendedTransaction) => row.channel == undefined || row.profile == undefined ? <Skeleton animation="wave" /> : `${row.profile.name} ${row.channel.name}`,
                            },
                        ]
                    ),
                    {
                        key: "total",
                        label: t("total"),
                        render: row => (
                        <Stack direction="row" gap={1} justifyContent="start" alignItems="center">
                            <CurrencySpan value={row.data.payment + row.data.tip} />
                            {
                                row.data.refundedAmount > 0 &&
                                <Tooltip title={t("refunded")}>
                                    <Chip
                                        label={<CurrencySpan value={-1 * row.data.refundedAmount} />}
                                        variant="outlined"
                                        icon={<RefundIcon style={{padding: "0.5rem"}}/>} 
                                    />
                                </Tooltip>
                            }
                        </Stack>
                        )
                    },
                    {
                        key: "amount",
                        label: t("amount"),
                        render: row => <CurrencySpan value={row.data.payment} />,
                    },
                    {
                        key: "tip",
                        label: t("tip"),
                        render: row => <CurrencySpan value={row.data.tip} />,
                    },
                    {
                        key: "employee",
                        label: t("employee"),
                        render: row => (
                            row.data.employeeId != undefined
                            ?
                            (
                                row.employee == undefined
                                ?
                                    <Skeleton animation="wave" />
                                :
                                    row.employee.name
                            )
                            :
                                <Typography variant="body1" fontWeight="bold">N/A</Typography>
                        )
                    },
                ]}
                onRowClick={row => onTransactionDetailsClicked?.(row.data.id)}
            />

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