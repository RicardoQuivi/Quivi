import { useTranslation } from 'react-i18next';
import { useEffect, useState } from "react";
import { Skeleton } from '@mui/material';

interface Props {
    readonly logo: string,
    readonly username?: string,
}

const loadImage = (url: string): Promise<HTMLImageElement> => new Promise((resolve, reject) => {
    const img = new Image();
    img.onload = () => resolve(img);
    img.onerror = (err) => reject(err);
    img.src = url;
});

const MerchantHeader: React.FC<Props> = (props: Props) => {    
    const { t } = useTranslation();
    const [image, setImage] = useState<HTMLImageElement>();

    useEffect(() => {
        setImage(undefined);
        loadImage(props.logo).then(setImage)
    }, [props.logo])

    const isLoading = image == undefined;

    return (
        <div style={{display: "flex", alignItems: "center", justifyContent: "center", flexDirection: "column", height: "100%"}}>
            {
                !!props.username &&
                <h2 style={{fontWeight: 400}}>{t("home.hello")}&nbsp;<span className="semi-bold">{props.username}</span></h2>
            }
            <div style={{
                    width: "100%", 
                    height: "100%", 
                    display: "flex", 
                    justifyContent: "center", 
                    alignItems: "center", 
                    flex: "1 1",
                    margin: "1rem 0",

                    backgroundImage: isLoading ? undefined : `url(${image.src})`,
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