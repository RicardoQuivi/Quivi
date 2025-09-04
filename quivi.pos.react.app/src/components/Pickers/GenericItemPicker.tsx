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
    const [leftChecked, setLeftChecked] = useState<T[]>(() => intersection([], unselectedItems, getItemKey));
    const [rightChecked, setRightChecked] = useState<T[]>(() => intersection([], selectedItems, getItemKey));

    useEffect(() => setLeftChecked(p => intersection(p, unselectedItems, getItemKey)), [unselectedItems])
    useEffect(() => setRightChecked(p => intersection(p, selectedItems, getItemKey)), [selectedItems])
  
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
        <Card sx={{height: "100%", display: "flex", flexDirection: "column", maxHeight: maxHeight ?? "unset"}}>
            {
                label &&
                <>
                    <CardHeader sx={{ px: 2, py: 1 }} title={label} />
                    <Divider sx={{flex: 1 }} />
                </>
            }
            <List
                dense
                component="div"
                role="list"
                sx={{
                    width: "100%",
                    height: "100%",
                    overflow: 'auto',
                    minHeight: 50,
                }}
            >
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
                                    <Checkbox
                                        checked={isChecked}
                                        tabIndex={-1}
                                        disableRipple
                                        slotProps={{
                                            input: {
                                                'aria-labelledby': labelId,
                                            }
                                        }}
                                    />
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
            <Grid
                size={{
                    xs: 12,
                    sm: "grow",
                }}
            >
                {customList(unselectedItems, leftChecked, setLeftChecked, unselectedLabel)}
            </Grid>
            <Grid
                size={{
                    xs: 12,
                    sm: "auto",
                }}
            >
                <Grid 
                    container
                    sx={{
                        width: {
                            xs: "100%",
                            sm: "min-content",
                        },

                        "& .MuiButtonBase-root": {
                            "& .MuiTypography-root": {
                                transform: "rotate(90deg)",
                            }
                        }
                    }}
                    gap={1}
                >
                    <Grid
                        size={{
                            xs: "grow",
                            sm: 12,
                        }}
                    >
                        <Button
                            variant="outlined"
                            size="small"
                            onClick={() => onItemsAdded(unselectedItems, true)} disabled={selectButtonsDisable == true || unselectedItems.length == 0}
                            aria-label="move all right"
                        >
                            <Typography variant="button" gutterBottom>
                            ≫
                            </Typography>
                        </Button>
                    </Grid>

                    <Grid
                        size={{
                            xs: "grow",
                            sm: 12,
                        }}
                    >
                        <Button
                            variant="outlined"
                            size="small"
                            onClick={() => onItemsAdded(leftChecked, false)} disabled={selectButtonsDisable == true || leftChecked.length == 0}
                            aria-label="move selected right"
                        >
                            <Typography variant="button" gutterBottom>
                                &gt;
                            </Typography>
                        </Button>
                    </Grid>

                    <Grid
                        size={{
                            xs: "grow",
                            sm: 12,
                        }}
                    >
                        <Button
                            variant="outlined"
                            size="small"
                            onClick={() => onItemsRemoved(rightChecked, false)} disabled={unselectButtonsDisable == true || rightChecked.length == 0}
                            aria-label="move selected left"
                        >
                            <Typography variant="button" gutterBottom>
                                &lt;
                            </Typography>
                        </Button>
                    </Grid>

                    <Grid
                        size={{
                            xs: "grow",
                            sm: 12,
                        }}
                    >
                        <Button
                            variant="outlined"
                            size="small"
                            onClick={() => onItemsRemoved(selectedItems, true)} disabled={unselectButtonsDisable == true || selectedItems.length == 0}
                            aria-label="move all left"
                        >
                            <Typography variant="button" gutterBottom>
                                ≪
                            </Typography>
                        </Button>
                    </Grid>
                </Grid>
            </Grid>
            <Grid
                size={{
                    xs: 12,
                    sm: "grow",
                }}
            >
                {customList(selectedItems, rightChecked, setRightChecked, selectedLabel)}
            </Grid>
        </Grid>
    )
}