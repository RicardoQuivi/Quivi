import * as yup from 'yup';
import { useTranslation } from 'react-i18next';
import { useEffect, useState } from 'react';
import { useQuiviForm } from '../../../hooks/api/exceptions/useQuiviForm';
import Button from '../../../components/ui/button/Button';
import { ClipLoader } from 'react-spinners';
import { useToast } from '../../../layout/ToastProvider';
import { TextField } from '../../../components/inputs/TextField';
import { CustomChargeMethod } from '../../../hooks/api/Dtos/customchargemethods/CustomChargeMethod';
import { ImageInput } from '../../../components/upload/ImageInput';
import { UploadHandler } from '../../../components/upload/UploadHandler';

const schema = yup.object<CustomChargeMethodFormState>({
    name: yup.string().required(),
    logoUrl: yup.string().optional(),
});

export interface CustomChargeMethodFormState {
    readonly name: string;
    readonly logoUrl?: string;
}
const getState = (model: CustomChargeMethod | undefined) => {
    return {
        name: model?.name ?? "",
        logoUrl: model?.logoUrl ?? "",
    }
}
interface Props {
    readonly model?: CustomChargeMethod;
    readonly onSubmit: (state: CustomChargeMethodFormState) => Promise<any>;
    readonly submitText: string;
}
export const CustomChargeMethodForm = (props: Props) => {
    const { t } = useTranslation();
    const toast = useToast();

    const [logoUploadHandler, setLogoUploadHandler] = useState<UploadHandler<string>>();
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
        let logo = undefined as string | undefined;
        if(logoUploadHandler != undefined) {
            logo = await logoUploadHandler.getUrl();
        }

        await props.onSubmit({
            name: state.name,
            logoUrl: logo,
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
                <ImageInput
                    label={t("common.logo")}
                    aspectRatio={1}
                    value={state.logoUrl}
                    inlineEditor
                    onUploadHandlerChanged={setLogoUploadHandler}
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