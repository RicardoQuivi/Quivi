import BigNumber from "bignumber.js";
import { Formatter } from "./formatter";

const calculateTipfromPercentage = (enteredAmount: number, tipPercentage: number) => {
    return Formatter.floatify(enteredAmount * tipPercentage / 100);
};

const roundUpTip = (tipAmount: number) => {
    const stringAmount = Formatter.amount(tipAmount);
    const decimals = +stringAmount.slice(-2);

    if (decimals === 0 || decimals === 50) return tipAmount;

    if (decimals > 0 && decimals < 50) {
        const tipInt = stringAmount.slice(0, -3);
        const newTip = tipInt + ".50";
        return +newTip;
    }

    else {
        const tipInt = +stringAmount.slice(0, -3);
        const newTipInt = tipInt + 1;
        const newTip = newTipInt + ".00";
        return +newTip;
    }
}

export class Calculations {
    static getTip = (enteredAmount: number, tipPercentage: number) => {
        const tipAmountFromPercentage = calculateTipfromPercentage(enteredAmount, tipPercentage);
        return roundUpTip(tipAmountFromPercentage);
    };

    static total = (billAmount: number, tipResult: number) => {
        return Formatter.floatify(billAmount + tipResult);
    }

    static shareEqual = (billAmount: number, peopleOnTable: number, payForPeople: number) => {
        const amountPerPerson = BigNumber(billAmount).dividedBy(peopleOnTable);
        return this.roundUp(amountPerPerson.multipliedBy(payForPeople).toNumber());
    }

    // Round decimals up when number has more than 2 decimals
    static roundUp = (amount: number) => {
        let aux = BigNumber(Math.ceil(BigNumber(amount).times(100).toNumber())).dividedBy(100);
        let result = aux.toNumber();
        return result;
    }
}