import { CashIcon, CreditCardIcon, MbWayIcon, QuiviIcon, TicketRestaurantIcon } from ".";
import { ChargeMethod } from "../hooks/api/Dtos/ChargeMethod";

interface ChargeMethodIconProps extends React.SVGProps<SVGSVGElement> {
  readonly chargeMethod: ChargeMethod;
}

export const ChargeMethodIcon = (props: ChargeMethodIconProps): React.ReactNode => {
  const {
    chargeMethod,
    ...rProps
  } = props;

  switch(chargeMethod)
  {
    case ChargeMethod.Cash: return <CashIcon {...rProps} />
    case ChargeMethod.TicketRestaurantMobile: return <TicketRestaurantIcon {...rProps} />
    case ChargeMethod.PaymentTerminal:
    case ChargeMethod.CreditCard: return <CreditCardIcon {...rProps} />
    case ChargeMethod.Wallet: return <QuiviIcon {...rProps} />;
    case ChargeMethod.MbWay: return <MbWayIcon {...rProps} />;
    case ChargeMethod.Custom: throw new Error("Not Implemented");
  }
}