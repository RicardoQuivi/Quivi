import { useEffect, useMemo, useState } from "react";
import { MenuItem } from "../../../../hooks/api/Dtos/menuItems/MenuItem";
import { useQuiviForm } from "../../../../hooks/api/exceptions/useQuiviForm";
import { Language } from "../../../../hooks/api/Dtos/Language";
import * as yup from 'yup';
import { useTranslation } from "react-i18next";
import { LanguageSelector } from "../../../../components/ui/language/LanguageSelector";
import { useToast } from "../../../../layout/ToastProvider";
import { TextField } from "../../../../components/inputs/TextField";
import { NumberField } from "../../../../components/inputs/NumberField";
import { MultiSelect } from "../../../../components/inputs/MultiSelect";
import { ModifierGroup } from "../../../../hooks/api/Dtos/modifierGroups/ModifierGroup";
import { useMenuItemsQuery } from "../../../../hooks/queries/implementations/useMenuItemsQuery";
import { Skeleton } from "../../../../components/ui/skeleton/Skeleton";
import Avatar from "../../../../components/ui/avatar/Avatar";
import { ResponsiveTable } from "../../../../components/tables/ResponsiveTable";
import { CurrencyField } from "../../../../components/inputs/CurrencyField";
import { Collections } from "../../../../utilities/Collectionts";

const syncedRecord = <T1, T2>(left: Record<string, T1>, right: Record<string, T2>, create: (a: T1) => T2): Record<string, T2> => {
    const result: Record<string, T2> = {};

    for (const key in left) {
        result[key] = key in right ? right[key] : create(left[key]);
    }

    return result;
}

const schema = yup.object<ModifierGroupFormState>({
    name: yup.string().required(),
    imageUrl: yup.string().optional(),
    items: yup.object().required()
                        .test(
                            'minEntries',
                            (value, testContext) => {
                                const min = 1;
                                if (value == null || Object.keys(value).length < min) {
                                    return testContext.createError({
                                        message: `At least ${min} item(s) required`,
                                        params: { min }
                                    });
                                }
                                return true;
                            }
                        )
});
export interface ModifierGroupFormState {
    readonly name: string;
    readonly minSelection: number;
    readonly maxSelection: number;
    readonly translations?: Record<Language, Translation>;
    readonly items: Record<string, ModifierItem>;
}
interface ModifierItem {
    readonly price: number;
    readonly sortIndex: number;
}
interface Translation {
    readonly name: string;
}
const getState = (model: ModifierGroup | undefined) => {
    const translations = Object.values(Language)
                                .filter((v): v is Language => typeof v === "string")
                                .reduce((acc, lang) => {
                                    const translation = model?.translations?.[lang];

                                    acc[lang] = {
                                        name: translation?.name ?? "",
                                    };

                                    return acc;
                                }, {} as Record<Language, Translation>);

    const items = {} as Record<string, ModifierItem>;
    for(const id of Object.keys(model?.items ?? {})) {
        const entry = model!.items[id];
        items[id] = {
            price: entry.price,
            sortIndex: entry.sortIndex,
        }
    }

    return {
        name: model?.name ?? "",
        minSelection: model?.minSelection ?? 0,
        maxSelection: model?.maxSelection ?? 1,
        translations: translations,
        items: items,
    }
}

const getItems = (previousValues: MenuItem[] | undefined, model: ModifierGroup | undefined, map: Map<string, MenuItem> | undefined) => {
    if(previousValues != undefined) {
        return previousValues;
    }

    if(map == undefined) {
        return undefined;
    }

    if(model == undefined) {
        return undefined;
    }

    const result = [] as MenuItem[];
    for(const id of Object.keys(model.items)) {
        const cat = map.get(id);
        if(cat == undefined) {
            continue;
        }
        result.push(cat);
    }
    return result;
}

