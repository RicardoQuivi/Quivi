import { useEffect, useMemo, useState } from "react"
import { useTranslation } from "react-i18next";
import { Skeleton, Stack, Tab, Tabs, Typography } from "@mui/material";
import { useTransactionsQuery } from "../../hooks/queries/implementations/useTransactionsQuery";
import { useEmployeesQuery } from "../../hooks/queries/implementations/useEmployeesQuery";
import { useChannelsQuery } from "../../hooks/queries/implementations/useChannelsQuery";
import { useChannelProfilesQuery } from "../../hooks/queries/implementations/useChannelProfilesQuery";
import { TransactionDetails } from "./TransactionDetails";

interface Props {
    readonly transactionId: string;
}
export const TransactionOverview = (props: Props) => {
    const { t } = useTranslation();
    //const loggedEmployeeContext = useLoggedEmployee();

    const transactionQuery = useTransactionsQuery({
        ids: [props.transactionId],
        page: 0,
    })
    const transaction = useMemo(() => transactionQuery.data.length == 0 ? undefined : transactionQuery.data[0], [transactionQuery.data])

    const transactionEmployeeQuery = useEmployeesQuery(transaction?.employeeId == undefined ? undefined : {
        ids: [transaction.employeeId],
        includeDeleted: true,
        page: 0,
    });
    const transactionEmployee = useMemo(() => transactionEmployeeQuery.data.length == 0 ? undefined : transactionEmployeeQuery.data[0], [transactionEmployeeQuery.data])

    const channelQuery = useChannelsQuery(transaction == undefined ? undefined : {
        ids: [transaction.channelId],
        includeDeleted: true,
        page: 0,
    });
    const channel = useMemo(() => channelQuery.data.length == 0 ? undefined : channelQuery.data[0], [channelQuery.data])


    const channelProfileQuery = useChannelProfilesQuery(channel == undefined ? undefined : {
        ids: [channel.channelProfileId],
        page: 0,
    });
    const channelProfile = useMemo(() => channelProfileQuery.data.length == 0 ? undefined : channelProfileQuery.data[0], [channelProfileQuery.data])

    const [tab, setTab] = useState(0);

    useEffect(() => {
        if(transaction == undefined) {
            return;
        }

        if(transaction.sessionId == undefined && tab == 1) {
            setTab(0);
        }
    }, [transactionQuery.data])

    return <Stack
        direction="column"
        gap={2}
    >
        <Typography 
            variant="h6"
            gutterBottom
            textAlign="center"
        >
        {
            channel == undefined || channelProfile == undefined
            ?
            <Skeleton animation="wave" />
            :
            `${channelProfile.name} ${channel.name}`
        }
        </Typography>
        <Typography 
            variant="subtitle1"
            gutterBottom
            textAlign="center"
        >
            {t("employee")}
            &nbsp;
            <Typography variant="caption" gutterBottom>
            {
                transaction == undefined
                ?
                <Skeleton animation="wave"/>
                :
                <>
                    (
                    {
                        transaction.employeeId == undefined 
                        ? 
                            t("notAvailable") 
                        :
                        (
                            transactionEmployee == undefined
                            ?
                            <Skeleton animation="wave"/>
                            : 
                            transactionEmployee.name
                        )
                    }
                    )
                </>
            }
            </Typography>
        </Typography>
        <Tabs value={tab} onChange={(c, value: number) => setTab(value)} indicatorColor="primary" textColor="primary" variant="fullWidth" style={{marginBottom: "1rem"}}>
            <Tab
                label={
                    <Typography variant="body2">
                        {t("details")}
                    </Typography>
                }
                value={0}
            />
            {/* {
                (transaction == undefined || transaction.sessionId != undefined) &&
                <Tab
                    label={
                        <Typography variant="body2">
                            {t("sessionDetails")}
                        </Typography>
                    }
                    value={1}
                />
            }
            {
                transaction != undefined &&
                transaction.refundedAmount == 0 &&
                loggedEmployeeContext.employee.restrictions.find(r => r == EmployeeRestriction.Refunds) == undefined &&
                <Tab
                    label={
                        <Typography variant="body2">
                            {t("actions")}
                        </Typography>
                    }
                    value={2}
                />
            } */}
        </Tabs>
        {
            tab == 0 &&
            <TransactionDetails transaction={transaction} />
        }
        {/* 
        {
            tab == 1 &&
            transaction?.sessionId != undefined &&
            <SessionOverview sessionId={transaction.sessionId} />
        }
        {
            tab == 2 &&
            transaction != undefined &&
            transaction.refundedAmount == 0 &&
            <TransactionActions
                transaction={transaction}
                onRefundSuccess={() => setTab(0)}
            />
        } */}
    </Stack>
}