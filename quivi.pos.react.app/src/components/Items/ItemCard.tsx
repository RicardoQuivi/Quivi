import { Badge, Box, ImageListItem, ImageListItemBar, Skeleton, badgeClasses, useTheme } from "@mui/material";
import React, { useMemo } from "react";
import { useGenerateImage } from "../../hooks/useGenerateImage";
import CurrencySpan from "../Currency/CurrencySpan";

export const ItemCard = (props: {
    readonly image?: string;
    readonly name?: string;
    readonly price?: number;

    readonly onClick?: () => any;
    readonly children?: React.ReactNode;
    readonly selectedQuantity?: number;
}) => {
    const width = 400;
    const height = 300;

    const borderRadius = "0.75rem";

    const generatedImageUrl = useGenerateImage({
        name: props.image == undefined ? props.name : undefined,
        width: width,
        height: height,
    })

    const imageUrl = useMemo(() => {
        if (props.image == undefined) {
            return generatedImageUrl;
        }
        return props.image;
    }, [props.image, generatedImageUrl])

    const theme = useTheme();
    const rgbaColor = useMemo(() => {
        const primaryRgb = parseColorToRgb(theme.palette.primary.main);
        return `rgba(${primaryRgb[0]}, ${primaryRgb[1]}, ${primaryRgb[2]}, 1)`;
    }, [theme.palette.primary.main]);

    return (
        <Badge
            badgeContent={props.selectedQuantity ?? 0}
            color="primary"
            sx={{
                cursor: props.onClick != undefined ? "pointer" : undefined,
                margin: "1rem",
                paddingTop: "0.5rem",
                paddingBottom: "0.5rem",
                minHeight: 0,

                [`& .${badgeClasses.badge}`]: {
                    fontSize: "1rem",
                    padding: "0.75rem",
                },
            }}
        >
            <ImageListItem
                sx={{
                    width: "100%",
                }}
                onClick={props.onClick}
            >
                <Box
                    sx={{
                        position: "relative",
                        transition: "box-shadow 0.3s ease-in-out, border 0.3s ease-in-out",
                        borderRadius: borderRadius,

                        boxShadow: () => {
                            if ((props.selectedQuantity ?? 0) <= 0) {
                                return undefined;
                            }

                            return `0 0px 8px 0 ${rgbaColor}, 0 0px 20px 0 ${rgbaColor}`;
                        },
                    }}
                >
                    {
                        imageUrl == undefined
                        ?
                        <Skeleton variant="rounded" height={200} animation="wave" />
                        :
                        <img
                            src={imageUrl}
                            alt={props.name}
                            loading="lazy"
                            style={{
                                aspectRatio: width/height,
                                objectFit: "cover",
                                width: "100%",
                                height: "auto",
                                display: "block",
                                WebkitBoxFlex: "1",
                                WebkitFlexGrow: "1",
                                msFlexPositive: 1,
                                flexGrow: 1,
                                borderRadius: borderRadius,
                            }}
                        />
                    }
                    <ImageListItemBar
                        sx={{
                            borderBottomLeftRadius: borderRadius,
                            borderBottomRightRadius: borderRadius,
                        }}
                        title={props.name == null ? <Skeleton animation="wave" /> : props.name}
                        subtitle={props.price == null ? <Skeleton animation="wave" /> : <CurrencySpan value={props.price} />}
                    />
                </Box>
                {props.children}
            </ImageListItem>
        </Badge>
    )
}

const parseColorToRgb = (color: string) => {
    if (!color) {
        throw new Error();
    }

    // Handle rgb/rgba directly
    if (color.startsWith('rgb')) {
        const match = color.match(/rgba?\((\d+),\s*(\d+),\s*(\d+)/);
        if (match) {
            return [parseInt(match[1]), parseInt(match[2]), parseInt(match[3])];
        }
    }

    let hex = color.replace('#', '');
    if (hex.length === 3) {
        hex = hex.split('').map(h => h + h).join('');
    }
    if (hex.length === 6) {
        const r = parseInt(hex.slice(0, 2), 16);
        const g = parseInt(hex.slice(2, 4), 16);
        const b = parseInt(hex.slice(4, 6), 16);
        return [r, g, b];
    }

    // Fallback
    throw new Error();
};