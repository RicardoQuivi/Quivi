import { Ref, useRef, useState } from "react";

export interface InjectedElementProps<TElement extends HTMLElement> {
    readonly draggable: true;
    readonly onDragStart: () => any;
    readonly ref: Ref<TElement> | undefined;
}

interface Props<T, TElement extends HTMLElement> {
    readonly items: T[];
    readonly render: (m: T, injectedProps: InjectedElementProps<TElement>) => React.ReactNode;
    readonly onOrderChanged: (items: T[]) => any;
}
export const DraggingContainer = <T, TElement extends HTMLElement,>(props: Props<T, TElement>) => {
    const [dragging, setDragging] = useState<T>();
    const itemRefs = useRef(new Map<T, TElement | null>());

    const handleDragOver = (e: React.DragEvent<HTMLDivElement>) => e.preventDefault();
    const handleDrop = (e: React.DragEvent<HTMLDivElement>) => {
        e.preventDefault();
        if (dragging == undefined) {
            return;
        }

        const dropY = e.clientY;
        const droppedIndex = props.items.findIndex((item) => {
            const itemElement = itemRefs.current.get(item);
            if (!itemElement) {
                return false;
            }
            const rect = itemElement.getBoundingClientRect();
            const taskMiddleY = rect.top + rect.height / 2;
            return dropY < taskMiddleY;
        });

        if (droppedIndex !== -1) {
            const result = [] as T[];
            for(let i = 0; i < props.items.length; ++i) {
                if(i == droppedIndex) {
                    result.push(dragging);
                }
                let item = props.items[i];
                if(item == dragging) {
                    continue;
                }
                result.push(item);
            }
            props.onOrderChanged(result);
        }
        setDragging(undefined);
    }

    return (
    <div onDragOver={handleDragOver} onDrop={handleDrop}>
        {
            props.items.map(i => props.render(i, {
                draggable: true,
                onDragStart: () => setDragging(i),
                ref: (el: TElement) => {
                    itemRefs.current.set(i, el);
                },
            }))
        }
    </div>
    )
}