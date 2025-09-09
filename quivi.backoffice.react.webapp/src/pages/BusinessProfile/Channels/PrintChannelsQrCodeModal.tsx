import { useTranslation } from "react-i18next";
import { Modal, ModalSize } from "../../../components/ui/modal";
import { ModalButtonsFooter } from "../../../components/ui/modal/ModalButtonsFooter";
import { CSSProperties, useState } from "react";
import { Spinner } from "../../../components/spinners/Spinner";
import { QrCodeRectangularFormat } from "../../../icons";
import { TextField } from "../../../components/inputs/TextField";
import { useChannelsApi } from "../../../hooks/api/useChannelsApi";
import { Files } from "../../../utilities/files";
import { useToast } from "../../../layout/ToastProvider";

enum QrCodeFormat {
    Rectangular,
}

interface Props {
    readonly applyToAll?: boolean;
    readonly channelIds: string[];
    readonly isOpen: boolean;
    readonly onClose: () => void,
}
export const PrintChannelsQrCodeModal = (props: Props) => {
    const { t } = useTranslation();
    const api = useChannelsApi();
    const toast = useToast();

    const [state, setState] = useState({
        format: QrCodeFormat.Rectangular,
        mainText: "Ver a conta e pagar",
        secondaryText: "Check your bill and pay here",
        isSubmiting: false,
    })

    const onSubmit = async () => {
        setState(s => ({...s, isSubmiting: true}))
        try {
            const response = await api.generateQrCodes({
                channelIds: props.applyToAll == true ? undefined : props.channelIds,
                mainText: state.mainText,
                secondaryText: state.secondaryText,
            })

            const base64String = response.base64Content;
            Files.saveBase64File(base64String, "qrcodes.pdf", 'application/pdf;base64');
            // const blob = Files.base64ToBlob(base64String, 'application/pdf;base64');
            // const url = URL.createObjectURL(blob);
            // window.open(url, '_blank');
            // const newWindow = window.open(url, '_blank');
            // if (newWindow != null) {
            //     newWindow.addEventListener('unload', () => URL.revokeObjectURL(url));
            // }

            props.onClose();
        } catch {
            toast.error(t("common.operations.failure.generic"));
        } finally {
            setState(s => ({...s, isSubmiting: false}))
        }
    }

    const getClasses = (format: QrCodeFormat) => {
        const classes = state.format == format ? "border-2 border-brand-500 dark:border-brand-500 xl:p-8" : "border border-gray-200 dark:border-gray-800";
        return `${classes} size-full flex flex-col bg-white p-6 dark:bg-white/[0.03] gap-1 text-center`;
    }

    return <Modal
        isOpen={props.isOpen}
        onClose={props.onClose}
        size={ModalSize.Medium}
        title={t('pages.channels.generateQrCode')}
        footer={(
            <ModalButtonsFooter 
                primaryButton={{
                    content: state.isSubmiting
                                ?
                                <Spinner />
                                :
                                t("common.confirm"),
                    onClick: onSubmit,
                }}
                secondaryButton={{
                    content: t("common.close"),
                    onClick: props.onClose,
                }}
            />
        )}
    >
        <div className="flex flex-col gap-4">
            <div className="grid grid-cols-1 sm:grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                <div
                    className={getClasses(QrCodeFormat.Rectangular)}
                >
                    <div className="mb-4">
                        <QrCodeRectangularFormat className="h-[66px] w-full block dark:hidden" style={{ '--border-color': 'black', '--font-color': 'black' } as CSSProperties}/>
                        <QrCodeRectangularFormat className="h-[66px] w-full hidden dark:block" style={{ '--border-color': 'white', '--font-color': 'white' } as CSSProperties}/>
                    </div>

                    <h4 className="font-medium text-black dark:text-white">{t("pages.channels.qrCodeFormat.rectangular.title")}</h4>
                    <p className="text-xs text-black dark:text-white">{t("pages.channels.qrCodeFormat.rectangular.description")}</p>
                </div>
            </div>
            <TextField
                label={t("pages.channels.qrCode.main")}
                value={state.mainText}
                onChange={v => setState(s => ({...s, mainText: v }))}
            />
            <TextField
                label={t("pages.channels.qrCode.secondary")}
                value={state.secondaryText}
                onChange={v => setState(s => ({...s, secondaryText: v }))}
            />
        </div>
    </Modal>
}