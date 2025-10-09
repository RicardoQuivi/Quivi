import * as yup from 'yup';
import { useTranslation } from 'react-i18next';
import { useEffect, useMemo, useState } from 'react';
import { useQuiviForm } from '../../../hooks/api/exceptions/useQuiviForm';
import Button from '../../../components/ui/button/Button';
import { useToast } from '../../../layout/ToastProvider';
import { TextField } from '../../../components/inputs/TextField';
import { Spinner } from '../../../components/spinners/Spinner';
import { Availability } from '../../../hooks/api/Dtos/availabilities/Availability';
import { ResponsiveTable } from '../../../components/tables/ResponsiveTable';
import { DayOfWeek } from '../../../hooks/api/Dtos/DayOfWeek';
import { WeeklyAvailability } from '../../../hooks/api/Dtos/availabilities/WeeklyAvailability';
import { PlusIcon, TrashBinIcon } from '../../../icons';
import { EnumHelper } from '../../../utilities/enum';
import { Modal, ModalSize } from '../../../components/ui/modal';
import { ModalButtonsFooter } from '../../../components/ui/modal/ModalButtonsFooter';
import { TimeField } from '../../../components/inputs/TimeField';
import { TimeSpanHelper } from '../../../utilities/timespanHelpers';
import Badge from '../../../components/ui/badge/Badge';
import { useDateHelper } from '../../../utilities/dateHelper';
import Alert from '../../../components/ui/alert/Alert';
import { ToggleSwitch } from '../../../components/inputs/ToggleSwitch';

interface WeeklyAvailabilitySeconds {
    readonly startAt: number;
    readonly endAt: number;
}
const getDayOfWeekSeconds = (dayOfWeek: DayOfWeek): number => {
    switch(dayOfWeek) {
        case DayOfWeek.Sunday: return 0 * secondsPerDay;
        case DayOfWeek.Monday: return 1 * secondsPerDay;
        case DayOfWeek.Tuesday: return 2 * secondsPerDay;
        case DayOfWeek.Wednesday: return 3 * secondsPerDay;
        case DayOfWeek.Thursday: return 4 * secondsPerDay;
        case DayOfWeek.Friday: return 5 * secondsPerDay;
        case DayOfWeek.Saturday: return 6 * secondsPerDay;
    }
}

const getDaySeconds = (date: Date) => {
    const hours = date.getHours();
    const minutes = date.getMinutes();
    const seconds = date.getSeconds();
    
    return hours * 3600 + minutes * 60 + seconds;
}

const getSeconds = (dateStr: string) => {
    const timespan = TimeSpanHelper.fromString(dateStr);
    return TimeSpanHelper.toSeconds(timespan);
}

const getIntervals = (dayOfWeek: DayOfWeek, startAt: Date, endAt: Date) => {
    const dayOfWeekSecondsOffset = getDayOfWeekSeconds(dayOfWeek);

    const intervals: {
        startAt: number,
        endAt: number,
    }[] = [];

    let startAtSeconds = getDaySeconds(startAt) + dayOfWeekSecondsOffset;
    let endAtSeconds = getDaySeconds(endAt) + dayOfWeekSecondsOffset;

    if(endAtSeconds <= startAtSeconds) {
        endAtSeconds += secondsPerDay;
    }

    const max = getDayOfWeekSeconds(DayOfWeek.Saturday) + secondsPerDay;
    if(endAtSeconds > max) {
        intervals.push({
            startAt: 0,
            endAt: endAtSeconds - max,
        })

        intervals.push({
            startAt: startAtSeconds,
            endAt: max,
        })
    } else {
        intervals.push({
            startAt: startAtSeconds,
            endAt: endAtSeconds,
        })
    }

    return intervals;
}

const secondsPerDay = 60 * 60 * 24;
const weekdays: DayOfWeek[] = EnumHelper.getValues(DayOfWeek);

const schema = yup.object<AvailabilityFormState>({
    name: yup.string().required(),
});

