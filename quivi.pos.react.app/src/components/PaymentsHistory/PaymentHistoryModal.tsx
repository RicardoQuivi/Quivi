import { Box, Grid, Typography } from '@mui/material';
import React, { useContext, useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import CustomModal, { ModalSize } from '../Modals/CustomModal';
import LoadingButton from '../Buttons/LoadingButton';
import { LeftArrowIcon } from '../../icons';
import { TransactionsTable } from './TransactionsTable';
import { TransactionOverview } from './TransactionOverview';

const defaultState = {
    transactionId: undefined as (string | undefined),
}
interface Props {
    readonly isOpen: boolean;
    readonly onClose: () => any;
}
export const PaymentHistoryModal: React.FC<Props> = ({
    isOpen,
    onClose,
}) => {
    const { t } = useTranslation();
    const [state, setState] = useState(defaultState)

    const setTransactionId = (transactionId: string | undefined) => setState(s => ({...s, transactionId: transactionId}))

    const getTitle = () => {
        if(state.transactionId == undefined) {
            return t("paymentHistory.title");
        }
        return <Box style={{display: "flex", flexDirection: "row"}}>
            <LeftArrowIcon style={{height: "35px", width: "35px", cursor: "pointer"}} onClick={() => setTransactionId(undefined)}/>
            <Typography variant='h4' style={{flex: "1 1 auto", textAlign: "center"}}>
                {t("paymentHistory.transaction")} {state.transactionId}
            </Typography>
        </Box>
    }

    useEffect(() => setState(defaultState), [isOpen])

    return (
    <CustomModal 
        isOpen={isOpen} 
        title={getTitle()} 
        size={ModalSize.ExtraLarge} 
        onClose={onClose} 
        disableCloseOutsideModal
        footer={
            <Grid container sx={{width: "100%", margin: "1rem 0.25rem"}} spacing={1} justifyContent="center">
                <Grid size={{md: "auto"}}>
                    <LoadingButton primaryButton onClick={onClose} style={{width: "100%"}}>
                        {t("close")}
                    </LoadingButton>
                </Grid>
            </Grid>
        }
    >
        {
            state.transactionId == undefined
            ?
                <TransactionsTable
                    onTransactionDetailsClicked={setTransactionId}
                />
            :
                <TransactionOverview transactionId={state.transactionId} />
        }
    </CustomModal>
    )
}