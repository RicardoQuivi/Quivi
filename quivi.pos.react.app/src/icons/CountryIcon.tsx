import { PortugalFlagIcon, UnitedKingdomFlagIcon } from ".";

interface Props extends React.SVGProps<SVGSVGElement>{
    readonly language: string;
}
export const CountryIcon = ({
    language,
    ...props
}: Props) => {

    if(language == "en") {
        return <UnitedKingdomFlagIcon {...props}/>
    }

    if(language == "pt") {
        return <PortugalFlagIcon {...props}/>
    }
    
    return <></>
}