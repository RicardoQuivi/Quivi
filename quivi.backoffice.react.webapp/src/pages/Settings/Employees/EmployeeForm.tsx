import * as yup from 'yup';
import { useTranslation } from 'react-i18next';
import { useEffect, useState } from 'react';
import { useQuiviForm } from '../../../hooks/api/exceptions/useQuiviForm';
import Button from '../../../components/ui/button/Button';
import { useToast } from '../../../layout/ToastProvider';
import { TextField } from '../../../components/inputs/TextField';
import { Employee, EmployeeRestriction } from '../../../hooks/api/Dtos/employees/Employee';
import { TimeField } from '../../../components/inputs/TimeField';
import { MultiSelectionZone } from '../../../components/inputs/MultiSelectionZone';
import { CloseLineIcon } from '../../../icons';
import { Spinner } from '../../../components/spinners/Spinner';

const schema = yup.object<EmployeeFormState>({
    name: yup.string().required(),
});

export interface EmployeeFormState {
    readonly name: string;
    readonly inactivityPeriod?: string | undefined,
    readonly restrictions: EmployeeRestriction[];
}

const getTimeInSeconds = (time: string): number => {
    const aux = time.split(":");
    if(aux.length == 0) {
        return 0;
    }
    if(aux.length == 1) {
        return +aux[0];
    }
    if(aux.length == 2) {
        return (+aux[0])*60+(+aux[1]);
    }
    if(aux.length == 3) {
        return (+aux[0])*60*60+(+aux[1])*60+(+aux[2]);
    }
    return (+aux[0])*60*60*24+(+aux[1])*60*60+(+aux[2])*60+(+aux[3]);
}

const toDate = (time: string | undefined): Date| undefined => {
    if(time == undefined) {
        return undefined;
    }
    const totalSeconds = getTimeInSeconds(time);

    const t = new Date(1970, 0, 1); // Epoch
    t.setSeconds(totalSeconds);
    return t;
}

const getState = (model: Employee | undefined) => {
    const restrictions = new Set<EmployeeRestriction>();
    for(const r of model?.restrictions ?? []) {
        restrictions.add(r);
    }

    return {
        name: model?.name ?? "",
        inactivityPeriod: toDate(model?.inactivityLogoutTimeout),
        restrictions: restrictions,
    }
}

const getInactivityTime = (inactivityPeriod: Date | undefined) => {
    if(inactivityPeriod == undefined) {
        return undefined;
    }
    
    const date = inactivityPeriod;
    const values = [
        date.getHours().toString().padStart(2, '0'),
        date.getMinutes().toString().padStart(2, '0'),
        date.getSeconds().toString().padStart(2, '0'),
    ]

    return values.join(":")
}

const getRestrictions = () => Object.values(EmployeeRestriction).filter(value => typeof value === 'number') as EmployeeRestriction[];

interface Props {
    readonly model?: Employee;
    readonly onSubmit: (state: EmployeeFormState) => Promise<any>;
    readonly submitText: string;
}
export const EmployeeForm = (props: Props) => {
    const { t } = useTranslation();
    const toast = useToast();
    const [state, setState] = useState(() => ({
        ...getState(props.model),
        apiErrors: [],
    }));

    useEffect(() => setState(_ => ({
        ...getState(props.model),

        apiErrors: [],
    })), [props.model]);

    const form = useQuiviForm(state, schema);

    const save = () => form.submit(async () => {
        await props.onSubmit({
            name: state.name,
            inactivityPeriod: getInactivityTime(state.inactivityPeriod),
            restrictions: Array.from(state.restrictions),
        })
    }, () => toast.error(t("common.operations.failure.generic")))

    return <>
        <div className="grid grid-cols-1 sm:grid-cols-1 md:grid-cols-2 lg:grid-cols-2 gap-4">
            <div className="col-span-1 lg:col-span-1">
                <TextField
                    label={t("common.name")}
                    value={state.name}
                    onChange={v => setState(s => ({ ...s, name: v }))}
                    errorMessage={form.touchedErrors.get("name")?.message}
                />
                <TimeField
                    label={t("common.inactivityPeriod")}
                    value={state.inactivityPeriod}
                    onChange={v => setState(s => ({ ...s, inactivityPeriod: v }))}
                    errorMessage={form.touchedErrors.get("inactivityPeriod")?.message}
                    format={'HH:mm'}
                />
            </div>
            <div className="p-5 border border-gray-200 rounded-2xl dark:border-gray-800 lg:p-6 col-span-1 gap-4">
                <h4 className="text-lg font-semibold text-gray-800 dark:text-white/90 lg:mb-2">
                    {t("pages.employees.restrictions")}
                </h4>
                <p className="mt-0.5 text-xs text-gray-600 sm:text-sm dark:text-white/70 mb-6">
                    {t("pages.employees.restrictionsDescription")}
                </p>
                <MultiSelectionZone
                    options={getRestrictions()}
                    selected={Array.from(state.restrictions)}
                    getId={s => s.toString()}
                    checkIcon={p => <CloseLineIcon className={`${p.className ?? ""} text-error-500`} />}
                    render={s => (
                        <div>
                            <h5 className="text-sm font-medium text-gray-800 dark:text-white/90">
                                {t(`common.employeeRestrictions.${EmployeeRestriction[s].toString()}`)}
                            </h5>
                            <p className="mt-0.5 text-theme-xs text-gray-500 dark:text-gray-400">
                                {t(`common.employeeRestrictionsDescriptions.${EmployeeRestriction[s].toString()}`)}
                            </p>
                        </div>
                    )}
                    onChange={r => setState(s => ({ ...s, restrictions: new Set(r)}))}
                />
            </div>
        </div>

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