export interface AvailabilityFormState {
    readonly name: string;
    readonly autoAddNewMenuItems: boolean;
    readonly autoAddNewChannelProfiles: boolean;
    readonly weeklyAvailabilities: WeeklyAvailability[];
}
const getState = (model: Availability | undefined) => {
    return {
        name: model?.name ?? "",
        autoAddNewMenuItems: model?.autoAddNewMenuItems ?? false,
        autoAddNewChannelProfiles: model?.autoAddNewChannelProfiles ?? false,
        weeklyAvailabilities: model?.weeklyAvailabilities.map(a => ({
            startAt: getSeconds(a.startAt),
            endAt: getSeconds(a.endAt),
        })) ?? [],
    }
}

interface Props {
    readonly model?: Availability;
    readonly onSubmit: (state: AvailabilityFormState) => Promise<any>;
    readonly submitText: string;
}
export const AvailabilityForm = (props: Props) => {
    const { t } = useTranslation();
    const dateHelper = useDateHelper();

    const toast = useToast();
    const [state, setState] = useState(() => ({
        ...getState(props.model),
        apiErrors: [],
    }));

    const [dayOfWeekToEdit, setDayOfWeekToEdit] = useState<WeeklyAvailabilityEditModel>();

    useEffect(() => setState(_ => ({
        ...getState(props.model),

        apiErrors: [],
    })), [props.model]);

    const form = useQuiviForm(state, schema);

    const save = () => form.submit(async () => {
        await props.onSubmit({
            name: state.name,
            autoAddNewChannelProfiles: state.autoAddNewChannelProfiles,
            autoAddNewMenuItems: state.autoAddNewMenuItems,
            weeklyAvailabilities: state.weeklyAvailabilities.map(a => ({
                startAt: TimeSpanHelper.toString(TimeSpanHelper.fromSeconds(a.startAt)),
                endAt: TimeSpanHelper.toString(TimeSpanHelper.fromSeconds(a.endAt)),
            })),
        })
    }, () => toast.error(t("common.operations.failure.generic")))

    const availabilitiesPerWeekDay = useMemo(() => {
        const result = new Map<DayOfWeek, WeeklyAvailability[]>();

        for(const availability of state.weeklyAvailabilities) {
            const startAtTotalSeconds = availability.startAt;
            const endAtTotalSeconds = availability.endAt;

            for (let currentDayStart = Math.trunc(startAtTotalSeconds / secondsPerDay) * secondsPerDay; currentDayStart < endAtTotalSeconds; currentDayStart += secondsPerDay) {
                const currentDayEnd = currentDayStart + secondsPerDay;
                const dayOfWeek = Math.trunc(currentDayStart / secondsPerDay);

                const startTimeInSeconds = Math.max(startAtTotalSeconds, currentDayStart);
                const startTimeSpan = TimeSpanHelper.fromSeconds(startTimeInSeconds);

                const endTimeInSeconds = Math.min(endAtTotalSeconds, currentDayEnd);
                const endTimeSpan = TimeSpanHelper.fromSeconds(endTimeInSeconds);

                const aux = result.get(dayOfWeek) ?? [];
                aux.push({
                    startAt: TimeSpanHelper.toString(startTimeSpan),
                    endAt: TimeSpanHelper.toString(endTimeSpan),
                })
                result.set(dayOfWeek, aux);
            }
        }
        return result;
    }, [state.weeklyAvailabilities]);

    return <>
        <div className="grid grid-cols-1 sm:grid-cols-1 md:grid-cols-2 lg:grid-cols-2 gap-4">
            <div className="col-span-1 lg:col-span-1">
                <div className="grid grid-cols-1 gap-4">
                    <TextField
                        label={t("common.name")}
                        value={state.name}
                        onChange={v => setState(s => ({ ...s, name: v }))}
                        errorMessage={form.touchedErrors.get("name")?.message}
                    />
                    
                    <ToggleSwitch
                        label={t("pages.availabilities.autoAddNewMenuItems")}
                        value={state.autoAddNewMenuItems}
                        onChange={v => setState(s => ({ ...s, autoAddNewMenuItems: v }))}
                        errorMessage={form.touchedErrors.get("autoAddNewMenuItems")?.message}
                    />

                    <ToggleSwitch
                        label={t("pages.availabilities.autoAddNewChannelProfiles")}
                        value={state.autoAddNewChannelProfiles}
                        onChange={v => setState(s => ({ ...s, autoAddNewChannelProfiles: v }))}
                        errorMessage={form.touchedErrors.get("autoAddNewChannelProfiles")?.message}
                    />
                </div>
            </div>

            <div className="p-5 border border-gray-200 rounded-2xl dark:border-gray-800 lg:p-6 col-span-1 flex flex-col">
                <h4 className="text-lg font-semibold text-gray-800 dark:text-white/90 lg:mb-2">
                    {t("pages.availabilities.weeklyAvailabilities")}
                </h4>
                <p className="mt-0.5 text-xs text-gray-600 sm:text-sm dark:text-white/70 mb-6">
                    {t("pages.availabilities.weeklyAvailabilitiesDescription")}
                </p>

                <ResponsiveTable
                    columns={[
                        {
                            key: "name",
                            label: t("pages.availabilities.weekday"),
                            render: day => {
                                return (
                                <div className="flex items-center gap-3">
                                    <div>
                                        <span className="block font-medium text-gray-800 text-theme-sm dark:text-white/90">
                                            {t(`dateHelper.weekdays.${DayOfWeek[day]}`)}
                                        </span>
                                    </div>
                                </div>
                                )
                            }
                        },
                        {
                            key: "schedule",
                            label: t("pages.availabilities.schedules"),
                            render: day => {
                                const weekDayAvailabilities = availabilitiesPerWeekDay.get(day) ?? [];
                                return (
                                    weekDayAvailabilities.length == 0 
                                    ? 
                                    <b>{t("pages.availabilities.noAvailability")}</b> 
                                    :
                                    <div className="flex flex-row space-x-2">
                                    {
                                        weekDayAvailabilities.map(d => {                                        
                                            const startAtSeconds = getSeconds(d.startAt);
                                            const endAtSeconds = getSeconds(d.endAt);

                                            const content = (startAtSeconds % secondsPerDay) == 0 && (endAtSeconds % secondsPerDay) == 0
                                                        ?
                                                        t("dateHelper.allDay")
                                                        :
                                                        <span className='lowercase'>
                                                            {dateHelper.toLocalString(TimeSpanHelper.toDate(TimeSpanHelper.fromString(d.startAt)), "HH:mm")} {t("common.to")} {dateHelper.toLocalString(TimeSpanHelper.toDate(TimeSpanHelper.fromString(d.endAt)), "HH:mm")}
                                                        </span>;

                                            return <div
                                                key={`${d.startAt}-${d.endAt}`}
                                                className='group relative cursor-pointer'
                                                onClick={() => setState(s => ({
                                                    ...s,
                                                    weeklyAvailabilities: removeInterval(s.weeklyAvailabilities, startAtSeconds,endAtSeconds),
                                                }))}
                                            >
                                                {/* Visible when not hovered */}
                                                <div className="block group-hover:hidden">
                                                    <Badge
                                                        size='md'
                                                        endIcon={<TrashBinIcon className='size-4' />}
                                                        variant='light'
                                                        color='primary'
                                                    >
                                                        { content }
                                                    </Badge>
                                                </div>

                                                {/* Visible when hovered */}
                                                <div className="hidden group-hover:block">
                                                    <Badge
                                                        size='md'
                                                        endIcon={<TrashBinIcon className='size-4' />}
                                                        variant='light'
                                                        color='error'
                                                    >
                                                        { content }
                                                    </Badge>
                                                </div>
                                            </div>
                                        })
                                    }
                                    </div>
                                );
                            }
                        }
                    ]}
                    actions={[
                        {
                            render: () => <PlusIcon className="size-5" />,
                            key: "add",
                            label: t("common.add"),
                            onClick: d => setDayOfWeekToEdit({
                                dayOfWeek: d,
                                weeklyAvailabilities: state.weeklyAvailabilities,
                                targetDuration: undefined,
                            }),
                        },
                    ]}
                    data={weekdays} 
                    getKey={id => id}
                />
            </div>
        </div>

        <WeeklyAvailabilityModal
            model={dayOfWeekToEdit}
            onSave={weeklyAvailabilities => setState(s => ({
                ...s,
                weeklyAvailabilities: weeklyAvailabilities,
            }))}
            onClose={() => setDayOfWeekToEdit(undefined)}
        />
        
        <Button
            size="md"
            onClick={save}
            disabled={form.isValid == false}
            variant="primary"
        >
            {
                form.isSubmitting
                ?
                <Spinner />
                :
                props.submitText
            }
        </Button>
    </>
}

