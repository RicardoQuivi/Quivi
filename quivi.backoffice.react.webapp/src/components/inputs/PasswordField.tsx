import { useState } from "react";
import Label from "../form/Label";
import { EyeCloseIcon, EyeIcon } from "../../icons";

interface PasswordFieldProps {
    readonly label?: string;
    readonly placeholder?: string;
    readonly value?: string;
    readonly onChange?: (v: string) => any;
    readonly errorMessage?: string;
    readonly name?: string;
    readonly disabled?: boolean;
    readonly autoComplete?: string;
    readonly onKeyUp?: (e: React.KeyboardEvent<HTMLInputElement>) => void;
}
export const PasswordField = (props: PasswordFieldProps) => {
    const [showPassword, setShowPassword] = useState(false);
    
    let inputClasses = ` h-11 w-full rounded-lg border appearance-none px-4 py-2.5 text-sm shadow-theme-xs placeholder:text-gray-400 focus:outline-hidden focus:ring-3  dark:bg-gray-900 dark:text-white/90 dark:placeholder:text-white/30`;

    if (props.disabled) {
        inputClasses += ` text-gray-500 border-gray-300 opacity-40 bg-gray-100 cursor-not-allowed dark:bg-gray-800 dark:text-gray-400 dark:border-gray-700 opacity-40`;
    } else if (props.errorMessage != undefined) {
        inputClasses += `  border-error-500 focus:border-error-300 focus:ring-error-500/20 dark:text-error-400 dark:border-error-500 dark:focus:border-error-800`;
    } else {
        inputClasses += ` bg-transparent text-gray-800 border-gray-300 focus:border-brand-300 focus:ring-brand-500/20 dark:border-gray-700 dark:text-white/90  dark:focus:border-brand-800`;
    }

    return (
    <div className="grid grid-cols-1">
        {
            props.label != undefined &&
            <Label>{props.label}</Label>
        }

        <div className="relative flex flex-col grid grid-cols-1">
            <div className="relative">
                <input
                    type={showPassword ? "text" : "password"}
                    name={props.name}
                    value={props.value}
                    onChange={(e) => props.onChange?.(e.target.value)}
                    disabled={props.disabled}
                    autoComplete={props.autoComplete}
                    onKeyUp={props.onKeyUp}
                    placeholder={props.placeholder}
                    className={inputClasses}
                />
                <span
                    onClick={() => setShowPassword(s => !s)}
                    className="absolute z-30 -translate-y-1/2 cursor-pointer right-4 top-1/2"
                >
                {
                    !!props.value &&
                    (
                        showPassword
                        ?
                            <EyeIcon className="fill-gray-500 dark:fill-gray-400 size-5" />
                        :
                            <EyeCloseIcon className="fill-gray-500 dark:fill-gray-400 size-5" />
                    )
                }
                </span>
            </div>
            {
                props.errorMessage != undefined && 
                <p
                    className={'mt-1.5 text-xs text-error-500'}
                >
                    {props.errorMessage}
                </p>
            }
        </div>
    </div>
    )
}