import { useMemo,} from 'react';
import { PaymentSplitter, type IShareEqualSettings } from '../helpers/paymentSplitter';
import { useSessionsQuery } from './queries/implementations/useSessionsQuery';
import { useTransactionsQuery } from './queries/implementations/useTransactionsQuery';

const loadingState: IShareEqualSettings = {
    amountPerPerson: 0,
    peopleAtTheTable: 0,
    peopleWhoPaid: 0,
    isShareable: false,
    isLoading: true,
}

const divisionNotAllowedState: IShareEqualSettings = {
    amountPerPerson: 0,
    peopleAtTheTable: 0,
    peopleWhoPaid: 0,
    isShareable: false,
    isLoading: false,
}

export const useShareEqualSettings = (channelId: string) => {
    const sessionQuery = useSessionsQuery({
        channelId: channelId,
    });

    const sessionPayments = useTransactionsQuery(sessionQuery.data == undefined ? undefined : {
        sessionId: sessionQuery.data.id,
        page: 0,
    });

    const shareSettings = useMemo<IShareEqualSettings>(() => {
        if(sessionQuery.isFirstLoading) {
            return loadingState;
        }

        const session = sessionQuery.data;
        if(session == undefined || session.items.length == 0) {
            return divisionNotAllowedState;
        }

        if(sessionPayments.isFirstLoading) {
            return loadingState;
        }

        if(sessionPayments.data.length > 0) {
            let firstPaymentDate = sessionPayments.data.filter(p => p.capturedDate != undefined).map(p => new Date(p.capturedDate!)).sort((a, b) => a.getTime() - b.getTime())[0];
            const orderedUnpaidItemDates = session.items.filter(item => item.isPaid == false).map(i => new Date(i.lastModified)).sort((a, b) => a.getTime() - b.getTime());
            if(orderedUnpaidItemDates.length > 0) {
                let lastUnpaidItemDate = orderedUnpaidItemDates[0];
                if (firstPaymentDate.getTime() < lastUnpaidItemDate.getTime()) {
                    return divisionNotAllowedState;
                }
            }
        }

        return PaymentSplitter.getShareEqualSettings(session.unpaid, sessionPayments.data.map(p => p.syncedAmount));
    }, [sessionQuery.data, sessionPayments.data])

    return shareSettings;
}