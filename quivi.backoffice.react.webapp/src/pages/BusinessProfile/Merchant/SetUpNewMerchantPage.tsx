import { useTranslation } from "react-i18next"
import PageMeta from "../../../components/common/PageMeta"
import PageBreadcrumb from "../../../components/common/PageBreadCrumb";
import ComponentCard from "../../../components/common/ComponentCard";
import Label from "../../../components/form/Label";
import Input from "../../../components/form/input/InputField";
import { ImageInput } from "../../../components/upload/ImageInput";
import { FileUploader } from "../../../components/upload/FileUploader";
import { IconButton } from "../../../components/ui/button/IconButton";
import { TrashBinIcon } from "../../../icons";
import * as yup from 'yup';
import { useAuth } from "../../../context/AuthContext";
import { useMerchantMutator } from "../../../hooks/mutators/useMerchantMutator";
import { useEffect, useState } from "react";
import { useQuiviForm } from "../../../hooks/api/exceptions/useQuiviForm";
import { UploadHandler } from "../../../components/upload/UploadHandler";
import Button from "../../../components/ui/button/Button";
import { FileExtension } from "../../../hooks/api/Dtos/fileStorage/FileExtension";
import { useNavigate } from "react-router";
import { useToast } from "../../../layout/ToastProvider";
import { TextField } from "../../../components/inputs/TextField";
import { Spinner } from "../../../components/spinners/Spinner";

interface IbanProof {
    readonly name: string;
    readonly url: string;
}
const schema = yup.object({
    fiscalName: yup.string().required(),
    vatNumber: yup.string().required(),
    name: yup.string().required(),
    postalCode: yup.string().required(),
    iban: yup.string().required(),
    //logoUrl: yup.string().required(),
    // ibanProofUrl: yup.object({
    //     name: yup.string().required(),
    //     url: yup.string().required(),
    // }).required(),
});

