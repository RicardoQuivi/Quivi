import { useMemo } from "react";
import { Avatar } from "@mui/material";
import { useGeneratedColor } from "../../hooks/useGeneratedColor";
import { MenuCategory } from "../../hooks/api/Dtos/menucategories/MenuCategory";

interface Props {
    readonly category: MenuCategory;
}
export const CategoryAvatar = (props: Props) => {
    const color = useGeneratedColor(props.category.imageUrl != undefined ? undefined : props.category.name ?? "");

    const initials = useMemo(() => {
        const nameSplit = props.category.name.split(' ');
        const initials = nameSplit.length > 1 ? `${nameSplit[0][0]}${nameSplit[1][0]}` : nameSplit[0][0];
        return initials;
    }, [props.category.name])
    
    if(props.category.imageUrl != undefined) {
        <Avatar alt={props.category.name} src={props.category.imageUrl} />
    }

    return <Avatar sx={{ bgcolor: color, }} alt={props.category.name}>{initials}</Avatar>
}