interface WeeklyAvailabilityEditModel {
    readonly dayOfWeek: DayOfWeek;
    readonly weeklyAvailabilities: WeeklyAvailabilitySeconds[];
    readonly targetDuration?: WeeklyAvailabilitySeconds;
}

interface WeeklyAvailabilityModalProps {
    readonly model?: WeeklyAvailabilityEditModel;
    readonly onSave: (durations: WeeklyAvailabilitySeconds[]) => any;
    readonly onClose: () => any;
}
const WeeklyAvailabilityModal = (props: WeeklyAvailabilityModalProps) => {
    const { t } = useTranslation();

    const [state, setState] = useState(() => ({
        startAt: TimeSpanHelper.toDate(TimeSpanHelper.fromSeconds(props.model?.targetDuration?.startAt ?? 0)),
        endAt: TimeSpanHelper.toDate(TimeSpanHelper.fromSeconds(props.model?.targetDuration?.endAt ?? 0)),
    }))

    useEffect(() => setState(() => ({
        startAt: TimeSpanHelper.toDate(TimeSpanHelper.fromSeconds(props.model?.targetDuration?.startAt ?? 0)),
        endAt: TimeSpanHelper.toDate(TimeSpanHelper.fromSeconds(props.model?.targetDuration?.endAt ?? 0)),
    })), [props.model]);

    const getTitle = () => {
        if(props.model == undefined) {
            return t("pages.availabilities.weeklyAvailabilities");
        }
        return <>{t("pages.availabilities.weeklyAvailabilities")} - {t(`dateHelper.weekdays.${DayOfWeek[props.model.dayOfWeek]}`)}</>
    }

    const getNextDay = () => {
        if(props.model == undefined) {
            return undefined;
        }

        switch(props.model.dayOfWeek) {
            case DayOfWeek.Sunday: return DayOfWeek.Monday;
            case DayOfWeek.Monday: return DayOfWeek.Tuesday;
            case DayOfWeek.Tuesday: return DayOfWeek.Wednesday;
            case DayOfWeek.Wednesday: return DayOfWeek.Thursday;
            case DayOfWeek.Thursday: return DayOfWeek.Friday;
            case DayOfWeek.Friday: return DayOfWeek.Sunday;
            case DayOfWeek.Sunday: return DayOfWeek.Sunday;
        }
    }
    const getExtensionToNextDayMessage = () => {
        const nextDay = getNextDay();
        if(nextDay == undefined) {
            return "";
        }

        return t("pages.availabilities.extendingToNextDayWarning", {
            nextDay: t(`dateHelper.weekdays.${DayOfWeek[nextDay]}`),
        });
    }

    return <Modal
        isOpen={props.model != undefined}
        onClose={props.onClose}
        size={ModalSize.Small}
        title={getTitle()}
        footer={(
            <ModalButtonsFooter 
                primaryButton={{
                    content: t("common.add"),
                    onClick: () => {
                        if(props.model == undefined) {
                            return;
                        }
                        
                        const intervals = getIntervals(props.model.dayOfWeek, state.startAt, state.endAt);
                        let result = props.model.weeklyAvailabilities;
                        for(const interval of intervals) {
                            result = addInterval(result, interval.startAt, interval.endAt);
                        }

                        const sortedResult = result.sort((a, b) => a.startAt - b.startAt);
                        props.onSave(sortedResult);
                        props.onClose();
                    },
                }}
                secondaryButton={{
                    content: t("common.cancel"),
                    onClick: props.onClose,
                }}
            />
        )}
    >
        <div className="grid grid-cols-2 sm:grid-cols-2 md:grid-cols-2 lg:grid-cols-2 gap-4">
            <TimeField
                label={t("pages.availabilities.startAt")}
                format='HH:mm'
                value={state.startAt}
                onChange={v => setState(s => ({ ...s, startAt: v}))}
            />
            <TimeField
                label={t("pages.availabilities.endAt")}
                format='HH:mm'
                value={state.endAt}
                onChange={v => setState(s => ({ ...s, endAt: v}))}
            />
        </div>
        {
            state.endAt <= state.startAt &&
            getDaySeconds(state.endAt) != 0 &&
            <Alert variant="info" message={getExtensionToNextDayMessage()} />
        }
    </Modal>
}

