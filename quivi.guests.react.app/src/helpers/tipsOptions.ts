export const LOW_AMOUNT_PAYMENT = 7.6;
export const SECOND_TIER_TIP = 4;

export enum TipLabel {
    Other = "Other",
    FirstOption = "10",
    SecondOption = "15",
    ThirdOption = "20",
    Empty = ""
}

export enum TipPercentage {
    Empty = 0,
    FirstOption = 10,
    SecondOption = 15,
    ThirdOption = 20,
}

export enum FirstTierFixedTip {
    Empty = 0,
    FirstOption = 0.1,
    SecondOption = 0.25,
    ThirdOption = 0.5,
}

export enum SecondTierFixedTip {
    Empty = 0,
    FirstOption = 0.5,
    SecondOption = 0.75,
    ThirdOption = 1,
}

export interface TipsOptionsConfiguration {
    percentage: TipPercentage,
    fisrtTierFixedTip: FirstTierFixedTip,
    secondTierFixedTip: SecondTierFixedTip,
    label: TipLabel,
    id: string,
}

export class TipsOptions {
    public static empty(): TipsOptionsConfiguration {
        return {
            percentage: TipPercentage.Empty,
            fisrtTierFixedTip: FirstTierFixedTip.Empty,
            secondTierFixedTip: SecondTierFixedTip.Empty,
            label: TipLabel.Empty, id: ""
        };
    }

    public static firstButton(): TipsOptionsConfiguration {
        return {
            percentage: TipPercentage.FirstOption,
            fisrtTierFixedTip: FirstTierFixedTip.FirstOption,
            secondTierFixedTip: SecondTierFixedTip.FirstOption,
            label: TipLabel.FirstOption,
            id: "FirstOption"
        };
    }

    public static secondButton(): TipsOptionsConfiguration {
        return {
            percentage: TipPercentage.SecondOption,
            fisrtTierFixedTip: FirstTierFixedTip.SecondOption,
            secondTierFixedTip: SecondTierFixedTip.SecondOption,
            label: TipLabel.SecondOption,
            id: "SecondOption"
        };
    }

    public static thirdButton(): TipsOptionsConfiguration {
        return {
            percentage: TipPercentage.ThirdOption,
            fisrtTierFixedTip: FirstTierFixedTip.ThirdOption,
            secondTierFixedTip: SecondTierFixedTip.ThirdOption,
            label: TipLabel.ThirdOption,
            id: "ThirdOption"
        };
    }

    public static otherButton(): TipsOptionsConfiguration {
        return {
            percentage: TipPercentage.Empty,
            fisrtTierFixedTip: FirstTierFixedTip.Empty,
            secondTierFixedTip: SecondTierFixedTip.Empty,
            label: TipLabel.Other,
            id: "Other"
        };
    }
}