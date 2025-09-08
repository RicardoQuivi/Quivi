import { Box, SpeedDial, Typography } from "@mui/material";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { CountryIcon } from "../../icons/CountryIcon";
import CustomModal, { ModalSize } from "../Modals/CustomModal";

const languages = [
    {
        id: "en",
        name: "English",
    },
    {
        id: "pt",
        name: "Português",
    }
]


export const FloatingLanguageButton = () => {
    const { t, i18n } = useTranslation();
    const [isOpen, setIsOpen] = useState(false);

    return <>
        <Box
            sx={{
                position: "absolute",
                bottom: 0,
                right: 0,
                transform: 'translateZ(0px)',
                flexGrow: 1
            }}
        >
            <SpeedDial
                sx={{ position: 'absolute', bottom: 16, right: 16 }}
                icon={<CountryIcon
                    language={i18n.language}
                    style={{
                        width: "auto",
                        height: "100%",
                        borderRadius: "9999px"
                    }} />}
                    ariaLabel={""}
                onClick={() => setIsOpen(s => !s)}
                
            />
        </Box>
        <CustomModal
            isOpen={isOpen}
            onClose={() => setIsOpen(false)}
            size={ModalSize.Small}
            title={t("changeLanguage")}
        >
            <Box
                sx={{
                    display: 'grid',
                    gridTemplateColumns: { xs: '1fr', md: '1fr 1fr' },
                    gap: 2,
                    mb: 8,
                }}
            >
            {
                languages.map((l) => <Box
                    key={l.id}
                    onClick={() => {
                        i18n.changeLanguage(l.id);
                        setIsOpen(false);
                    }}
                    sx={{
                        display: 'flex',
                        alignItems: 'center',
                        cursor: 'pointer',
                    }}
                >
                    <CountryIcon
                        language={l.id}
                        style={{
                            width: "auto",
                            height: 60,
                            borderRadius: '50%',
                        }}
                    />
                    <Box
                        sx={{
                            ml: 3,
                            display: 'flex',
                            alignItems: 'center',
                        }}
                    >
                        <Typography>
                            {l.name}
                        </Typography>
                    </Box>
                </Box>)
            }
            </Box>
        </CustomModal>
    </>
}
