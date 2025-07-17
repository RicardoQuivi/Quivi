import { useEffect, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import { currencyFormatter } from "../../components/CurrencyInput/CurrencyInput";
import { NumericFormat, type NumberFormatValues } from "react-number-format";
import { Calculations } from "../../helpers/calculations";
import { Formatter } from "../../helpers/formatter";
import { useBrowserStorageService } from "../../hooks/useBrowserStorageService";
import { useChannelContext } from "../../context/AppContextProvider";
import { useSessionsQuery } from "../../hooks/queries/implementations/useSessionsQuery";

interface Props {
    readonly onChangeAmount: (userBillAmount: number) => void;
    readonly sessionPending?: number | null;
}

export const ShareCustom: React.FC<Props> = ({ 
    onChangeAmount,
    sessionPending,
}) => {
    const { t } = useTranslation();

    const channelContext = useChannelContext();
    const sessionQuery = useSessionsQuery({
        channelId: channelContext.channelId,
    });

    const browserStorage = useBrowserStorageService();
    const savedUserBill = browserStorage.getPaymentDetails()?.amount ?? 0;
    const [userBill, setUserBill] = useState(savedUserBill);
    const [showDefault, setShowDefault] = useState(!!savedUserBill);
    const [inputValue, setInputValue] = useState(0);
    const inputRef = useRef<HTMLInputElement>(null);


    const isOverPaying = () => !!sessionPending && inputValue > sessionPending;

    const handleCustomAmountChange = (values: NumberFormatValues) => {
        setShowDefault(false);

        if (values.formattedValue) {
            onChangeAmount(parseFloat(values.formattedValue.replace(",", ".")));
            setInputValue(parseFloat(values.formattedValue.replace(",", ".")));
        } else {
            onChangeAmount(0);
            setInputValue(0);
        }
    }

    useEffect(() => onChangeAmount(savedUserBill), []);
    useEffect(() => inputRef.current?.focus(), [inputRef.current])

    useEffect(() => {
        if(sessionQuery.isFirstLoading) {
            return;
        }

        if(sessionQuery.data == undefined) {
            return;
        }

        if (isAmountPayable(sessionQuery.data.unpaid, userBill) == false) {
            setUserBill(Calculations.roundUp(sessionQuery.data.unpaid));
        }
    }, [sessionQuery.data, sessionQuery.isFirstLoading, userBill]);

    return (
        <div className={"mb-8"}>
            <div className="pay__amount">
                <label htmlFor="amount">
                    {t("pay.shareCustomTitle")}
                </label>
                <NumericFormat
                    getInputRef={inputRef}
                    value={showDefault ? Formatter.amount(savedUserBill).replace(",", "") : undefined}
                    onValueChange={(values) => handleCustomAmountChange(values)}
                    format={currencyFormatter}
                    isNumericString
                    thousandSeparator=""
                    decimalSeparator=","
                    placeholder="0,00 €"
                    {...{} as any}
                />
                {
                    sessionPending != null && sessionPending != undefined &&
                    <>
                        <p className="small ta-r mt-1">{t("pay.billAmount")}&nbsp;<span className="semi-bold">{Formatter.price(sessionPending, "€")}</span></p>
                        {
                            isOverPaying() &&
                            <div className="alert alert--error mt-4">
                                <p>{t("pay.overPayAlert")}</p>
                            </div>
                        }
                    </>
                }
            </div>
        </div>
    );
}

const isAmountPayable = (unpaidAmount: number, amount: number): boolean => {
    return Calculations.roundUp(unpaidAmount) >= Calculations.roundUp(amount);
}