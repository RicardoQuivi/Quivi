import { Modal, ModalSize } from "../../../../components/ui/modal"
import { ModalButtonsFooter } from "../../../../components/ui/modal/ModalButtonsFooter";
import { useTranslation } from "react-i18next";
import { useToast } from "../../../../layout/ToastProvider";
import { useModifierGroupMutator } from "../../../../hooks/mutators/useModifierGroupMutator";
import { ModifierGroup, ModifierGroupItem, ModifierGroupTranslation } from "../../../../hooks/api/Dtos/modifierGroups/ModifierGroup";
import { useMemo, useState } from "react";
import { Spinner } from "../../../../components/spinners/Spinner";
import { ModifierGroupBaseForm, ModifierGroupFormState } from "../ModifierGroups/ModifierGroupBaseForm";

interface Props {
    readonly isOpen: boolean;
    readonly initialName: string;
    readonly onClose: () => any;
    readonly onSave: (modifier: ModifierGroup) => any;
}
export const CreateModifierGroupModal = (props: Props) => {
    const { t } = useTranslation();
    const toast = useToast();
    const mutator = useModifierGroupMutator();
    
    const [state, setState] = useState({
        isSubmitting: false,
        submit: undefined as undefined | (() => Promise<void>),
    })

    const submit = async (state: ModifierGroupFormState) => {
        setState(s => ({ ...s, isSubmitting: true }));
        try {
            const modifier = await mutator.create({
                name: state.name,
                minSelection: state.minSelection,
                maxSelection: state.maxSelection,
                items: state.items,
                translations: state.translations,
            })
            toast.success(t("common.operations.success.new"));
            props.onSave(modifier);
            props.onClose();
        } catch (e){
            throw e;
        } finally {
            setState(s => ({ ...s, isSubmitting: false }));
        }
    }

    const model = useMemo(() => props.isOpen ? {
        id: "",
        items: {} as Record<string, ModifierGroupItem>,
        maxSelection: 1,
        minSelection: 0,
        name: props.initialName,
        translations: {} as Record<string, ModifierGroupTranslation>,
    } : undefined, [props.isOpen, props.initialName])

    return <Modal
        size={ModalSize.Auto}
        isOpen={props.isOpen}
        onClose={props.onClose}
        title={t(`common.operations.new`, {
            name: t("common.entities.modifierGroup")
        })}
        footer={
            <ModalButtonsFooter
                primaryButton={{
                    content: state.isSubmitting ? <Spinner /> : t("common.confirm"),
                    disabled: state.submit == undefined || state.isSubmitting,
                    onClick: state.submit,
                }}
                secondaryButton={{
                    content: t("common.close"),
                    onClick: props.onClose,
                }}
            />
        }
    >
        <ModifierGroupBaseForm
            model={model}
            categoryId={undefined}
            onSubmit={submit}
            onFormChange={(isValid, submit) => setState(s => ({
                ...s,
                submit: isValid == false ? undefined : submit,
            }))}
        />
    </Modal>
}