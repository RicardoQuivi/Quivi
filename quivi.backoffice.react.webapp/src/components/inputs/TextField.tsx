import Label from "../form/Label";
import { Skeleton } from "../ui/skeleton/Skeleton";
import { InputErrorMessage } from "./InputErrorMessage";

interface TextFieldProps {
    readonly label?: string;
    readonly value?: string;
    readonly placeholder?: string;
    readonly onChange?: (v: string) => any;
    readonly errorMessage?: React.ReactNode;
    readonly name?: string;
    readonly disabled?: boolean;
    readonly autoComplete?: string;
    readonly onKeyUp?: (e: React.KeyboardEvent<HTMLInputElement>) => void;
    readonly type?: "text" | "email";
    readonly className?: string;
    readonly isLoading?: boolean;
}
export const TextField = (props: TextFieldProps) => {
    let inputClasses = ` h-11 w-full rounded-lg border appearance-none px-4 py-2.5 text-sm shadow-theme-xs placeholder:text-gray-400 focus:outline-hidden focus:ring-3  dark:bg-gray-900 dark:text-white/90 dark:placeholder:text-white/30`;

    if (props.disabled) {
        inputClasses += ` text-gray-500 border-gray-300 opacity-40 bg-gray-100 cursor-not-allowed dark:bg-gray-800 dark:text-gray-400 dark:border-gray-700 opacity-40`;
    } else if (props.errorMessage != undefined) {
        inputClasses += `  border-error-500 focus:border-error-300 focus:ring-error-500/20 dark:text-error-400 dark:border-error-500 dark:focus:border-error-800`;
    } else {
        inputClasses += ` bg-transparent text-gray-800 border-gray-300 focus:border-brand-300 focus:ring-brand-500/20 dark:border-gray-700 dark:text-white/90  dark:focus:border-brand-800`;
    }

    if(props.isLoading == true) {
        inputClasses += ` invisible`;
    }

    return (
    <div className={`flex flex-col ${props.className ?? ""}`}>
        {
            props.label != undefined &&
            <Label>{props.label}</Label>
        }
        <div className="relative flex flex-col">
            <div>
                <input
                    type={props.type ?? "text"}
                    name={props.name}
                    value={props.value}
                    onChange={(e) => props.onChange?.(e.target.value)}
                    disabled={props.disabled}
                    autoComplete={props.autoComplete}
                    onKeyUp={props.onKeyUp}
                    placeholder={props.placeholder}
                    className={inputClasses}
                />
                {
                    props.isLoading == true &&
                    <Skeleton className="absolute inset-0"/>
                }
            </div>
            <InputErrorMessage message={props.errorMessage} />
        </div>
    </div>
    )
}