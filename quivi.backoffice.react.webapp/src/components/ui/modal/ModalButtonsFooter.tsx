import Button from "../button/Button";

interface ModalButtonProps {
    readonly onClick?: () => any;
    readonly content: React.ReactNode;
    readonly disabled?: boolean;
    readonly isLoading?: boolean;
}

interface ModalButtonsFooterProps {
    readonly primaryButton: ModalButtonProps;
    readonly secondaryButton?: ModalButtonProps;
}
export const ModalButtonsFooter = (props: ModalButtonsFooterProps) => {
    return (
    <div className="flex items-center gap-3 px-2 mt-6 lg:justify-end w-full">
        {
            props.secondaryButton != undefined &&
            <Button
                size="sm"
                variant="outline"
                onClick={props.secondaryButton.onClick}
                className="w-full"
                disabled={props.secondaryButton.disabled}
                isLoading={props.secondaryButton.isLoading}
            >
                {props.secondaryButton.content}
            </Button>
        }
        <Button
            size="sm"
            onClick={props.primaryButton.onClick}
            className="w-full"
            disabled={props.primaryButton.disabled}
            isLoading={props.primaryButton.isLoading}
        >
            {props.primaryButton.content}
        </Button>
    </div>
    )
}