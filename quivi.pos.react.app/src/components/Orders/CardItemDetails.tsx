import { Divider, List, ListItemButton, ListItemIcon, ListItemSecondaryAction, ListItemText, Skeleton } from "@mui/material";
import React from "react";

interface CardItemDetailsProps<T> {
    readonly items: T[] | undefined;
    readonly header?: React.ReactNode;
    readonly hideQuantityAndActionsIfZero?: boolean;

    readonly getId: (item: T) => string;
    readonly getSubItems?: (item: T) => T[];
    readonly getQuantity?: (item: T) => number;

    readonly renderName: (item: T) => React.ReactNode;
    readonly renderExtraAction?: (item: T) => React.ReactNode;

    readonly onItemClicked?: (item: T) => any;
}
export const CardItemDetails = <T,>(props: CardItemDetailsProps<T>) => {
    return <>
        {
            props.header != undefined &&
            <Divider>{props.header}</Divider>
        }
        <List disablePadding dense sx={{
            "& .MuiListItem-dense": {
                paddingTop: 0,
                paddingBottom: 0,
            },
            "& .MuiListItemIcon-root": {
                minWidth: 30,
            }
        }}>
            {
                props.items == undefined
                ?
                [0, 1, 2].map(i => <CardListItem 
                    key={`Loading-${i}`}

                    getId={props.getId}

                    renderName={props.renderName}
                />)
                :
                props.items.map(item => <CardListItem 
                    key={props.getId(item)} 
                    item={item}
                    hideQuantityAndActionsIfZero={props.hideQuantityAndActionsIfZero}

                    getId={props.getId}
                    getSubItems={props.getSubItems}
                    getQuantity={props.getQuantity}

                    renderName={props.renderName}
                    renderExtraAction={props.renderExtraAction}                    

                    onItemClick={props.onItemClicked}
                />)
            }
        </List>
    </>
}

interface CardListItemProps<T> {
    readonly item?: T;
    readonly hideQuantityAndActionsIfZero?: boolean;
    
    readonly getId: (item: T) => string;
    readonly getSubItems?: (item: T) => T[];
    readonly getQuantity?: (item: T) =>  number;

    readonly renderName: (item: T) => React.ReactNode;
    readonly renderExtraAction?: (item: T) => React.ReactNode;

    readonly onItemClick?: (item: T) => any;
}
const CardListItem = <T,>(props: CardListItemProps<T>) => {

    const extras = props.item != undefined && props.getSubItems != undefined ? props.getSubItems(props.item) : [];

    const renderExtras = () => {
        if(extras.length == 0) {
            return undefined;
        }
                 
        return <>
            <div style={{marginLeft: "2.5rem"}}>
                <CardItemDetails 
                    items={extras}
                    hideQuantityAndActionsIfZero={props.hideQuantityAndActionsIfZero}

                    getId={props.getId}
                    getQuantity={props.getQuantity} 
                    getSubItems={undefined}

                    renderName={props.renderName}
                    renderExtraAction={props.renderExtraAction} 

                    onItemClicked={props.onItemClick} 
                />
            </div>
        </>
    }
    
    const extraAction = props.item != undefined && props.renderExtraAction?.(props.item);
    const quantity = props.item != undefined && props.getQuantity != undefined ? props.getQuantity(props.item) : undefined;
    const noActions = !(props.hideQuantityAndActionsIfZero != true || (quantity != undefined && quantity != 0));
    const canClick = props.onItemClick != undefined && props.item != undefined && !noActions;

    return <>
        <ListItemButton
            onClick={(evt) => { 
                evt.stopPropagation(); 
                props.onItemClick?.(props.item!); 
            }}
            sx={
                canClick 
                ?
                {
                    cursor: "pointer"
                }
                :
                {
                    pointerEvents: "none",
                }
            }
        >
                {
                    quantity != undefined &&
                    !noActions &&
                    <ListItemIcon>
                        {
                            props.item == undefined
                            ?
                                <Skeleton animation="wave" />
                            :
                                <b style={{color: "black"}}>{quantity}</b>
                        }
                    </ListItemIcon>
                }
                <ListItemText
                    primary={props.item == undefined ? <Skeleton animation="wave" /> : props.renderName(props.item)}
                    style={extraAction != undefined && !noActions ? {marginRight: 25} : undefined}
                />
                {
                    extraAction != undefined && 
                    !noActions &&
                    <ListItemSecondaryAction children={extraAction} />
                }
        </ListItemButton>
        { renderExtras() }
    </>
}