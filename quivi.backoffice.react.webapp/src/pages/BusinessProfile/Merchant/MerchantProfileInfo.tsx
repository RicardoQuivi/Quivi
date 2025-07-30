import { useTranslation } from "react-i18next"
import PageMeta from "../../../components/common/PageMeta";
import PageBreadcrumb from "../../../components/common/PageBreadCrumb";
import ComponentCard from "../../../components/common/ComponentCard";
import { useMerchantsQuery } from "../../../hooks/queries/implementations/useMerchantsQuery";
import { useAuthenticatedUser } from "../../../context/AuthContext";
import { useEffect, useState } from "react";
import { ImageInput } from "../../../components/upload/ImageInput";
import { UploadHandler } from "../../../components/upload/UploadHandler";
import { Merchant } from "../../../hooks/api/Dtos/merchants/Merchant";
import Button from "../../../components/ui/button/Button";
import * as yup from 'yup';
import { useQuiviForm } from "../../../hooks/api/exceptions/useQuiviForm";
import { useMerchantMutator } from "../../../hooks/mutators/useMerchantMutator";
import { useToast } from "../../../layout/ToastProvider";
import { NumberField } from "../../../components/inputs/NumberField";
import { ChargeMethodIcon } from "../../../icons/ChargeMethodIcon";
import { ChargeMethod } from "../../../hooks/api/Dtos/ChargeMethod";
import { SingleSelect } from "../../../components/inputs/SingleSelect";
import { FeeUnit } from "../../../hooks/api/Dtos/merchants/FeeUnit";
import { MerchantFee } from "../../../hooks/api/Dtos/merchants/MerchantFee";
import { Spinner } from "../../../components/spinners/Spinner";

const mapApiRecordToEnum = <TEnum extends { [key: string]: string | number }, TValue>(apiData: Record<string, TValue>, enumType: TEnum): Record<TEnum[keyof TEnum] & number, TValue> => {
    const result = {} as Record<TEnum[keyof TEnum] & number, TValue>;
    for (const key in apiData) {
        const enumKey = enumType[key as keyof TEnum];
        if (typeof enumKey === 'number') {
            result[enumKey as TEnum[keyof TEnum] & number] = apiData[key];
        }
    }
    return result;
}

const getState = (m: Merchant | undefined) => {
    const surchargeFees: Record<ChargeMethod, MerchantFee> = mapApiRecordToEnum(m?.surchargeFees ?? {}, ChargeMethod);
    return {
        logoUrl: m?.logoUrl ?? "",

        surchargeFee: m?.surchargeFee ?? 0,
        surchargeFeeUnit: m?.surchargeFeeUnit ?? 0,

        surchargeFees: surchargeFees,
    }
}

const schema = yup.object({
    logoUrl: yup.string().optional(),
});

