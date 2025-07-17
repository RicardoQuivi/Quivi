import BigNumber from "bignumber.js";
import { useEffect, useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useShareEqualSettings } from "../../hooks/useShareEqualSettings";
import { useChannelContext } from "../../context/AppContextProvider";
import type { IShareEqualSettings } from "../../helpers/paymentSplitter";
import { Calculations } from "../../helpers/calculations";
import { Formatter } from "../../helpers/formatter";
import { useSessionsQuery } from "../../hooks/queries/implementations/useSessionsQuery";
import { useTransactionsQuery } from "../../hooks/queries/implementations/useTransactionsQuery";
import { MinusIcon, PlusIcon } from "../../icons";
import type { Transaction } from "../../hooks/api/Dtos/transactions/Transaction";
import { useBrowserStorageService } from "../../hooks/useBrowserStorageService";

interface Props {
    readonly onChangeAmount: (userBillAmount: number) => void;
    readonly sessionPending: number | null;
}

export const ShareEqual: React.FC<Props> = ({ 
    onChangeAmount,
    sessionPending,
}) => {
    const { t } = useTranslation();
    const browserStorage = useBrowserStorageService();
    const channelContext = useChannelContext();

    const sessionQuery = useSessionsQuery({
        channelId: channelContext.channelId,
    });
    const transactionsQuery = useTransactionsQuery(sessionQuery.data == undefined ? undefined : {
        sessionId: sessionQuery.data.id,
        page: 0,
    });
    const auxShareSettings = useShareEqualSettings(channelContext.channelId);

    const [shareSettings, setShareSettings] = useState<IShareEqualSettings | null>(null);
    const [initialPayments, setInitialPayments] = useState<Transaction[] | undefined>(() => transactionsQuery.isFirstLoading ? undefined : transactionsQuery.data );

    const [shareQuoficient, setShareQuoficient] = useState(1);
    const [peopleAtTheTable, setPeopleAtTheTable] = useState(1);
    const [payForPeople, setPayForPeople] = useState(1);
    const [userBill, setUserBill] = useState<number | null>(null);

    const sessionOriginalAmount = useMemo(() => calculateAmount(initialPayments ?? [], sessionPending ?? 0), [initialPayments, sessionPending])

    const recalculateAmounts = (peopleOnTableCount: number, payForPeopleCount: number): number => {
        if(sessionPending == null) {
            return 0;
        }
        let result = Calculations.shareEqual(sessionOriginalAmount, peopleOnTableCount, payForPeopleCount);
        return result > sessionPending ? sessionPending : result;
    }

    useEffect(() => {
        if(initialPayments != undefined) {
            return;
        }

        if(transactionsQuery.isFirstLoading) {
            return;
        }

        setInitialPayments(transactionsQuery.data);
    }, [transactionsQuery.data])

    useEffect(() => {
        if(auxShareSettings.isLoading) {
            return;
        }

        if(shareSettings != null) {
            return;
        }

        setShareSettings(auxShareSettings);
        setPeopleAtTheTable(auxShareSettings.peopleAtTheTable);
        setUserBill(recalculateAmounts(auxShareSettings.peopleAtTheTable, payForPeople));
    }, [auxShareSettings])

    useEffect(() => {
        if(shareSettings == null) {
            return;
        }

        const newCount = (shareSettings.peopleAtTheTable) * shareQuoficient;
        setPeopleAtTheTable(newCount);
    }, [shareQuoficient, shareSettings])

    useEffect(() => {
        onChangeAmount(userBill ?? 0);
    }, [userBill])

    useEffect(() => {
        setUserBill(recalculateAmounts(peopleAtTheTable, payForPeople));
    }, [peopleAtTheTable, payForPeople]);

    useEffect(() => {
        let paymentDivision = browserStorage.getPaymentDivision();
        if(paymentDivision == null) {
            paymentDivision = {
                divideEvenly: {
                    payForPeople: payForPeople,
                    peopleAtTheTable: peopleAtTheTable,
                },
                selectedItems: [],
            }
        } else {
            paymentDivision = {
                ...paymentDivision,
                divideEvenly: {
                    payForPeople: payForPeople,
                    peopleAtTheTable: peopleAtTheTable,
                },
            }
        }
        browserStorage.savePaymentDivision(paymentDivision);
    }, [peopleAtTheTable, payForPeople]);

    useEffect(() => {
        if(payForPeople > peopleAtTheTable) {
            setPayForPeople(peopleAtTheTable);
        }
    }, [payForPeople])

    return (
        <div className={"pay__amount mb-8"}>
            <div className="mb-7">
                <label>
                    {t("pay.peopleOnTableQuestion")}
                </label>
                <div className="people-counter">
                    <button
                        className={`counter-button ${peopleAtTheTable === 1 || peopleAtTheTable == shareSettings?.peopleAtTheTable ? "disabled" : ""}`}
                        type="button"
                        onClick={() => setShareQuoficient(shareQuoficient - 1)}
                    >
                        <MinusIcon />
                    </button>
                    <span>{peopleAtTheTable}</span>
                    <button
                        className={`counter-button ${sessionOriginalAmount / ((shareSettings?.peopleAtTheTable ?? 1) * (shareQuoficient + 1)) <= 0.01 ? "disabled" : ""}`}
                        type="button"
                        onClick={() => setShareQuoficient(shareQuoficient + 1)}
                    >
                        <PlusIcon />
                    </button>
                </div>
            </div>
            <div className="mb-8">
                <label>
                    {t("pay.payForPeopleQuestion")}
                </label>
                <div className="people-counter">
                    <button
                        className={`counter-button ${payForPeople === 1 ? "disabled" : ""}`}
                        type="button"
                        onClick={() => setPayForPeople(p => p - 1)}
                    >
                        <MinusIcon />
                    </button>
                    <span>{payForPeople}</span>
                    <button
                        className={`counter-button ${(userBill ?? 0) >= (sessionPending ?? 0) ? "disabled" : ""}`}
                        type="button"
                        onClick={() => setPayForPeople(p => p + 1)}
                    >
                        <PlusIcon />
                    </button>
                </div>
            </div>
            <div className={"total-container"}>
                <h2 className="mb-1">{t("pay.divisionTotal")}</h2>
                <span>
                    <h1 style={{ display: "inline-block" }}>{Formatter.price(userBill ?? 0, "€")}</h1>
                    <span style={{ marginLeft: "0.8rem", marginRight: "0.8rem" }}>{t("of")}</span>
                    <h1 style={{ display: "inline-block" }}>{Formatter.price(sessionOriginalAmount, "€")} </h1>
                </span>
            </div>
        </div>
    )
}

const calculateAmount = (transactions: Transaction[], sessionPending: number) => {
    let pending = new BigNumber(sessionPending);
    let paid = transactions.reduce((acc, p) => acc.plus(p.syncedAmount), new BigNumber(0))
    return Calculations.roundUp(pending.plus(paid).toNumber());
}