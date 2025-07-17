import { useState } from "react";
import { useTranslation } from "react-i18next";
import LoadingButton from "../Buttons/LoadingButton";
import { useChannelContext } from "../../context/AppContextProvider";
import { useNavigate } from "react-router";
import { SchedulerDialog, SchedulerDialogState } from "../Ordering/SchedulerDialog";
import { useMenuItemsQuery } from "../../hooks/queries/implementations/useMenuItemsQuery";

export const MenuSelector = () => {
    const { t } = useTranslation();
    const navigate = useNavigate();
    const channelContext = useChannelContext();

    const [orderScheduleModalOpen, setOrderScheduleModalOpen] = useState(false);
    const [schedulerDialogLoading, setSchedulerDialogLoading] = useState(false);

    const itemsQuery = useMenuItemsQuery({
        channelId: channelContext.channelId,
        page: 0,
        pageSize: 0,
    })

    const goToDigitalMenuWithSchedule = async (date: Date | undefined) => {
        setOrderScheduleModalOpen(false);
        const timestamp = date?.getTime();
        navigate(`/c/${channelContext.channelId}/menu${timestamp == undefined ? "" : `?timestamp=${timestamp}`}`)
    }

    const onDialogChange = (state: SchedulerDialogState) => {
        switch(state)
        {
            case SchedulerDialogState.Closed: setOrderScheduleModalOpen(false); break;
            case SchedulerDialogState.IsOpening: setSchedulerDialogLoading(true); break;         
            case SchedulerDialogState.Opened: setSchedulerDialogLoading(false); break;         
        }
    }

    if(itemsQuery.isFirstLoading == false && itemsQuery.totalItems == 0) {
        return <></>;
    }
    
    return <>
        <LoadingButton
            className="w-100"
            onClick={() => setOrderScheduleModalOpen(true)}
            primaryButton={false}
            isLoading={schedulerDialogLoading}
            disabled={itemsQuery.isFirstLoading}
        >
            {
                channelContext.features.ordering.isActive
                ?
                    t("home.order")
                :
                    t("home.seeMenu")
            }
        </LoadingButton>
        <SchedulerDialog isOpen={orderScheduleModalOpen} onDialogChange={onDialogChange} onDateSelected={goToDigitalMenuWithSchedule}/>
    </>
}