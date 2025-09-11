import { useMemo } from "react";
import Label from "../form/Label";
import Select from "../form/Select";
import { Collections } from "../../utilities/Collectionts";

interface Props<T,> {
    readonly label?: React.ReactNode,
    readonly placeholder?: string;
    readonly disabled?: boolean,
    readonly value: T,
    readonly options: T[],
    readonly getId: (e: T) => string;
    readonly render: (e: T) => React.ReactNode;
    readonly onChange: (value: T) => void,
    readonly className?: string;
}

export const SingleSelect = <T,>(props: Props<T>) => {
    const itemsMap = useMemo(() => Collections.toMap(props.options, props.getId), [props.options, props.getId])

    return <div className="grid grid-cols-1 cursor-pointer">
        { props.label != undefined && <Label>{props.label}</Label> }
        <Select
            options={props.options.map(o => ({
                value: props.getId(o),
                label: props.render(o),
            }))}
            placeholder={props.placeholder}
            onChange={e => props.onChange(itemsMap.get(e)!)}
            value={props.getId(props.value)}
            className={`bg-gray-50 dark:bg-gray-800 cursor-pointer ${props.className}`}
            disabled={props.disabled}
        />
    </div>
}