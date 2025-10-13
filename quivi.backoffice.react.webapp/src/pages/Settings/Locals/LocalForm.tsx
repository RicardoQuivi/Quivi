import * as yup from 'yup';
import { Local } from '../../../hooks/api/Dtos/locals/Local';
import { useTranslation } from 'react-i18next';
import { useEffect, useState } from 'react';
import { useQuiviForm } from '../../../hooks/api/exceptions/useQuiviForm';
import Button from '../../../components/ui/button/Button';
import { useToast } from '../../../layout/ToastProvider';
import { TextField } from '../../../components/inputs/TextField';

const schema = yup.object<LocalFormState>({
    name: yup.string().required(),
});

export interface LocalFormState {
    readonly name: string;
}
const getState = (model: Local | undefined) => {
    return {
        name: model?.name ?? "",
    }
}
interface Props {
    readonly model?: Local;
    readonly onSubmit: (state: LocalFormState) => Promise<any>;
    readonly submitText: string;
    readonly isLoading: boolean;
}
export const LocalForm = (props: Props) => {
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