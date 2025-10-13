import type React from "react";
import { Skeleton } from "../../ui/skeleton/Skeleton";

interface CheckboxProps {
    readonly label?: string;
    readonly checked: boolean;
    readonly className?: string;
    readonly id?: string;
    readonly onChange?: (checked: boolean) => void;
    readonly disabled?: boolean;
    readonly isLoading?: boolean;
}

const Checkbox: React.FC<CheckboxProps> = ({
    label,
    checked,
    id,
    onChange,
    className = "",
    disabled = false,
    isLoading,
}) => {
    if(isLoading == true) {
        disabled = true;
    }

    return (
        <label
            className={`flex items-center space-x-3 group cursor-pointer ${disabled ? "cursor-not-allowed opacity-60" : ""}`}
        >
            <div className="relative w-5 h-5">
                <div
                    className="relative"
                >
                    <input
                        id={id}
                        type="checkbox"
                        className={`w-5 h-5 appearance-none cursor-pointer dark:border-gray-700 border border-gray-300 checked:border-transparent rounded-md checked:bg-brand-500 disabled:opacity-60 ${className}`}
                        checked={checked}
                        onChange={(e) => onChange?.(e.target.checked)}
                        disabled={disabled}
                    />
                    {
                        isLoading == true &&
                        <Skeleton className="absolute top-0 left-0 w-5 h-5 "/>
                    }
                </div>
                {
                    isLoading !== true &&
                    checked &&
                    <svg
                        className="absolute transform -translate-x-1/2 -translate-y-1/2 pointer-events-none top-1/2 left-1/2 dark:text-gray-25"
                        xmlns="http://www.w3.org/2000/svg"
                        width="14"
                        height="14"
                        viewBox="0 0 14 14"
                        fill="none"
                    >
                        <path
                            d="M11.6666 3.5L5.24992 9.91667L2.33325 7"
                            stroke="currentColor"
                            strokeWidth={disabled ? "2.33333" : "1.94437"}
                            strokeLinecap="round"
                            strokeLinejoin="round"
                        />
                    </svg>
                }
            </div>
            {
                label &&
                <span className="text-sm font-medium text-gray-800 dark:text-gray-200">
                    {label}
                </span>
            }
        </label>
    );
};
export default Checkbox;