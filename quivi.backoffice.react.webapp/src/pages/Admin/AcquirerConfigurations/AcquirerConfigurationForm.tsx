import * as yup from 'yup';
import { useTranslation } from 'react-i18next';
import { useEffect, useMemo, useState } from 'react';
import { useQuiviForm } from '../../../hooks/api/exceptions/useQuiviForm';
import Button from '../../../components/ui/button/Button';
import { useToast } from '../../../layout/ToastProvider';
import { AcquirerConfiguration } from '../../../hooks/api/Dtos/acquirerconfigurations/AcquirerConfiguration';
import { ToggleSwitch } from '../../../components/inputs/ToggleSwitch';
import { Spinner } from '../../../components/spinners/Spinner';
import { ChargePartner } from '../../../hooks/api/Dtos/acquirerconfigurations/ChargePartner';
import { TextField } from '../../../components/inputs/TextField';
import { ChargeMethod } from '../../../hooks/api/Dtos/ChargeMethod';
import { SingleSelect } from '../../../components/inputs/SingleSelect';

const schema = yup.object<AcquirerConfigurationFormState>({
});

export interface PaybyrdState {
    readonly apiKey: string;
}
export interface PaybyrdTerminalState extends PaybyrdState {
    readonly terminalId: string;
}

type States = PaybyrdState | PaybyrdTerminalState;

export interface AcquirerConfigurationFormState {
    readonly isActive: boolean;
    readonly states: Record<ChargePartner, Record<ChargeMethod, States>>;
    readonly partner: ChargePartner;
    readonly method: ChargeMethod;
}

const getState = (model: AcquirerConfiguration | undefined) => {
    const states = {} as Record<ChargePartner, Record<ChargeMethod, States>>;
    const partner = model?.partner ?? ChargePartner.Quivi;
    if(partner == ChargePartner.Quivi) {
        
    } else if(partner == ChargePartner.Paybyrd) {
        let partnerSettings = model?.settings[ChargePartner[ChargePartner.Paybyrd]];
        let method = model?.method ?? ChargeMethod.MbWay;
        let settings = partnerSettings?.[ChargeMethod[method]];
        let state: PaybyrdState | PaybyrdTerminalState = {
            apiKey: settings?.apiKey ?? "",
        };

        if(method == ChargeMethod.PaymentTerminal) {
            let aux = state as any;
            aux["terminalId"] = settings?.terminalId ?? "";
            state = aux;
        }

        let result = {} as Record<ChargeMethod, States>;
        result[method] = state;
        states[partner] = result;
    }

    return {
        isActive: model?.isActive ?? true,
        partner: model?.partner ?? ChargePartner.Quivi,
        method: model?.method ?? ChargeMethod.Cash,
        states: states,
    }
}

