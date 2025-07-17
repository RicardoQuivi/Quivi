import { LocalizationProvider } from "@mui/x-date-pickers"
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns';
import { pt, enGB } from "date-fns/locale";
import { useEffect, useState } from "react";
import SplashScreen from "../pages/SplashScreen";
import '../i18n';
import i18n from "../i18n";

interface Props {
    readonly children: React.ReactNode;
}

const getAdapter = (language: string) => {
    if(!language) {
        return enGB;
    }

    const baseLanguage = language.toLowerCase().split("-");
    switch (baseLanguage[0]) {
        case "pt":
            return pt;
        case 'en':
            return enGB;
        default:
            return enGB;
    }
    throw new Error(`Language ${baseLanguage} was not found!`);
}

export const Localization: React.FC<Props> = (props) => {
    const [adapter, setAdapter] = useState(getAdapter(i18n.language))
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() =>{
        if(isLoading == false) {
            return
        }

        i18n.init(() => setIsLoading(false))
        if(i18n.isInitialized) {
            setIsLoading(false);
        }
    }, [isLoading])
    
    useEffect(() => {
        if(isLoading) {
            return;
        }

        const listener =  (lng: string) => setAdapter(getAdapter(lng));
        i18n.on("languageChanged", listener)
        return () => i18n.off("languageChanged", listener);
    }, [isLoading])

    return <LocalizationProvider dateAdapter={AdapterDateFns} adapterLocale={adapter}> 
    {
        isLoading
        ?
            <SplashScreen />
        :
            props.children
    }
    </LocalizationProvider>
}