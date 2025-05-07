import { useEffect, useMemo, useState } from "react"
import { ApiException } from "./ApiException";
import { useTranslation } from "react-i18next";
import * as Yup from 'yup';

function camelCase(str: string) {
    // Using replace method with regEx
    return str.replace(/(?:^\w|[A-Z]|\b\w)/g, function (word, index) {
        return index == 0 ? word.toLowerCase() : word.toUpperCase();
    }).replace(/\s+/g, '');
}

export interface ErrorMessage {
    readonly message: string;
}

interface FormResult<TModel> {
    readonly submit: (run: () => Promise<void>, onException: (e: unknown) => any) => Promise<void>;
    readonly errors: Map<string, ErrorMessage>;
    readonly touchedErrors: Map<string, ErrorMessage>;
    readonly isValid: boolean;
    readonly isSubmitting: boolean;
    readonly isTouched: (key: keyof TModel) => boolean;
}
export const useQuiviForm = <TModel extends Yup.Maybe<Yup.AnyObject>, TTransformedValues = TModel>(model: TModel, schema: Yup.ObjectSchema<TModel, any, TTransformedValues, any> | ReturnType<typeof Yup.lazy<Yup.ObjectSchema<TModel, any, TTransformedValues, any>>>): FormResult<TModel> => {
    const { t } = useTranslation();

    const [touchedFields, setTouchedFiles] = useState(new Map<keyof TModel, boolean>());
    const [modelAtEntry] = useState({
        ...model,
    });
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [errors, setErrors] = useState(new Map<string, ErrorMessage>());

    useEffect(() => setTouchedFiles((p) => {
        const result = new Map<keyof TModel, boolean>(p);
        for(const entry in model) {
            const fValue = modelAtEntry?.[entry];
            const currentValue = model[entry];

            if(fValue != currentValue) {
                const isTouched = result.get(entry as keyof TModel);
                if(isTouched) {
                    continue;
                }
                result.set(entry as keyof TModel, true);
            }
        }
        return result;
    }), [model]);

    useEffect(() => {
        validate();
    }, [model, schema])

    const validate = (): Promise<boolean> => {
        return schema.validate(model, { abortEarly: false })
                        .then(() => {
                            setErrors(s => {
                                if(s.size == 0) {
                                    return s;
                                }
                                return new Map<string, ErrorMessage>();
                            });
                            return true;
                        })
                        .catch((err) => {
                            setErrors(s => {
                                const errors = new Map<string, ErrorMessage>();
                                err.inner?.forEach((error: Yup.ValidationError) => {
                                    if(error.type == "optionality") {
                                        return;
                                    }

                                    errors.set(error.path ?? "", {
                                        message: getMessage(error),
                                    });
                                });
                                if(s.size == 0 && errors.size == 0) {
                                    return s;
                                }
                                return errors;
                            })
                            return false;
                        });
    }

    const getMessage = (error: Yup.ValidationError): string => {
        if(error.type == undefined) {
            throw new Error("Unkown type? Implement me!");
        }
        switch(error.type)
        {
            case "required": return t("common.errors.required", error.params);
            case "min": return t("common.errors.minLength", error.params);
            case "email": return t("common.errors.notAnEmail", error.params);
        }
        throw new Error(`Unkown type ${error.type}! Implement me!`);
    }

    const result = useMemo((): FormResult<TModel> => {
        const touchedErrors = new Map<string, ErrorMessage>();
        for(const e of errors) {
            const name = e[0];
            const isTouched = touchedFields.get(name as keyof TModel);
            if(isTouched == true) {
                touchedErrors.set(name, e[1]);
            }
        }

        return {
            submit: async (run: () => Promise<void>, onException: (e: unknown) => any): Promise<void> => {
                try {
                    setIsSubmitting(true);
                    const isValid = await validate();
                    if(isValid == false) {
                        throw new Error("Not Valid");
                    }

                    try {
                        await run();
                    } catch (ex) {
                        if(ex instanceof ApiException) {
                            setErrors(() => {
                                const map = new Map<string, ErrorMessage>();
                                for(const e of ex.errors) {
                                    const aux: Record<string, string> = { ...(e.context ?? {}) };
                                    map.set(camelCase(e.property), {
                                        message: t(`common.apiErrors.${e.errorCode}`, aux)
                                    });
                                }
                                return map;
                            });
                            setTouchedFiles((p) => {
                                const result = new Map<keyof TModel, boolean>(p);
                                for(const e of ex.errors) {
                                    const entry = e.property;
                                    const isTouched = result.get(entry as keyof TModel);
                                    if(isTouched) {
                                        continue;
                                    }
                                    result.set(entry as keyof TModel, true);
                                }
                                return result;
                            })
                            return;
                        }
                        onException(ex);
                    }
                } finally {
                    setIsSubmitting(false);
                }
            },
            errors: errors,
            touchedErrors: touchedErrors,
            isValid: errors.size == 0,
            isSubmitting: isSubmitting,
            isTouched: (key: keyof TModel) => {
                const isTouched = touchedFields.get(key);
                return isTouched == true;
            }
        }
    }, [errors, isSubmitting, touchedFields]);

    return result;
}