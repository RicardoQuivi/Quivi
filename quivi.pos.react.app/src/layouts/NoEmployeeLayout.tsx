import { AppBar, Avatar, Box, Grid, Toolbar, Typography } from "@mui/material";
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
            }}
        >
            {/* Background grid image - top right */}
            <Box
                sx={{
                    position: 'absolute',
                    top: 0,
                    bottom: 0,
                    right: 0,
                    zIndex: -1,
                    width: '50%',
                    transform: "translate(0, -50%)",
                }}
            >
                <img
                    alt="grid"
                    src="/images/shape/grid-01.svg"
                    style={{ width: '100%' }}
                />
            </Box>

            {/* Background grid image - bottom left, rotated */}
            <Box
                sx={{
                    position: 'absolute',
                    top: 0,
                    bottom: 0,
                    left: 0,
                    zIndex: -1,
                    width: '50%',
                    transform: 'rotate(180deg) translate(0, -50%)',
                }}
            >
                <img 
                    alt="grid"
                    src="/images/shape/grid-01.svg"
                    style={{
                        width: '100%',
                    }}
                />
            </Box>

            {/* Content */}
            <Box 
                sx={{
                    display: 'flex',
                    flexDirection: 'column',
                    alignItems: 'center',
                }}
            >
                <Box 
                    sx={{
                        mb: 2,
                        "& svg": {
                            fill: t => t.palette.primary.light,
                        }
                    }}
                >
                    <QuiviFullIcon
                        height="auto"
                        width={231}
                    />
                </Box>

                <Typography
                    variant="body1"
                    sx={{
                        textAlign: 'center',
                        color: 'grey.400',
                    }}
                >
                        {t("quivi.product.description")}
                </Typography>
            </Box>
        </Box>
    );
}