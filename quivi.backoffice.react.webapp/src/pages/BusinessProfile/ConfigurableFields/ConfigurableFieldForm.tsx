import { useTranslation } from "react-i18next";
import { useEffect, useState } from "react";
import { useQuiviForm } from "../../../hooks/api/exceptions/useQuiviForm";
import * as yup from 'yup';
import Button from "../../../components/ui/button/Button";
import { useToast } from "../../../layout/ToastProvider";
import { TextField } from "../../../components/inputs/TextField";
import { ConfigurableFieldType } from "../../../hooks/api/Dtos/configurableFields/ConfigurableFieldType";
import { PrintedOn } from "../../../hooks/api/Dtos/configurableFields/PrintedOn";
import { ConfigurableField } from "../../../hooks/api/Dtos/configurableFields/ConfigurableField";
import { AssignedOn } from "../../../hooks/api/Dtos/configurableFields/AssignedOn";
import { Language } from "../../../hooks/api/Dtos/Language";
import { LanguageSelector } from "../../../components/ui/language/LanguageSelector";
import { SingleSelect } from "../../../components/inputs/SingleSelect";
import { NumberField } from "../../../components/inputs/NumberField";
import { MultiSelectionZone } from "../../../components/inputs/MultiSelectionZone";
import Checkbox from "../../../components/form/input/Checkbox";

const configurableTypes = Object.values(ConfigurableFieldType).filter(value => typeof value === 'number') as ConfigurableFieldType[];
const printedOnTypes = Object.values(PrintedOn).filter(value => typeof value === 'number') as PrintedOn[];

enum FieldFeature {
    IsRequired,
    IsAutoFill,
    ForPoSSessions,
    ForOrdering,
}
const getFieldFeatures = () => Object.values(FieldFeature).filter(value => typeof value === 'number') as FieldFeature[];

const schema = yup.object<ConfigurableFieldFormState>({
    name: yup.string().required(),
    defaultValue: yup.string().optional(),
});

export interface ConfigurableFieldFormState {
    readonly name: string;
    readonly isRequired: boolean;
    readonly isAutoFill: boolean;
    readonly defaultValue?: string;
    readonly printedOn: PrintedOn;
    readonly assignedOn: AssignedOn;
    readonly type: ConfigurableFieldType;
    readonly translations?: Record<Language, Translation>;
}

interface Translation {
    readonly name: string;
}

const getFeatures = (model?: ConfigurableField | undefined) => {
    const result = new Set<FieldFeature>();

    if(model == undefined) {
        return result;
    }

    if(model.isRequired) {
        result.add(FieldFeature.IsRequired);
    }

    if(model.isAutoFill) {
        result.add(FieldFeature.IsAutoFill);
    }

    if ((model.assignedOn & AssignedOn.PoSSessions) == AssignedOn.PoSSessions) {
        result.add(FieldFeature.ForPoSSessions);
    }

    if ((model.assignedOn & AssignedOn.Ordering) == AssignedOn.Ordering) {
        result.add(FieldFeature.ForOrdering);
    }

    return result;
}

const getState = (model: ConfigurableField | undefined) => {
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
        defaultValue: model?.defaultValue,
        printedOn: model?.printedOn ?? PrintedOn.None,
        type: model?.type ?? ConfigurableFieldType.Text,
        translations: translations,
        features: getFeatures(model),
    }
}

