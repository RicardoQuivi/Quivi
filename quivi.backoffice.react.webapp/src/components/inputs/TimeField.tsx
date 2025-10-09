import { TimeIcon } from "../../icons";
import Input from "../form/input/InputField";
import Label from "../form/Label";
import { InputErrorMessage } from "./InputErrorMessage";

interface TimeFieldProps {
    readonly label?: string;
    readonly value?: Date;
    readonly placeholder?: string;
    readonly onChange?: (v: Date) => any;
    readonly errorMessage?: React.ReactNode;
    readonly name?: string;
    readonly disabled?: boolean;
    readonly autoComplete?: string;
    readonly onKeyUp?: (e: React.KeyboardEvent<HTMLInputElement>) => void;
    readonly className?: string;
    readonly format: string;
}
export const TimeField = (props: TimeFieldProps) => {
    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const timeStr = e.target.value;
        const [hoursStr, minutesStr] = timeStr.split(":");
        const date = new Date();
        date.setHours(parseInt(hoursStr));
        date.setMinutes(parseInt(minutesStr));
        date.setSeconds(0);
        date.setMilliseconds(0);

        props.onChange?.(date);
    };

    return (
    <div className={`flex flex-col ${props.className ?? ""}`}>
        {
            props.label != undefined &&
            <Label>{props.label}</Label>
        }
        <div className="relative">
            <Input
                type="time"
                name={props.name}
                value={formatTimeValue(props.value)}
                onChange={handleChange}
                disabled={props.disabled}
                autoComplete={props.autoComplete}
                placeholder={props.placeholder}
            />
            <span className="absolute text-gray-500 -translate-y-1/2 pointer-events-none right-3 top-1/2 dark:text-gray-400">
                <TimeIcon className="size-6" />
            </span>
        </div>
        <InputErrorMessage message={props.errorMessage} />
    </div>
    )
}

const formatTimeValue = (date?: Date): string => {
    if (!date) return "";
    const hours = date.getHours().toString().padStart(2, "0");
    const minutes = date.getMinutes().toString().padStart(2, "0");
    return `${hours}:${minutes}`;
};