interface Props {
    readonly model?: ModifierGroup;
    readonly onSubmit: (state: ModifierGroupFormState) => Promise<any>;
    readonly categoryId?: string;
    readonly onFormChange: (isValid: boolean, submit: () => Promise<void>) => any;
    readonly isLoading?: boolean;
}
export const ModifierGroupBaseForm = (props: Props) => {
    const { t } = useTranslation();
    const toast = useToast();

    const itemsQuery = useMenuItemsQuery({
        page: 0,
    });
    const itemsMap = useMemo(() => itemsQuery.isFirstLoading ? undefined : Collections.toMap(itemsQuery.data, i => i.id), [itemsQuery.data])   

    const [currentLanguage, setCurrentLanguage] = useState(Language.Portuguese);
    const [items, setItems] = useState<MenuItem[] | undefined>(() => getItems(undefined, props.model, itemsMap));
    const [state, setState] = useState(() => ({
        ...getState(props.model),
        apiErrors: [],
    }));

    useEffect(() => setState(_ => ({
        ...getState(props.model),

        apiErrors: [],
    })), [props.model]);
    useEffect(() => setItems(p => getItems(p, props.model, itemsMap)), [itemsMap])

    useEffect(() => setState(s => {
        if(items == undefined) {
            return s;
        }

        const newItems = {} as Record<string, MenuItem>;
        for(const item of items) {
            newItems[item.id] = item;
        }

        return {
            ...s,
            items: syncedRecord(newItems, state.items, () => ({
                price: 0,
                sortIndex: Object.keys(state.items).length,
            })),
        };
    }), [items])

    const form = useQuiviForm(state, schema);

    useEffect(() => {
        const save = () => form.submit(async () => {
            await props.onSubmit({
                name: state.name,
                minSelection: state.minSelection,
                maxSelection: state.maxSelection,
                translations: state.translations,
                items: state.items,
            })
        }, () => toast.error(t("common.operations.failure.generic")))

        props.onFormChange(form.isValid, save);
    }, [
        toast,
        t,
        form.submit,
        form.isValid,

        state.name,
        state.minSelection,
        state.maxSelection,
        state.translations,
        state.items,
    ])

    return (
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
            <div className="col-span-1 grid grid-cols-2 gap-4">
                <NumberField
                    label={t("pages.modifierGroups.minSelection")}
                    value={state.minSelection}
                    onChange={(e) => setState(s => ({ ...s, minSelection: e }))}
                    errorMessage={form.touchedErrors.get("minSelection")?.message}
                    className="col-span-1 sm:col-span-2 md:col-span-1 lg:col-span-1 xl:col-span-1"
                    isLoading={props.isLoading}
                />
                <NumberField
                    label={t("pages.modifierGroups.maxSelection")}
                    value={state.maxSelection}
                    onChange={(e) => setState(s => ({ ...s, maxSelection: e }))}
                    errorMessage={form.touchedErrors.get("maxSelection")?.message}
                    className="col-span-1 sm:col-span-2 md:col-span-1 lg:col-span-1 xl:col-span-1"
                    isLoading={props.isLoading}
                />
            </div>
            <MultiSelect
                label={t("common.entities.menuItems")}
                options={itemsQuery.data}
                values={items ?? []}
                onChange={setItems}
                render={c => c.name}
                getId={c => c.id}
                isLoading={props.isLoading}
            />
            <ResponsiveTable
                isLoading={props.isLoading}
                columns={[
                    {
                        key: "name",
                        label: t("common.name"),
                        render: id => {
                            const item = itemsMap?.get(id);
                            if(item == undefined) {
                                return <Skeleton className="w-24"/>
                            }

                            return (
                            <div className="flex items-center gap-3">
                                <Avatar
                                    src={item.imageUrl}
                                    alt={item.name}
                                    size="large"
                                />
                                <div>
                                    <span className="block font-medium text-gray-800 text-theme-sm dark:text-white/90">
                                        {item.name}
                                    </span>
                                    <span className="block text-gray-500 text-theme-xs dark:text-gray-400">
                                        {item.description}
                                    </span>
                                </div>
                            </div>
                            )
                        }
                    },
                    {
                        key: "price",
                        label: t("common.price"),
                        render: id => (
                        <CurrencyField
                            value={state.items[id]!.price}
                            endElement={(
                                <div className="h-full px-4 flex content-center flex-wrap">
                                    €
                                </div>
                            )}
                            onChange={v => setState(s => {
                                const items = { ...s.items };
                                items[id] = {
                                    ...items[id]!,
                                    price: v,
                                };
                                return {
                                    ...s,
                                    items: items,
                                }
                            })}
                        />
                        )
                    }
                ]}
                data={Object.keys(state.items)} 
                getKey={id => id}
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
    </div>
    )
}