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
import { ChevronDownIcon, CloseIcon } from "../../icons";

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
    readonly onCreateOption?: (value: string) => Promise<any> | any;
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
            IndicatorsContainer: ({
                clearValue,
                getStyles,
                getClassNames,
                getValue,
                hasValue,
                isMulti,
                isRtl,
                selectOption,
                selectProps,
                setValue,
                isDisabled,
                cx,
                ...p
            }) => <span className="absolute text-gray-500 -translate-y-1/2 right-4 top-1/2 dark:text-gray-400 pointer-events-none" {...p}>
                <ChevronDownIcon className="size-5"/>
            </span>,
            MultiValue: p => <CustomMultiValue {...p} render={props.render} />,
            MultiValueContainer: CustomMultiValueContainer,
            MultiValueRemove: CustomMultiValueRemove,
        }}
        classNames={{
            valueContainer: () => "h-full",
            control: () => "relative h-11 w-full appearance-none rounded-lg border border-gray-300 bg-transparent pr-11 text-sm shadow-theme-xs placeholder:text-gray-400 focus:border-brand-300 focus:outline-hidden focus:ring-3 focus:ring-brand-500/10 dark:border-gray-700 dark:bg-gray-900 dark:text-white/90 dark:placeholder:text-white/30 dark:focus:border-brand-800 text-gray-400 dark:text-gray-400 cursor-pointer",
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
            const result = props.onCreateOption(input);
            if (result instanceof Promise) {
                await result;
            }
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
                <CloseIcon className="size-4" />
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
                            t("common.operations.new", {
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