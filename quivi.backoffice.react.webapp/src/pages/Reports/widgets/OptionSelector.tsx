import { Key } from "react";

interface PeriodSelectorProps<T> {
    readonly options: T[];
    readonly selected: T;
    readonly onChange: (s: T) => any;
    readonly getKey: (s: T) => Key;
    readonly render: (s: T) => React.ReactNode;
}
export const OptionSelector = <T,>(props: PeriodSelectorProps<T>) => {

    const getButtonClass = (option: T) => props.getKey(props.selected) === props.getKey(option) ? "shadow-theme-xs text-gray-900 dark:text-white bg-white dark:bg-gray-800" : "text-gray-500 dark:text-gray-400";
    
    return (
        <div className="flex items-center gap-0.5 rounded-lg bg-gray-100 p-0.5 dark:bg-gray-900">
            {
                props.options.map(o => 
                    <button
                        key={props.getKey(o)}
                        onClick={() => props.onChange(o)}
                        className={`px-3 py-2 font-medium w-full rounded-md text-theme-sm hover:text-gray-900 dark:hover:text-white ${getButtonClass(o)} whitespace-nowrap`}
                    >
                        {props.render(o)}
                    </button>
                )
            }
        </div>
    );
};