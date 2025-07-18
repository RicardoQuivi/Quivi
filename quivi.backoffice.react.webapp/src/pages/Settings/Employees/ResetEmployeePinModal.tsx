import { Trans, useTranslation } from "react-i18next";
import { ClipLoader } from "react-spinners";
import { useState } from "react";
import { Employee } from "../../../hooks/api/Dtos/employees/Employee";
import { useToast } from "../../../layout/ToastProvider";
import { Modal, ModalSize } from "../../../components/ui/modal";
import { ModalButtonsFooter } from "../../../components/ui/modal/ModalButtonsFooter";
import { useEmployeeMutator } from "../../../hooks/mutators/useEmployeeMutator";

interface Props {
    readonly model: Employee | undefined;
    readonly onClose: () => void;
}
export const ResetEmployeePinModal = (props: Props) => {
    const { t } = useTranslation();
    const toast = useToast();
    const mutator = useEmployeeMutator();
    const [isSubmitting, setIsSubmitting] = useState(false);

    const save = async () => {
        if(props.model == undefined) {
            return;
        }

        try {
            setIsSubmitting(true);
            
            await mutator.resetPin(props.model)

            toast.success(t("common.operations.success.delete"));
            props.onClose();
        } catch {
            toast.error(t("common.operations.failure.generic"));
            props.onClose();
        } finally {
            setIsSubmitting(false);
        }
    }

    return <Modal
        isOpen={props.model != undefined}
        onClose={props.onClose}
        size={ModalSize.Medium}
        title={t(`pages.employees.resetPin`)}
        footer={(
            <ModalButtonsFooter 
                primaryButton={{
                    content: isSubmitting
                                ?
                                <ClipLoader
                                    size={20}
                                    cssOverride={{
                                        borderColor: "white"
                                    }}
                                />
                                :
                                t("common.confirm"),
                    disabled: isSubmitting,
                    onClick: save,
                }}
                secondaryButton={{
                    content: t("common.close"),
                    onClick: props.onClose,
                }}
            />
        )}
    >
        <Trans
            t={t}
            i18nKey="pages.employees.resetPinDescription"
            shouldUnescape={true}
            values={{
                name: props.model?.name,
            }}
            components={{
                b: <b/>,
            }}
        />
    </Modal>
}