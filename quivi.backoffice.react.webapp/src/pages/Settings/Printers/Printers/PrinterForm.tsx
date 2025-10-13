import { useEffect, useMemo, useState } from "react";
import Button from "../../../../components/ui/button/Button";
import { useQuiviForm } from "../../../../hooks/api/exceptions/useQuiviForm";
import * as yup from 'yup';
import { useTranslation } from "react-i18next";
import { useToast } from "../../../../layout/ToastProvider";
import { TextField } from "../../../../components/inputs/TextField";
import { SingleSelect } from "../../../../components/inputs/SingleSelect";
import { useLocalsQuery } from "../../../../hooks/queries/implementations/useLocalsQuery";
import { Skeleton } from "../../../../components/ui/skeleton/Skeleton";
import { Local } from "../../../../hooks/api/Dtos/locals/Local";
import { Printer } from "../../../../hooks/api/Dtos/printers/Printer";
import { PrinterWorker } from "../../../../hooks/api/Dtos/printerWorkers/PrinterWorker";
import { usePrinterWorkersQuery } from "../../../../hooks/queries/implementations/usePrinterWorkersQuery";
import { MultiSelectionZone } from "../../../../components/inputs/MultiSelectionZone";
import { NotificationType } from "../../../../hooks/api/Dtos/notifications/NotificationType";

const schema = yup.object<PrinterFormState>({
    name: yup.string().required(),
    address: yup.string().required(),
});
export interface PrinterFormState {
    readonly name: string;
    readonly address: string;
    readonly printerWorkerId: string;
    readonly locationId?: string;
    readonly notifications: NotificationType[];
}
const getState = (model: Printer | undefined) => {
    const notifications = new Set<NotificationType>();
    for(const r of model?.notifications ?? getNotifications()) {
        notifications.add(r);
    }

    return {
        name: model?.name ?? "",
        address: model?.address ?? "",
        notifications: notifications,
    }
}

const getWorker = (previousValue: PrinterWorker | undefined, model: Printer | undefined, map: Map<string, PrinterWorker>, defaultPrinter: PrinterWorker | undefined) => {
    if(previousValue != undefined) {
        return previousValue;
    }
    if(model == undefined) {
        return defaultPrinter;
    }
    return map.get(model.printerWorkerId)
}

const getLocal = (previousValue: Local | undefined, model: Printer | undefined, map: Map<string, Local>, defaultLocal: Local | undefined) => {
    if(previousValue != undefined) {
        return previousValue;
    }
    if(model == undefined) {
        return defaultLocal;
    }
    if(model.locationId == undefined) {
        return undefined;
    }
    return map.get(model.locationId)
}

const getNotifications = () => Object.values(NotificationType).filter(value => typeof value === 'number') as NotificationType[];

