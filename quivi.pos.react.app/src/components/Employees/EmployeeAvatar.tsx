import { useMemo } from "react";
import { Avatar } from "@mui/material";
import { Employee } from "../../hooks/api/Dtos/employees/Employee";
import { useGeneratedColor } from "../../hooks/useGeneratedColor";

interface Props {
    readonly employee: Employee;
}
export const EmployeeAvatar = (props: Props) => {
    const color = useGeneratedColor(props.employee.name ?? "");

    const initials = useMemo(() => {
        const nameSplit = props.employee.name.split(' ');
        const initials = nameSplit.length > 1 ? `${nameSplit[0][0]}${nameSplit[1][0]}` : nameSplit[0][0];
        return initials;
    }, [props.employee.name])
    
    return <Avatar sx={{ bgcolor: color, }}>{initials}</Avatar>
}