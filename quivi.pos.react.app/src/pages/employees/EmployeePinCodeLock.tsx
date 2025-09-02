import { useEffect, useState } from "react"
import { useTranslation } from "react-i18next";
import { Employee } from "../../hooks/api/Dtos/employees/Employee";
import { useToast } from "../../context/ToastProvider";
import { useEmployeeManager } from "../../context/employee/EmployeeContextProvider";
import { PinCodeInput } from "./PinCodeInput";

interface Props {
    readonly employee: Employee;
}
export const EmployeePinCodeLock = (props: Props) => {
    const { t } = useTranslation();
    const toast = useToast();
    const employeeManager = useEmployeeManager();

    const [state, setState] = useState({
        pinCode: "",
        isLoading: false,
    })

    const loginWithPinCode = async () => {
        try {
            setState(s => ({...s, isLoading: true}))
            await employeeManager.login(props.employee.id, state.pinCode);
        } catch {
            toast.error(t('invalidPinDescription'));
            setState(s => ({...s, isLoading: false, pinCode: ""}))
        }
    }

    useEffect(() => {
        if(state.pinCode.length != 4) {
            return;
        }

        loginWithPinCode();
    }, [state.pinCode])
    
    return <PinCodeInput
        pin={state.pinCode}
        onChange={(p) => setState(s => ({...s, pinCode: p}))}
        loading={state.isLoading}
    />
}