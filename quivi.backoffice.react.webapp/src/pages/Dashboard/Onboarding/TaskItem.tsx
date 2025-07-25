import { useTranslation } from "react-i18next";
import Badge from "../../../components/ui/badge/Badge";
import { HandleIcon } from "../../../icons";
import { Spinner } from "../../../components/spinners/Spinner";
import { Tooltip } from "../../../components/ui/tooltip/Tooltip";
import { useMemo } from "react";

export enum TaskType {
    Required,
    Optional,
}

export interface Task {
    readonly id: string;
    readonly title: string;
    readonly description: string;
    readonly isChecked: boolean;
    readonly isLoading: boolean;
    readonly type: TaskType;
    readonly onClick: () => any;
    readonly requires?: Task[];
}
  
const getColor = (type: TaskType): "error" | "light" => {
    switch(type) {
        case TaskType.Required: return "error";
        case TaskType.Optional: return "light"
    }
}

const getMessage = (type: TaskType): string => {
    switch(type) {
        case TaskType.Required: return "common.required";
        case TaskType.Optional: return "common.optional";
    }
}

const getPendingTasks = (task: Task): Task[] => {
    const result = [] as Task[];
    for(const t of task.requires ?? []) {
        const pendingTasks = getPendingTasks(t);
        result.push(...pendingTasks);
        if(t.isChecked == false) {
            result.push(t);
        }
    }
    
    return result;
}
const TaskItem = (props: Task) => {
    const { t } = useTranslation();

    const pendingRequiredTasks = useMemo(() => getPendingTasks(props), [props.requires])

    const aux = (
        <div
            id={`task-${props.id}`}
            className="p-5 mb-4 bg-white border border-gray-200 task rounded-xl shadow-theme-sm dark:border-gray-800 dark:bg-white/5"
            style={{
                cursor: "pointer",
                pointerEvents: pendingRequiredTasks.length > 0 ? "none" : undefined,
                opacity: pendingRequiredTasks.length > 0 ? 0.5 : undefined,
            }}
            onClick={props.isChecked ? undefined : props.onClick}
        >
            <div className="flex flex-col gap-5 xl:flex-row xl:items-center xl:justify-between">
                <div className="flex items-start w-full gap-4">
                    <span className="text-gray-400">
                        <HandleIcon />
                    </span>

                    <label
                        htmlFor={`taskCheckbox${props.id}`}
                        className="w-full cursor-pointer"
                    >
                        <div className="relative flex items-start">
                            {
                                props.isLoading
                                ?
                                <div className="flex items-center justify-center w-full h-5 mr-3 rounded-md box max-w-5">
                                    <Spinner />
                                </div>
                                :
                                props.isChecked &&
                                <>
                                    <input
                                        type="checkbox"
                                        id={`taskCheckbox${props.id}`}
                                        className="sr-only taskCheckbox cursor-none"
                                        checked={props.isChecked}
                                        onChange={() => {}}
                                    />
                                    <div className="flex items-center justify-center w-full h-5 mr-3 border border-gray-300 rounded-md box max-w-5 dark:border-gray-700">
                                        <span className={`opacity-${props.isLoading == false && props.isChecked ? "100" : "0"}`}>
                                            <svg
                                                width="14"
                                                height="14"
                                                viewBox="0 0 14 14"
                                                fill="none"
                                                xmlns="http://www.w3.org/2000/svg"
                                            >
                                                <path
                                                    d="M11.6668 3.5L5.25016 9.91667L2.3335 7"
                                                    stroke="white"
                                                    strokeWidth="1.94437"
                                                    strokeLinecap="round"
                                                    strokeLinejoin="round"
                                                />
                                            </svg>
                                        </span>
                                    </div>
                                </>
                            }
                            <p className="-mt-0.5 text-base text-gray-800 dark:text-white/90">
                                {props.title}
                            </p>
                        </div>
                    </label>
                </div>

                <div className="flex flex-col-reverse items-start justify-end w-full gap-3 xl:flex-row xl:items-center xl:gap-5">
                    <Badge variant="light" color={getColor(props.type)} size="sm">
                        {t(getMessage(props.type))}
                    </Badge>

                    <div className="flex items-center justify-between w-full gap-5 xl:w-auto xl:justify-normal">
                        <div className="h-6 w-full max-w-6 overflow-hidden rounded-full border-[0.5px] border-gray-200 dark:border-gray-800">
                        </div>
                    </div>
                </div>
            </div>

            <div className="flex flex-col gap-5 xl:flex-row xl:items-center xl:justify-between mt-5">
                <div className="flex items-start w-full gap-4">
                    <label
                        className="w-full cursor-pointer"
                    >
                        <div className="relative flex items-start">
                            <input
                                type="checkbox"
                                className="sr-only taskCheckbox cursor-none hidden"
                                checked={props.isChecked}
                                onChange={() => {}}
                            />
                            <p className="-mt-0.5 text-base text-gray-800 dark:text-white/90 text-sm">
                                {props.description} 
                            </p>
                        </div>
                    </label>
                </div>

                <div className="flex flex-col-reverse items-start justify-end w-full gap-3 xl:flex-row xl:items-center xl:gap-5">
                    <div className="flex items-center justify-between w-full gap-5 xl:w-auto xl:justify-normal">
                        <div className="flex items-center gap-3">
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );

    if(pendingRequiredTasks.length > 0) {
        return <Tooltip 
            message={<div
                className="text-sm"
            >
                {t("pages.onboarding.taskRequires")}
                <div className="rounded-lg sm:w-fit">
                    <ul className="flex flex-col">
                    {
                        pendingRequiredTasks.map(r => (
                            <li key={r.id} className="flex items-center gap-2 px-3 text-sm last:border-b-0 dark:border-gray-800 dark:text-gray-400 text-sm">
                                <span className="ml-2 block h-[3px] w-[3px] rounded-full bg-gray-500 dark:bg-gray-400"></span>
                                <span>{r.title}</span>
                            </li>
                        ))
                    }
                    </ul>
                </div>
            </div>}
            className="w-full"
        >
            {aux}
        </Tooltip>
    }
    return aux;
};
export default TaskItem;