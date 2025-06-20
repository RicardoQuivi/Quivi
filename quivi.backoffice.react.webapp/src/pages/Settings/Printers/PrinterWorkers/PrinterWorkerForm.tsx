import { useTranslation } from "react-i18next";
import { PrinterWorker } from "../../../../hooks/api/Dtos/printerWorkers/PrinterWorker";
import * as yup from 'yup';
import { useEffect, useState } from "react";
import { useQuiviForm } from "../../../../hooks/api/exceptions/useQuiviForm";
import Button from "../../../../components/ui/button/Button";
import { ClipLoader } from "react-spinners";
import { useToast } from "../../../../layout/ToastProvider";
import { TextField } from "../../../../components/inputs/TextField";

const schema = yup.object<PrinterWorkerFormState>({
    identifier: yup.string().required(),
    name: yup.string().optional(),
});

export interface PrinterWorkerFormState {
    readonly identifier: string;
    readonly name: string;
}

const getState = (model: PrinterWorker | undefined) => {
    return {
        identifier: model?.identifier ?? "",
        name: model?.name ?? "",
    }
}
interface Props {
    readonly model?: PrinterWorker;
    readonly onSubmit: (state: PrinterWorkerFormState) => Promise<any>;
    readonly submitText: string;
}
export const PrinterWorkerForm = (props: Props) => {
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
            identifier: state.identifier,
            name: state.name,
        })
    }, () => toast.error(t("common.operations.failure.generic")))

    return <>
        <div className="grid grid-cols-1 sm:grid-cols-1 md:grid-cols-2 lg:grid-cols-2 gap-4">
            <div className="grid col-span-1 gap-4">
                <TextField
                    label={t("common.identifier")}
                    type="text"
                    value={state.name}
                    onChange={(e) => setState(s => ({ ...s, name: e }))}
                    errorMessage={form.touchedErrors.get("identifier")?.message}
                />
                <TextField
                    label={t("common.name")}
                    type="text"
                    value={state.name}
                    onChange={(e) => setState(s => ({ ...s, name: e }))}
                    errorMessage={form.touchedErrors.get("name")?.message}
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
                <ClipLoader
                    size={20}
                    cssOverride={{
                        borderColor: "white"
                    }}
                />
                :
                props.submitText
            }
        </Button>
    </>
}