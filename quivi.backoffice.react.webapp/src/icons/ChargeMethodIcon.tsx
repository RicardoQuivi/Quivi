import { SVGProps } from "react";
import { ChargeMethod } from "../hooks/api/Dtos/ChargeMethod";
import { CashIcon, MbWayIcon, PaymentTerminalIcon, TicketRestaurantIcon, VisaCreditCardIcon, WalletIcon } from ".";

interface Props extends SVGProps<SVGSVGElement> {
    readonly chargeMethod: ChargeMethod;
}

export const ChargeMethodIcon = ({
    chargeMethod,
    ...props
}: Props) => {
    switch (chargeMethod) {
        case ChargeMethod.Custom: throw new Error();
        case ChargeMethod.Wallet: return <WalletIcon {...props} />
        case ChargeMethod.Cash: return <CashIcon {...props} />
        case ChargeMethod.TicketRestaurantMobile: return <TicketRestaurantIcon {...props} />
        case ChargeMethod.MbWay: return <MbWayIcon {...props} />
        case ChargeMethod.CreditCard: return <VisaCreditCardIcon {...props} />
        case ChargeMethod.PaymentTerminal: return <PaymentTerminalIcon {...props} />
    }
}