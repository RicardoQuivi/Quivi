import { useMemo } from "react";

export interface IValidatorService {
    validatePhone(value: string): boolean;
    validateEmail(value: string): boolean;
    validatePortugueseVat(value: string): boolean;
}

class ValidatorService implements IValidatorService {
    validatePhone(value: string): boolean {
        const regEx: RegExp = /^\+\d{1,3}\d{6,}$/;
        return regEx.test(value);
    }
    validateEmail(value: string): boolean {
        const regEx: RegExp = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
        return regEx.test(value);
    }
    validatePortugueseVat(value: string): boolean {
        const nifPattern = /^[1235689][0-9]{8}$/;
        if (!nifPattern.test(value)) {
            return false;
        }

        let checkSum = 0;
        for (let i = 0; i < 8; i++) {
            checkSum += Number(value[i]) * (9 - i);
        }

        const checkDigit = 11 - (checkSum % 11);
        if (checkDigit >= 10) {
            return Number(value[8]) === 0;
        }

        return Number(value[8]) === checkDigit;
    }
}

const validatorService: IValidatorService = new ValidatorService();
const useValidatorService = () => {
    const result = useMemo(() => validatorService, [])
    return result;
}
export default useValidatorService;