const addInterval = (currentIntervals: WeeklyAvailabilitySeconds[], from: number, to: number): WeeklyAvailabilitySeconds[]  => {
    const sortedAvailabilities = currentIntervals.sort((a, b) => a.startAt - b.startAt);

    const result: WeeklyAvailabilitySeconds[] = [];
    let mergingAvailability: WeeklyAvailabilitySeconds | undefined;

    for (const availability of sortedAvailabilities) {
        const availStart = availability.startAt;
        const availEnd = availability.endAt;

        if (!mergingAvailability) {
            // No overlap or adjacency → keep it
            if (availEnd < from || availStart > to) {
                result.push(availability);
                continue;
            }

            // Exact same interval → nothing to add
            if (availStart === from && availEnd === to)
                return currentIntervals;

            // Fully contained → nothing to add
            if (availStart <= from && to <= availEnd)
                return currentIntervals;

            // Start merging
            mergingAvailability = {
                startAt: Math.min(availStart, from),
                endAt: Math.max(availEnd, to),
            };
            continue;
        }

        // Already merging → extend if overlapping
        if (availability.startAt <= mergingAvailability.endAt) {
            mergingAvailability = {
                startAt: mergingAvailability.startAt,
                endAt: Math.max(mergingAvailability.endAt, availability.endAt),
            }
            continue;
        }

        // If no overlap, finalize current merge and keep going
        result.push(mergingAvailability);
        mergingAvailability = undefined;
        result.push(availability);
    }

    // After the loop: finalize merge or add new interval
    if (mergingAvailability) {
        result.push(mergingAvailability);
    } else if (!sortedAvailabilities.some(a => a.startAt <= from && a.endAt >= to)) {
        result.push({ startAt: from, endAt: to });
    }

    // Resort the result to keep consistency
    return result.sort((a, b) => a.startAt - b.startAt);
}

