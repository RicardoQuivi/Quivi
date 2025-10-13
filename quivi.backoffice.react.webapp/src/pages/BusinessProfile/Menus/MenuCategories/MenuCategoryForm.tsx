import { useTranslation } from "react-i18next";
import { MenuCategory } from "../../../../hooks/api/Dtos/menuCategories/MenuCategory";
import * as yup from 'yup';
import { useEffect, useState } from "react";
import { ImageInput } from "../../../../components/upload/ImageInput";
import { UploadHandler } from "../../../../components/upload/UploadHandler";
import { useQuiviForm } from "../../../../hooks/api/exceptions/useQuiviForm";
import Button from "../../../../components/ui/button/Button";
import { Language } from "../../../../hooks/api/Dtos/Language";
import { LanguageSelector } from "../../../../components/ui/language/LanguageSelector";
import { useToast } from "../../../../layout/ToastProvider";
import { TextField } from "../../../../components/inputs/TextField";

const schema = yup.object<MenuCategoryFormState>({
    name: yup.string().required(),
    imageUrl: yup.string().optional(),
});

export interface MenuCategoryFormState {
    readonly name: string;
    readonly imageUrl?: string;
    readonly translations?: Record<Language, Translation>;
}

interface Translation {
    readonly name: string;
}

const getState = (model: MenuCategory | undefined) => {
    const translations = Object.values(Language)
                                .filter((v): v is Language => typeof v === "string")
                                .reduce((acc, lang) => {
                                    const translation = model?.translations?.[lang];

                                    acc[lang] = {
                                        name: translation?.name ?? "",
                                    };

                                    return acc;
                                }, {} as Record<Language, Translation>);

    return {
        name: model?.name ?? "",
        imageUrl: model?.imageUrl ?? "",
        translations: translations,
    }
}
interface Props {
    readonly model?: MenuCategory;
    readonly onSubmit: (state: MenuCategoryFormState) => Promise<any>;
    readonly submitText: string;
    readonly isLoading: boolean;
}
export const MenuCategoryForm = (props: Props) => {
    const { t } = useTranslation();
    const toast = useToast();
    
    const [logoUploadHandler, setLogoUploadHandler] = useState<UploadHandler<string>>();
    const [currentLanguage, setCurrentLanguage] = useState(Language.Portuguese);
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
        let image = undefined as string | undefined;
        if(logoUploadHandler != undefined) {
            image = await logoUploadHandler.getUrl();
        }

        await props.onSubmit({
            name: state.name,
            imageUrl: image,
            translations: state.translations,
        })
    }, () => toast.error(t("common.operations.failure.generic")))

    return <>
        <div className="grid grid-cols-1 sm:grid-cols-1 md:grid-cols-2 lg:grid-cols-2 gap-4">
            <div className="grid col-span-1 gap-4">
                <TextField
                    label={t("common.name")}
                    type="text"
                    value={state.name}
                    onChange={(e) => setState(s => ({ ...s, name: e }))}
                    errorMessage={form.touchedErrors.get("name")?.message}
                    isLoading={props.isLoading}
                />
                <ImageInput
                    label={t("common.image")}
                    aspectRatio={1}
                    value={state.imageUrl}
                    inlineEditor
                    onUploadHandlerChanged={setLogoUploadHandler}
                    isLoading={props.isLoading}
                />
            </div>

            <div className="p-5 border border-gray-200 rounded-2xl dark:border-gray-800 lg:p-6 col-span-1 lg:col-span-1 gap-4">
                <h4 className="text-lg font-semibold text-gray-800 dark:text-white/90 lg:mb-2">
                    {t("common.guestsApp.settings")}
                </h4>
                <p className="mt-0.5 text-xs text-gray-600 sm:text-sm dark:text-white/70 mb-6">
                    {t("common.guestsApp.settingsDescription")}
                </p>
                <div className="w-full mb-6">
                    <LanguageSelector
                        language={currentLanguage}
                        onChange={setCurrentLanguage}
                    />
                </div>
                <TextField
                    type="text"
                    label={t("common.name")}
                    value={state.translations[currentLanguage]?.name ?? ""}
                    onChange={(e) => setState(s => {
                        const aux = { ...s.translations };
                        aux[currentLanguage] = {
                            name: e,
                        }
                        return {
                            ...s, 
                            translations: aux,
                        };
                    })}
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