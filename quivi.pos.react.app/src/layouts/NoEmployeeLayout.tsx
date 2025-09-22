import { AppBar, Avatar, Box, Grid, Stack, Toolbar, Typography } from "@mui/material";
import { useTranslation } from "react-i18next";
import { QuiviFullIcon, QuiviIcon } from "../icons";
import { FloatingLanguageButton } from "../components/Buttons/FloatingLanguageButton";

interface Props {
    readonly children: React.ReactNode;
}
export const NoEmployeeLayout = (props: Props) => {
    return (
        <Box
            position="relative"
            zIndex={1}
            sx={{
                bgcolor: t => t.palette.background.paper
            }}
        >
            <Grid
                container
                direction={{
                    xs: 'column',
                    lg: 'row',
                }}
                justifyContent="center"
                sx={{
                    width: '100%',
                    height: '100vh',
                    p: {
                        xs: 0,
                        sm: 0,
                    },
                }}
            >
                <Grid 
                    size={{ 
                        xs: 12,
                        sm: 12,
                        md: 6,
                        lg: 6,
                        xl: 6, 
                    }}
                    height="100%"
                    display="flex"
                    flexDirection="column"
                >
                    <AppBar
                        position="static"
                        sx={{
                            display: {
                                xs: 'grid',
                                md: 'none',
                                lg: 'none'
                            },
                            marginBottom: "1rem",
                        }}
                    >
                        <Toolbar
                            sx={{
                                flexGrow: 1,
                                display: "flex",
                                justifyContent: "center"
                            }}
                        >
                            <Avatar
                                sx={{
                                    '& svg': {
                                        width: '100%',
                                        height: '100%',
                                    },
                                }}
                            >
                                <QuiviIcon />
                            </Avatar>
                        </Toolbar>
                    </AppBar>

                    <Box
                        sx={{
                            marginInline: "auto",
                            display: "flex",
                            flex: 1,
                            flexDirection: "column",
                            justifyContent: "center",
                            mx: "1.5rem",
                            alignItems: "center",
                            overflow: "hidden",
                        }}
                    >
                        {props.children}
                    </Box>
                </Grid>

                {/* Right side graphic panel */}
                <Grid
                    size={{ 
                        xs: 0,
                        sm: 0,
                        md: 6,
                        lg: 6,
                    }}
                    sx={{
                        display: {
                            xs: 'none',
                            sm: 'none',
                            md: 'flex',
                            lg: 'flex',
                        },
                        alignItems: 'center',
                        justifyContent: 'center',
                        width: '100%',
                        height: '100%',
                        bgcolor: p => p.palette.primary.main,
                    }}
                >
                    <RightSidePanel />
                </Grid>
            </Grid>

            <FloatingLanguageButton />
        </Box>
    )
}

const RightSidePanel = () => {
    const { t } = useTranslation();
    return (
        <Box
            sx={{
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                position: 'relative',
                zIndex: 1,
                width: "100%",
                height: "100%",
                bgcolor: t => t.palette.primary.main,
            }}
        >
            <Stack
                direction="column"
                sx={{
                    alignItems: 'center',
                    color: "#272727",

                    "& svg": {
                        fill: "#272727"
                    }
                }}
                gap={8}
            >
                <QuiviFullIcon
                    height="auto"
                    width={231}
                />

                <Typography
                    variant="h5"
                    sx={{
                        textAlign: 'center',
                        fontFamily: "Atelia",
                        fontWeight: 400,
                        fontStyle: "Regular",
                        leadingTrim: "NONE",
                        lineHeight: "83%",
                        letterSpacing: "0%",
                        textTransform: "uppercase",
                    }}
                >
                        {t("quivi.product.description")}
                </Typography>
                
                <img
                    src="/images/quivi/Kiwi-3.svg"
                    width="456"
                    height="auto"
                />

                <Typography
                    variant="body1"
                    sx={{
                        textAlign: 'center',
                        fontWeight: 400,
                        fontStyle: "Regular",
                        leadingTrim: "NONE",
                        lineHeight: "83%",
                        letterSpacing: "0%",
                    }}
                >
                    {t("quivi.product.description")}
                </Typography>
            </Stack>
        </Box>
    );
}