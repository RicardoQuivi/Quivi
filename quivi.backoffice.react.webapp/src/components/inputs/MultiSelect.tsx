import Select, {
    components,
    SingleValueProps,
    OptionProps,
    ContainerProps,
    MultiValueProps,
    MultiValueGenericProps,
    MultiValueRemoveProps,
} from "react-select";
import Label from "../form/Label";
import { useTranslation } from "react-i18next";
import CreatableSelect from 'react-select/creatable';
import { useState } from "react";

interface Props<T,> {
    readonly label?: React.ReactNode,
    readonly placeholder?: string;
    readonly disabled?: boolean,
    readonly values: T[],
    readonly options: T[],
    readonly getId: (e: T) => string;
    readonly render: (e: T) => string;
    readonly onChange: (values: T[]) => void;

    readonly createOptionLabel?: (value: string) => React.ReactNode;
    readonly onCreateOption?: (value: string) => Promise<any>;
}

export const MultiSelect = <T,>(props: Props<T>) => {
    const { t } = useTranslation();
    const [isLoading, setIsLoading] = useState(false);

    const SelectAux = props.onCreateOption ? CreatableSelect : Select;
    return (
    <SelectAux
        getOptionValue={props.getId}
        getOptionLabel={(e) => props.render(e)?.toString() ?? ""}
        
        options={props.options}
        value={props.values}
        components={{
            SingleValue: CustomSingleValue,
            Option: p =>  <CustomOption {...p} render={props.render} createOptionLabel={props.createOptionLabel} />,
            SelectContainer: p => <CustomContainer label={props.label} {...p} />,
            IndicatorsContainer: p => <span className="absolute z-30 text-gray-500 -translate-y-1/2 right-4 top-1/2 dark:text-gray-400" {...p}>
                <svg className="stroke-current" width="20" height="20" viewBox="0 0 20 20" fill="none" xmlns="http://www.w3.org/2000/svg">
                    <path d="M4.79175 7.396L10.0001 12.6043L15.2084 7.396" stroke="" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round"></path>
                </svg>
            </span>,
            MultiValue: p => <CustomMultiValue {...p} render={props.render} />,
            MultiValueContainer: CustomMultiValueContainer,
            MultiValueRemove: CustomMultiValueRemove,
        }}
        classNames={{
            valueContainer: () => "h-full",
            control: () => "relative z-20 h-11 w-full appearance-none rounded-lg border border-gray-300 bg-transparent pr-11 text-sm shadow-theme-xs placeholder:text-gray-400 focus:border-brand-300 focus:outline-hidden focus:ring-3 focus:ring-brand-500/10 dark:border-gray-700 dark:bg-gray-900 dark:text-white/90 dark:placeholder:text-white/30 dark:focus:border-brand-800 text-gray-400 dark:text-gray-400 cursor-pointer",
        }}
        placeholder={props.placeholder ?? ""}
        isMulti
        onChange={(selected) => {
            props.onChange(selected as T[] ?? []);
        }}
        isDisabled={props.disabled}
        noOptionsMessage={(inputValue) => inputValue ? t("common.noOptions") : t("common.startTypingToSearch")}
        styles={{
            control: () => ({

            }),
        }}
        onCreateOption={async (input) => {
            if(props.onCreateOption == undefined) {
                return;
            }

            setIsLoading(true);
            await props.onCreateOption(input);
            setIsLoading(false);
        }}
        isLoading={isLoading}
    />
    )
}

