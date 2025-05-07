import { useEffect, useState } from "react";
import {
    AlertHexaIcon,
    CheckCircleIcon,
    CloseIcon,
    ErrorHexaIcon,
    InfoIcon,
} from "../../../icons";

interface NotificationProps {
    variant: "success" | "info" | "warning" | "error"; // Notification type
    title: string; // Title text
    description?: string; // Optional description
    hideDuration?: number; // Time in milliseconds to hide the notification (default: 5000ms)
    close: () => any;
}

const Notification: React.FC<NotificationProps> = ({
    variant,
    title,
    description,
    close,
    hideDuration = 3000, // Default hide duration: 5 seconds
}) => {
    const [progress, setProgress] = useState(100);

    useEffect(() => {
        const start = Date.now();

        const interval = setInterval(() => {
            const elapsed = Date.now() - start;
            const percentage = Math.max(100 - (elapsed / hideDuration) * 100, 0);
            setProgress(percentage);
        }, 100);

        const timeout = setTimeout(close, hideDuration);
        return () => {
            clearInterval(interval);
            clearTimeout(timeout);
        };
    }, [close, hideDuration]);
    
    // Styling configuration for each alert type
    const variantStyles = {
        success: {
            borderColor: "border-success-500",
            iconBg: "bg-success-50 text-success-500 dark:text-success-500 dark:bg-success-500/[0.15]",
            icon: <CheckCircleIcon />,
        },
        info: {
            borderColor: "border-blue-light-500",
            iconBg: "bg-blue-light-50 text-blue-light-500 dark:bg-blue-light-500/[0.15] dark:text-blue-light-500",
            icon: <InfoIcon />,
        },
        warning: {
            borderColor: "border-warning-500",
            iconBg: "bg-warning-50 text-warning-500 dark:bg-warning-500/[0.15] dark:text-orange-400",
            icon: <AlertHexaIcon />,
        },
        error: {
            borderColor: "border-error-500",
            iconBg: "bg-error-50 text-error-500 dark:bg-error-500/[0.15] dark:text-error-500",
            icon: <ErrorHexaIcon className="size-5" />,
        },
    };

    const { borderColor, iconBg, icon } = variantStyles[variant];

return (
        <div
            className={`relative flex items-center justify-between gap-3 w-full sm:max-w-[340px] rounded-md p-3 shadow-theme-sm bg-white dark:bg-[#1E2634]`}
        >
            <div className="flex items-center gap-4">
                {/* Icon */}
                <div
                    className={`flex items-center justify-center w-10 h-10 rounded-lg ${iconBg}`}
                >
                    {icon}
                </div>

                {/* Title and Description */}
                <div>
                    <h4 className="text-sm text-gray-800 sm:text-base dark:text-white/90">
                        {title}
                    </h4>
                    {
                        description && (
                            <p className="mt-1 text-xs text-gray-600 sm:text-sm dark:text-white/70">
                                {description}
                            </p>
                        )
                    }
                </div>
            </div>

            {/* Close Button */}
            <button
                onClick={close}
                className="text-gray-400 hover:text-gray-800 dark:hover:text-white/90"
            >
                <CloseIcon />
            </button>

            {/* Progress bar */}
            <div className="absolute bottom-0 left-0 h-1 w-full bg-transparent">
                <div
                    className={`h-full transition-all duration-100 ease-linear border-b-4 ${borderColor}`}
                    style={{ width: `${progress}%` }}
                />
            </div>
        </div>
    );
};
export default Notification;