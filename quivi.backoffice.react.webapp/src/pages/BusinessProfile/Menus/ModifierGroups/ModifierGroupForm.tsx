import { useState } from "react";
import Button from "../../../../components/ui/button/Button";
import { ModifierGroup } from "../../../../hooks/api/Dtos/modifierGroups/ModifierGroup";
import { Spinner } from "../../../../components/spinners/Spinner";
import { ModifierGroupBaseForm, ModifierGroupFormState } from "./ModifierGroupBaseForm";

interface Props {
    readonly model?: ModifierGroup;
    readonly onSubmit: (state: ModifierGroupFormState) => Promise<any>;
    readonly submitText: string;
    readonly categoryId?: string;
}
export const ModifierGroupForm = (props: Props) => {
    const [state, setState] = useState({
        isSubmitting: false,
        submit: undefined as undefined | (() => Promise<void>),
    })

    return <>
        <ModifierGroupBaseForm
            model={props.model}
            categoryId={props.categoryId}
            onSubmit={async (state) => {
                setState(s => ({ ...s, isSubmitting: true }));
                try {
                    await props.onSubmit(state);
                } catch (e){
                    throw e;
                } finally {
                    setState(s => ({ ...s, isSubmitting: false }));
                }
            }}
            onFormChange={(isValid, submit) => setState(s => ({
                ...s,
                submit: isValid == false ? undefined : submit,
            }))}
        />

        <Button
            size="md"
            onClick={state.submit}
            disabled={state.submit == undefined || state.isSubmitting}
            variant="primary"
        >
            {
                state.isSubmitting
                ?
                <Spinner />
                :
                props.submitText
            }
        </Button>
    </>
}