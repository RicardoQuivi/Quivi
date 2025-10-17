import { useState } from "react";
import { Modal, ModalSize } from "../ui/modal/Modal"
import { ApiException } from "../../hooks/api/exceptions/ApiException";
import { useTranslation } from "react-i18next";
import { ModalButtonsFooter } from "../ui/modal/ModalButtonsFooter";
import { Spinner } from "../spinners/Spinner";

interface GenericEntityModalProps<TEntity> {
    readonly model?: TEntity;
    readonly isOpen: boolean;
    readonly onClose: (hasChanges: boolean) => void,
    readonly size?: ModalSize;
    readonly children: React.ReactNode;

    readonly entityName: string;
    readonly title?: () => React.ReactNode;
    readonly createAction: () => Promise<void>;
    readonly updateAction: () => Promise<void>;
    readonly isValid: boolean;
}
export const GenericEntityModal = <TEntity,>(props: GenericEntityModalProps<TEntity>) => {
    const { t } = useTranslation();
    const [isSubmitting, setIsSubmitting] = useState(false);

    const save = async () => {
        setIsSubmitting(true);
        
        //let apiErrors = [] as InvalidModelMessage[];
        try {
            if(props.model == undefined) {
                //TODO: pass form
                await props.createAction();
            } else {
                //TODO: Pass form
                await props.updateAction();
            }
            props.onClose(true);
        } catch(e) {
            if(e instanceof ApiException) {
                //apiErrors = e.errors;
            }
        } finally {
            setIsSubmitting(false);
        }
    }

    const getTitle = (): React.ReactNode => {
        if(props.title != undefined) {
            return props.title();
        }

        const key = `common.operations.${props.model == undefined ? 'create' : 'edit'}`;
        return t(key, {
            name: props.entityName,
        })
    }

    return (
    <Modal
        isOpen={props.isOpen}
        onClose={() => props.onClose(false)}
        size={props.size}
        title={getTitle()}
        footer={(
            <ModalButtonsFooter 
                primaryButton={{
                    content: isSubmitting
                                ?
                                    <Spinner />
                                :
                                t("common.confirm"),
                    disabled: isSubmitting || props.isValid == false,
                    onClick: save,
                }}
                secondaryButton={{
                    content: t("common.close"),
                    onClick: () => props.onClose(false),
                }}
            />
        )}
    >
        {props.children}
    </Modal>
    )
}