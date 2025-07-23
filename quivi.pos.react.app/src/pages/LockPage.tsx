import { useTranslation } from "react-i18next";
import { useToast } from "../context/ToastProvider";
import { useEffect, useState } from "react";
import { Employee } from "../hooks/api/Dtos/employees/Employee";
import { EmployeeSelectPage } from "./employees/EmployeeSelectPage";
import { EmployeePinCodeLock } from "./employees/EmployeePinCodeLock";
import { DefineEmployeePinCode } from "./employees/DefineEmployeePinCode";
import { Box, Grid, Link, Stack, Typography } from "@mui/material";
import { GridShape } from "../components/common/GridShape";
import { LeftArrowIcon, QuiviFullIcon } from "../icons";

enum Page {
    SelectEmployee,
    PinCode,
    DefinePinCode,
}
export const LockPage = () => {
    const { t } = useTranslation();
    const toast = useToast();

    const [selectedEmployee, setSelectedEmployee] = useState<Employee>();
    const [currentPage, setCurrentPage] = useState(Page.SelectEmployee);

    const onComplete = (hasError: boolean) => {
        if(hasError == false) {
            return;
        }
        toast.error(t('unexpectedErrorHasOccurred'));
    }

    useEffect(() => setSelectedEmployee(undefined), [])

    useEffect(() => {
        if(selectedEmployee == undefined) {
            setCurrentPage(Page.SelectEmployee);
        } else if(selectedEmployee.hasPinCode) {
            setCurrentPage(Page.PinCode);
        } else {
            setCurrentPage(Page.DefinePinCode);
        }
    }, [selectedEmployee])

    return <Layout>
        <Box
            display="flex"
            flexDirection="column"
            flex={1}
            width="100%"
            overflow="auto"
            sx={{
                scrollbarWidth: 'none',
                '&::-webkit-scrollbar': {
                    display: 'none',
                },
                lg: {
                    width: '50%',
                },
            }}
        >
            {/* Back link section */}
            <Box
                width="100%"
                maxWidth="md"
                mx="auto"
                mb={5}
                pt={{ sm: 10 }}
            >
                {
                    currentPage != Page.SelectEmployee &&
                    <Link
                        sx={{
                            color: "inherit",
                            display: "flex",
                            alignContent: "center",
                            flexWrap: "wrap",
                            alignItems: "center",
                            "&:hover": {
                                cursor: "pointer",
                            },
                        }}
                        onClick={() => setSelectedEmployee(undefined)}
                    >
                        <Stack
                            direction="row"
                            gap={2}
                        >
                            <LeftArrowIcon style={{ fontSize: 20, marginRight: 0.5 }} />
                            {t("pages.employeeLockPage.backToEmployee")}
                        </Stack>
                    </Link>
                }
            </Box>

            <Box
                display="flex"
                flexDirection="column"
                justifyContent="center"
                flex={1}
                width="100%"
                maxWidth="md"
                mx="auto"
            >
                { currentPage == Page.SelectEmployee && <EmployeeSelectPage onEmployeeSelect={setSelectedEmployee} /> }
                { currentPage == Page.PinCode && <EmployeePinCodeLock employee={selectedEmployee!} /> }
                { currentPage == Page.DefinePinCode && <DefineEmployeePinCode employee={selectedEmployee!} onComplete={onComplete} />}
            </Box>
        </Box>
    </Layout>
}

interface Props {
    readonly children: React.ReactNode;
}
const Layout = (props: Props) => {
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
            <Grid size={{xs: 12, lg: 6}}>
                <Stack
                    direction="column"
                    sx={{
                        flex: 1,
                        height: "100%",
                        width: "100%"
                    }}
                >
                    <Box
                        sx={{
                            marginInline: "auto",
                            display: "flex",
                            flex: 1,
                            flexDirection: "column",
                            justifyContent: "center",
                            mx: "4rem",
                        }}
                    >
                        {props.children}
                    </Box>
                </Stack>
            </Grid>

            {/* Right side graphic panel */}
            <Grid
                size={{lg:6}}
                sx={{
                    display: { 
                        xs: 'none',
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

            <Box
                position="fixed"
                zIndex={50}
                bottom={6}
                right={6}
                display={{ xs: 'none', sm: 'block' }}
            >
                <Box display="flex" flexWrap="wrap" justifyContent="space-between" gap={2}>
                    {/* <FloatingLanguageButton /> */}
                </Box>
            </Box>
        </Grid>
    </Box>
    )
}