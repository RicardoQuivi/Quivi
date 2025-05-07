import { useTranslation } from "react-i18next";
import { useToast } from "../context/ToastProvider";
import { useEffect, useState } from "react";
import { Employee } from "../hooks/api/Dtos/employees/Employee";
import { EmployeeSelectPage } from "./employees/EmployeeSelectPage";
import { EmployeePinCodeLock } from "./employees/EmployeePinCodeLock";
import { DefineEmployeePinCode } from "./employees/DefineEmployeePinCode";

enum Page {
    SelectEmployee,
    PinCode,
    DefinePinCode,
}
export const LockPage = () => {
    const { t } = useTranslation();
    const toast = useToast();

    const [selectedEmployee, setSelectedEmployee] = useState<Employee>();
    const [currentPage, setCurrentPage] = useState(Page.SelectEmployee);

    const onComplete = (hasError: boolean) => {
        if(hasError == false) {
            return;
        }
        toast.error(t('unexpectedErrorHasOccurred'));
    }

    useEffect(() => setSelectedEmployee(undefined), [])

    useEffect(() => {
        if(selectedEmployee == undefined) {
            setCurrentPage(Page.SelectEmployee);
        } else if(selectedEmployee.hasPinCode) {
            setCurrentPage(Page.PinCode);
        } else {
            setCurrentPage(Page.DefinePinCode);
        }
    }, [selectedEmployee])

    return <>
        { currentPage == Page.SelectEmployee && <EmployeeSelectPage onEmployeeSelect={setSelectedEmployee} /> }
        { currentPage == Page.PinCode && <EmployeePinCodeLock employee={selectedEmployee!} /> }
        { currentPage == Page.DefinePinCode && <DefineEmployeePinCode employee={selectedEmployee!} onComplete={onComplete} />}
    </>
}