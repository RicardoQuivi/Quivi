import { AppBar, Avatar, Box, Grid, Stack, Toolbar } from "@mui/material";
import { useTranslation } from "react-i18next";
import { QuiviFullIcon, QuiviIcon } from "../icons";
import { FloatingLanguageButton } from "../components/Buttons/FloatingLanguageButton";
import { GridShape } from "../components/common/GridShape";

interface Props {
    readonly children: React.ReactNode;
}
export const NoEmployeeLayout = (props: Props) => {
    const { t } = useTranslation();

    return (
    <Box 
        position="relative"
        zIndex={1}
        sx={{
            bgcolor: 'white',
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
                bgcolor: 'white',
                p: { 
                    xs: 0, 
                    sm: 0,
                },
            }}
        >
            <Grid size={{xs: 12, lg: 6}} height="100%" display="flex" flexDirection="column">
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
                size={{lg:6}}
                sx={{
                    display: { 
                        xs: 'none',
                        md: 'grid',
                        lg: 'grid' 
                    },
                    alignItems: 'center',
                    justifyContent: 'center',
                    width: '100%',
                    height: '100%',
                    bgcolor: 'var(--color-brand-950)',
                }}
            >
                <Box
                    position="relative"
                    display="flex"
                    justifyContent="center"
                    alignItems="center"
                    zIndex={1}
                >
                    <GridShape />
                    <Box display="flex" flexDirection="column" alignItems="center" maxWidth="xs">
                        <Box style={{ marginBottom: '1rem', display: 'block' }}>
                            <QuiviFullIcon height="auto" width={231} /> 
                        </Box>
                        <Box
                            textAlign="center"
                            sx={{
                                color: 'grey.400',
                            }}
                        >
                            {t("quivi.product.description")}
                        </Box>
                    </Box>
                </Box>
            </Grid>
        </Grid>

        <FloatingLanguageButton />
    </Box>
    )
}