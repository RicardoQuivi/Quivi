interface AvatarProps {
    readonly src?: string | React.ReactElement;
    readonly alt: string;
    readonly size?: "xsmall" | "small" | "medium" | "large" | "xlarge" | "xxlarge"; // Avatar size
    readonly status?: "online" | "offline" | "busy" | "none"; // Status indicator
}

const sizeClasses = {
    xsmall: "h-6 w-6 max-w-6",
    small: "h-8 w-8 max-w-8",
    medium: "h-10 w-10 max-w-10",
    large: "h-12 w-12 max-w-12",
    xlarge: "h-14 w-14 max-w-14",
    xxlarge: "h-16 w-16 max-w-16",
};

const statusSizeClasses = {
    xsmall: "h-1.5 w-1.5 max-w-1.5",
    small: "h-2 w-2 max-w-2",
    medium: "h-2.5 w-2.5 max-w-2.5",
    large: "h-3 w-3 max-w-3",
    xlarge: "h-3.5 w-3.5 max-w-3.5",
    xxlarge: "h-4 w-4 max-w-4",
};

const statusColorClasses = {
    online: "bg-success-500",
    offline: "bg-error-400",
    busy: "bg-warning-500",
};

const getColorFromString = (str: string): string => {
    let hash = 0;
    for (let i = 0; i < str.length; i++) {
        hash = str.charCodeAt(i) + ((hash << 5) - hash);
    }

    // Generate HSL color
    const hue = hash % 360;
    return `hsl(${hue}, 70%, 60%)`;
}

const Avatar: React.FC<AvatarProps> = ({
    src,
    alt = "User Avatar",
    size = "medium",
    status = "none",
}) => {
    const isStringSrc = typeof src === "string";
    const showFallback = !src;
    const backgroundColor = showFallback ? getColorFromString(alt) : undefined;
    const firstLetter = alt.charAt(0).toUpperCase();

    return (
        <div
            className={`relative flex items-center justify-center text-white font-medium rounded-full ${sizeClasses[size]}`}
            style={{ backgroundColor: showFallback ? backgroundColor : undefined }}
        >
            {showFallback ? (
                <span>{firstLetter}</span>
            ) : isStringSrc ? (
                <img src={src as string} alt={alt} className="object-cover w-full h-full rounded-full" />
            ) : (
                // ReactComponent (like an imported SVG)
                <div className="w-full h-full rounded-full overflow-hidden flex items-center justify-center">
                    {src}
                </div>
            )}

            {status !== "none" && (
                <span
                    className={`absolute bottom-0 right-0 rounded-full border-[1.5px] border-white dark:border-gray-900 ${statusSizeClasses[size]} ${statusColorClasses[status] || ""}`}
                />
            )}
        </div>
    );
};
export default Avatar;