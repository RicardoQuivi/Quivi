import React, { useEffect, useState } from "react"
import { useTranslation } from "react-i18next";
import { Box } from "@mui/system";
import { SessionItem } from "../../hooks/api/Dtos/sessions/SessionItem";
import CustomModal, { ModalSize } from "../Modals/CustomModal";
import { Grid } from "@mui/material";
import LoadingButton from "../Buttons/LoadingButton";
import { MenuItem } from "../../hooks/api/Dtos/menuitems/MenuItem";
import { NumberInputField } from "../Inputs/NumberInput";

interface Props {
    readonly item?: SessionItem;
    readonly canAdd: boolean;
    readonly canRemove: boolean;
    readonly canApplyDiscounts: boolean;
    readonly onClose: () => void;
    readonly onSubmit: (item: SessionItem, quantity: number) => any;
    readonly itemsMap: Map<string, MenuItem>;
}
export const PickItemQuantityModal: React.FC<Props> = (props) => {
    const { t } = useTranslation();

    const [state, setState] = useState({
        quantity: props.item?.quantity ?? 0,
    })

    useEffect(() => setState(s => ({...s, quantity: props.item?.quantity ?? 0})), [props.item])

    const onSubmit = (item: SessionItem) => {
        props.onSubmit(item, state.quantity);
        props.onClose();
    }

    const item = props.item;
    const canAdd = props.canAdd && (props.canApplyDiscounts == true || item?.discountPercentage == 0);
    const canRemove = props.canRemove;

    if(item == undefined) {
        return <></>
    }

    return (
        <CustomModal 
            isOpen={!!item}
            onClose={props.onClose}
            title={t("pickItemQuantity", {
                name: item == undefined ? undefined : props.itemsMap.get(item.menuItemId)?.name,
            })} 
            size={ModalSize.Small}
            disableCloseOutsideModal
            footer={
                <Grid container sx={{width: "100%", margin: "1rem 0.25rem"}} spacing={1}>
                    <Grid size="grow">
                        <LoadingButton isLoading={false} onClick={props.onClose} style={{width: "100%"}}>
                            {t("close")}
                        </LoadingButton>
                    </Grid>
                    <Grid size="grow">
                        <LoadingButton isLoading={false} disabled={item.quantity == state.quantity} onClick={() => onSubmit(item)} primaryButton style={{width: "100%"}}>
                            {t("confirm")}
                        </LoadingButton>
                    </Grid>
                </Grid>
            }
        >
            <Box sx={{ display: "flex", gap: 2, justifyContent: "center", p: 2 }}>
                <NumberInputField 
                    value={state.quantity}
                    onChange={(q) => setState(s => ({...s, quantity: canAdd == false ? Math.min(q, item.quantity) : Math.max(q, 0) }))}

                    minValue={0}
                    maxValue={canAdd ? undefined : item.quantity}

                    decrementDisabled={canRemove == false ? state.quantity == item.quantity : state.quantity == 0}
                    incrementDisabled={canAdd == false ? state.quantity == item.quantity : undefined}
                />
            </Box>
        </CustomModal>
    )
}