export const SetUpNewMerchantPage = () => {
    const { t } = useTranslation();
    const toast = useToast();
    const auth = useAuth();
    const mutator = useMerchantMutator();
    const navigate = useNavigate();

    const [state, setState] = useState({
        fiscalName: "",
        vatNumber: "",
        name: "",
        logoUrl: "",
        postalCode: "",
        iban: "",
        ibanProofUrl: undefined as IbanProof | undefined,
    });
    const form = useQuiviForm(state, schema);
    const [logoUploadHandler, setLogoUploadHandler] = useState<UploadHandler<string>>();

    useEffect(() => {
        if(logoUploadHandler == undefined) {
            return;
        }

        logoUploadHandler.onDone(url => setState(s => ({ ...s, logoUrl: url })));
    }, [logoUploadHandler])

    const save = () => form.submit(async () => {
        if(state.ibanProofUrl == undefined) {
            return;
        }

        if(logoUploadHandler == undefined) {
            return;
        }

        let logo = await logoUploadHandler.getUrl();
        const result = await mutator.create({
            name: state.name,
            postalCode: state.postalCode,
            fiscalName: state.fiscalName,
            vatNumber: state.vatNumber,
            logoUrl: logo,
            ibanProofUrl: state.ibanProofUrl.url,
            iban: state.iban,
        })
        await auth.switchMerchant(result.id);
        toast.success(t("common.operations.success.new"));
        navigate("/");
    }, () => toast.error(t("common.operations.failure.generic")))

    return <>
        <PageMeta
            title={t("pages.setUpNewMerchant.title")}
            description={t("quivi.product.description")}
        />
        <PageBreadcrumb
            pageTitle={t("pages.setUpNewMerchant.title")}
            breadcrumb={t("pages.setUpNewMerchant.title")}
        />

        <ComponentCard title={t("pages.setUpNewMerchant.title")}>
            <div className="grid grid-cols-2 sm:grid-cols-2 md:grid-cols-2 lg:grid-cols-2 gap-4">
                {/* Fiscal Information */}
                <div className="p-5 border border-gray-200 rounded-2xl dark:border-gray-800 lg:p-6 col-span-2">
                    <h4 className="text-lg font-semibold text-gray-800 dark:text-white/90 lg:mb-6">
                        {t("pages.setUpNewMerchant.fiscalInfo")}
                    </h4>

                    <div className="grid grid-cols-1 gap-x-6 gap-y-5 lg:grid-cols-2">
                        <TextField
                            label={t("pages.setUpNewMerchant.socialName")}
                            type="text"
                            value={state.fiscalName}
                            onChange={e => setState(s => ({ ...s, fiscalName: e }))}
                            errorMessage={form.touchedErrors.get("fiscalName")?.message}
                        />

                        <TextField
                            label={t("common.vatNumber")}
                            type="text"
                            value={state.vatNumber}
                            onChange={(e) => setState(s => ({ ...s, vatNumber: e }))}
                            errorMessage={form.touchedErrors.get("vatNumber")?.message}
                        />
                    </div>
                </div>

                {/* Comercial Information */}
                <div className="p-5 border border-gray-200 rounded-2xl dark:border-gray-800 lg:p-6 col-span-2 sm:col-span-2 md:col-span-1 lg:col-span-1">
                    <h4 className="text-lg font-semibold text-gray-800 dark:text-white/90 lg:mb-6">
                        {t("pages.setUpNewMerchant.comercialInfo")}
                    </h4>

                    <div className="grid grid-cols-1 gap-x-6 gap-y-5 lg:grid-cols-2">
                        <TextField
                            label={t("pages.setUpNewMerchant.brand")}
                            className="col-span-2 lg:col-span-1"
                            type="text"
                            value={state.name}
                            onChange={(e) => setState(s => ({ ...s, name: e }))}
                            errorMessage={form.touchedErrors.get("name")?.message}
                        />

                        <TextField
                            className="col-span-2 lg:col-span-1"
                            label={t("common.postalCode")}
                            type="text"
                            value={state.postalCode}
                            onChange={(e) => setState(s => ({ ...s, postalCode: e }))}
                            errorMessage={form.touchedErrors.get("postalCode")?.message}
                        />
                    </div>
                    <div className="grid grid-cols-1 gap-x-6 gap-y-5">
                        <div className="col-span-1 lg:col-span-1">
                            <ImageInput
                                label={t("common.logo")}
                                aspectRatio={1}
                                value={state.logoUrl}
                                inlineEditor
                                onUploadHandlerChanged={setLogoUploadHandler}
                            />
                        </div>
                    </div>
                </div>

                {/* Banking Information */}
                <div className="p-5 border border-gray-200 rounded-2xl dark:border-gray-800 lg:p-6 col-span-2 sm:col-span-2 md:col-span-1 lg:col-span-1">
                    <h4 className="text-lg font-semibold text-gray-800 dark:text-white/90 lg:mb-6">
                        {t("pages.setUpNewMerchant.bankingInfo")}
                    </h4>

                    <div className="grid grid-cols-1 gap-x-6 gap-y-5">
                        <TextField
                            className="col-span-1"
                            label={t("common.iban")}
                            type="text"
                            value={state.iban}
                            onChange={(e) => setState(s => ({ ...s, iban: e }))}
                            errorMessage={form.touchedErrors.get("iban")?.message}
                        />

                        <div className="col-span-1">
                            <Label>{t("pages.setUpNewMerchant.ibanProof")}</Label>
                            {
                                state.ibanProofUrl == undefined
                                ?
                                <FileUploader
                                    allowedFiles={[FileExtension.JPG, FileExtension.PNG, FileExtension.PDF ]}
                                    onUploaded={(name, url) => setState(s => ({ ...s, ibanProofUrl: { name: name, url: url } }))}
                                />
                                :
                                <div className="relative">
                                    <Input
                                        value={state.ibanProofUrl.name}
                                        type="text"
                                        disabled
                                    />
                                    <IconButton
                                        className="absolute z-30 -translate-y-1/2 right-4 top-1/2 !fill-gray-700 hover:!fill-error-500 dark:!fill-gray-400 dark:!hover:fill-error-500"
                                        onClick={() => setState(s => ({ ...s, ibanProofUrl: undefined }))}
                                    >
                                        <TrashBinIcon/>
                                    </IconButton>
                                </div>
                            }
                        </div>
                    </div>
                </div>
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
                    t("common.operations.new", {
                        name: t(`common.entities.merchant`),
                    })
                }
            </Button>
        </ComponentCard>
    </>
}