import { MouseEventHandler, ReactNode } from "react";
import { Spinner } from "../../spinners/Spinner";

interface ButtonProps {
    readonly children: ReactNode;
    readonly size?: "sm" | "md";
    readonly variant?: "primary" | "outline";
    readonly startIcon?: ReactNode;
    readonly endIcon?: ReactNode;
    readonly onClick?: MouseEventHandler<HTMLButtonElement>;
    readonly disabled?: boolean;
    readonly className?: string;
    readonly type?: "submit" | "reset" | "button" | undefined;
    readonly isLoading?: boolean;
}

const Button: React.FC<ButtonProps> = ({
    children,
    size = "md",
    variant = "primary",
    startIcon,
    endIcon,
    onClick,
    className = "",
    disabled = false,
    type = "button",
    isLoading,
}) => {
    // Size Classes
    const sizeClasses = {
        sm: "px-4 py-3 text-sm",
        md: "px-5 py-3.5 text-sm",
    };

    // Variant Classes
    const variantClasses = {
        primary:
            "bg-brand-500 text-white shadow-theme-xs hover:bg-brand-600 disabled:bg-brand-300",
        outline:
            "bg-white text-gray-700 ring-1 ring-inset ring-gray-300 hover:bg-gray-50 dark:bg-gray-800 dark:text-gray-400 dark:ring-gray-700 dark:hover:bg-white/[0.03] dark:hover:text-gray-300",
    };

    if(isLoading) {
        disabled = true;
    }
    
    return (
        <button
            className={`inline-flex items-center justify-center gap-2 rounded-lg transition ${className} ${sizeClasses[size]} ${variantClasses[variant]} ${disabled ? "cursor-not-allowed opacity-50" : ""}`}
            onClick={onClick}
            disabled={disabled}
            type={type}
        >
            {startIcon && <span className="flex items-center">{startIcon}</span>}
            <div className="relative w-full">
                <div className={isLoading == true ? "invisible" : "w-full flex justify-center"}>
                    {children}
                </div>
                {
                    isLoading == true &&
                    <div className="absolute top-0 bottom-0 flex w-full flex-row justify-center">
                        <Spinner />
                    </div>
                }
            </div>
            {endIcon && <span className="flex items-center">{endIcon}</span>}
        </button>
    );
};
export default Button;