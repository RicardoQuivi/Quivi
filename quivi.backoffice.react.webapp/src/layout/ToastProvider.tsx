import { createContext, ReactNode, useContext } from "react";
import { toast, ToastContainer } from "react-toastify";
import Notification from "../components/ui/notification/Notfication";
import { useTranslation } from "react-i18next";

type ToastType = 'success' | 'error' | 'info' | 'warning';

interface ToastOptions {
    readonly title?: string | null;
}

interface ToastService {
    readonly success: (message: string, options?: ToastOptions) => any;
    readonly error: (message: string, options?: ToastOptions) => any;
    readonly info: (message: string, options?: ToastOptions) => any;
    readonly warning: (message: string, options?: ToastOptions) => any;
}

const ToastContext = createContext<ToastService | undefined>(undefined);

const autoClose = 5000;
const showToast = (type: ToastType, title: string, message: string, options?: ToastOptions) => {
    let toastTitle: string | undefined = title;
    if(options != undefined) {
        if(options.title === null) {
            toastTitle = undefined;
        }
    }
    const id = toast(<Notification 
        variant={type}
        title={toastTitle ?? ""}
        description={message} 
        hideDuration={autoClose}
        close={() => toast.dismiss(id)}
    />, {
        autoClose: autoClose,
        className: '!p-0 !bg-transparent !shadow-none',
        hideProgressBar: true,
        closeButton: <></>
    })
};

export const ToastProvider = ({ children }: { children: ReactNode }) => {
    const { t } = useTranslation();
    return <>
        <ToastContext.Provider value={{
            success: (m, options) => showToast("success",t("common.toast.success"), m, options),
            error: (m, options) => showToast("error", t("common.toast.error"), m, options),
            info: (m, options) => showToast("info", t("common.toast.info"), m, options),
            warning: (m, options) => showToast("warning", t("common.toast.warning"), m, options),
        }}>
            {children}
        </ToastContext.Provider>
        <ToastContainer
            position="top-right"
            style={{
                zIndex: 9999999999,
            }}
        />
    </>;
}


export const useToast = () => {
    const context = useContext(ToastContext);
    if (!context) {
        throw new Error('useToast must be used within a ToastProvider');
    }
    return context;
}