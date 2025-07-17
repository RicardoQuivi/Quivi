import { useEffect, useState } from "react";
import Dialog from "../Shared/Dialog"
import { StaticDateTimePicker } from "@mui/x-date-pickers";
import { Trans, useTranslation } from "react-i18next";
import { differenceInDays } from "date-fns";
import ActionButton from "../Buttons/ActionButton";
import { ButtonsSection } from "../../layout/ButtonsSection";
import { List, ListItem, ListItemText, styled } from "@mui/material";
import { useQuiviTheme, type IColor } from "../../hooks/theme/useQuiviTheme";
import { useChannelContext } from "../../context/AppContextProvider";
import { useCart } from "../../context/OrderingContextProvider";
import type { ISchedulerChanger } from "../../context/cart/ISchedulerChanger";
import { CloseIcon } from "../../icons";

interface StyledStaticDateTimePickerProps {
    readonly primarycolor: IColor;
}
 
const StyledStaticDateTimePicker = styled(StaticDateTimePicker)(({
    primarycolor,
}: StyledStaticDateTimePickerProps) => ({
    '& .MuiPickersLayout-contentWrapper': {
        '& .MuiTabs-root': {
            boxShadow: "unset",
            '& .MuiTabs-scroller': {
                '& .MuiTabs-indicator': {
                    backgroundColor: 'unset',
                    background: `linear-gradient(90deg, ${primarycolor.hex} 0%, ${primarycolor.hex} 50%, rgba(0, 0, 0, 0) 50%)`,
                    height: '6px',
                    bottom: '0px',
                },

                '& .MuiTabs-flexContainer': {
                    '& > *': {
                        margin: "7px",
                        '&:first-child': {
                            marginLeft: '0px',
                        },
                        
                        '&:last-child': {
                            marginRight: '0px',
                        },
                    },
        
                    '& .MuiTab-root': {
                        padding: 0,
                        minWidth: 'unset',
                        fontFamily: 'unset'
                    },
        
                    '& .MuiButtonBase-root': {
                        marginTop: '0px',
                        marginBottom: '0px',

                        '& .MuiTab-wrapper': {
                            textTransform: 'capitalize',
                            padding: '5px'
                        },
        
                        '&.Mui-selected': {
                            background: `rgba(${primarycolor.r}, ${primarycolor.g}, ${primarycolor.b}, 0.15)`,
                            borderRadius: '4px',
                            color: primarycolor.hex,
                        }
                    },
                }
            },
        },

        '& .MuiDateCalendar-root': {
            '& .MuiDayCalendar-root': {
                "& .MuiDayCalendar-monthContainer": {
                    "& .MuiDayCalendar-weekContainer": {
                        "& .Mui-selected": {
                            backgroundColor: primarycolor.hex,
                        },

                        '& .MuiPickersDay-today': {
                            borderColor: primarycolor.hex,
                        },
                    },
                },
            },
        },

        '& .MuiTimeClock-root': {
            '& .MuiClock-clock': {
                "& .MuiClock-pin": {
                    backgroundColor: primarycolor.hex,
                },

                '& .MuiClockPointer-root': {
                    backgroundColor: primarycolor.hex,

                    '& .MuiClockPointer-thumb': {
                        backgroundColor: primarycolor.hex,
                        borderColor: primarycolor.hex,
                    },
                },
            },
        },
    },
}));

export enum SchedulerDialogState {
    IsOpening,
    Opened,
    Closed,
}

interface Props {
    readonly date?: Date;
    readonly isOpen: boolean;
    onDialogChange: (state: SchedulerDialogState) => any;
    onDateSelected: (date: Date | undefined) => any;
}

