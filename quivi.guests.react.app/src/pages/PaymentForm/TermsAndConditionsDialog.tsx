import Dialog from "../../components/Shared/Dialog";
import { makeStyles } from "@mui/styles";
import type { Theme } from "@mui/material/styles";

const useStyles = makeStyles<Theme>(() => ({
    contentContainer: {
        borderRadius: "15px 15px 0 0",
        backgroundColor: "white",
        margin: "0 auto",
        padding: "40px",
        position: "relative",
    },
    name: {
        fontWeight: 500,
        fontSize: "1.25rem",
        marginBottom: "1.25rem",
        textAlign: "center",
    },
    description: {
        textAlign: "center",
        overflowY: "auto",
        flex: 1,
    },
    divider: {
        borderBottom: "1px solid #DFE0DF",
        width: "35%",
        margin: "1.5rem auto",
    },
}));

interface Props {
    readonly isOpen: boolean;
    readonly onClose?: () => void;
}
export const TermsAndConditionsDialog = (props: Props) => {
    const classes = useStyles();

    return (
        <Dialog isOpen={props.isOpen} onClose={() => props.onClose?.()}>
            <div className={classes.contentContainer}>
                <div className="container" style={{padding: "unset", display: "flex", flexDirection: "column", height: "80vh"}}>
                    <h2 className={classes.name}>Termos Gerais de Utilização e Contratação</h2>
                    <div className={classes.description}>
                    </div>
                    <div className={classes.divider}></div>
                    <button className="secondary-button" onClick={props.onClose}>Fechar</button>
                </div>
            </div>
        </Dialog>
    );
}