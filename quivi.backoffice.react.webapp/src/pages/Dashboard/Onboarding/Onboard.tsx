import { useMemo } from "react";
import { useTranslation } from "react-i18next";
import { useMerchantsQuery } from "../../../hooks/queries/implementations/useMerchantsQuery";
import TaskItem, { Task, TaskType } from "./TaskItem";
import { useChannelsQuery } from "../../../hooks/queries/implementations/useChannelsQuery";
import { useChannelProfilesQuery } from "../../../hooks/queries/implementations/useChannelProfilesQuery";
import { useNavigate } from "react-router";
import { useAuthenticatedUser } from "../../../context/AuthContext";
import { useEmployeesQuery } from "../../../hooks/queries/implementations/useEmployeesQuery";
import { useLocalsQuery } from "../../../hooks/queries/implementations/useLocalsQuery";
import { useCustomChargeMethodsQuery } from "../../../hooks/queries/implementations/useCustomChargeMethodsQuery";
import { Spinner } from "../../../components/spinners/Spinner";

export const Onboarding = () => {
    const { t } = useTranslation();
    const navigate = useNavigate();
    
    const user = useAuthenticatedUser();
    
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
    
    const localsQuery = useLocalsQuery({
        page: 0,
        pageSize: 0,
    })

    const customChargeMethodsQuery = useCustomChargeMethodsQuery({
        page: 0,
        pageSize: 0,
    })

    const employeesQuery = useEmployeesQuery({
        page: 0,
        pageSize: 0,
    })

    const tasks = useMemo(() => {
        const result = [] as Task[];

        const createMerchant = {
            id: "createMerchant",
            isChecked: user.merchantActivated == true || merchantsQuery.totalItems != 0,
            isLoading: merchantsQuery.isFirstLoading && user.subMerchantId != undefined,
            title: t("pages.onboarding.createMerchant."),
            description: t("pages.onboarding.createMerchant.description"),
            type: TaskType.Required,
            onClick: () => navigate("/businessProfile/merchant/setup"),
        };
        result.push(createMerchant);

        const activateMerchant = {
            id: "activateMerchant",
            isChecked: user.merchantActivated == true,
            isLoading: false,
            title: t("pages.onboarding.termsAndConditions."),
            description: t("pages.onboarding.termsAndConditions.description"),
            type: TaskType.Required,
            onClick: () => navigate("/termsAndConditions"),
            requires: [createMerchant],
        };
        result.push(activateMerchant);

        const createChannelProfile = {
            id: "createChannelProfile",
            isChecked: channelProfilesQuery.totalItems != 0,
            isLoading: channelProfilesQuery.isFirstLoading && user.subMerchantId != undefined,
            title: t("pages.onboarding.createChannelProfile."),
            description: t("pages.onboarding.createChannelProfile.description"),
            type: TaskType.Required,
            onClick: () => navigate("/businessProfile/channelprofiles/add"),
            requires: [activateMerchant],
        }
        result.push(createChannelProfile);

        result.push({
            id: "createChannel",
            isChecked: channelsQuery.totalItems != 0,
            isLoading: channelsQuery.isFirstLoading && user.subMerchantId != undefined,
            title: t("pages.onboarding.createChannel."),
            description: t("pages.onboarding.createChannel.description"),
            type: TaskType.Required,
            onClick: () => navigate("/businessProfile/channels"),
            requires: [createChannelProfile],
        });

        result.push({
            id: "createLocal",
            isChecked: localsQuery.totalItems != 0,
            isLoading: localsQuery.isFirstLoading && user.subMerchantId != undefined,
            title: t("pages.onboarding.createLocal."),
            description: t("pages.onboarding.createLocal.description"),
            type: TaskType.Optional,
            onClick: () => navigate("/settings/locals"),
            requires: [activateMerchant],
        });

        result.push({
            id: "createChargeMethods",
            isChecked: customChargeMethodsQuery.totalItems != 0,
            isLoading: customChargeMethodsQuery.isFirstLoading && user.subMerchantId != undefined,
            title: t("pages.onboarding.createCustomChargeMethod."),
            description: t("pages.onboarding.createCustomChargeMethod.description"),
            type: TaskType.Optional,
            onClick: () => navigate("/settings/chargemethods"),
            requires: [activateMerchant],
        });

        result.push({
            id: "createEmployee",
            isChecked: employeesQuery.totalItems != 0,
            isLoading: employeesQuery.isFirstLoading && user.subMerchantId != undefined,
            title: t("pages.onboarding.createEmployee."),
            description: t("pages.onboarding.createEmployee.description"),
            type: TaskType.Optional,
            onClick: () => navigate("/settings/employees"),
            requires: [activateMerchant],
        });

        return result;
    }, [
        merchantsQuery.totalItems, merchantsQuery.isLoading,
        channelProfilesQuery.totalItems, channelProfilesQuery.isLoading,
        channelsQuery.totalItems, channelsQuery.isLoading,
        localsQuery.totalItems, localsQuery.isLoading,
        customChargeMethodsQuery.totalItems, customChargeMethodsQuery.isLoading,
        employeesQuery.totalItems, employeesQuery.isLoading,
        user.merchantActivated,
    ])
    
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
                        {t("pages.onboarding.todo")}
                        <span
                            className="inline-flex rounded-full gap-2 px-2 py-0.5 text-theme-xs font-medium bg-gray-100 text-gray-700 dark:bg-white/[0.03] dark:text-white/80"
                        >
                            { tasks.filter(f => f.isLoading == false && f.isChecked == false).length }
                            {
                                tasks.find(f => f.isLoading) != undefined &&
                                <Spinner />
                            }
                        </span>
                    </h3>
                </div>
                {tasks.map(task => (
                    <TaskItem key={task.id} {...task} />
                ))}
            </div>
        </div>
    </div>
    );
}