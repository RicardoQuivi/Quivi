import { useTranslation } from "react-i18next"
import PageMeta from "../../../components/common/PageMeta";
import PageBreadcrumb from "../../../components/common/PageBreadCrumb";
import ComponentCard from "../../../components/common/ComponentCard";
import { useMerchantsQuery } from "../../../hooks/queries/implementations/useMerchantsQuery";
import { useAuth } from "../../../context/AuthContext";
import { useEffect, useState } from "react";
import { ImageInput } from "../../../components/upload/ImageInput";
import { UploadHandler } from "../../../components/upload/UploadHandler";
import { Merchant } from "../../../hooks/api/Dtos/merchants/Merchant";
import Button from "../../../components/ui/button/Button";
import { ClipLoader } from "react-spinners";
import * as yup from 'yup';
import { useQuiviForm } from "../../../hooks/api/exceptions/useQuiviForm";
import { useMerchantMutator } from "../../../hooks/mutators/useMerchantMutator";
import { useToast } from "../../../layout/ToastProvider";

const getState = (m: Merchant | undefined) => {
    return {
        logoUrl: m?.logoUrl ?? "",
    }
}

const schema = yup.object({
    logoUrl: yup.string().optional(),
});

export const MerchantProfileInfo = () => {
    const { t } = useTranslation();
    const toast = useToast();
    const auth = useAuth();
    const mutator = useMerchantMutator();
    
    const merchantQuery = useMerchantsQuery({
        ids: auth.subMerchantId == undefined ? undefined : [auth.subMerchantId],
        page: 0,
        pageSize: 1,
    })

    const [state, setState] = useState(getState(merchantQuery.data.length == 0 ? undefined : merchantQuery.data[0]))
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
        })
        toast.success(t("common.operations.success.edit"));
    }, () => toast.error(t("common.operations.failure.generic")))


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
                    <ClipLoader
                        size={20}
                        cssOverride={{
                            borderColor: "white"
                        }}
                    />
                    :
                    t("common.submit")
                }
            </Button>
        </ComponentCard>
    </>
}