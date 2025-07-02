import { Trans, useTranslation } from "react-i18next";
import { Modal, ModalSize } from "../ui/modal"
import { ClipLoader } from "react-spinners";
import { ModalButtonsFooter } from "../ui/modal/ModalButtonsFooter";
import { useState } from "react";
import { Entity } from "../../hooks/EntitiesName";
import { useToast } from "../../layout/ToastProvider";

interface Props<T,> {
    readonly model: T | undefined;
    readonly entity: Entity;
    readonly getName: (m: T) => string;
    readonly action: (m: T) => Promise<any>;
    readonly onClose: () => void;
}
export const DeleteEntityModal = <T,>(props: Props<T>) => {
    const { t } = useTranslation();
    const toast = useToast();
    
    const [isSubmitting, setIsSubmitting] = useState(false);

    const getEntityName = (): string => {
        switch(props.entity)
        {
            case Entity.ChannelProfiles: return t("common.entities.channelProfile");
            case Entity.Merchants: return t("common.entities.merchant");
            case Entity.PosIntegrations: return t("common.entities.posIntegration");
            case Entity.Channels: return t("common.entities.channel");
            case Entity.MenuCategories: return t("common.entities.menuCategory");
            case Entity.MenuItems: return t("common.entities.menuItem");
            case Entity.Locals: return t("common.entities.local");
            case Entity.Employees: return t("common.entities.employee");
            case Entity.ModifierGroups: return t("common.entities.modifierGroup");
            case Entity.CustomChargeMethods: return t("common.entities.customChargeMethod");
            case Entity.PrinterWorkers: return t("common.entities.printerWorker");
            case Entity.Printers: return t("common.entities.printer");
            case Entity.PrinterMessages: return t("common.entities.printerMessage");
        }
    }

    const getTitle = (): React.ReactNode => {
        if(props.model == undefined) {
            return;
        }

        return t(`common.operations.delete`, {
            name: getEntityName(),
        })
    }

    const save = async () => {
        if(props.model == undefined) {
            return;
        }

        try {
            setIsSubmitting(true);
            await props.action(props.model);
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
        onClose={() => props.onClose()}
        size={ModalSize.Default}
        title={getTitle()}
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
                    onClick: () => props.onClose(),
                }}
            />
        )}
    >
        <Trans
            t={t}
            i18nKey="common.operations.deleteDescription"
            shouldUnescape={true}
            values={{
                name: props.model != undefined && props.getName(props.model),
                entity: getEntityName(),
            }}
            components={{
                b: <b/>,
            }}
        />
    </Modal>
}