import Label from "../form/Label";
import { Skeleton } from "../ui/skeleton/Skeleton";
import { InputErrorMessage } from "./InputErrorMessage";

interface TextFieldProps {
    readonly label?: string;
    readonly value?: string;
    readonly placeholder?: string;
    readonly onChange?: (v: string) => any;
    readonly errorMessage?: string;
    readonly name?: string;
    readonly disabled?: boolean;
    readonly autoComplete?: string;
    readonly className?: string;
    readonly rows?: number;
    readonly isLoading?: boolean;
}
export const TextAreaField = (props: TextFieldProps) => {
    let textareaClasses = `w-full flex-1 rounded-lg border px-4 py-2.5 text-sm shadow-theme-xs focus:outline-hidden ${props.className} `;

    if (props.disabled) {
        textareaClasses += ` bg-gray-100 opacity-50 text-gray-500 border-gray-300 cursor-not-allowed opacity40 dark:bg-gray-800 dark:text-gray-400 dark:border-gray-700`;
    } else if (props.errorMessage != undefined) {
        textareaClasses += ` bg-transparent  border-gray-300 focus:border-error-300 focus:ring-3 focus:ring-error-500/10 dark:border-gray-700 dark:bg-gray-900 dark:text-white/90 dark:focus:border-error-800`;
    } else {
        textareaClasses += ` bg-transparent text-gray-900 dark:text-gray-300 text-gray-900 border-gray-300 focus:border-brand-300 focus:ring-3 focus:ring-brand-500/10 dark:border-gray-700 dark:bg-gray-900 dark:text-white/90 dark:focus:border-brand-800`;
    }

    if(props.isLoading == true) {
        textareaClasses += ` invisible`;
    }

    return (
    <div className={`flex flex-col ${props.className ?? ""}`}>
        {
            props.label != undefined &&
            <Label>{props.label}</Label>
        }
        <div className="relative flex flex-col flex-1">
            <div className="relative">
                <textarea
                    name={props.name}
                    value={props.value ?? ""}
                    onChange={(e) => props.onChange?.(e.target.value)}
                    disabled={props.disabled}
                    autoComplete={props.autoComplete}
                    placeholder={props.placeholder}
                    className={textareaClasses}
                    rows={props.rows}
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