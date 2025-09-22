import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import './i18n';
import { AuthProvider } from './context/AuthContextProvider.tsx'
import { WebEventsProvider } from './hooks/signalR/useWebEvents.tsx'
import { QueryContextProvider } from './context/QueryContextProvider.tsx'
import { alpha, createTheme, darken, lighten, ThemeProvider } from '@mui/material/styles'
import { ToastProvider } from './context/ToastProvider.tsx'
import { App } from './App.tsx'
import { EmployeeProvider } from './context/employee/EmployeeContextProvider.tsx';

const baseColors = {
    neutrals: {
        beige: "#FAFFE9",
        black: "#232323",
    },
    accent: {
        brightGreen: "#5ACC61",
        lime: "#D5ED3E",
        mediumGreen: "#A4CC44",
        darkGreen: "#2B7830",
        skyBlue: "#9FDFF2",
        burntOrange: "#FF894D",
    },
}
const colors = {
    ...baseColors,
    buttons: {
        primary: {
            backgroundColor: baseColors.accent.brightGreen,
            borderColor: baseColors.accent.brightGreen,
            color: baseColors.neutrals.black,
            stroke: baseColors.neutrals.black,

            "&:hover": {
                backgroundColor: darken(baseColors.accent.brightGreen, 0.3),
                borderColor: darken(baseColors.accent.brightGreen, 0.3),
            },

            "&.Mui-disabled": {
                backgroundColor: "#B1EBB5",
                borderColor: "#B1EBB5",
                color: "#474747",
                stroke: "#474747",
            }
        },
        secondary: {
            backgroundColor: baseColors.neutrals.beige,
            borderColor: baseColors.neutrals.black,
            color: baseColors.neutrals.black,
            stroke: baseColors.neutrals.black,

            "&:hover": {
                backgroundColor: baseColors.neutrals.black,
                borderColor: baseColors.neutrals.black,
                color: baseColors.neutrals.beige,
                stroke: baseColors.neutrals.beige,
            },

            "&.Mui-disabled": {
                backgroundColor: baseColors.neutrals.beige,
                borderColor: "#989898",
                color: "#989898",
                stroke: "#989898",
            }
        },
    },
    alerts: {
        success: {
            backgroundColor: baseColors.accent.brightGreen,
            borderColor: baseColors.accent.brightGreen,
            color: baseColors.neutrals.black,

            "& svg": {
                fill: baseColors.neutrals.black,
            },
        },
        error: {
            backgroundColor: baseColors.accent.burntOrange,
            borderColor: baseColors.accent.burntOrange,
            color: baseColors.neutrals.black,

            "& svg": {
                fill: baseColors.neutrals.black,
            },
        },
        warning: {
            backgroundColor: baseColors.accent.lime,
            borderColor: baseColors.accent.lime,
            color: baseColors.neutrals.black,

            "& svg": {
                fill: baseColors.neutrals.black,
            },
        },
        info: {
            backgroundColor: baseColors.accent.skyBlue,
            borderColor: baseColors.accent.skyBlue,
            color: baseColors.neutrals.black,

            "& svg": {
                fill: baseColors.neutrals.black,
            },
        },
    }
}
const theme = createTheme({
    palette: {
        primary: {
            main: colors.accent.brightGreen,
            dark: colors.neutrals.black,
            light: colors.neutrals.beige,
        },
        secondary: {
            main: "#FF0000", //TODO: Never used
            contrastText: "#ff0000", //TODO: Never used
        },
        text: {
            primary: colors.neutrals.black,
            secondary: lighten(colors.neutrals.black, 0.3),
        },
        background: {
            default: colors.neutrals.beige,
            paper: colors.neutrals.beige,
        },
        success: {
            main: baseColors.accent.brightGreen,
            dark: baseColors.accent.brightGreen,
            light: baseColors.accent.brightGreen,
            contrastText: baseColors.neutrals.black,
        },
        info: {
            main: colors.accent.skyBlue,
            dark: colors.accent.skyBlue,
            light: colors.accent.skyBlue,
            contrastText: baseColors.neutrals.black,
        },
        warning: {
            main: colors.accent.lime,
            dark: baseColors.accent.lime,
            light: baseColors.accent.lime,
            contrastText: baseColors.neutrals.black,
        },
        error: {
            main:baseColors.accent.burntOrange,
            dark: baseColors.accent.burntOrange,
            light: baseColors.accent.burntOrange,
            contrastText: baseColors.neutrals.black,
        },
        action: {
            activatedOpacity: 0.12,
            active: alpha(colors.neutrals.black, 0.54),
            disabled: alpha(colors.neutrals.black, 0.26),
            disabledBackground: alpha(colors.neutrals.black, 0.12),
            disabledOpacity: 0.38,
            focus: alpha(colors.neutrals.black, 0.12),
            focusOpacity: 0.12,
            hover: alpha(colors.accent.darkGreen, 0.04),
            hoverOpacity: 0.04,
            selected: alpha(colors.neutrals.black, 0.08),
            selectedOpacity: 0.08,
        },
    },
    typography: {
        fontFamily: "BDOGrotesk",
        allVariants: {
            fontFamily: "BDOGrotesk",
        },
        h1: {
            fontFamily: "Atelia",
        },
        h2: {
            fontFamily: "Atelia",
        },
        h3: {
            fontFamily: "Atelia",
        },
        h4: {
            fontFamily: "BDOGrotesk",
        },
        h5: {
            fontFamily: "BDOGrotesk",
        },
        h6: {
            fontFamily: "BDOGrotesk",
        },
        body1: {
            fontFamily: "BDOGrotesk",
        },
    },
    components: {
        MuiButton: {
            styleOverrides: {
                containedPrimary: colors.buttons.primary,
                outlinedPrimary: colors.buttons.secondary,
            },
        },
        MuiAlert: {
            styleOverrides: {
                standardSuccess: colors.alerts.success,
                standardError: colors.alerts.error,
                standardWarning: colors.alerts.warning,
                standardInfo: colors.alerts.info,

                outlinedSuccess: colors.alerts.success,
                outlinedError: colors.alerts.error,
                outlinedWarning: colors.alerts.warning,
                outlinedInfo: colors.alerts.info,
            }
        },
    }
});

createRoot(document.getElementById('root')!).render(
    <StrictMode>
        <ThemeProvider theme={theme}>
            <ToastProvider>
                <AuthProvider>
                    <WebEventsProvider>
                        <QueryContextProvider>
                            <EmployeeProvider>
                                <App />
                            </EmployeeProvider>
                        </QueryContextProvider>
                    </WebEventsProvider>
                </AuthProvider>
            </ToastProvider>
        </ThemeProvider>
    </StrictMode>,
)