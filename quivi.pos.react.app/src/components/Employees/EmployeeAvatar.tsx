import { useEffect, useState } from "react";
import { Avatar } from "@mui/material";
import { Employee } from "../../hooks/api/Dtos/employees/Employee";

interface Props {
    readonly employee: Employee;
}
export const EmployeeAvatar = (props: Props) => {
    const [initials, setInitials] = useState("");
    const [color, setColor] = useState("#FFFFFF");

    useEffect(() => {
        const nameSplit = props.employee.name.split(' ');
        const initials = nameSplit.length > 1 ? `${nameSplit[0][0]}${nameSplit[1][0]}` : nameSplit[0][0];
        setInitials(initials);
    }, [props.employee])

    useEffect(() => {
        let hash = 0;
        /* eslint-disable no-bitwise */
        for (let i = 0; i < props.employee.name.length; i += 1) {
          hash = props.employee.name.charCodeAt(i) + ((hash << 5) - hash);
        }
      
        let color = '#';
        for (let i = 0; i < 3; i += 1) {
          const value = (hash >> (i * 8)) & 0xff;
          color += `00${value.toString(16)}`.slice(-2);
        }
        /* eslint-enable no-bitwise */

        setColor(color);
    }, [props.employee])
    return <Avatar sx={{ bgcolor: color, }}>{initials}</Avatar>
}