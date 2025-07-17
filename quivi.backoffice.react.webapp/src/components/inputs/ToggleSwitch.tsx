import Label from "../form/Label";

interface Props {
    readonly label?: string;
    readonly value: boolean;
    readonly onChange?: (active: boolean) => any;
    readonly errorMessage?: string;
}

export const ToggleSwitch = (props: Props) => {
    return <div x-data="{ switcherToggle: false }" className="flex flex-col">
        <label className="flex cursor-pointer items-center gap-3 text-sm font-medium text-gray-700 select-none dark:text-gray-400">
            <div className="relative">
            <input 
                type="checkbox"
                className="sr-only" 
                onChange={() => props.onChange?.(!props.value)}
            />
            <div className={`block h-6 w-11 rounded-full bg-brand-500 dark:bg-brand-500 ${props.value ? 'bg-brand-500 dark:bg-brand-500' : 'bg-gray-200 dark:bg-white/10'}`}></div>
            <div className={`shadow-theme-sm absolute top-0.5 left-0.5 h-5 w-5 rounded-full bg-white duration-300 ease-linear ${props.value ? 'translate-x-full': 'translate-x-0'} `}></div>
            </div>
            {
                props.label != undefined &&
                <Label>{props.label}</Label>
            }
        </label>

        {
            props.errorMessage != undefined && 
            <p
                className={'mt-1.5 text-xs text-error-500'}
            >
                {props.errorMessage}
            </p>
        }
    </div>
}