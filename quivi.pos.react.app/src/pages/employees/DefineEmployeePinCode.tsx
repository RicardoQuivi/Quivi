import { useEffect, useState } from "react"
import { PinCodeInput } from "./PinCodeInput";
import { Alert, Fade, Step, StepLabel, Stepper, } from "@mui/material";
import { useTranslation } from "react-i18next";
import { useEmployeesApi } from "../../hooks/api/useEmployeesApi";
import { useEmployeeManager } from "../../context/employee/EmployeeContextProvider";
import { LoadingAnimation } from "../../components/Loadings/LoadingAnimation";

enum PinCodeStep {
    InsertingPinCode,
    InsertingConfirmation,
    Loading
}

interface Props {
    readonly employeeId: string;
    readonly onComplete: (error: boolean) => any; 
}
export const DefineEmployeePinCode = (props: Props) => {
    const { t } = useTranslation();
    const api = useEmployeesApi();
    const employeeManager = useEmployeeManager();

    const [pinCode, setPinCode] = useState<string>("");
    const [confirmationPinCode, setConfirmationPinCode] = useState<string>("");
    const [pinsDontMatch, setPinsDontMatch] = useState(false);
    const [step, setStep] = useState(PinCodeStep.InsertingPinCode);
    const [isLoading, setIsLoading] = useState(false);

    const steps = [
        t("definePinCode"),
        t("confirmPin"),
    ];

    const goTo = (step: number) => {
        switch(step)
        {
            case 0: {
                setPinCode("");
                setConfirmationPinCode("");
                break;
            }
            case 1: setConfirmationPinCode("");
        }
    }

    useEffect(() => {
        if(step != PinCodeStep.InsertingPinCode) {
            return;
        }

        if(pinCode.length != 4) {
            return;
        }

        const timeout = setTimeout(() => setStep(PinCodeStep.InsertingConfirmation), 1000);
        return () => clearTimeout(timeout);
    }, [step, pinCode]);

    useEffect(() => {
        if(step != PinCodeStep.InsertingConfirmation) {
            return;
        }

        if(confirmationPinCode.length != 4) {
            return;
        }

        const timeout = setTimeout(() => setStep(PinCodeStep.Loading), 1000);
        return () => clearTimeout(timeout);
    }, [step, confirmationPinCode]);

    useEffect(() => {
        if(step != PinCodeStep.Loading) {
            return;
        }

        if(confirmationPinCode.length != 4) {
            return;
        }

        
        if(pinCode.length != 4) {
            return;
        }

        if(confirmationPinCode != pinCode) {
            setPinCode("");
            setConfirmationPinCode("");
            setPinsDontMatch(true);
            setStep(PinCodeStep.InsertingPinCode);
            return;
        }
        
        updatePin();
    }, [step, confirmationPinCode]);

    useEffect(() => {
        if(pinsDontMatch == false) {
            return;
        }

        const timeout = setTimeout(() => setPinsDontMatch(false), 5000);
        return () => clearTimeout(timeout);
    }, [pinsDontMatch])

    const updatePin = async () => {
        try {
            setIsLoading(true);
            await api.updatePinCode({
                id: props.employeeId,
                pincode: pinCode,
            });
            await employeeManager.login(props.employeeId, pinCode);
            props.onComplete(false);
        } catch {
            props.onComplete(true);
        } finally {
            setIsLoading(false);
        }
    }

    const getActiveStep = () => {
        switch(step){
            case PinCodeStep.InsertingPinCode: return 0;
            case PinCodeStep.InsertingConfirmation: return 1;
            case PinCodeStep.Loading: return 1;
        }
    }

    const onDigitPress = (button: string) => {
        switch(step)
        {
            case PinCodeStep.InsertingPinCode: 
                if(button == "Backspace") {
                    return;
                }
                if(pinCode.length != 4) {
                    return;
                }
                setStep(PinCodeStep.InsertingConfirmation);
                setConfirmationPinCode(button);
                return;
            case PinCodeStep.InsertingConfirmation:
                if(confirmationPinCode.length != 4) {
                    if(confirmationPinCode.length == 0 && button == "Backspace") {
                        setStep(PinCodeStep.InsertingPinCode);
                        setPinCode(p => p.substring(0, 3));
                    }
                    return;
                }

                if(button == "Backspace") {
                    return;
                }

                setStep(PinCodeStep.Loading);
                return;
        }
    }

    return <>
        <Stepper activeStep={getActiveStep()} alternativeLabel className="mb-4">
        {
            steps.map((label, index) => (
                <Step key={index} onClick={() => goTo(index)} style={{ cursor: "pointer" }}>
                    <StepLabel>{label}</StepLabel>
                </Step>
            ))
        }
        </Stepper>
        <Fade in={pinsDontMatch}>
            <Alert severity="error" className="mb-4">
                {t("pinCodesDontMatch")}
            </Alert>
        </Fade>
        { step == PinCodeStep.InsertingPinCode && <PinCodeInput pin={pinCode} onChange={(p) => setPinCode(p)} onDigitPress={onDigitPress} /> }
        { step == PinCodeStep.InsertingConfirmation && <PinCodeInput pin={confirmationPinCode} onChange={(p) => setConfirmationPinCode(p)} onDigitPress={onDigitPress} loading={isLoading} /> }
        { step == PinCodeStep.Loading && <LoadingAnimation /> }
    </>
}