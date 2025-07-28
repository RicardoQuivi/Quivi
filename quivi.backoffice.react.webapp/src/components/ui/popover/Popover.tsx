import { useState, useRef, useEffect, ReactNode } from "react";
import { Popover as TinyPopover } from 'react-tiny-popover'

type Position = "top" | "right" | "bottom" | "left";

interface PopoverProps {
    readonly position: Position;
    readonly trigger: React.ReactNode;
    readonly children: ReactNode;
}

export default function Popover({ position, trigger, children }: PopoverProps) {
    const [isOpen, setIsOpen] = useState(false);
    const popoverRef = useRef<HTMLDivElement>(null);
    const triggerRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        function handleClickOutside(event: MouseEvent) {
            if (
                popoverRef.current &&
                !popoverRef.current.contains(event.target as Node) &&
                triggerRef.current &&
                !triggerRef.current.contains(event.target as Node)
            ) {
                setIsOpen(false);
            }
        }

        document.addEventListener("mousedown", handleClickOutside);
        return () => document.removeEventListener("mousedown", handleClickOutside);
    }, []);

    return (
        <div className="relative inline-block">
            <TinyPopover
                isOpen={isOpen}
                positions={[position]}
                transform={{ top: -20, left: 0 }}
                transformMode='relative'
                onClickOutside={() => setIsOpen(false)}
                content={(
                    <div 
                        className="w-full bg-white rounded-xl shadow-theme-lg dark:bg-[#1E2634]"
                        onClick={() => setIsOpen(false)}
                    >
                        {children}
                    </div>
                )}
            >
                <div
                    ref={triggerRef}
                    onClick={() => setIsOpen(s => !s)}
                >
                    {trigger}
                </div>
            </TinyPopover>
        </div>
    );
}