const getOptions = (partner: ChargePartner) => {
    switch(partner)
    {
        case ChargePartner.Quivi: return [ChargeMethod.Cash];
        case ChargePartner.Paybyrd: return [ChargeMethod.MbWay, ChargeMethod.CreditCard, ChargeMethod.PaymentTerminal];
    }
    throw new Error("Not implemented");
}
interface Props {
    readonly model?: AcquirerConfiguration;
    readonly onSubmit: (state: AcquirerConfigurationFormState) => Promise<any>;
    readonly submitText: string;
}
export const AcquirerConfigurationForm = (props: Props) => {
    const { t } = useTranslation();
    const toast = useToast();
    const [state, setState] = useState(() => ({
        ...getState(props.model),
        apiErrors: [],
    }));

    useEffect(() => setState(_ => ({
        ...getState(props.model),

        apiErrors: [],
    })), [props.model]);

    const availableMethods = useMemo(() => getOptions(state.partner), [state.partner]);
    
    const form = useQuiviForm(state, schema);

    const save = () => form.submit(async () => {
        await props.onSubmit({
            isActive: state.isActive,
            partner: state.partner,
            method: state.method,
            states: state.states,
        })
    }, () => toast.error(t("common.operations.failure.generic")))

    return <>
        <div className="grid grid-cols-1 sm:grid-cols-1 md:grid-cols-2 lg:grid-cols-2 gap-4">
            <div className="col-span-1 lg:col-span-1">
                <div
                    className="grid grid-cols-1 gap-4"
                >
                    <div
                        className="grid grid-cols-2 gap-4"
                    >
                        <SingleSelect
                            label={t("pages.acquirerConfigurations.chargePartner")}
                            value={state.partner}
                            options={[
                                ChargePartner.Quivi,
                                ChargePartner.Paybyrd,
                            ]}
                            getId={e => e.toString()}
                            render={e => ChargePartner[e]}
                            onChange={e => {
                                let method = state.method;
                                const availableOptions = getOptions(e);
                                if(availableOptions.includes(method) == false) {
                                    method = availableOptions[0];
                                }
                                setState(s => ({ ...s, partner: e, method: method, }));
                            }}
                            disabled={props.model != undefined}
                        />

                        <SingleSelect
                            label={t("pages.acquirerConfigurations.chargePartner")}
                            value={state.method}
                            options={availableMethods}
                            getId={e => e.toString()}
                            render={e => ChargeMethod[e]}
                            onChange={e => setState(s => ({ ...s, method: e}))}
                            disabled={props.model != undefined}
                        />
                    </div>
                   
                    <PaybyrdForm 
                        partner={state.partner} 
                        method={state.method}
                        state={state.states[ChargePartner.Paybyrd]} 
                        onChange={state => setState(s => {
                            const result = {...s};
                            result.states[ChargePartner.Paybyrd] = state;
                            return result;
                        })}
                    />

                    <ToggleSwitch
                        label={t("common.active")}
                        value={state.isActive}
                        onChange={v => setState(s => ({ ...s, isActive: v }))}
                        errorMessage={form.touchedErrors.get("isActive")?.message}
                    />
                </div>
            </div>
        </div>

        <Button
            size="md"
            onClick={save}
            disabled={form.isValid == false}
            variant="primary"
        >
            {
                form.isSubmitting
                ?
                <Spinner />
                :
                props.submitText
            }
        </Button>
    </>
}

interface PaybyrdFormProps {
    readonly partner: ChargePartner;
    readonly method: ChargeMethod;
    readonly state?: Record<ChargeMethod, PaybyrdState | PaybyrdTerminalState>;
    readonly onChange: (state: Record<ChargeMethod, PaybyrdState | PaybyrdTerminalState>) => any;

}
const PaybyrdForm = (props: PaybyrdFormProps) => {
    const { t } = useTranslation();
    
    const state = useMemo<PaybyrdState | PaybyrdTerminalState>(() => {
        const result = props.state?.[props.method];
        if(result != undefined) {
            return result;
        }

        if(props.method != ChargeMethod.PaymentTerminal) {
            return {
                apiKey: "",
            }
        }

        return {
            apiKey: "",
            terminalId: "",
        }
    }, [props.method, props.state])

    if(props.partner != ChargePartner.Paybyrd) {
        return <></>
    }

    return <div className="flex flex-col gap-4 flex-1">
        <TextField
            label={t("pages.acquirerConfigurations.paybyrd.apiKey")}
            type="text"
            value={state.apiKey ?? ""}
            onChange={(e) => {
                const result = { ...(props.state ?? {}) } as Record<ChargeMethod, PaybyrdState | PaybyrdTerminalState>;
                result[props.method] = {
                    ...state,
                    apiKey: e,
                }
                props.onChange(result);
            }}
        />
        {
            'terminalId' in state &&
            <TextField
                label={t("pages.acquirerConfigurations.paybyrd.terminalId")}
                type="text"
                value={state.terminalId ?? ""}
                onChange={(e) => {
                    const result = { ...(props.state ?? {}) } as Record<ChargeMethod, PaybyrdState | PaybyrdTerminalState>;
                    result[props.method] = {
                        ...state,
                        terminalId: e,
                    }
                    props.onChange(result);
                }}
            />
        }
    </div>
}