export const SchedulerDialog = (props: Props) => {
    const { t } = useTranslation();
    const theme = useQuiviTheme();
    const channelContext = useChannelContext();

    const [isOpen, setIsOpen] = useState<boolean>(false)
    const [asSoonAsPossible, setAsSoonAsPossible] = useState<boolean>();
    const [orderScheduleDate, setOrderScheduleDate] = useState<Date | undefined>(props.date);
    const [scheduler, setScheduler] = useState<ISchedulerChanger>();

    const cart = useCart();
    
    useEffect(() => setOrderScheduleDate(props.date), [props.date])

    useEffect(() => {
        if(props.isOpen == false) {
            setIsOpen(false);
            return;
        }

        props.onDialogChange(SchedulerDialogState.IsOpening);

        if(channelContext.features.ordering.isActive == false || channelContext.features.ordering.allowScheduling == false) {
            next(undefined);
            return;
        }

        setIsOpen(true);
    }, [props.isOpen])

    useEffect(() => {
        if(isOpen == false) {
            setScheduler(undefined);
            return;
        }
        
        props.onDialogChange(SchedulerDialogState.Opened);
    }, [isOpen])

    useEffect(() => {
        if(asSoonAsPossible == undefined) {
            return;
        }

        if(asSoonAsPossible == false) {
            setOrderScheduleDate(new Date());
            return;
        }
    }, [asSoonAsPossible])

    const next = async (scheduleDate: Date | undefined) => {
        const scheduler = await cart.setScheduleDate(scheduleDate);

        if(scheduler.unavailableItems.length == 0) {
            await confirmAndGo(scheduler);
            return;
        }
        setScheduler(scheduler);
    }
    
    const confirmAndGo = async (scheduler: ISchedulerChanger) => {
        await scheduler.confirm();
        props.onDateSelected(scheduler.date);
    }

    const chooseNewDate = () => setScheduler(undefined);

    return <Dialog isOpen={isOpen} onClose={() => props.onDialogChange(SchedulerDialogState.Closed)}>
        <div className="container">
            <div className="modal__header mb-5" style={{alignItems: "baseline"}}>
                <h3>{t("orderScheduling.pickUpDate")}</h3>
                <div className="close-icon" onClick={() => props.onDialogChange(SchedulerDialogState.Closed)}>
                    <CloseIcon />
                </div>
            </div>
            {
                scheduler == undefined 
                ?
                (
                    asSoonAsPossible == undefined
                    ?
                    <ButtonsSection>
                        <ActionButton className="w-100" onClick={() => next(undefined)} primaryButton={true}>
                            {t("orderScheduling.asSoonAsPossible")}
                        </ActionButton>
                        <ActionButton className="w-100" onClick={() => setAsSoonAsPossible(false)} primaryButton={false}>
                            {t("orderScheduling.forLater")}
                        </ActionButton>
                    </ButtonsSection>
                    :
                    <>                            
                        <StyledStaticDateTimePicker 
                            value={orderScheduleDate}
                            onChange={(m) => setOrderScheduleDate(m as Date)}
                            orientation="landscape"
                            openTo="day"
                            localeText={{
                                toolbarTitle: "",
                            }}
                            shouldDisableDate={(d) => differenceInDays(d as Date, new Date()) > 6}
                            disablePast
                            slots={{
                                actionBar: () => <></>,
                            }} 
                            primarycolor={theme.primaryColor}                  
                        />

                        <ButtonsSection>
                            <ActionButton className="w-100" onClick={() => setAsSoonAsPossible(undefined)} primaryButton={true}>
                                {t("back")}
                            </ActionButton>
                            <ActionButton className="w-100" onClick={() => next(orderScheduleDate)} primaryButton={false}>
                                {t("next")}
                            </ActionButton>
                        </ButtonsSection>
                    </>
                )
                :
                <>
                    <Trans
                        t={t}
                        i18nKey="orderScheduling.itemsUnavailableAtPickUpDate"
                        components={{
                            list:   <List>
                                        {
                                            scheduler.unavailableItems.map(item => (<ListItem key={item.id}>
                                                <ListItemText primary={item.name} />
                                            </ListItem>))
                                        }
                                    </List>,
                        }}
                        />

                    <ButtonsSection>
                        <ActionButton className="w-100" onClick={() => chooseNewDate()} primaryButton={true}>
                            {t("orderScheduling.chooseNewDate")}
                        </ActionButton>
                        <ActionButton className="w-100" onClick={() => confirmAndGo(scheduler)} primaryButton={false}>
                            {t("orderScheduling.confirm")}
                        </ActionButton>
                    </ButtonsSection>
                </>
            }
        </div>
    </Dialog>
}