const removeInterval = (currentIntervals: WeeklyAvailabilitySeconds[], from: number, to: number): WeeklyAvailabilitySeconds[]  => {
    const sortedAvailabilities = currentIntervals.sort((a, b) => a.startAt - b.startAt);
    
    let goingThroughRemoval = false;
    let removalDone = false;
    const result: WeeklyAvailabilitySeconds[] = [];

    for (let i = 0; i < sortedAvailabilities.length; i++) {
        const availability = sortedAvailabilities[i];
        const availStart = availability.startAt;
        const availEnd = availability.endAt;

        if (removalDone) {
            result.push(availability);
            continue;
        }

        if (!goingThroughRemoval) {
            // No overlap with the removal start → keep it
            if (!(availStart <= from && from <= availEnd)) {
                result.push(availability);
                continue;
            }

            // Exact match → delete it
            if (availStart === from && availEnd === to) {
                removalDone = true;
                continue;
            }

            // Fully contained removal → trim or split
            if (availStart <= from && to <= availEnd) {
                if (availStart === from) {
                    // Trim from the start
                    result.push({ startAt: to, endAt: availEnd });
                } else if (availEnd === to) {
                    // Trim from the end
                    result.push({ startAt: availStart, endAt: from });
                } else {
                    // Split into two
                    result.push({ startAt: availStart, endAt: from });
                    result.push({ startAt: to, endAt: availEnd });
                }
                removalDone = true;
                continue;
            }

            // Partial overlap → shrink from start
            result.push({ startAt: availStart, endAt: from });
            goingThroughRemoval = true;
            continue;
        }

        // While removing through overlapping intervals
        if (availEnd < to) {
            // Fully covered by removal
            continue;
        }

        // Ends the removal span
        result.push({ startAt: to, endAt: availEnd });
        goingThroughRemoval = false;
        removalDone = true;
    }

    return result;
}