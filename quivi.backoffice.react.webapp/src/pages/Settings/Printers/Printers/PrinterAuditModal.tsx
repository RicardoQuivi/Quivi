import { useTranslation } from "react-i18next";
import { Modal, ModalSize } from "../../../../components/ui/modal"
import { ModalButtonsFooter } from "../../../../components/ui/modal/ModalButtonsFooter";
import { Printer } from "../../../../hooks/api/Dtos/printers/Printer";
import Button from "../../../../components/ui/button/Button";

interface Props {
    readonly printer?: Printer;
    readonly onClose: () => any;
}
export const PrinterAuditModal = (props: Props) => {
    const { t } = useTranslation();
    
    const getTitle = () => {
        if(props.printer == undefined) {
            return;
        }

        return t("pages.printers.printerInfo", {
            name: props.printer.name
        })
    }
    return <Modal
        isOpen={props.printer != undefined}
        size={ModalSize.Default}
        onClose={props.onClose}
        title={getTitle()}
        footer={(
            <ModalButtonsFooter 
                primaryButton={{
                    content: t("common.close"),
                    onClick: props.onClose,
                }}
            />
        )}
    >
        <div className="grid grid-cols-[auto_auto] gap-2 w-full">
            <Button
                size="sm"
                variant="outline"
            >
                {t("pages.printers.pingPrinter")}
            </Button>
            <Button
                size="sm"
                variant="outline"
            >
                {t("pages.printers.printTest")}
            </Button>
        </div>
    </Modal>
}