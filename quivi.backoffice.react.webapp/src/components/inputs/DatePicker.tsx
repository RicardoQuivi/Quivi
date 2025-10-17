import { useEffect, useId, useRef } from "react";
import flatpickr from "flatpickr";
import "flatpickr/dist/flatpickr.css";
import Label from "../form/Label";
import { CalendarIcon } from "../../icons";
import Hook = flatpickr.Options.Hook;
import DateOption = flatpickr.Options.DateOption;
import { useTranslation } from "react-i18next";
import { InputErrorMessage } from "./InputErrorMessage";
import { Skeleton } from "../ui/skeleton/Skeleton";

interface PropsType {
    readonly mode?: "single" | "multiple" | "range" | "time";
    readonly onChange?: Hook | Hook[];
    readonly defaultDate?: DateOption | DateOption[];
    readonly label?: string;
    readonly placeholder?: string;
    readonly errorMessage?: React.ReactNode;
    readonly isLoading?: boolean;
};

export const DatePicker = (props: PropsType) => {
    const id = useId();
    const { t } = useTranslation();
    const inputRef = useRef<HTMLInputElement | null>(null);
    const fpRef = useRef<flatpickr.Instance | null>(null);

    useEffect(() => {
        if (!inputRef.current) {
            return;
        }

        const picker = flatpickr(inputRef.current, {
            mode: props.mode ?? "single",
            monthSelectorType: "static",
            dateFormat: "Y-m-d",
            defaultDate: props.defaultDate,
            onChange: props.onChange,
            locale: {
                rangeSeparator: ` ${t("dateHelper.datePicker.rangeSeparator")} `,
            },
        });
        fpRef.current = picker;
        return () => picker.destroy();
    }, [props.mode, props.onChange, props.defaultDate]);

    return (
        <div className='flex flex-col'>
            {
                props.label != undefined &&
                <Label htmlFor={id}>{props.label}</Label>
            }

            <div className="relative flex flex-col">
                <div className="relative">
                    <div className={props.isLoading == true ? "invisible" : undefined}>
                        <input
                            ref={inputRef}
                            id={id}
                            placeholder={props.placeholder}
                            className="h-11 w-full rounded-lg border appearance-none px-4 py-2.5 text-sm shadow-theme-xs placeholder:text-gray-400 focus:outline-hidden focus:ring-3 dark:bg-gray-900 dark:text-white/90 dark:placeholder:text-white/30  bg-transparent text-gray-800 border-gray-300 focus:border-brand-300 focus:ring-brand-500/20 dark:border-gray-700  dark:focus:border-brand-800"
                        />

                        <span className="absolute text-gray-500 -translate-y-1/2 pointer-events-none right-3 top-1/2 dark:text-gray-400">
                            <CalendarIcon className="size-6" />
                        </span>
                    </div>
                    {
                        props.isLoading == true &&
                        <Skeleton className="absolute inset-0"/>
                    }
                </div>

                <InputErrorMessage message={props.errorMessage} />
            </div>
        </div>
    );
}
