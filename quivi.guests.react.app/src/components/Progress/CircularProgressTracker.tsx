import { Box, CircularProgress, Typography } from "@mui/material";
import { useEffect, useMemo, useState } from "react";
import { useQuiviTheme } from "../../hooks/theme/useQuiviTheme";

interface ProgressProps {
    readonly totalMinutes: number;
    readonly startDate: Date;
}

export const CircularProgressTracker: React.FC<ProgressProps> = ({
    totalMinutes,
    startDate,
}) => {
    const theme = useQuiviTheme();

    const expirationDate = useMemo(() => {
        let date = new Date(startDate);
        date.setMinutes(startDate.getMinutes() + totalMinutes);
        return date;
    }, [startDate])

    const [ellapsedSeconds, setEllapsedSeconds] = useState<number>(totalMinutes * 60.0 - (new Date().getTime() - startDate.getTime()) / 1000.0)
    
    useEffect(() => {
        const interval = setInterval(() => {
            setEllapsedSeconds(totalMinutes * 60.0 - (new Date().getTime() - startDate.getTime()) / 1000.0);
        }, 1000);
        return () => clearInterval(interval);
    }, [])
    
    const toTimeString = (totalSeconds: number) => new Date(totalSeconds * 1000).toISOString().slice(14, 19)

    const value = (new Date().getTime() - startDate.getTime())/(expirationDate.getTime() - startDate.getTime())*100.0;
    const label = toTimeString(ellapsedSeconds);

    return (
    <Box 
        position="relative"
        display="inline-flex"
        sx={{
            '& .MuiCircularProgress-circle': {
                color: theme.primaryColor.hex,
            },
            '& .MuiCircularProgress-root': {
                width: "100% !important",
                height: "100% !important"
            },
            '& .MuiTypography-root': {
                fontSize: "1rem",
                fontWeight: "bolder",
            },
            width: "100%",
            height: "100%",
        }}
    >
        { value <= 100  && <CircularProgress variant="determinate" value={value} color="primary" /> }
        { value > 100  && <CircularProgress color="primary" /> }
        <Box
            top={0}
            left={0}
            bottom={0}
            right={0}
            position="absolute"
            display="flex"
            alignItems="center"
            justifyContent="center"
        >
            { 
                value <= 100 &&
                label != undefined &&
                <Typography variant="caption" component="div" color="textSecondary">{label}</Typography>
            }
        </Box>
    </Box>
    );
}