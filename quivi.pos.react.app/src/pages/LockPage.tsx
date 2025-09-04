import { useTranslation } from "react-i18next";
import { useToast } from "../context/ToastProvider";
import { useEffect, useState } from "react";
import { Employee } from "../hooks/api/Dtos/employees/Employee";
import { EmployeeSelectPage } from "./employees/EmployeeSelectPage";
import { EmployeePinCodeLock } from "./employees/EmployeePinCodeLock";
import { DefineEmployeePinCode } from "./employees/DefineEmployeePinCode";
import { Box, Link, Stack, Typography } from "@mui/material";
import { LeftArrowIcon } from "../icons";

enum Page {
    PinCode,
    DefinePinCode,
}
export const LockPage = () => {
    const { t } = useTranslation();
    const toast = useToast();

    const [selectedEmployee, setSelectedEmployee] = useState<Employee>();
    const [currentPage, setCurrentPage] = useState<Page>();

    const onComplete = (hasError: boolean) => {
        if(hasError == false) {
            return;
        }
        toast.error(t('unexpectedErrorHasOccurred'));
    }

    useEffect(() => setSelectedEmployee(undefined), [])

    useEffect(() => {
        if(selectedEmployee == undefined) {
            setCurrentPage(undefined);
        } else if(selectedEmployee.hasPinCode == true) {
            setCurrentPage(Page.PinCode);
        } else {
            setCurrentPage(Page.DefinePinCode);
        }
    }, [selectedEmployee])

    return <Box
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
        {/* Title header */}
        <Box
            width="100%"
            maxWidth="md"
            mx="auto"
            mb={2}
            pt={{ sm: 10 }}
        >
            {
                selectedEmployee == undefined
                ?
                <Typography variant="h5" gutterBottom>
                    {t("pages.employeeLockPage.selectEmployee")}
                </Typography>
                :
                <Typography variant="h5" gutterBottom>
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
                        underline="none"
                    >
                        <Stack
                            direction="row"
                            gap={2}
                            sx={{
                                alignItems: "center",
                            }}
                        >
                                <LeftArrowIcon style={{ fontSize: 20, marginRight: 0.5 }} />
                            {t("pages.employeeLockPage.backToEmployee")}
                        </Stack>
                    </Link>
                </Typography>
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
            { 
                selectedEmployee == undefined || currentPage == undefined
                ? 
                <EmployeeSelectPage onEmployeeSelect={setSelectedEmployee} />
                :
                <>
                    { currentPage == Page.PinCode && <EmployeePinCodeLock employeeId={selectedEmployee.id} /> }
                    { currentPage == Page.DefinePinCode && <DefineEmployeePinCode employeeId={selectedEmployee.id} onComplete={onComplete} />}
                </>
            }
        </Box>
    </Box>
}