import { useEffect, useMemo, useState } from "react";
import Button from "../../../../components/ui/button/Button";
import { MenuItem } from "../../../../hooks/api/Dtos/menuItems/MenuItem";
import { useQuiviForm } from "../../../../hooks/api/exceptions/useQuiviForm";
import { UploadHandler } from "../../../../components/upload/UploadHandler";
import { Language } from "../../../../hooks/api/Dtos/Language";
import * as yup from 'yup';
import { useTranslation } from "react-i18next";
import { ImageInput } from "../../../../components/upload/ImageInput";
import { LanguageSelector } from "../../../../components/ui/language/LanguageSelector";
import { PriceType } from "../../../../hooks/api/Dtos/menuItems/PriceType";
import { useToast } from "../../../../layout/ToastProvider";
import { TextField } from "../../../../components/inputs/TextField";
import { SingleSelect } from "../../../../components/inputs/SingleSelect";
import { useLocalsQuery } from "../../../../hooks/queries/implementations/useLocalsQuery";
import { Skeleton } from "../../../../components/ui/skeleton/Skeleton";
import { Local } from "../../../../hooks/api/Dtos/locals/Local";
import { TextAreaField } from "../../../../components/inputs/TextAreaField";
import { MultiSelect } from "../../../../components/inputs/MultiSelect";
import { useMenuCategoriesQuery } from "../../../../hooks/queries/implementations/useMenuCategoriesQuery";
import { MenuCategory } from "../../../../hooks/api/Dtos/menuCategories/MenuCategory";
import { useMenuCategoryMutator } from "../../../../hooks/mutators/useMenuCategoryMutator";
import { Spinner } from "../../../../components/spinners/Spinner";
import { CurrencyField } from "../../../../components/inputs/CurrencyField";

const schema = yup.object<MenuItemFormState>({
    name: yup.string().required(),
    imageUrl: yup.string().optional(),
});
const vatRates = [
    6,
    13,
    23
]
export interface MenuItemFormState {
    readonly name: string;
    readonly description?: string;
    readonly imageUrl?: string;
    readonly price: number;
    readonly priceType: PriceType;
    readonly vatRate: number;
    readonly locationId?: string;
    readonly translations?: Record<Language, Translation>;
    readonly menuCategoryIds?: string[];
}
interface Translation {
    readonly name: string;
    readonly description: string;
}
const getState = (model: MenuItem | undefined) => {
    const translations = Object.values(Language)
                                .filter((v): v is Language => typeof v === "string")
                                .reduce((acc, lang) => {
                                    const translation = model?.translations?.[lang];

                                    acc[lang] = {
                                        name: translation?.name ?? "",
                                        description: translation?.description ?? "",
                                    };

                                    return acc;
                                }, {} as Record<Language, Translation>);

    return {
        name: model?.name ?? "",
        price: model?.price ?? 0,
        priceType: model?.priceType ?? PriceType.Unit,
        vatRate: model?.vatRate ?? 23,
        description: model?.description,
        imageUrl: model?.imageUrl ?? "",
        translations: translations,
    }
}

