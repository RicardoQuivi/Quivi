import { useTranslation } from "react-i18next";
import { useNavigate, useParams } from "react-router";
import { useToast } from "../../../layout/ToastProvider";
import { useMemo } from "react";
import PageMeta from "../../../components/common/PageMeta";
import PageBreadcrumb from "../../../components/common/PageBreadCrumb";
import ComponentCard from "../../../components/common/ComponentCard";
import { AvailabilityForm, AvailabilityFormState } from "./AvailabilityForm";
import { useAvailabilitiesQuery } from "../../../hooks/queries/implementations/useAvailabilitiesQuery";
import { useAvailabilityMutator } from "../../../hooks/mutators/useAvailabilityMutator";

export const AvailabilityFormPage = () => {
    const { id } = useParams();
    const { t } = useTranslation();
    const navigate = useNavigate();
    const toast = useToast();
    const mutator = useAvailabilityMutator();

    const title = t(`common.operations.${id == undefined ? "new" : "edit"}`, {
        name: t("common.entities.availability")
    });

    const availabilitiesQuery = useAvailabilitiesQuery(id == undefined ? undefined : {
        ids: [ id ],
        page: 0,
    })

    const availability = useMemo(() => {
        if(id == undefined) {
            return undefined;
        }
        if(availabilitiesQuery.data.length == 0) {
            return undefined;
        }
        return availabilitiesQuery.data[0];
    }, [id, availabilitiesQuery.data])

    const submit = async (state: AvailabilityFormState) => {
        if(availability == undefined) {
            await mutator.create({
                name: state.name,
                autoAddNewMenuItems: state.autoAddNewMenuItems,
                autoAddNewChannelProfiles: state.autoAddNewChannelProfiles,
                weeklyAvailabilities: state.weeklyAvailabilities,
            })
            toast.success(t("common.operations.success.new"));
        } else {
            await mutator.patch(availability, {
                name: state.name,
                autoAddNewMenuItems: state.autoAddNewMenuItems,
                autoAddNewChannelProfiles: state.autoAddNewChannelProfiles,
                weeklyAvailabilities: state.weeklyAvailabilities,
            })
            toast.success(t("common.operations.success.edit"));
        }
        navigate("/businessProfile/availabilities")
    }
    
    return <>
        <PageMeta
            title={t("pages.availabilities.title")}
            description={t("quivi.product.description")}
        />
        <PageBreadcrumb
            pageTitle={t("pages.availabilities.title")}
            breadcrumbs={[
                {
                    title: t("pages.availabilities.title"),
                    to: "/businessProfile/availabilities",
                }
            ]}
            breadcrumb={title}
        />

        <ComponentCard title={title}>
            <AvailabilityForm
                model={availability}
                isLoading={id != undefined && availability == undefined}
                onSubmit={submit}
                submitText={t(`common.operations.save`, {
                    name: t("common.entities.availability")
                })}
            />
        </ComponentCard>
    </>
}