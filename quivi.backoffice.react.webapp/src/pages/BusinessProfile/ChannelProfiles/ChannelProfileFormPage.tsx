import { useTranslation } from "react-i18next"
import PageMeta from "../../../components/common/PageMeta";
import PageBreadcrumb from "../../../components/common/PageBreadCrumb";
import ComponentCard from "../../../components/common/ComponentCard";
import { ChannelProfileForm, QrCodeProfileFormState } from "./ChannelProfileForm";
import { useChannelProfilesQuery } from "../../../hooks/queries/implementations/useChannelProfilesQuery";
import { useMemo } from "react";
import { useChannelProfileMutator } from "../../../hooks/mutators/useChannelProfileMutator";
import { useNavigate, useParams } from "react-router";
import { useToast } from "../../../layout/ToastProvider";

export const ChannelProfileFormPage = () => {
    const { id } = useParams();
    const navigate = useNavigate();
    const { t } = useTranslation();
    const toast = useToast();

    const mutator = useChannelProfileMutator();
    
    const profilesQuery = useChannelProfilesQuery(id == undefined ? undefined : {
        ids: [ id ],
        page: 0,
    })

    const profile = useMemo(() => {
        if(id == undefined) {
            return undefined;
        }
        if(profilesQuery.data.length == 0) {
            return undefined;
        }
        return profilesQuery.data[0];
    }, [id, profilesQuery.data])

    const submit = async (state: QrCodeProfileFormState) => {
        if(profile == undefined) {
            await mutator.create({
                name: state.name,
                minimumPrePaidOrderAmount: state.minimumPrePaidOrderAmount,
                features: state.features,
                posIntegrationId: state.posIntegrationId,
                sendToPreparationTimer: state.sendToPreparationTimer ?? undefined,
            })
            toast.success(t("common.operations.success.new"));
        } else {
            await mutator.patch(profile, {
                name: state.name,
                minimumPrePaidOrderAmount: state.minimumPrePaidOrderAmount,
                features: state.features,
                posIntegrationId: state.posIntegrationId,
                sendToPreparationTimer: state.sendToPreparationTimer ?? undefined,
            })
            toast.success(t("common.operations.success.edit"));
        }
        navigate("/businessProfile/channelprofiles")
    }

    const title = t(`common.operations.${id == undefined ? "new" : "edit"}`, {
        name: t("common.entities.channelProfile")
    });

    return <>
        <PageMeta
            title={t("pages.channelProfiles.title")}
            description={t("quivi.product.description")}
        />
        <PageBreadcrumb
            pageTitle={t("pages.channelProfiles.title")}
            breadcrumbs={[
                {
                    title: t("pages.channelProfiles.title"),
                    to: "/businessProfile/channelprofiles",
                }
            ]}
            breadcrumb={title}
        />

        <ComponentCard title={title}>
            <ChannelProfileForm
                model={profile}
                onSubmit={submit}
                submitText={t(`common.operations.save`, {
                    name: t("common.entities.channelProfile")
                })}
            />
        </ComponentCard>
    </>
}