import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuiviTheme } from "../../hooks/theme/useQuiviTheme";
import { Box, IconButton } from "@mui/material";
import { Formatter } from "../../helpers/formatter";
import { DashCircleIcon, PlusCircleIcon } from "../../icons";

interface QuantitySelectorProps {
    readonly quantity: number;
    readonly alwaysOpened?: boolean
    readonly onDecrement: () => any;
    readonly onIncrement: () => any;
    readonly shouldCollapse: boolean,
    readonly pixelSize?: number,
    readonly decrementDisabled?: boolean,
    readonly incrementDisabled?: boolean,
}
export const QuantitySelector: React.FC<QuantitySelectorProps> = ({
    quantity,
    alwaysOpened,
    onDecrement,
    onIncrement,
    shouldCollapse,
    pixelSize,
    decrementDisabled,
    incrementDisabled,
}) => {
    const size = pixelSize ?? 26;

    const theme = useQuiviTheme();
    const { t, i18n } = useTranslation();

    const [quickCartOpened, setIsQuickCartOpened] = useState(alwaysOpened ?? false);

    //The only purpose of this variable is to set a timer for the quick cart to collapse
    //This could be done with CSS and would be preferable but I couldn't solve some issues with CSS
    const [quickCartHover, setIsQuickCartHover] = useState(alwaysOpened ?? false);

    //The only purpose of this variable is fix an issue where on mobile, with the quick cart closed and any amount
    //of the item is already selected, then if you would click on the amount it would automatically increment
    //that amount instead of just opening the quick cart. The issue seems to be underlying to how events are managed
    //by the browser and it doesn't happen on Safari for instance. My guess is if we manage to make it work via CSS,
    //then this issue would be erased by itself
    const [timeStamp, setTimestamp] = useState(0);

    const decrementQty = (e: React.MouseEvent<HTMLElement>) => { 
        e.stopPropagation();

        if(quickCartOpened == false) {
            return;
        }

        onDecrement();
    }

    const incrementQty = (e: React.MouseEvent<HTMLElement>) => {
        e.stopPropagation();

        if(quantity > 0) {
            //350 is the transition time
            const t = new Date().getTime();
            if(t - timeStamp < 350) {
                return;
            }
        }

        if(quickCartOpened == false) {
            return;
        }

        onIncrement();
    }

    useEffect(() => {
        if(alwaysOpened == true) {
            return;
        }

        if(quickCartHover) {
            if(quickCartOpened == false) {
                setTimestamp(new Date().getTime());
            }
            setIsQuickCartOpened(true);
            return;
        }

        const timeout = setTimeout(() => setIsQuickCartOpened(false), 1000);
        return () => clearTimeout(timeout);
    }, [quickCartHover])

    return <Box 
            sx={{
                zIndex: 0,
                width: quickCartOpened && !shouldCollapse ? `${size * 3 + size / 2}px` : `${size}px`,
                height: `${size}px`,
                position: "relative",
                alignSelf: "end",
                transition: "width 0.35s",
                display: "flex",
                flexWrap: "wrap",
                alignContent: "center",
            }}
            onTouchStart={() => setIsQuickCartHover(true)} 
            onTouchEnd={() => setIsQuickCartHover(false)} 
            onMouseEnter={() => setIsQuickCartHover(true)} 
            onMouseLeave={() => setIsQuickCartHover(false)}
        >

        <>
            <IconButton
                disabled={decrementDisabled} 
                title={t("digitalMenu.decrementQty")}
                onClick={decrementQty}
                sx={{
                    zIndex: 0,
                    position: "absolute",
                    left: 0,

                    top: 0,
                    bottom: 0,
                    
                    opacity: decrementDisabled ? 0.6 : 1,
                }}
            >
                <DashCircleIcon
                    style={{
                        width: `${size}px`,
                        height:`${size}px`,
                        fill: theme.primaryColor.hex,
                        position: "absolute",
                        left: 0,
                    }}
                />
            </IconButton>
            <span style={{
                zIndex: 1,
                position: "absolute",
                left: "50%",
                transform: "translate(-50%, 0)",
                lineHeight: `${size}px`,
                fontWeight: 400,
            }}>
                {Formatter.number(quantity, i18n.language)}
            </span>
        </>

        <Box
            sx={{
                zIndex: 2,
                backgroundColor: theme.primaryColor.hex,
                color: "white",
                borderRadius: "50%",
                lineHeight: `${size}px`,
                width: `${size}px`,
                height: `${size}px`,
                textAlign: "center",
                position: "absolute",
                right: 0,
                transition: "opacity 0.35s",
                opacity: !shouldCollapse && !quickCartOpened ? 1 : 0,

                top: 0,
                bottom: 0,
            }}
        >
            <span>{Formatter.number(quantity, i18n.language)}</span>
        </Box>

        <IconButton 
            disabled={incrementDisabled}
            title={t("digitalMenu.incrementQty")}
            onClick={incrementQty}
            sx={{
                zIndex: 2,
                position: "absolute",
                right: 0,
                transition: "opacity 0.35s",
                opacity: !shouldCollapse && !quickCartOpened ? 0 : (incrementDisabled == true ? 0.6 : 1),

                top: 0,
                bottom: 0,
            }}
        >
            <PlusCircleIcon
                style={{
                    width: `${size}px`,
                    height: `${size}px`,
                    fill: theme.primaryColor.hex,
                    backgroundColor: alwaysOpened == true ? "unset" : "#F1F3F1",
                    position: "absolute",
                    right: 0,
                }}
            />
        </IconButton>
    </Box>
}