interface Props {
    readonly model?: ConfigurableField;
    readonly onSubmit: (state: ConfigurableFieldFormState) => Promise<any>;
    readonly submitText: string;
    readonly isLoading: boolean;
}
export const ConfigurableFieldForm = (props: Props) => {
    const { t } = useTranslation();
    const toast = useToast();
    
    const [currentLanguage, setCurrentLanguage] = useState(Language.Portuguese);
    const [state, setState] = useState(() => ({
        ...getState(props.model),
        apiErrors: [],
    }));
    const form = useQuiviForm(state, schema);

    useEffect(() => setState(_ => ({
        ...getState(props.model),

        apiErrors: [],
    })), [props.model]);
    
    const getTypeName = (type: ConfigurableFieldType): string => {
        switch(type)
        {
            case ConfigurableFieldType.Text: return t("pages.configurableFields.configurableFieldType.text");
            case ConfigurableFieldType.LongText: return t("pages.configurableFields.configurableFieldType.longText");
            case ConfigurableFieldType.Check: return t("pages.configurableFields.configurableFieldType.check");
            case ConfigurableFieldType.Number: return t("pages.configurableFields.configurableFieldType.number");
        }
    }

    const getPrintedOnName = (type: PrintedOn): string => {
        switch(type)
        {
            case PrintedOn.None: return t("pages.configurableFields.printedOn.none");
            case PrintedOn.PreparationRequest: return t("pages.configurableFields.printedOn.preparationRequest");
            case PrintedOn.SessionBill: return t("pages.configurableFields.printedOn.sessionBill");
        }
    }

    const save = () => form.submit(async () => {
        const assignedOn = AssignedOn.None | 
                            (state.features.has(FieldFeature.ForPoSSessions) ? AssignedOn.PoSSessions : AssignedOn.None) |
                            (state.features.has(FieldFeature.ForOrdering) ? AssignedOn.Ordering : AssignedOn.None);
        const defaultValue = state.type == ConfigurableFieldType.Check ? (["1", "0"].includes(state.defaultValue ?? "") ? state.defaultValue : "0") : state.defaultValue;
        return await props.onSubmit({
            name: state.name,
            type: state.type,
            isRequired: state.features.has(FieldFeature.IsRequired),
            isAutoFill: state.features.has(FieldFeature.IsAutoFill),
            printedOn: state.printedOn,
            defaultValue: defaultValue,
            assignedOn: assignedOn,
            translations: state.translations,
        });
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
                <SingleSelect 
                    value={state.printedOn} 
                    options={printedOnTypes} 
                    getId={t => t.toString()} 
                    render={getPrintedOnName} 
                    onChange={t => setState(s => ({...s, printedOn: t}))}
                    label={t("pages.configurableFields.printable")}
                    isLoading={props.isLoading}
                />
                <SingleSelect 
                    value={state.type} 
                    options={configurableTypes} 
                    getId={t => t.toString()} 
                    render={getTypeName} 
                    onChange={t => setState(s => ({...s, type: t}))}
                    label={t("common.type")}
                    isLoading={props.isLoading}
                />
                {
                    state.type == ConfigurableFieldType.Check 
                    ?
                    <Checkbox
                        label={t("common.defaultValue")}
                        checked={state.defaultValue == "1"}
                        onChange={(e) => setState(s => ({ ...s, defaultValue: e ? "1" : "0" }))}
                        isLoading={props.isLoading}
                    />
                    :
                    (
                        state.type == ConfigurableFieldType.Number
                        ?
                        <NumberField
                            label={t("common.defaultValue")}
                            onChange={(e) => setState(s => ({ ...s, defaultValue: e.toString() }))}
                            value={state.defaultValue == undefined ? 0 : +state.defaultValue}
                            decimalPlaces={0}
                            errorMessage={form.touchedErrors.get("defaultValue")?.message}
                            isLoading={props.isLoading}
                        />
                        :
                        <TextField
                            label={t("common.defaultValue")}
                            onChange={(e) => setState(s => ({ ...s, defaultValue: e }))}
                            value={state.defaultValue ?? ""}
                            errorMessage={form.touchedErrors.get("defaultValue")?.message}
                            isLoading={props.isLoading}
                        />
                    )
                }
            </div>

            <div className="p-5 border border-gray-200 rounded-2xl dark:border-gray-800 lg:p-6 col-span-1 gap-4">
                <h4 className="text-lg font-semibold text-gray-800 dark:text-white/90 lg:mb-2">
                    {t("pages.configurableFields.featuresTitle")}
                </h4>
                <p className="mt-0.5 text-xs text-gray-600 sm:text-sm dark:text-white/70 mb-6">
                    {t("pages.configurableFields.featuresDescription")}
                </p>
                <MultiSelectionZone
                    options={getFieldFeatures()}
                    selected={Array.from(state.features)}
                    getId={s => s.toString()}
                    render={s => (
                        <div>
                            <h5 className="text-sm font-medium text-gray-800 dark:text-white/90">
                                {t(`pages.configurableFields.features.${FieldFeature[s].toString()}`)}
                            </h5>
                            <p className="mt-0.5 text-theme-xs text-gray-500 dark:text-gray-400">
                                {t(`pages.configurableFields.featuresDescriptions.${FieldFeature[s].toString()}`)}
                            </p>
                        </div>
                    )}
                    onChange={r => setState(s => ({ ...s, features: new Set(r)}))}
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