const getLocal = (previousValue: Local | undefined, model: MenuItem | undefined, map: Map<string, Local>, defaultLocal: Local | undefined) => {
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

const getCategories = (previousValues: MenuCategory[] | undefined, model: MenuItem | undefined, map: Map<string, MenuCategory> | undefined, defaultCategory: MenuCategory | undefined) => {
    if(previousValues != undefined) {
        return previousValues;
    }

    if(map == undefined) {
        return undefined;
    }

    if(model == undefined) {
        return defaultCategory == undefined ? undefined : [defaultCategory];
    }

    const result = [] as MenuCategory[];
    for(const id of model.menuCategoryIds) {
        const cat = map.get(id);
        if(cat == undefined) {
            continue;
        }
        result.push(cat);
    }
    return result;
}

interface Props {
    readonly model?: MenuItem;
    readonly onSubmit: (state: MenuItemFormState) => Promise<any>;
    readonly submitText: string;
    readonly categoryId?: string;
}
export const MenuItemForm = (props: Props) => {
    const { t } = useTranslation();
    const toast = useToast();
    const categoryMutator = useMenuCategoryMutator();

    const categoriesQuery = useMenuCategoriesQuery({
        page: 0,
        pageSize: undefined,
    });
    const categoriesInfo = useMemo(() => {
        if(categoriesQuery.isFirstLoading) {
            return undefined;
        }

        const map = new Map<string, MenuCategory>();
        let defaultCategory = undefined as MenuCategory | undefined;
        
        if(categoriesQuery.data.length > 0) {
            for(const l of categoriesQuery.data) {
                map.set(l.id, l);
            }
        }

        if(props.categoryId != undefined) {
            defaultCategory = map.get(props.categoryId)
        }

        return {
            map: map,
            default: defaultCategory,
        };
    }, [categoriesQuery.data, props.categoryId])

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
    const priceTypesMap = useMemo(() => {
        const priceTypes = Object.keys(PriceType)
                                    .filter(key => isNaN(Number(key)))
                                    .map(key => ({
                                        name: key,
                                        value: PriceType[key as keyof typeof PriceType]
                                    }));

        const map = new Map<PriceType, string>();
        for(const e of priceTypes){
            map.set(e.value, e.name);
        }
        return map;
    }, [t])
    

    const [logoUploadHandler, setLogoUploadHandler] = useState<UploadHandler<string>>();
    const [currentLanguage, setCurrentLanguage] = useState(Language.Portuguese);
    const [local, setLocal] = useState<Local | undefined>(() => getLocal(undefined, props.model, localsInfo.map, localsInfo.default));
    const [categories, setCategories] = useState<MenuCategory[] | undefined>(() => getCategories(undefined, props.model, categoriesInfo?.map, categoriesInfo?.default));
    const [state, setState] = useState(() => ({
        ...getState(props.model),
        apiErrors: [],
    }));

    useEffect(() => setState(_ => ({
        ...getState(props.model),

        apiErrors: [],
    })), [props.model]);
    useEffect(() => setLocal(p => getLocal(p, props.model, localsInfo.map, localsInfo.default)), [props.model, localsInfo])
    useEffect(() => setCategories(p => getCategories(p, props.model, categoriesInfo?.map, categoriesInfo?.default)), [props.model, categoriesInfo])

    const form = useQuiviForm(state, schema);

    const createAndAssignCategory = async (name: string) => {
        const category = await categoryMutator.create({
            name: name,
        });
        toast.success(t("common.operations.success.new"));
        setCategories(c => ([...(c ?? []), category]))
    }

    const save = () => form.submit(async () => {
        let image = undefined as string | undefined;
        if(logoUploadHandler != undefined) {
            image = await logoUploadHandler.getUrl();
        }
        await props.onSubmit({
            name: state.name,
            description: state.description,
            imageUrl: image,
            price: state.price,
            priceType: state.priceType,
            vatRate: state.vatRate,
            locationId: local?.id,
            translations: state.translations,
            menuCategoryIds: categories?.map(s => s.id) ?? [],
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
                />
                <div className="col-span-1 grid grid-cols-7 gap-4">
                    <CurrencyField
                        label={t("common.price")}
                        value={state.price}
                        onChange={(e) => setState(s => ({ ...s, price: e }))}
                        errorMessage={form.touchedErrors.get("price")?.message}
                        endElement={<SingleSelect
                            className="rounded-none border-0 h-full"
                            value={state.priceType}
                            options={Array.from(priceTypesMap.keys())}
                            getId={e => priceTypesMap.get(e)!}
                            render={e => `€/${t(`common.priceTypes.${priceTypesMap.get(e)!}`)}`}
                            onChange={e => setState(s => ({ ...s, priceType: e}))}
                        />}
                        decimalPlaces={2}
                        minValue={0}
                        className="col-span-5 sm:col-span-7 md:col-span-7 lg:col-span-7 xl:col-span-5"
                    />
                    <div
                        className="col-span-2 sm:col-span-7 md:col-span-7 lg:col-span-7 xl:col-span-2"
                    >
                        <SingleSelect
                            label={t("common.vatRate")}
                            value={state.vatRate}
                            options={vatRates}
                            getId={e => e.toString()}
                            render={e => `${e}%`}
                            onChange={e => setState(s => ({ ...s, vatRate: e}))}
                        />
                    </div>
                </div>
                <MultiSelect
                    label={t("common.entities.menuCategories")}
                    options={categoriesQuery.data}
                    values={categories ?? []}
                    onChange={setCategories}
                    render={c => c.name}
                    getId={c => c.id}

                    onCreateOption={createAndAssignCategory}
                />
                <TextAreaField
                    label={t("common.description")}
                    value={state.description}
                    onChange={e => setState(s => ({ ...s, description: e}))}
                    rows={5}
                />
                <ImageInput
                    label={t("common.image")}
                    aspectRatio={1}
                    value={state.imageUrl}
                    inlineEditor
                    onUploadHandlerChanged={setLogoUploadHandler}
                />
            </div>

            <div className="p-5 border border-gray-200 rounded-2xl dark:border-gray-800 lg:p-6 col-span-1 flex flex-col">
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
                <div className="flex flex-col gap-4 flex-1">
                    <TextField
                        label={t("common.name")}
                        type="text"
                        value={state.translations[currentLanguage]?.name ?? ""}
                        onChange={(e) => setState(s => {
                            const aux = { ...s.translations };
                            aux[currentLanguage] = {
                                name: e,
                                description: s.translations[currentLanguage].description,
                            }
                            return {
                                ...s, 
                                translations: aux,
                            };
                        })}
                    />
                    <TextAreaField
                        label={t("common.description")}
                        value={state.translations[currentLanguage]?.description ?? ""}
                        onChange={(e) => setState(s => {
                            const aux = { ...s.translations };
                            aux[currentLanguage] = {
                                description: e,
                                name: s.translations[currentLanguage].name,
                            }
                            return {
                                ...s, 
                                translations: aux,
                            };
                        })}
                        className="flex-1"
                    />
                </div>
            </div>

            <div className="p-5 border border-gray-200 rounded-2xl dark:border-gray-800 lg:p-6 col-span-1 gap-4">
                <h4 className="text-lg font-semibold text-gray-800 dark:text-white/90 lg:mb-2">
                    {t("common.entities.local")}
                </h4>
                <p className="mt-0.5 text-xs text-gray-600 sm:text-sm dark:text-white/70 mb-6">
                    {t("pages.menuItems.locationDescription")}
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
                    />
                }
                </div>
            </div>
            
            <div className="p-5 border border-gray-200 rounded-2xl dark:border-gray-800 lg:p-6 col-span-1 gap-4">
                <h4 className="text-lg font-semibold text-gray-800 dark:text-white/90 lg:mb-2">
                    {t("pages.menuItems.availabilityPeriods")}
                </h4>
                <p className="mt-0.5 text-xs text-gray-600 sm:text-sm dark:text-white/70 mb-6">
                    {t("pages.menuItems.availabilityPeriodsDescription")}
                </p>

                <div>
                    
                </div>
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