interface Props {
    readonly model?: Printer;
    readonly onSubmit: (state: PrinterFormState) => Promise<any>;
    readonly submitText: string;
    readonly printerWorkerId?: string;
    readonly isLoading: boolean;
}
export const PrinterForm = (props: Props) => {
    const { t } = useTranslation();
    const toast = useToast();

    const workersQuery = usePrinterWorkersQuery({
        page: 0,
        pageSize: undefined,
    });
    const workersInfo = useMemo(() => {
        const map = new Map<string, PrinterWorker>();
        let defaultWorker = undefined as PrinterWorker | undefined;
        
        if(workersQuery.data.length > 0) {
            for(const l of workersQuery.data) {
                map.set(l.id, l);
            }
        }

        if(props.printerWorkerId != undefined) {
            defaultWorker = map.get(props.printerWorkerId)
        } else {
            defaultWorker = workersQuery.data.length > 0 ? workersQuery.data[0] : undefined;
        }

        return {
            map: map,
            default: defaultWorker,
        };
    }, [workersQuery.data, props.printerWorkerId])

    const localsQuery = useLocalsQuery({
        page: 0,
    })
    const localsInfo = useMemo(() => {
        const map = new Map<string, Local>();
        let defaultLocal = undefined as Local | undefined;
        
        if(localsQuery.data.length > 0) {
            defaultLocal = localsQuery.data[0];
            for(const l of localsQuery.data) {
                map.set(l.id, l);
            }
        }

        return {
            map: map,
            default: defaultLocal,
        };
    }, [localsQuery.data])

    const [local, setLocal] = useState<Local | undefined>(() => getLocal(undefined, props.model, localsInfo.map, localsInfo.default));
    const [worker, setWorker] = useState<PrinterWorker | undefined>(() => getWorker(undefined, props.model, workersInfo.map, workersInfo.default));
    const [state, setState] = useState(() => ({
        ...getState(props.model),
        apiErrors: [],
    }));

    useEffect(() => setState(_ => ({
        ...getState(props.model),

        apiErrors: [],
    })), [props.model]);

    useEffect(() => setLocal(p => getLocal(p, props.model, localsInfo.map, localsInfo.default)), [localsInfo])
    useEffect(() => setWorker(p => getWorker(p, props.model, workersInfo.map, workersInfo.default)), [workersInfo])

    const form = useQuiviForm(state, schema);

    const save = () => form.submit(async () => {
        if(worker == undefined) {
            return;
        }

        await props.onSubmit({
            name: state.name,
            address: state.address,
            printerWorkerId: worker.id,
            locationId: local?.id,
            notifications: Array.from(state.notifications.values()),
        })
    }, () => toast.error(t("common.operations.failure.generic")))

    return <>
        <div className="grid grid-cols-1 sm:grid-cols-1 md:grid-cols-2 lg:grid-cols-2 gap-4">
            <div className="grid col-span-1 gap-4">
                <TextField
                    label={t("common.name")}
                    value={state.name}
                    onChange={v => setState(s => ({ ...s, name: v }))}
                    errorMessage={form.touchedErrors.get("name")?.message}
                    isLoading={props.isLoading}
                />
                <TextField
                    label={t("common.hardwareAddress")}
                    value={state.address}
                    onChange={v => setState(s => ({ ...s, address: v }))}
                    errorMessage={form.touchedErrors.get("address")?.message}
                    isLoading={props.isLoading}
                />

                <div className="p-5 border border-gray-200 rounded-2xl dark:border-gray-800 lg:p-6 col-span-1 gap-4">
                    <h4 className="text-lg font-semibold text-gray-800 dark:text-white/90 lg:mb-2">
                        {t("common.entities.printerWorker")}
                    </h4>
                    <p className="mt-0.5 text-xs text-gray-600 sm:text-sm dark:text-white/70 mb-6">
                        {t("pages.printers.printerWorkerDescription")}
                    </p>

                    <div>
                    {
                        worker == undefined
                        ?
                        <Skeleton className="w-full h-full"/>
                        :
                        <SingleSelect
                            options={workersQuery.data}
                            value={worker}
                            getId={e => e.id}
                            onChange={setWorker}
                            render={e => e.name}
                            label={t(`common.entities.printerWorker`)}
                            isLoading={props.isLoading}
                        />
                    }
                    </div>
                </div>

                <div className="p-5 border border-gray-200 rounded-2xl dark:border-gray-800 lg:p-6 col-span-1 gap-4">
                    <h4 className="text-lg font-semibold text-gray-800 dark:text-white/90 lg:mb-2">
                        {t("common.entities.local")}
                    </h4>
                    <p className="mt-0.5 text-xs text-gray-600 sm:text-sm dark:text-white/70 mb-6">
                        {t("pages.printers.locationDescription")}
                    </p>

                    <div>
                    {
                        localsQuery.isFirstLoading
                        ?
                        <Skeleton className="w-full h-full"/>
                        :
                        <SingleSelect
                            options={[
                                undefined,
                                ...localsQuery.data,
                            ]}
                            value={local}
                            getId={e => e?.id ?? "No-local"}
                            onChange={setLocal}
                            render={e => e?.name ?? t("common.noLocation")}
                            label={t(`common.entities.local`)}
                            isLoading={props.isLoading}
                        />
                    }
                    </div>
                </div>
            </div>

            <div className="p-5 border border-gray-200 rounded-2xl dark:border-gray-800 lg:p-6 col-span-1 gap-4">
                <h4 className="text-lg font-semibold text-gray-800 dark:text-white/90 lg:mb-2">
                    {t("pages.printers.notifications")}
                </h4>
                <p className="mt-0.5 text-xs text-gray-600 sm:text-sm dark:text-white/70 mb-6">
                    {t("pages.printers.notificationsDescription")}
                </p>
                
                <MultiSelectionZone
                    options={getNotifications()}
                    selected={Array.from(state.notifications)}
                    getId={s => s.toString()}
                    render={s => (
                        <div>
                            <h5 className="text-sm font-medium text-gray-800 dark:text-white/90">
                                {t(`common.notificationTypes.${NotificationType[s].toString()}`)}
                            </h5>
                            <p className="mt-0.5 text-theme-xs text-gray-500 dark:text-gray-400">
                                {t(`common.notificationTypeDescriptions.${NotificationType[s].toString()}`)}
                            </p>
                        </div>
                    )}
                    onChange={r => setState(s => ({ ...s, notifications: new Set(r)}))}
                    isLoading={props.isLoading}
                />
            </div>
        </div>

        <Button
            size="md"
            onClick={save}
            disabled={form.isValid == false}
            variant="primary"
            isLoading={form.isSubmitting || props.isLoading}
        >
            {props.submitText}
        </Button>
    </>
}