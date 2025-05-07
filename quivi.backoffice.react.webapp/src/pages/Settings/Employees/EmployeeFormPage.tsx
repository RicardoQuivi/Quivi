import { useTranslation } from "react-i18next";
import { useNavigate, useParams } from "react-router";
import { useToast } from "../../../layout/ToastProvider";
import { useMemo } from "react";
import PageMeta from "../../../components/common/PageMeta";
import PageBreadcrumb from "../../../components/common/PageBreadCrumb";
import ComponentCard from "../../../components/common/ComponentCard";
import { LocalForm, EmployeeFormState } from "./EmployeeForm";
import { useEmployeeMutator } from "../../../hooks/mutators/useEmployeeMutator";
import { useEmployeesQuery } from "../../../hooks/queries/implementations/useEmployeesQuery";

export const EmployeeFormPage = () => {
    const { id } = useParams();
    const { t } = useTranslation();
    const navigate = useNavigate();
    const toast = useToast();
    const mutator = useEmployeeMutator();

    const title = t(`common.operations.${id == undefined ? 'new' : 'edit'}`, {
        name: t("common.entities.employee")
    });

    const employeesQuery = useEmployeesQuery(id == undefined ? undefined : {
        ids: [ id ],
        page: 0,
    })

    const employee = useMemo(() => {
        if(id == undefined) {
            return undefined;
        }
        if(employeesQuery.data.length == 0) {
            return undefined;
        }
        return employeesQuery.data[0];
    }, [id, employeesQuery.data])

    const submit = async (state: EmployeeFormState) => {
        if(employee == undefined) {
            await mutator.create({
                name: state.name,
                inactivityLogoutTimeout: state.inactivityPeriod,
                restrictions: state.restrictions,
            })
            toast.success(t("common.operations.success.new"));
        } else {
            await mutator.patch(employee, {
                name: state.name,
                inactivityLogoutTimeout: state.inactivityPeriod,
                restrictions: state.restrictions,
            })
            toast.success(t("common.operations.success.edit"));
        }
        navigate("/settings/employees")
    }
    
    return <>
        <PageMeta
            title={t("pages.employees.title")}
            description={t("quivi.product.description")}
        />
        <PageBreadcrumb
            pageTitle={t("pages.employees.title")}
            breadcrumbs={[
                {
                    title: t("pages.employees.title"),
                    to: "/settings/employees",
                }
            ]}
            breadcrumb={title}
        />

        <ComponentCard title={title}>
            <LocalForm
                model={employee}
                onSubmit={submit}
                submitText={title}
            />
        </ComponentCard>
    </>
}