const CustomMultiValueRemove = (props: MultiValueRemoveProps<any>) => {
    const {
        children,
        ...rProps
    } = props;
    return (
        <components.MultiValueRemove {...rProps}>
            <div
                className="pl-2 text-gray-500 cursor-pointer group-hover:text-gray-400 dark:text-gray-400"
            >
                <svg
                    className="fill-current"
                    role="button"
                    width="14"
                    height="14"
                    viewBox="0 0 14 14"
                    xmlns="http://www.w3.org/2000/svg"
                >
                    <path
                        fillRule="evenodd"
                        clipRule="evenodd"
                        d="M3.40717 4.46881C3.11428 4.17591 3.11428 3.70104 3.40717 3.40815C3.70006 3.11525 4.17494 3.11525 4.46783 3.40815L6.99943 5.93975L9.53095 3.40822C9.82385 3.11533 10.2987 3.11533 10.5916 3.40822C10.8845 3.70112 10.8845 4.17599 10.5916 4.46888L8.06009 7.00041L10.5916 9.53193C10.8845 9.82482 10.8845 10.2997 10.5916 10.5926C10.2987 10.8855 9.82385 10.8855 9.53095 10.5926L6.99943 8.06107L4.46783 10.5927C4.17494 10.8856 3.70006 10.8856 3.40717 10.5927C3.11428 10.2998 3.11428 9.8249 3.40717 9.53201L5.93877 7.00041L3.40717 4.46881Z"
                    />
                </svg>
            </div>
        </components.MultiValueRemove>
    )
}

const CustomMultiValueContainer = (props: MultiValueGenericProps<any>) => {
    const {
        children,
        ...rProps
    } = props;
    return (
        <components.MultiValueContainer {...rProps}>
            <div className="group flex items-center justify-center rounded-full border-[0.7px] border-transparent bg-gray-100 py-1 pl-2.5 pr-2 text-sm text-gray-800 hover:border-gray-200 dark:bg-gray-800 dark:text-white/90 dark:hover:border-gray-800 !dark:shadow-none">
                {children}
            </div>
        </components.MultiValueContainer>
    )
}

interface CustomMultiValueProps<T> extends MultiValueProps<any> {
    readonly render: (e: T) => React.ReactNode;
}
const CustomMultiValue = (props: CustomMultiValueProps<any>) => {
    const {
        children,
        render,
        ...rProps
    } = props;

    return (
        <components.MultiValue {...rProps} className="!bg-transparent">
            <span className="flex-initial max-w-full text-sm text-gray-800 dark:text-white/90">
                {render(props.data)}
            </span>
        </components.MultiValue>
    )
}

interface CustomContainerProps<T> extends ContainerProps<T> {
    readonly label?: React.ReactNode, 
}
const CustomContainer = (props: CustomContainerProps<any>) => {
    const {
        children,
        ...rProps
    } = props;

    return (
        <components.SelectContainer {...rProps} className="flex flex-col cursor-pointer">
            { props.label != undefined && <Label>{props.label}</Label> }
            {children}
        </components.SelectContainer>
    )
}

const CustomSingleValue = (props: SingleValueProps<any>) => (
  <components.SingleValue {...props}>
    <span className="flex items-center gap-2">
      <span className="font-semibold">{props.data.label} CustomSingleValue</span>
      <span className="text-xs text-gray-400">({props.data.value}) CustomSingleValue</span>
    </span>
  </components.SingleValue>
);

interface CustomOptionProps<T> extends OptionProps<any> {
    readonly render: (e: T) => React.ReactNode;
    readonly createOptionLabel?: (value: string) => React.ReactNode;
}

const CustomOption = <T,>(props: CustomOptionProps<T>) => {
    const {
        children,
        render,
        createOptionLabel,
        ...rProps
    } = props;
    const { t } = useTranslation();

    const isNew = (props.data as any).__isNew__;

    return (
    <components.Option {...rProps}>
        <div className="flex justify-between items-center">
            {
                isNew
                ?
                (
                    createOptionLabel == undefined
                    ?
                    <span className="italic text-gray-500">
                        {
                            t("common.operations.create", {
                                name: props.data.value
                            })
                        }
                    </span>
                    :
                    createOptionLabel(props.data.label)
                )
                : 
                    <span>{render(props.data as T)}</span>
            }
            {props.isSelected && <span className="text-green-500">✔</span>}
        </div>
    </components.Option>
    )
}