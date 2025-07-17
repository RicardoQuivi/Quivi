import type { SessionItem } from "../hooks/api/Dtos/sessions/SessionItem";
import { Calculations } from "./calculations";
import BigNumber from "bignumber.js";

export interface IShareEqualSettings {
    readonly isLoading: boolean;
    readonly peopleAtTheTable: number;
    readonly amountPerPerson: number;
    readonly peopleWhoPaid: number;
    readonly isShareable: boolean;
}

const getOriginalAmount = (pendingAmount: BigNumber, paymentAmounts: BigNumber[]): BigNumber => {
    let paid: BigNumber = paymentAmounts.reduce((acc, p) => acc.plus(p), new BigNumber(0));
    let pending: BigNumber = new BigNumber(pendingAmount);
    return new BigNumber(Calculations.roundUp(paid.plus(pending).toNumber()));
}

export class PaymentSplitter {
    static getShareEqualSettings = (pendingAmountNumber: number, paymentAmountsNumber: number[]): IShareEqualSettings => {
        if (paymentAmountsNumber.length == 0)
            return { peopleAtTheTable: 1, amountPerPerson: pendingAmountNumber, peopleWhoPaid: 0, isShareable: true, isLoading: false };

        let pendingAmount = new BigNumber(pendingAmountNumber).decimalPlaces(6);
        const paymentAmounts = paymentAmountsNumber.sort((a, b) => a - b).map(p => BigNumber(p));
        const originalTotal = getOriginalAmount(pendingAmount, paymentAmounts);

        for (let quoficient = 1; ; ++quoficient) {
            let lowestAmount = paymentAmounts[0].dividedBy(quoficient);

            if (lowestAmount.isLessThan(0.01))
                return { peopleAtTheTable: 1, amountPerPerson: pendingAmountNumber, peopleWhoPaid: 0, isShareable: false, isLoading: false };

            if (pendingAmount.isLessThan(lowestAmount)) {
                let division = lowestAmount.dividedBy(pendingAmount);
                if (division.toNumber() == Math.round(division.toNumber())) {
                    lowestAmount = pendingAmount;
                }
            }

            let possibleRemainder = 0.01 * paymentAmounts.length;
            let peopleWhoPaid = 0;

            for (let i = 0; i < paymentAmounts.length; ++i) {
                let v = paymentAmounts[i];

                let division = v.dividedBy(lowestAmount);
                if (division.toNumber() == Math.round(division.toNumber())) {
                    peopleWhoPaid += Math.round(division.toNumber());
                    continue;
                }

                //If there is a rounding, then there is a rounding of 1 cent on each payment
                let adjustedAmountLowBoundary = v.minus(possibleRemainder);
                let adjustedAmountHighBoundary = v.plus(possibleRemainder);
                if (adjustedAmountLowBoundary.isLessThanOrEqualTo(v) && v.isLessThanOrEqualTo(adjustedAmountHighBoundary)) {
                    peopleWhoPaid += Math.round(division.toNumber());
                    continue;
                }

                //We cannot estimate because it seems people have paid randomly
                return { peopleAtTheTable: 1, amountPerPerson: pendingAmountNumber, peopleWhoPaid: 0, isShareable: false, isLoading: false };
            }

            //The lowest amount can be one of two cases:
            //1 - The person paid rounded up and thus an extra cent. In this scenario this payment must be the highest payment.
            //2 - The person paid the exact amount. In this scenario this payment can either be the lowest or highest payment.
            //3 - The person paid rounded down and thus less one cent. In this scenario this payment must be the lowest payment.
            let estimated1People = Math.round(originalTotal.dividedBy(lowestAmount.minus(0.01)).toNumber());
            let estimated2People = Math.round(originalTotal.dividedBy(lowestAmount).toNumber());
            let estimated3People = Math.round(originalTotal.dividedBy(lowestAmount.plus(0.01)).toNumber());
            let estimated1AmountPerPerson = originalTotal.dividedBy(estimated1People);
            let estimated2AmountPerPerson = originalTotal.dividedBy(estimated2People);
            let estimated3AmountPerPerson = originalTotal.dividedBy(estimated3People);
            let estimated1HighestPayment = new BigNumber(Calculations.roundUp(estimated1AmountPerPerson.toNumber()));
            let estimated2HighestPayment = new BigNumber(Calculations.roundUp(estimated2AmountPerPerson.toNumber()));
            let estimated3HighestPayment = new BigNumber(Calculations.roundUp(estimated3AmountPerPerson.toNumber()));
            let estimated2LowestPayment = estimated2HighestPayment.minus(0.01);
            let estimated3LowestPayment = estimated3HighestPayment.minus(0.01);

            //If a sharing can be done, then the paid amount must intersect two of the situations explained above
            let matches1 = lowestAmount.isEqualTo(estimated1HighestPayment) || estimated1People == Infinity;
            let matches2 = lowestAmount.isEqualTo(estimated2LowestPayment) || lowestAmount.isEqualTo(estimated2HighestPayment);
            let matches3 = lowestAmount.isEqualTo(estimated3LowestPayment);

            let estimatedPeople = 0;
            if (matches1 && matches2) {
                if (estimated1People != estimated2People && estimated1People != Infinity) //If they are not the same estimation then the payment doesn't make sense
                    return { peopleAtTheTable: 1, amountPerPerson: pendingAmountNumber, peopleWhoPaid: 0, isShareable: false, isLoading: false };

                estimatedPeople = estimated2People;
            } else if (matches2 && matches3) {
                if (estimated2People != estimated3People) //If they are not the same estimation then the payment doesn't make sense
                    return { peopleAtTheTable: 1, amountPerPerson: pendingAmountNumber, peopleWhoPaid: 0, isShareable: false, isLoading: false };
                estimatedPeople = estimated3People;
            } else {
                continue; //Try other numbers
            }

            let amountPerPerson = lowestAmount;
            let peopleAtTheTable = estimatedPeople;
            let peopleRemainingToPay = peopleAtTheTable - peopleWhoPaid;

            if (peopleRemainingToPay == 1)
                amountPerPerson = pendingAmount;

            //If the extra cent (due to roundings) are already paid, then remove the rounding
            if (amountPerPerson.minus(0.01).multipliedBy(peopleRemainingToPay).isEqualTo(pendingAmount)) {
                amountPerPerson = amountPerPerson.minus(0.01);
            }

            //START a sanity check if the payments we found make sense
            let originalDivision = originalTotal.dividedBy(peopleAtTheTable);
            if (pendingAmount.isGreaterThan(Calculations.roundUp(originalDivision.multipliedBy(peopleRemainingToPay).toNumber()))) {
                //Try a different quoficient
                continue;
            }

            let nextQuoficient = false;
            for (let i = 0; i < paymentAmounts.length; ++i) {
                let v = paymentAmounts[i];

                let division = v.dividedBy(originalDivision);
                let paymentPaymentForPeople = Math.round(division.toNumber());

                let adjustedAmountHighBoundary = Calculations.roundUp(originalDivision.multipliedBy(paymentPaymentForPeople).toNumber());
                let adjustedAmountLowBoundary = adjustedAmountHighBoundary - 0.01;
                if (adjustedAmountLowBoundary <= v.toNumber() && v.toNumber() <= adjustedAmountHighBoundary) {
                    continue;
                }
                nextQuoficient = true;
                break;
            }
            if (nextQuoficient == true)
                continue;
            //END sanity check

            let finalAmountPerPerson = BigNumber(pendingAmount).dividedBy(peopleRemainingToPay).toNumber();
            if (Calculations.roundUp(finalAmountPerPerson) == pendingAmount.toNumber()) {
                finalAmountPerPerson = pendingAmount.toNumber();
            } else if (Math.floor(finalAmountPerPerson) * peopleRemainingToPay == pendingAmount.toNumber()) {
                finalAmountPerPerson = Math.floor(finalAmountPerPerson);
            }

            return {
                peopleAtTheTable: peopleAtTheTable,
                amountPerPerson: finalAmountPerPerson,
                peopleWhoPaid: peopleWhoPaid,
                isShareable: true,
                isLoading: false,
            };
        }
    }

    static isShareItemsAvailable = (sessionItems: SessionItem[]) => sessionItems.some((tableItem: SessionItem) => {
        return tableItem.quantity >= 1 && tableItem.isPaid == false;
    });
}