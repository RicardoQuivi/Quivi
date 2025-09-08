import * as React from 'react';
import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { InvalidModelResponse, ValidationErrorCode } from '../../hooks/api/exceptions/InvalidModelResponse';
import { Alert } from '@mui/material';

function camelCase(str: string) {
    // Using replace method with regEx
    return str.replace(/(?:^\w|[A-Z]|\b\w)/g, function (word, index) {
        return index == 0 ? word.toLowerCase() : word.toUpperCase();
    }).replace(/\s+/g, '');
}

interface Props {
    readonly errorMessages: InvalidModelResponse[];
    readonly propertyPath: string | RegExp;
    readonly onErrorMessage?: (e: ValidationErrorCode | undefined, defaultMessage: string) => string;
}

const ValidationMessage: React.FC<Props> = ({
    errorMessages,
    propertyPath,
    onErrorMessage
}) => {
    const { t } = useTranslation();
    const [isInvalid, setIsInvalid] = useState(false);

    useEffect(() => {
        for (let i = 0; i < errorMessages.length; ++i) {
            let error = errorMessages[i];

            if (!memberNamesMatchValidator([error.property]))
                continue;

            setIsInvalid(true);
            return;
        }
        setIsInvalid(false);
    }, [errorMessages, propertyPath])

    const memberNamesMatchValidator = (memberNames: string[]) =>{
        if(!!(propertyPath as string)) {
            return memberNames.some(name => camelCase(name) == propertyPath);
        }
        if(!!(propertyPath as RegExp)) {
            return memberNames.some(name => camelCase(name).match(propertyPath as RegExp));
        }
        return false;
    }

    const getDefaultMessage = (e: InvalidModelResponse) => {
        if(e.errorCode == undefined) {
            return e.errorMessage;
        }

        const result = t(`EnumResources.ValidationError_${e.errorCode.toString()}`);
        return !result ? e.errorMessage : result;
    }

    const getErrorMessage = (e: InvalidModelResponse) => {
        const defaultMessage = getDefaultMessage(e);
        if(!!onErrorMessage) {
            return onErrorMessage(e.errorCode, defaultMessage);
        }
        return defaultMessage;
    }
   
    return isInvalid 
    ?
    <Alert variant="outlined" severity="warning" sx={{ width: "100%", }}>
        {errorMessages.filter(e => memberNamesMatchValidator([e.property])).slice(0,1).map((e, i) => <span key={i} >{getErrorMessage(e)}</span>)}
    </Alert>
    : 
    <></>
}
export default ValidationMessage;