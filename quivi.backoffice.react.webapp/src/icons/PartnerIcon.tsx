import { SVGProps } from "react";
import { ChargePartner } from "../hooks/api/Dtos/acquirerconfigurations/ChargePartner";
import { CheckoutIcon, PaybyrdIcon, QuiviIcon, SpgSibsIcon, StripeIcon, TicketIcon } from ".";

interface Props extends SVGProps<SVGSVGElement> {
    readonly partner: ChargePartner;
}

export const PartnerIcon = ({
    partner,
    ...props
}: Props) => {
    switch (partner) {
        case ChargePartner.Quivi: return <QuiviIcon {...props} />
        case ChargePartner.TicketRestaurant: return <TicketIcon {...props} />
        case ChargePartner.SibsPaymentGateway: return <SpgSibsIcon {...props} />
        case ChargePartner.Stripe: return <StripeIcon {...props} />
        case ChargePartner.Checkout: return <CheckoutIcon {...props} />
        case ChargePartner.Paybyrd: return <PaybyrdIcon {...props} />
    }
}