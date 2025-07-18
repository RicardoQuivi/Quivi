import React, { useEffect, useRef } from "react";
import { useQuiviTheme, type IColor } from "../../hooks/theme/useQuiviTheme";
import { Paper, styled, Tab, Tabs, type TabsActions } from "@mui/material";

interface StyledPaperProps {
    readonly primarycolor: IColor;
}
 
const StyledPaper = styled(Paper)(({
    primarycolor,
}: StyledPaperProps) => ({
    boxShadow: "unset",
    '& .MuiTabs-root': {
        position: "relative",
        
        "&.MuiTabs-vertical": {
            '& .MuiTabScrollButton-root': {
                width: "100%",
    
                "&:first-child": {
                    top: 0,
                    background: `linear-gradient(to bottom, rgba(255, 255, 255, 1) 0%, rgba(255, 255, 255, 1) 20%, rgba(255, 255, 255, 0.8) 60%, rgba(255, 255, 255, 0) 100%) !important`,
                },
    
                "&:last-child": {
                    bottom: 0,
                    background: `linear-gradient(to top, rgba(255, 255, 255, 1) 0%, rgba(255, 255, 255, 1) 20%, rgba(255, 255, 255, 0.8) 60%, rgba(255, 255, 255, 0) 100%)`,
                },
            },

            '& .MuiTabs-scroller': {           
                '& .MuiTabs-indicator': {
                    background: `linear-gradient(to bottom, ${primarycolor.hex} 0%, ${primarycolor.hex} 50%, rgba(0, 0, 0, 0) 50%)`,
                    width: '6px',
                    right: '6px',
                },
            }
        },

        "&:not(.MuiTabs-vertical)": {
            '& .MuiTabScrollButton-root': {
                height: "100%",
    
                "&:first-child": {
                    left: 0,
                    background: `linear-gradient(to right, rgba(255, 255, 255, 1) 0%, rgba(255, 255, 255, 1) 20%, rgba(255, 255, 255, 0.8) 60%, rgba(255, 255, 255, 0) 100%) !important`,
                },
    
                "&:last-child": {
                    right: 0,
                    background: `linear-gradient(to left, rgba(255, 255, 255, 1) 0%, rgba(255, 255, 255, 1) 20%, rgba(255, 255, 255, 0.8) 60%, rgba(255, 255, 255, 0) 100%)`,
                },
            },

            '& .MuiTabs-scroller': {           
                '& .MuiTabs-indicator': {
                    background: `linear-gradient(to right, ${primarycolor.hex} 0%, ${primarycolor.hex} 50%, rgba(0, 0, 0, 0) 50%)`,
                    height: '6px',
                    bottom: '6px',
                },
            }
        },

        '& .MuiTabScrollButton-root': {
            width: "20px",
            transition: "width, opacity ease",
            transitionDuration: "0.35s",
            zIndex: 1,
            opacity: 1,

            position: "absolute",

            "&.Mui-disabled": {
                opacity: 0,
            },
        },

        '& .MuiTabScrollButton-root.Mui-disabled': {
            width: 0,
        },

        '& .MuiTabs-scroller': {
            '& .MuiTabs-indicator': {
                backgroundColor: 'unset',
            },

            '& .MuiTabs-flexContainer': {
                '& > *': {
                    margin: "7px",

                    '&:first-child': {
                        marginLeft: '0px',
                    },
                    
                    '&:last-child': {
                        marginRight: '0px',
                    },
                },
    
                '& .MuiTab-root': {
                    padding: 0,
                    minWidth: 'unset',
                    fontFamily: 'unset'
                },
    
                '& .MuiButtonBase-root': {
                    marginTop: '0px',
                    marginBottom: '0px',
    
                    '& .MuiTab-wrapper': {
                        textTransform: 'capitalize',
                        padding: '5px'
                    },
    
                    '&.Mui-selected': {
                        '& > .MuiTab-wrapper': {
                            background: `rgba(${primarycolor.r}, ${primarycolor.g}, ${primarycolor.b}, 0.15)`,
                            borderRadius: '4px',
                        },
                    }
                },
    
                '& .MuiTab-textColorPrimary.Mui-selected': {
                    color: '#000000',
                    fontWeight: 'bold',
                }
            }
        }
    },
}));

interface Props<T> {
    readonly orientation?: "horizontal" | "vertical";
    readonly tabs: T[];
    readonly selectedTab?: T;
    readonly onTabSelected?: (tab: T) => void;

    readonly getKey: (t: T) => string;
    readonly getValue: (t: T) => React.ReactNode;
}
const TabOptions = <T,>({
    orientation,
    tabs,
    selectedTab,
    onTabSelected,

    getKey,
    getValue,
}: Props<T>) => {
    const theme = useQuiviTheme();
    const ref = useRef<TabsActions>(null);

    useEffect(() => {
        if(!ref.current) {
            return;
        }

        //Workaround: MUI has an issue with animations
        //which cause the first time tabs are rendered
        //the indicator will not appear. Here we explicitly
        //tell MUI to update indicator each second. Allegedly solved in V5.
        const timeout = setInterval(() => {
            ref.current?.updateIndicator()
        }, 1000);
        return () => {
            clearInterval(timeout);
        }
    }, [ref]);

    return (
        <StyledPaper primarycolor={theme.primaryColor}>
            <Tabs
                action={ref}
                value={selectedTab != undefined ? (getKey(selectedTab) ?? false) : false}
                onChange={(_, v: string) => onTabSelected?.(tabs.find(t => getKey(t) == v)!)}
                indicatorColor="primary"
                textColor="primary"
                variant="scrollable"
                orientation={orientation}
                allowScrollButtonsMobile
            >
                {
                    tabs.map(c => {
                        const key = getKey(c);
                        return <Tab label={getValue(c)} value={key} key={key} disableRipple={true}/>
                    })
                }
            </Tabs>
        </StyledPaper>
    )
}
export default TabOptions;