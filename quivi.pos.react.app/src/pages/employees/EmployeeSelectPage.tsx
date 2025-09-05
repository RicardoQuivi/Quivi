import { Box, Card, CardContent, CardMedia, Grid, Typography } from "@mui/material"
import { Employee } from "../../hooks/api/Dtos/employees/Employee";
import { useEmployeesQuery } from "../../hooks/queries/implementations/useEmployeesQuery";
import { useState } from "react";
import { PaginationFooter } from "../../components/Pagination/PaginationFooter";
import { useGenerateImage } from "../../hooks/useGenerateImage";
import { LoadingAnimation } from "../../components/Loadings/LoadingAnimation";

const pageSize = 20;
interface Props {
    readonly onEmployeeSelect: (employee: Employee) => any;
}
export const EmployeeSelectPage = (props: Props) => {
    const [state, setState] = useState({
        page: 0,
    })
    const employeesQuery = useEmployeesQuery({
        page: state.page,
        pageSize: pageSize,
    })

    return <Box
        sx={{
            display: "flex",
            flexDirection: "column",
            height: "100%",
            rowGap: 1,
        }}
    >
        <Box
            flex={1}
            display="flex"
            alignContent="center"
            flexWrap="wrap"
            justifyContent="center"
        >
            {
                employeesQuery.isFirstLoading || employeesQuery.data.length == 0
                ?
                <LoadingAnimation 
                    sx={{
                        width: {
                            xs: "50%",
                            sm: "25%",
                        }
                    }}
                />
                :
                <Grid 
                    container 
                    spacing={2}
                    sx={{
                        width: "100%",
                        height: "100%",
                        flex: 1,
                    }}
                >
                    {
                        employeesQuery.data.map((employee) => (
                            <Grid
                                size={{
                                    xs: 6,
                                    sm: 6,
                                    md: 4,
                                    lg: 4,
                                    xl: 3, 
                                }}
                                key={employee.id}
                            >
                                <EmployeeCard employee={employee} onClick={() => props.onEmployeeSelect(employee)}/>
                            </Grid>
                        ))
                    }
                </Grid>
            }
        </Box>
        {
            employeesQuery.isFirstLoading == false &&
            employeesQuery.totalPages > 1 &&
            <Box
                sx={{
                    width: "100%",
                }}
                flex={0}
            >
                <PaginationFooter
                    currentPage={state.page}
                    numberOfPages={employeesQuery.totalPages}
                    onPageChanged={p => setState(s => ({ ...s, page: p }))}
                />
            </Box>
        }
    </Box>
}

interface EmployeeCardProps {
    readonly employee: Employee;
    readonly onClick: () => any;
}
const EmployeeCard = (props: EmployeeCardProps) => {
    const imageUrl = useGenerateImage({
        name: props.employee.name,
        width: 400,
        height: 300,
    })

    return (
        <Card
            sx={{
                cursor: "pointer",
                width: "100%",
                transition: "transform 0.3s ease, box-shadow 0.3s ease",

                "&:hover": {
                    transform: "translateY(-4px) scale(1.02)",
                    boxShadow: theme => theme.shadows[14],
                },
            }}
            elevation={5}
            onClick={props.onClick}
        >
            <CardMedia
                component="img"
                image={imageUrl}
                alt={props.employee.name}
                sx={{
                    width: "100%",
                    objectFit: "cover",
                }}
            />
            <CardContent
                sx={{
                    paddingY: "0.75rem !important",
                }}
            >
                <Typography
                    variant="body1"
                    sx={{
                        color: 'text.secondary',
                    }}
                >
                    {props.employee.name}
                </Typography>
            </CardContent>
        </Card>
    );
}