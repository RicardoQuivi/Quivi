import { useState } from "react";
import TipOption from "./TipOption";
import { useTranslation } from "react-i18next";
import { TipsOptions, type TipsOptionsConfiguration } from "../../../helpers/tipsOptions";
import InputCurrency from "../../../components/CurrencyInput/CurrencyInput";
import { useBrowserStorageService } from "../../../hooks/useBrowserStorageService";

interface Props {
    readonly userBill: number;
    readonly setTipResult: (tipResult: number) => void;
    readonly setTipChoice: (tipOption: TipsOptionsConfiguration) => void;
    readonly selectedTip: TipsOptionsConfiguration;
}

export const TipSection: React.FC<Props> = ({ userBill, setTipResult, setTipChoice, selectedTip }) => {
    const { t } = useTranslation();
    const browserStorage = useBrowserStorageService();
    const paymentDetails = browserStorage.getPaymentDetails();
    const [showDefault, setShowDefault] = useState(paymentDetails != null && paymentDetails.selectedTip.id === TipsOptions.otherButton().id);

    const [customTip, setCustomTip] = useState(0);

    const handleCustomTipClick = (_: number, tipOption: TipsOptionsConfiguration) => {
        if (tipOption.id === selectedTip.id) {
            setTipResult(0);
            setTipChoice(TipsOptions.empty());
            return;
        }

        setTipChoice(TipsOptions.otherButton());
        setTipResult(customTip);
    }

    const handlePredefinedTipChoice = (tipAmount: number, tipOption: TipsOptionsConfiguration) => {
        if (tipOption.id === selectedTip.id) {
            setTipResult(0);
            setTipChoice(TipsOptions.empty());
            return;
        }

        setTipChoice(tipOption);

        let tip: number = 0;
        if (userBill > 0) {
            tip = tipAmount;
        }
        setTipResult(tip);
    };

    const handleCustomTipChange = (value: number) => {
        setShowDefault(false);
        setCustomTip(value);
        setTipResult(value);
    }

    return (
        <div className="mb-8">
            <label className="pay__tip">
                {t("tip.title")}
            </label>
            <div className="tips-container">
                <TipOption
                    onChooseTip={handlePredefinedTipChoice}
                    icon={false}
                    amount={userBill}
                    selected={selectedTip.id}
                    tipOption={TipsOptions.firstButton()}
                />
                <TipOption
                    onChooseTip={handlePredefinedTipChoice}
                    icon={false}
                    amount={userBill}
                    selected={selectedTip.id}
                    tipOption={TipsOptions.secondButton()}
                />
                <TipOption
                    onChooseTip={handlePredefinedTipChoice}
                    icon={false}
                    amount={userBill}
                    selected={selectedTip.id}
                    tipOption={TipsOptions.thirdButton()}
                />
                <TipOption
                    onChooseTip={handleCustomTipClick}
                    icon={false}
                    amount={0}
                    selected={selectedTip.id}
                    tipOption={TipsOptions.otherButton()}
                />
            </div>
            <div
                className="pay__other"
                style={{
                    display: selectedTip.id === TipsOptions.otherButton().id ? "block" : "none",
                    marginBottom: "18px"
                }}
            >
                <label className="pay__tip">
                    {t("tip.tipAmount")}
                </label>
                <InputCurrency
                    value={showDefault ? (paymentDetails?.tip ?? 0) : ""}
                    onChange={handleCustomTipChange}
                    placeholder={0}
                />
            </div>
        </div>
    );
}