import * as yup from 'yup';
import { useTranslation } from 'react-i18next';
import { useEffect, useState } from 'react';
import { useQuiviForm } from '../../../hooks/api/exceptions/useQuiviForm';
import Button from '../../../components/ui/button/Button';
import { useToast } from '../../../layout/ToastProvider';
import { Spinner } from '../../../components/spinners/Spinner';
import { IntegrationType, PosIntegration } from '../../../hooks/api/Dtos/posIntegrations/PosIntegration';
import { SingleSelect } from '../../../components/inputs/SingleSelect';
import { useIntegrationHelper } from '../../../utilities/useIntegrationHelper';
import { TextField } from '../../../components/inputs/TextField';
import { ToggleSwitch } from '../../../components/inputs/ToggleSwitch';

const schema = yup.object<PosIntegrationFormState>({
});

export interface QuiviViaFacturaLusaState {
    readonly accessToken: string;
    readonly skipInvoice: boolean;
    readonly invoicePrefix: string;
    readonly includeTipInInvoice: boolean;
}

type States = QuiviViaFacturaLusaState;
export interface PosIntegrationFormState {
    readonly type: IntegrationType;
    readonly states: Record<IntegrationType, States>;
}
const getState = (model: PosIntegration | undefined): PosIntegrationFormState => {
    const states = {} as Record<IntegrationType, States>;
    const type = model?.type ?? IntegrationType.QuiviViaFacturalusa;
    if(type == IntegrationType.QuiviViaFacturalusa) {
        let settings = model?.settings[IntegrationType[IntegrationType.QuiviViaFacturalusa]];
        let state: QuiviViaFacturaLusaState = {
            accessToken: settings?.accessToken ?? "",
            skipInvoice: settings?.skipInvoice ?? true,
            invoicePrefix: settings?.invoicePrefix ?? "",
            includeTipInInvoice: settings?.includeTipInInvoice ?? false,
        };
        states[type] = state;
    }

    return {
        type: type,
        states: states,
    }
}

interface Props {
    readonly model?: PosIntegration;
    readonly onSubmit: (state: PosIntegrationFormState) => Promise<any>;
    readonly submitText: string;
}
export const PosIntegrationForm = (props: Props) => {
    const { t } = useTranslation();
    const toast = useToast();
    const integrationHelper = useIntegrationHelper();
    
    const [state, setState] = useState(() => ({
        ...getState(props.model),
        apiErrors: [],
    }));

    useEffect(() => setState(_ => ({
        ...getState(props.model),

        apiErrors: [],
    })), [props.model]);

    const form = useQuiviForm(state, schema);

    const save = () => form.submit(async () => {
        await props.onSubmit({
            states: state.states,
            type: state.type,
        })
    }, () => toast.error(t("common.operations.failure.generic")))

    return <>
        <div className="grid grid-cols-1 sm:grid-cols-1 md:grid-cols-2 lg:grid-cols-2 gap-4">
            <div className="col-span-1 lg:col-span-1">
                <div
                    className="grid grid-cols-1 gap-4"
                >
                    <SingleSelect
                        label={t("common.type")}
                        value={state.type}
                        options={[
                            IntegrationType.QuiviViaFacturalusa
                        ]}
                        getId={e => e.toString()}
                        render={e => integrationHelper.getStrategyName(e)}
                        onChange={e => setState(s => ({ ...s, type: e}))}
                    />

                    <QuiviFiaFacturaLusaForm 
                        type={state.type}
                        state={state.states[state.type] as QuiviViaFacturaLusaState}
                        onChange={state => setState(s => {
                            const result = {...s};
                            result.states[IntegrationType.QuiviViaFacturalusa] = state;
                            return result;
                        })}
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


interface QuiviFiaFacturaLusaFormProps {
    readonly type: IntegrationType;
    readonly state: QuiviViaFacturaLusaState;
    readonly onChange: (state: QuiviViaFacturaLusaState) => any;

}
const QuiviFiaFacturaLusaForm = (props: QuiviFiaFacturaLusaFormProps) => {
    const { t } = useTranslation();
    
    if(props.type != IntegrationType.QuiviViaFacturalusa) {
        return <></>
    }

    return <div className="flex flex-col gap-4 flex-1">
        <TextField
            label={t("pages.posIntegrations.quiviViaFacturaLusa.accessToken")}
            type="text"
            value={props.state.accessToken}
            onChange={(e) => props.onChange({
                ...props.state,
                accessToken: e,
            })}
        />

        <TextField
            label={t("pages.posIntegrations.quiviViaFacturaLusa.invoicePrefix")}
            type="text"
            value={props.state.invoicePrefix}
            onChange={(e) => props.onChange({
                ...props.state,
                invoicePrefix: e,
            })}
        />

        <div
            className="grid grid-cols-2"
        >
            <ToggleSwitch
                label={t("pages.posIntegrations.quiviViaFacturaLusa.invoicingEnabled")}
                value={!props.state.skipInvoice}
                onChange={(e) => props.onChange({
                    ...props.state,
                    skipInvoice: !e,
                })}
            />

            <ToggleSwitch
                label={t("pages.posIntegrations.quiviViaFacturaLusa.includeTipInInvoice")}
                value={!props.state.skipInvoice && props.state.includeTipInInvoice}
                onChange={(e) => props.onChange({
                    ...props.state,
                    includeTipInInvoice: e,
                })}
            />
        </div>
    </div>
}