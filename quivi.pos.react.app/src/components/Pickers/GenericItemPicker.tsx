import React, { useEffect, useState } from 'react';
import { Button, Card, CardHeader, Checkbox, Divider, List, ListItemButton, ListItemIcon, ListItemText, Typography } from '@mui/material';
import Grid from '@mui/material/Grid';

const intersection = <T,>(a: T[], b: readonly T[], getItemKey: (item: T) => string): T[] => b.filter((value) => a.findIndex(a => getItemKey(a) == getItemKey(value)) !== -1);

interface Props<T> {
    readonly unselectedItems: T[];
    readonly selectedItems: T[];
    readonly getItemName: (item: T) => React.ReactNode;
    readonly getItemKey: (item: T) => string;
    readonly getItemStyle: (item: T) => React.CSSProperties | undefined;
    readonly onItemsAdded: (item: T[], isAll: boolean) => any;
    readonly onItemsRemoved: (item: T[], isAll: boolean) => any;
    readonly unselectedLabel?: React.ReactNode;
    readonly selectedLabel?: React.ReactNode;
    readonly selectButtonsDisable?: boolean;
    readonly unselectButtonsDisable?: boolean;
    readonly maxHeight?: string;
}

export const GenericItemPicker = <T,>({
    unselectedItems,
    selectedItems,
    getItemName,
    getItemKey,
    getItemStyle,
    onItemsAdded,
    onItemsRemoved,
    unselectedLabel,
    selectedLabel,
    selectButtonsDisable,
    unselectButtonsDisable,
    maxHeight,
}: Props<T>) => {
    const [leftChecked, setLeftChecked] = useState<T[]>([]);
    const [rightChecked, setRightChecked] = useState<T[]>([]);

    useEffect(() => {
        setLeftChecked(p => intersection(p, unselectedItems, getItemKey));
    }, [unselectedItems])

    useEffect(() => {
        setRightChecked(p => intersection(p, selectedItems, getItemKey));
    }, [selectedItems])
  
    const handleToggle = (value: T, checked: T[], action: (result: T[]) => any) => () => {
        const currentIndex = checked.findIndex(v => getItemKey(v) == getItemKey(value));
        const newChecked = [...checked];
    
        if (currentIndex == -1) {
            newChecked.push(value);
        } else {
            newChecked.splice(currentIndex, 1);
        }
    
        action(newChecked);
    };
  
    const customList = (items: readonly T[], checked: T[], action: (result: T[]) => any, label: React.ReactNode | undefined) => (
        <Card style={{height: "100%", display: "flex", flexDirection: "column", maxHeight: maxHeight ?? "unset"}}>
            {
                label &&
                <>
                    <CardHeader sx={{ px: 2, py: 1 }} title={label} />
                    <Divider style={{flex: "1 1 auto"}} />
                </>
            }
            <List dense component="div" role="list" sx={{ width: "100%", height: "100%", overflow: 'auto' }}>
                {
                    items.map((item: T) => {
                        const key = getItemKey(item);
                        const labelId = `transfer-list-item-${key}-label`;
                        const isChecked = checked.findIndex(c => getItemKey(c) == key) !== -1;
                        return (
                            <ListItemButton
                                key={key}
                                role="listitem"
                                onClick={handleToggle(item, checked, action)}
                            >
                                <ListItemIcon>
                                    <Checkbox checked={isChecked} tabIndex={-1} disableRipple inputProps={{'aria-labelledby': labelId, }} />
                                </ListItemIcon>
                                <ListItemText id={labelId}>
                                    <Typography style={getItemStyle(item)}>
                                        {getItemName(item)}
                                    </Typography>
                                </ListItemText>  
                            </ListItemButton>
                        );
                    })
                }
            </List>
        </Card>
    );

    return (
        <Grid container spacing={2} justifyContent="center" alignItems="stretch">
            <Grid size={{xs: 5}}>
                {customList(unselectedItems, leftChecked, (r) => setLeftChecked(r), unselectedLabel)}
            </Grid>
            <Grid size={{xs: 2}}>
                <Grid container direction="column" alignItems="center" justifyContent="center" style={{height: "100%"}}>
                    <Button sx={{ my: 0.5 }} variant="outlined" size="small" onClick={() => onItemsAdded(unselectedItems, true)} disabled={selectButtonsDisable == true || unselectedItems.length == 0} aria-label="move all right">
                        ≫
                    </Button>
                    <Button sx={{ my: 0.5 }} variant="outlined" size="small" onClick={() => onItemsAdded(leftChecked, false)} disabled={selectButtonsDisable == true || leftChecked.length == 0} aria-label="move selected right">
                        &gt;
                    </Button>
                    <Button sx={{ my: 0.5 }} variant="outlined" size="small" onClick={() => onItemsRemoved(rightChecked, false)} disabled={unselectButtonsDisable == true || rightChecked.length == 0} aria-label="move selected left">
                        &lt;
                    </Button>
                    <Button sx={{ my: 0.5 }} variant="outlined" size="small" onClick={() => onItemsRemoved(selectedItems, true)} disabled={unselectButtonsDisable == true || selectedItems.length == 0} aria-label="move all left">
                        ≪
                    </Button>
                </Grid>
            </Grid>
            <Grid size={{xs: 5}}>
                {customList(selectedItems, rightChecked, (r) => setRightChecked(r), selectedLabel)}
            </Grid>
        </Grid>
    )
}