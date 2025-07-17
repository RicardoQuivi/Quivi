import { useTranslation } from "react-i18next";
import { LOW_AMOUNT_PAYMENT, SECOND_TIER_TIP, TipLabel, type TipsOptionsConfiguration } from "../../../helpers/tipsOptions";
import { Calculations } from "../../../helpers/calculations";
import { Formatter } from "../../../helpers/formatter";

interface Props {
    readonly onChooseTip: (tipAmount: number, tipOption: TipsOptionsConfiguration) => void;
    readonly icon: boolean;
    readonly amount: number;
    readonly selected: string;
    readonly tipOption: TipsOptionsConfiguration;
}

const TipOption: React.FC<Props> = ({
    onChooseTip,
    icon,
    amount,
    selected,
    tipOption,
}) => {
    const { t } = useTranslation();

    const firstTierTip = amount < SECOND_TIER_TIP;
    const secondTierTip = amount >= SECOND_TIER_TIP && amount < LOW_AMOUNT_PAYMENT;
    const percentageTip = amount >= LOW_AMOUNT_PAYMENT;
    const firstTierFixedTip = tipOption.fisrtTierFixedTip;
    const secondTierFixedTip = tipOption.secondTierFixedTip;
    const amountIsPositive = amount > 0;
    const percentageTipResult = Calculations.getTip(amount, tipOption.percentage);
    const label = tipOption.label;
    const id = tipOption.id;

    const handleTipChoice = () => {
        let tip = 0;
        if (amountIsPositive) {
            if (firstTierTip) {
                tip = firstTierFixedTip;
            } else if (secondTierTip) {
                tip = secondTierFixedTip;
            } else {
                tip = percentageTipResult;
            }
        }
        onChooseTip(tip, tipOption);
    }

    return (
        <button
            id={id}
            className={`tips-group ${selected === id ? "selected" : ""}`}
            onClick={handleTipChoice}
            type="button"
        >
            {icon && (
                <svg
                    width="25"
                    height="25"
                    viewBox="0 0 25 25"
                    fill="none"
                    xmlns="http://www.w3.org/2000/svg"
                >
                    <path
                        d="M12.5 2.08333L15.7187 8.60416L22.9166 9.65625L17.7083 14.7292L18.9375 21.8958L12.5 18.5104L6.06248 21.8958L7.29165 14.7292L2.08331 9.65625L9.28123 8.60416L12.5 2.08333Z"
                        stroke="#222327"
                        strokeWidth="2"
                        strokeLinecap="round"
                        strokeLinejoin="round"
                    />
                </svg>
            )}

            {
                firstTierTip &&
                <span className="tips-fixed">
                    {label === TipLabel.Other ? `${t("tip.other")}` : `${Formatter.price(firstTierFixedTip, "€")}`}
                </span>
            }
            {
                secondTierTip && 
                <span className="tips-fixed">
                    {label === TipLabel.Other ? `${t("tip.other")}` : `${Formatter.price(secondTierFixedTip, "€")}`}
                </span>
            }
            {
                percentageTip &&
                <>
                    <span className="tips-percentage">
                        {label === TipLabel.Other ? `${t("tip.other")}` : `${label}%`}
                    </span>
                    {
                        label !== TipLabel.Other &&
                        <span className="tips-result">
                            {Formatter.price(percentageTipResult, "€")}
                        </span>
                    }
                </>
            }
        </button>
    );
};

export default TipOption;
