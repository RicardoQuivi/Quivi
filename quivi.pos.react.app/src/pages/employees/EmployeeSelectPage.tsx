import { CircularProgress, Grid, ListItem, ListItemAvatar, ListItemButton, ListItemText } from "@mui/material"
import { Employee } from "../../hooks/api/Dtos/employees/Employee";
import { useEmployeesQuery } from "../../hooks/queries/implementations/useEmployeesQuery";
import { EmployeeAvatar } from "../../components/Employees/EmployeeAvatar";

interface Props {
    readonly onEmployeeSelect: (employee: Employee) => any;
}
export const EmployeeSelectPage = (props: Props) => {
    const employeesQuery = useEmployeesQuery({
        page: 0,
        pageSize: 100,
    })
    
    const handleListItemClick = (value: Employee) => props.onEmployeeSelect(value);

    return <Grid container spacing={2}>
        <Grid size={12} sx={{display: "flex", justifyContent: "center"}}>
            { 
                employeesQuery.isFirstLoading
                ?
                <CircularProgress color="primary" />
                :
                <Grid container spacing={2}>
                {
                    employeesQuery.data.map((employee) => (
                        <Grid size="auto" key={employee.id}>
                            <ListItem disableGutters>
                                <ListItemButton onClick={() => handleListItemClick(employee)}>
                                    <ListItemAvatar>
                                        <EmployeeAvatar employee={employee} />
                                    </ListItemAvatar>
                                    <ListItemText primary={employee.name} />
                                </ListItemButton>
                            </ListItem>
                        </Grid>
                    ))
                }
                </Grid>
            }
        </Grid>
    </Grid>
}