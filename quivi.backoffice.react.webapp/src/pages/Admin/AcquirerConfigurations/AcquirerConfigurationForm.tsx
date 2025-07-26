import * as yup from 'yup';
import { useTranslation } from 'react-i18next';
import { useEffect, useState } from 'react';
import { useQuiviForm } from '../../../hooks/api/exceptions/useQuiviForm';
import Button from '../../../components/ui/button/Button';
import { useToast } from '../../../layout/ToastProvider';
import { AcquirerConfiguration } from '../../../hooks/api/Dtos/acquirerconfigurations/AcquirerConfiguration';
import { ToggleSwitch } from '../../../components/inputs/ToggleSwitch';
import { Spinner } from '../../../components/spinners/Spinner';

const schema = yup.object<AcquirerConfigurationFormState>({
});

export interface AcquirerConfigurationFormState {
    readonly isActive: boolean;
}
const getState = (model: AcquirerConfiguration | undefined) => {
    return {
        isActive: model?.isActive ?? true,
    }
}
interface Props {
    readonly model?: AcquirerConfiguration;
    readonly onSubmit: (state: AcquirerConfigurationFormState) => Promise<any>;
    readonly submitText: string;
}
export const AcquirerConfigurationForm = (props: Props) => {
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
            isActive: state.isActive,
        })
    }, () => toast.error(t("common.operations.failure.generic")))

    return <>
        <div className="grid grid-cols-1 sm:grid-cols-1 md:grid-cols-2 lg:grid-cols-2 gap-4">
            <div className="col-span-1 lg:col-span-1">
                <ToggleSwitch
                    label={t("common.active")}
                    value={state.isActive}
                    onChange={v => setState(s => ({ ...s, isActive: v }))}
                    errorMessage={form.touchedErrors.get("isActive")?.message}
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