export const MerchantProfileInfo = () => {
    const { t } = useTranslation();
    const toast = useToast();
    const user = useAuthenticatedUser();
    const mutator = useMerchantMutator();
    
    const merchantQuery = useMerchantsQuery({
        ids: user.subMerchantId == undefined ? undefined : [user.subMerchantId],
        page: 0,
        pageSize: 1,
    })

    const [state, setState] = useState(() => getState(merchantQuery.data.length == 0 ? undefined : merchantQuery.data[0]))
    const [logoUploadHandler, setLogoUploadHandler] = useState<UploadHandler<string>>();
    const form = useQuiviForm(state, schema);

    useEffect(() => setState(getState(merchantQuery.data.length == 0 ? undefined : merchantQuery.data[0])), [merchantQuery.data])

    const save = () => form.submit(async () => {
        const merchant = merchantQuery.data.length == 0 ? undefined : merchantQuery.data[0];
        if(merchant == undefined) {
            return;
        }

        let logo = state.logoUrl;
        if(logoUploadHandler != undefined) {
            logo = await logoUploadHandler.getUrl();
        }
        await mutator.patch(merchant, {
            logoUrl: logo,

            ...(
                user.isAdmin
                ?
                {
                    surchargeFee: state.surchargeFee,
                    surchargeFeeUnit: state.surchargeFeeUnit,
                    surchargefees: state.surchargeFees,
                }
                :
                {}
            )
        })
        toast.success(t("common.operations.success.edit"));
    }, () => toast.error(t("common.operations.failure.generic")))
    
    const getFeeValue = (method: ChargeMethod) => {
        if(method in state.surchargeFees) {
            const fee = state.surchargeFees[method];
            return fee.fee;
        }
        return state.surchargeFee;
    }

    const getFeeUnit = (method: ChargeMethod) => {
        if(method in state.surchargeFees) {
            const fee = state.surchargeFees[method];
            return fee.unit;
        }
        return state.surchargeFeeUnit;
    }

    const setFeeValue = (method: ChargeMethod, value: number) => setState(s => {
        const surchargeFees = { ...s.surchargeFees };
        let fee: MerchantFee = {
            fee: value,
            unit: state.surchargeFeeUnit,
        }
        if(method in surchargeFees) {
            fee = surchargeFees[method];
        }
        surchargeFees[method] = {
            ...fee,
            fee: value,
        }
        return { ...s, surchargeFees: surchargeFees };
    })

    
    const setFeeUnit = (method: ChargeMethod, unit: FeeUnit) => setState(s => {
        const surchargeFees = { ...s.surchargeFees };
        let fee: MerchantFee = {
            fee: state.surchargeFee,
            unit: unit,
        }
        if(method in surchargeFees) {
            fee = surchargeFees[method];
        }
        surchargeFees[method] = {
            ...fee,
            unit: unit,
        }
        return { ...s, surchargeFees: surchargeFees };
    })

    return <>
        <PageMeta
            title={t("pages.merchantProfileInfo.title")}
            description={t("quivi.product.description")}
        />
        <PageBreadcrumb
            pageTitle={t("pages.merchantProfileInfo.title")}
            breadcrumb={t("pages.merchantProfileInfo.title")}
        />
        
        <ComponentCard
            title={t("pages.merchantProfileInfo.title")}
        >
            <div className="grid grid-cols-1 sm:grid-cols-1 md:grid-cols-2 lg:grid-cols-2 gap-4">
                <div className="col-span-1 lg:col-span-1">
                    <ImageInput
                        label={t("common.logo")}
                        aspectRatio={1}
                        value={state.logoUrl}
                        inlineEditor
                        onUploadHandlerChanged={setLogoUploadHandler}
                    />
                </div>
                {
                    user.isAdmin &&
                    <div className="p-5 border border-gray-200 rounded-2xl dark:border-gray-800 lg:p-6 col-span-1 flex flex-col">
                        <h4 className="text-lg font-semibold text-gray-800 dark:text-white/90 lg:mb-2">
                            {t("common.admin.settings")}
                        </h4>
                        <p className="mt-0.5 text-xs text-gray-600 sm:text-sm dark:text-white/70 mb-6">
                            {t("common.admin.settingsDescription")}
                        </p>
                        <div className="grid grid-cols-1 sm:grid-cols-1 md:grid-cols-2 lg:grid-cols-2 gap-4">
                            <NumberField
                                label={t("common.surcharge")}
                                value={state.surchargeFee}
                                onChange={(e) => setState(s => ({ ...s, surchargeFee: e }))}
                                errorMessage={form.touchedErrors.get("surchargeFee")?.message}
                                decimalPlaces={2}
                                endElement={<SingleSelect
                                    className="rounded-none border-0 h-full"
                                    value={state.surchargeFeeUnit}
                                    options={[
                                        FeeUnit.Absolute,
                                        FeeUnit.Percentage
                                    ]}
                                    getId={e => e.toString()}
                                    render={e => e == FeeUnit.Absolute ? "€" : "%"}
                                    onChange={e => setState(s => ({ ...s, surchargeFeeUnit: e}))}
                                />}
                                className="col-span-1 sm:col-span-1 md:col-span-2 lg:col-span-2 xl:col-span-2"
                            />
                            <NumberField
                                value={getFeeValue(ChargeMethod.CreditCard)}
                                onChange={(e) => setFeeValue(ChargeMethod.CreditCard, e)}
                                errorMessage={form.touchedErrors.get("price")?.message}
                                startElement={<ChargeMethodIcon chargeMethod={ChargeMethod.CreditCard} height="100%" style={{width: "auto", padding: "5px" }}/>}
                                decimalPlaces={2}
                                endElement={<SingleSelect
                                    className="rounded-none border-0 h-full"
                                    value={getFeeUnit(ChargeMethod.CreditCard)}
                                    options={[
                                        FeeUnit.Absolute,
                                        FeeUnit.Percentage
                                    ]}
                                    getId={e => e.toString()}
                                    render={e => e == FeeUnit.Absolute ? "€" : "%"}
                                    onChange={e => setFeeUnit(ChargeMethod.CreditCard, e)}
                                />}
                                className="col-span-1"
                            />
                            <NumberField
                                value={getFeeValue(ChargeMethod.MbWay)}
                                onChange={(e) => setFeeValue(ChargeMethod.MbWay, e)}
                                errorMessage={form.touchedErrors.get("price")?.message}
                                startElement={<ChargeMethodIcon chargeMethod={ChargeMethod.MbWay} height="100%" style={{width: "auto", padding: "8px", background: "white", }}/>}
                                decimalPlaces={2}
                                endElement={<SingleSelect
                                    className="rounded-none border-0 h-full"
                                    value={getFeeUnit(ChargeMethod.MbWay)}
                                    options={[
                                        FeeUnit.Absolute,
                                        FeeUnit.Percentage
                                    ]}
                                    getId={e => e.toString()}
                                    render={e => e == FeeUnit.Absolute ? "€" : "%"}
                                    onChange={e => setFeeUnit(ChargeMethod.MbWay, e)}
                                />}
                                className="col-span-1"
                            />
                        </div>
                    </div>
                }
            </div>

            <Button
                size="md"
                onClick={save}
                disabled={form.isValid == false}
                variant="primary"
            >
                {
                    form.isSubmitting
                    ?
                    <Spinner />
                    :
                    t("common.operations.save", {
                        name: t(`common.entities.merchant`),
                    })
                }
            </Button>
        </ComponentCard>
    </>
}