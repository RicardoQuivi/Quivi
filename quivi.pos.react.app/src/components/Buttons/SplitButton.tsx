import React from "react";
import { Button, ButtonGroup, ClickAwayListener, Grow, MenuItem, MenuList, Paper, Popper } from "@mui/material";
import { ChevronDownIcon } from "../../icons";

export interface ISplitButtonOption {
    readonly onClick: () => void;
    readonly children: React.ReactNode;
}

interface SplitButtonProps {
    readonly onClick: () => void;
    readonly isLoading?: boolean;
    readonly isDisabled?: boolean;
    readonly style?: React.CSSProperties;
    readonly children: React.ReactNode;
    readonly options: ISplitButtonOption[];
    readonly variant?: 'text' | 'outlined' | 'contained';
}

const SplitButton: React.FC<SplitButtonProps> = (props) => {
    const [isOpen, setIsOpen] = React.useState(false);
    const anchorRef = React.useRef<HTMLDivElement>(null);
  
    const handleClick = () => {
        setIsOpen(false);
        props.onClick();
    };
  
    const handleMenuItemClick = (item: ISplitButtonOption) => {
        item.onClick();
        setIsOpen(false);
    };
  
    const handleToggle = () => {
        setIsOpen((prevOpen) => !prevOpen);
    };
  
    const handleClose = (event: Event) => {
        if (
            anchorRef.current &&
            anchorRef.current.contains(event.target as HTMLElement)
        ) {
            return;
        }

        setIsOpen(false);
    };
  
    return (
    <React.Fragment>
        <ButtonGroup 
            disabled={props.isDisabled}
            ref={anchorRef}
            style={props.style}
            sx={{width: "100%"}}
            variant={props.variant}
        >
            <Button variant={props.variant} sx={{width: "100%"}} loading={props.isLoading} disabled={props.isDisabled} onClick={handleClick}>
                {props.children}
            </Button>
            <Button variant={props.variant} size="small" loading={props.isLoading} disabled={props.isDisabled} onClick={handleToggle}>
                <ChevronDownIcon height={25} width={25} />
            </Button>
        </ButtonGroup>
        <Popper
            sx={{zIndex: 1}}
            open={isOpen}
            anchorEl={anchorRef.current}
            role={undefined}
            transition
            disablePortal
        >
            {({ TransitionProps, placement }) => (
            <Grow
                {...TransitionProps}
                style={{
                    transformOrigin: placement === 'bottom' ? 'center top' : 'center bottom',
                }}
            >
                <Paper>
                    <ClickAwayListener onClickAway={handleClose}>
                        <MenuList autoFocusItem>
                        {props.options.map((option, index) => (
                            <MenuItem
                                sx={{
                                    height: anchorRef.current?.clientHeight, 
                                    width: anchorRef.current?.clientWidth,
                                    justifyContent: "center",
                                }}
                                key={index}
                                onClick={() => {
                                    handleMenuItemClick(option);
                                }}
                            >
                                {option.children}
                            </MenuItem>
                        ))}
                        </MenuList>
                    </ClickAwayListener>
                </Paper>
            </Grow>
            )}
        </Popper>
    </React.Fragment>
    )
}
export default SplitButton;