import { createContext, ReactNode, useContext } from "react";
import { toast, ToastContainer } from "react-toastify";
import { useTranslation } from "react-i18next";

type ToastType = 'success' | 'error' | 'info' | 'warning';

interface ToastService {
    readonly success: (message: string) => any;
    readonly error: (message: string) => any;
    readonly info: (message: string) => any;
    readonly warning: (message: string) => any;
}

const ToastContext = createContext<ToastService | undefined>(undefined);

const showToast = (_: ToastType, _1: string, message: string) => {
    toast(message)
};

export const ToastProvider = ({ children }: { children: ReactNode }) => {
    const { t } = useTranslation();
    return <>
        <ToastContext.Provider value={{
            success: m => showToast("success", t("common.toast.success"), m),
            error: m => showToast("error", t("common.toast.error"), m),
            info: m => showToast("info", t("common.toast.info"), m),
            warning: m => showToast("warning", t("common.toast.warning"), m),
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