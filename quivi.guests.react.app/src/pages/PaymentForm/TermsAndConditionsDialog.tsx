import Dialog from "../../components/Shared/Dialog";
import { Box } from "@mui/material";

interface Props {
    readonly isOpen: boolean;
    readonly onClose: () => any;
}
export const TermsAndConditionsDialog = (props: Props) => {
    return (
        <Dialog
            isOpen={props.isOpen}
            onClose={props.onClose}
        >
            <Box
                sx={{
                    borderRadius: "15px 15px 0 0",
                    backgroundColor: "white",
                    margin: "0 auto",
                    padding: "40px",
                    position: "relative",
                }}
            >
                <Box
                    className="container"
                    sx={{
                        padding: "unset",
                        display: "flex",
                        flexDirection: "column",
                        height: "80vh"
                    }}
                >
                    <h2
                        style={{
                            fontWeight: 500,
                            fontSize: "1.25rem",
                            marginBottom: "1.25rem",
                            textAlign: "center",
                        }}
                    >
                        Termos Gerais de Utilização e Contratação
                    </h2>
                    <Box
                        sx={{
                            textAlign: "center",
                            overflowY: "auto",
                            flex: 1,
                        }}
                    />
                    <Box
                        sx={{
                            borderBottom: "1px solid #DFE0DF",
                            width: "35%",
                            margin: "1.5rem auto",
                        }}
                    />
                    <button className="secondary-button" onClick={props.onClose}>Fechar</button>
                </Box>
            </Box>
        </Dialog>
    );
}