import { useTranslation } from "react-i18next";
import { Local } from "../../hooks/api/Dtos/locals/Local";
import { useLocalsQuery } from "../../hooks/queries/implementations/useLocalsQuery";
import { useEffect, useState } from "react";
import CustomModal from "../Modals/CustomModal";
import { Box, Grid } from "@mui/material";
import LoadingButton from "../Buttons/LoadingButton";
import { SingleSelect } from "../Inputs/SingleSelect";

interface Props {
    readonly isOpen: boolean;
    readonly selectedLocal: Local | undefined;
    readonly onClose: (selectedLocal: Local | undefined) => any;
}
export const ChangeLocalModal = (props: Props) => {
    const { t } = useTranslation();
    const localsQuery = useLocalsQuery({
    })
    
    const [state, setState] = useState({
        selectedLocation: props.selectedLocal,
    })

    useEffect(() => {
        if(props.isOpen == false) {
            return;
        }
        
        setState({
            selectedLocation: props.selectedLocal,
        });
    }, [props.isOpen])

    return <CustomModal 
        isOpen={props.isOpen}
        onClose={() => props.onClose(state.selectedLocation)}
        title={t("changeLocal")}
        footer={
            <Grid container sx={{width: "100%", margin: "1rem 0.25rem"}} spacing={1}>
                <Grid size="grow">
                    <LoadingButton style={{width: "100%"}} onClick={() => props.onClose(state.selectedLocation)} primaryButton={false}>
                        {t("cancel")}
                    </LoadingButton>
                </Grid>
                <Grid size="grow">
                    <LoadingButton style={{width: "100%"}} primaryButton onClick={() => props.onClose(state.selectedLocation)} disabled={props.selectedLocal == state.selectedLocation}>
                        {t("confirm")}
                    </LoadingButton>
                </Grid>
            </Grid>
        }
    >
        <Box sx={{padding: "1rem 0.5rem"}}>
            <SingleSelect
                label={t("local")}
                value={state.selectedLocation} 
                options={[
                    undefined,
                    ...localsQuery.data,
                ]}
                getId={l => l?.id ?? "all"}
                render={l => l?.name ?? t("all")}
                onChange={l => setState(s => ({...s, selectedLocation: l}))}
            />
        </Box>
    </CustomModal>
}