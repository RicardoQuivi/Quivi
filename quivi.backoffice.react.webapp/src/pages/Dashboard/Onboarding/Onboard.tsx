import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { useMerchantsQuery } from "../../../hooks/queries/implementations/useMerchantsQuery";
import TaskItem, { Task, TaskType } from "./TaskItem";
import { useChannelsQuery } from "../../../hooks/queries/implementations/useChannelsQuery";
import { useChannelProfilesQuery } from "../../../hooks/queries/implementations/useChannelProfilesQuery";
import { useNavigate } from "react-router";
import { useAuthenticatedUser } from "../../../context/AuthContext";

export const Onboarding = () => {
    const { t } = useTranslation();
    const navigate = useNavigate();
    
    const user = useAuthenticatedUser();
    const [tasks, setTasks] = useState<Map<string, Task>>(new Map<string, Task>());
    
    const merchantsQuery = useMerchantsQuery({
        page: 0,
        pageSize: 0,
    })

    const channelProfilesQuery = useChannelProfilesQuery({
        page: 0,
        pageSize: 0,
    })

    const channelsQuery = useChannelsQuery({
        page: 0,
        pageSize: 0,
    })
    
    useEffect(() => setTasks(tasks => {
        const result = new Map<string, Task>(tasks);

        if(merchantsQuery.isLoading == false) {
            result.set("createMerchant", {
                id: "createMerchant",
                isChecked: merchantsQuery.totalItems != 0,
                title: t("pages.onboarding.createMerchant."),
                description: t("pages.onboarding.createMerchant.description"),
                type: TaskType.Required,
                onClick: () => navigate("/businessProfile/merchant/setup"),
            });

            result.set("activateMerchant", {
                id: "activateMerchant",
                isChecked: user.merchantActivated == true,
                title: t("pages.onboarding.termsAndConditions."),
                description: t("pages.onboarding.termsAndConditions.description"),
                type: TaskType.Required,
                onClick: () => navigate("/termsAndConditions"),
            });

            if(channelProfilesQuery.isLoading == false) {
                result.set("createChannelProfile", {
                    id: "createChannelProfile",
                    isChecked: channelProfilesQuery.totalItems != 0,
                    title: t("pages.onboarding.createChannelProfile."),
                    description: t("pages.onboarding.createChannelProfile.description"),
                    type: TaskType.Required,
                    onClick: () => navigate("/businessProfile/channelprofiles/add"),
                });

                if(channelsQuery.isLoading == false) {
                    result.set("createChannel", {
                        id: "createChannel",
                        isChecked: channelsQuery.totalItems != 0,
                        title: t("pages.onboarding.createChannel."),
                        description: t("pages.onboarding.createChannel.description"),
                        type: TaskType.Required,
                        onClick: () => navigate("/businessProfile/channels"),
                    });
                }
            }
        }

        return result;
    }), [
        merchantsQuery.totalItems, merchantsQuery.isLoading,
        channelProfilesQuery.totalItems, channelProfilesQuery.isLoading,
        channelsQuery.totalItems, channelsQuery.isLoading,
        user.merchantActivated,
    ])
    
    const taskValues = Array.from(tasks.values());
    return (
    <div className="min-h-screen rounded-2xl border border-gray-200 bg-white px-5 py-7 dark:border-gray-800 dark:bg-white/[0.03] xl:px-10 xl:py-12">
        <div className="mx-auto w-full text-center">
            <h3 className="mb-4 font-semibold text-gray-800 text-theme-xl dark:text-white/90 sm:text-2xl">
                {t("pages.onboarding.title")}
            </h3>

            <p className="text-sm text-gray-500 dark:text-gray-400 sm:text-base">
                {t("pages.onboarding.description")}
            </p>

            <br/>
            <div>
                <div className="flex items-center justify-between mb-2">
                    <h3 className="flex items-center gap-3 text-base font-medium text-gray-800 capitalize dark:text-white/90">
                        {"todo"}
                        <span
                            className="inline-flex rounded-full px-2 py-0.5 text-theme-xs font-medium bg-gray-100 text-gray-700 dark:bg-white/[0.03] dark:text-white/80"
                        >
                            {taskValues.filter(f => f.isChecked == false).length}
                        </span>
                    </h3>
                </div>
                {taskValues.map(task => (
                    <TaskItem key={task.id} {...task} />
                ))}
            </div>
        </div>
    </div>
    );
}