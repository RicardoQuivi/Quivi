import { useTranslation } from "react-i18next";
import { useNavigate, useParams } from "react-router";
import { useToast } from "../../../layout/ToastProvider";
import { useLocalMutator } from "../../../hooks/mutators/useLocalMutator";
import { useLocalsQuery } from "../../../hooks/queries/implementations/useLocalsQuery";
import { useMemo } from "react";
import PageMeta from "../../../components/common/PageMeta";
import PageBreadcrumb from "../../../components/common/PageBreadCrumb";
import ComponentCard from "../../../components/common/ComponentCard";
import { LocalForm, LocalFormState } from "./LocalForm";

export const LocalFormPage = () => {
    const { id } = useParams();
    const { t } = useTranslation();
    const navigate = useNavigate();
    const toast = useToast();
    const mutator = useLocalMutator();

    const title = t(`common.operations.${id == undefined ? "new" : "edit"}`, {
        name: t("common.entities.local")
    });

    const localsQuery = useLocalsQuery(id == undefined ? undefined : {
        ids: [ id ],
        page: 0,
    })

    const local = useMemo(() => {
        if(id == undefined) {
            return undefined;
        }
        if(localsQuery.data.length == 0) {
            return undefined;
        }
        return localsQuery.data[0];
    }, [id, localsQuery.data])

    const submit = async (state: LocalFormState) => {
        if(local == undefined) {
            await mutator.create({
                name: state.name,
            })
            toast.success(t("common.operations.success.new"));
        } else {
            await mutator.patch(local, {
                name: state.name,
            })
            toast.success(t("common.operations.success.edit"));
        }
        navigate("/settings/locals")
    }
    
    return <>
        <PageMeta
            title={t("pages.locals.title")}
            description={t("quivi.product.description")}
        />
        <PageBreadcrumb
            pageTitle={t("pages.locals.title")}
            breadcrumbs={[
                {
                    title: t("pages.locals.title"),
                    to: "/settings/locals",
                }
            ]}
            breadcrumb={title}
        />

        <ComponentCard title={title}>
            <LocalForm
                model={local}
                onSubmit={submit}
                submitText={t(`common.operations.save`, {
                    name: t("common.entities.local")
                })}
                isLoading={id != undefined && local == undefined}
            />
        </ComponentCard>
    </>
}