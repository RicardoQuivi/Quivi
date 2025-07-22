import { useTranslation } from 'react-i18next';
import { useEffect, useState } from "react";
import { Skeleton } from '@mui/material';

interface Props {
    readonly logo: string,
    readonly username?: string,
}

const getImage = (url: string, onLoad: () => any): HTMLImageElement => {
    const image = new Image();
    image.onload = onLoad;
    image.src = url;
    return image;
}

const MerchantHeader: React.FC<Props> = ({
    logo,
    username,
}) => {    
    const { t } = useTranslation();
    const [isLoading, setIsLoading] = useState(true);
    const [image, setImage] = useState<HTMLImageElement>();

    useEffect(() => {
        setIsLoading(true);
        setImage(getImage(logo, () => setIsLoading(false)));
    }, [logo])

    return (
        <div style={{display: "flex", alignItems: "center", justifyContent: "center", flexDirection: "column", height: "100%"}}>
            {
                username != undefined && username != "" &&
                <h2 style={{fontWeight: 400}}>{t("home.hello")}&nbsp;<span className="semi-bold">{username}</span></h2>
            }
            <div style={{
                    width: "100%", 
                    height: "100%", 
                    display: "flex", 
                    justifyContent: "center", 
                    alignItems: "center", 
                    flex: "1 1",
                    margin: "1rem 0",

                    backgroundImage: isLoading || image == undefined ? undefined : `url(${image.src})`,
                    backgroundSize: "contain",
                    backgroundRepeat: "no-repeat",
                    backgroundPosition: "center",
                }}>
                {isLoading && <Skeleton variant="rounded" style={{width: "80%", height: "65%"}}/>}
            </div>
        </div>
    );
};

export default MerchantHeader;
