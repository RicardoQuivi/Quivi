import { Trans, useTranslation } from "react-i18next";
import { Modal, ModalSize } from "../ui/modal"
import { ModalButtonsFooter } from "../ui/modal/ModalButtonsFooter";
import { useState } from "react";
import { Entity } from "../../hooks/EntitiesName";
import { useToast } from "../../layout/ToastProvider";
import { Spinner } from "../spinners/Spinner";

interface Props<T,> {
    readonly isOpen: boolean;
    readonly model: T[];
    readonly entity: Entity;
    readonly getName: (m: T) => string;
    readonly action: (m: T[]) => Promise<any>;
    readonly onClose: () => void;
}
export const DeleteEntitiesModal = <T,>(props: Props<T>) => {
    const { t } = useTranslation();
    const toast = useToast();
    const [isSubmitting, setIsSubmitting] = useState(false);

    const getEntityName = (): string => {
        switch(props.entity)
        {
            case Entity.ChannelProfiles: return t("common.entities.channelProfiles");
            case Entity.Merchants: return t("common.entities.merchants");
            case Entity.PosIntegrations: return t("common.entities.posIntegrations");
            case Entity.Channels:  return t("common.entities.channels");
            case Entity.MenuCategories:  return t("common.entities.menuCategories");
            case Entity.MenuItems:  return t("common.entities.menuItems");
            case Entity.Locals:  return t("common.entities.locals");
            case Entity.Employees:  return t("common.entities.employees");
            case Entity.ModifierGroups: return t("common.entities.modifierGroups");
            case Entity.CustomChargeMethods: return t("common.entities.customChargeMethods");
            case Entity.PrinterWorkers: return t("common.entities.printerWorkers");
            case Entity.Printers: return t("common.entities.printers");
            case Entity.PrinterMessages: return t("common.entities.printerMessages");
            case Entity.AcquirerConfigurations: return t("common.entities.acquirerConfigurations");
            case Entity.Transactions: return t("common.entities.transactions");
            case Entity.Reviews: return t("common.entities.reviews");
            case Entity.MerchantDocuments: return t("common.entities.merchantDocuments");
            case Entity.ConfigurableFields: return t("common.entities.configurableFields");
            case Entity.ConfigurableFieldAssociations: return t("common.entities.configurableFieldAssociations");
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
        isOpen={props.isOpen}
        onClose={() => props.onClose()}
        size={ModalSize.Medium}
        title={getTitle()}
        footer={(
            <ModalButtonsFooter 
                primaryButton={{
                    content: isSubmitting
                                ?
                                <Spinner />
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
        <p className="text-sm text-gray-500 dark:text-gray-400">
            <Trans
                t={t}
                i18nKey="common.operations.deleteManyDescription"
                shouldUnescape={true}
                values={{
                    count: props.model.length,
                    entity: getEntityName(),
                }}
                components={{
                    b: <b/>,
                }}
            />
        </p>
    </Modal>
}