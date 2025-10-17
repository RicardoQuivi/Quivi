import { useMemo } from "react";
import { useAuthenticatedHttpClient } from "../../context/AuthContext";
import { GetSalesRequest } from "./Dtos/reporting/GetSalesRequest";
import { GetSalesResponse } from "./Dtos/reporting/GetSalesResponse";
import { SalesPeriod } from "./Dtos/reporting/SalesPeriod";
import { GetProductSalesRequest } from "./Dtos/reporting/GetProductSalesRequest";
import { GetProductSalesResponse } from "./Dtos/reporting/GetProductSalesResponse";
import { ProductSalesSortBy } from "./Dtos/reporting/ProductSalesSortBy";
import { GetCategorySalesRequest } from "./Dtos/reporting/GetCategorySalesRequest";
import { GetCategorySalesResponse } from "./Dtos/reporting/GetCategorySalesResponse";
import { GetChargeMethodSalesRequest } from "./Dtos/reporting/GetChargeMethodSalesRequest";
import { GetChargeMethodSalesResponse } from "./Dtos/reporting/GetChargeMethodSalesResponse";
import { GetPartnerChargeMethodSalesRequest } from "./Dtos/reporting/GetPartnerChargeMethodSalesRequest";
import { GetPartnerChargeMethodSalesResponse } from "./Dtos/reporting/GetPartnerChargeMethodSalesResponse";
import { ChargeMethod } from "./Dtos/ChargeMethod";
import { ChargePartner } from "./Dtos/acquirerconfigurations/ChargePartner";
import { ExportSalesRequest } from "./Dtos/reporting/ExportSalesRequest";
import { ExportSalesResponse } from "./Dtos/reporting/ExportSalesResponse";
import { ExportFileType } from "./Dtos/ExportFileType";

export const useReportingApi = () => {
    const client = useAuthenticatedHttpClient();

    const getSales = (request: GetSalesRequest) => {
        const queryParams = new URLSearchParams();
        queryParams.set("page", request.page.toString());
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }

        if(request.from != undefined) {
            queryParams.set("from", request.from);
        }
        
        if(request.to != undefined) {
            queryParams.set("to", request.to);
        }

        if(request.period != undefined) {
            queryParams.set("period", SalesPeriod[request.period]);
        }

        if(request.adminView != undefined) {
            queryParams.set(`adminView`, request.adminView.toString())
        }

        const url = new URL(`api/reporting/sales?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return client.get<GetSalesResponse>(url, {});
    }

    const getProductSales = (request: GetProductSalesRequest) => {
        const queryParams = new URLSearchParams();
        queryParams.set("page", request.page.toString());
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }

        if(request.from != undefined) {
            queryParams.set("from", request.from);
        }
        
        if(request.to != undefined) {
            queryParams.set("to", request.to);
        }

        if(request.period != undefined) {
            queryParams.set("period", SalesPeriod[request.period]);
        }

        if(request.adminView != undefined) {
            queryParams.set(`adminView`, request.adminView.toString())
        }

        queryParams.set("sortBy", ProductSalesSortBy[request.sortBy]);

        const url = new URL(`api/reporting/sales/products?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return client.get<GetProductSalesResponse>(url, {});
    }

    const getCategorySales = (request: GetCategorySalesRequest) => {
        const queryParams = new URLSearchParams();
        queryParams.set("page", request.page.toString());
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }

        if(request.from != undefined) {
            queryParams.set("from", request.from);
        }
        
        if(request.to != undefined) {
            queryParams.set("to", request.to);
        }

        if(request.period != undefined) {
            queryParams.set("period", SalesPeriod[request.period]);
        }

        if(request.adminView != undefined) {
            queryParams.set(`adminView`, request.adminView.toString())
        }

        queryParams.set("sortBy", ProductSalesSortBy[request.sortBy]);

        const url = new URL(`api/reporting/sales/categories?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return client.get<GetCategorySalesResponse>(url, {});
    }

    const getChargeMethodSales = (request: GetChargeMethodSalesRequest) => {
        const queryParams = new URLSearchParams();
        queryParams.set("page", request.page.toString());
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }

        if(request.from != undefined) {
            queryParams.set("from", request.from);
        }
        
        if(request.to != undefined) {
            queryParams.set("to", request.to);
        }

        if(request.period != undefined) {
            queryParams.set("period", SalesPeriod[request.period]);
        }

        if(request.adminView != undefined) {
            queryParams.set(`adminView`, request.adminView.toString())
        }

        queryParams.set("sortBy", ProductSalesSortBy[request.sortBy]);

        const url = new URL(`api/reporting/sales/chargeMethods?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return client.get<GetChargeMethodSalesResponse>(url, {});
    }

    const getPartnerChargeMethodSales = (request: GetPartnerChargeMethodSalesRequest) => {
        const queryParams = new URLSearchParams();
        queryParams.set("page", request.page.toString());
        if(request.pageSize != undefined) {
            queryParams.set("pageSize", request.pageSize.toString());
        }

        if(request.from != undefined) {
            queryParams.set("from", request.from);
        }
        
        if(request.to != undefined) {
            queryParams.set("to", request.to);
        }

        if(request.period != undefined) {
            queryParams.set("period", SalesPeriod[request.period]);
        }

        if(request.adminView != undefined) {
            queryParams.set(`adminView`, request.adminView.toString())
        }

        if(request.chargeMethods != undefined) {
            request.chargeMethods.forEach((c, i) => queryParams.set(`chargeMethods[${i}]`, ChargeMethod[c]));
        }
        
        if(request.chargePartners != undefined) {
            request.chargePartners.forEach((c, i) => queryParams.set(`chargePartners[${i}]`, ChargePartner[c]));
        }

        const url = new URL(`api/reporting/sales/chargeMethods/quivi?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return client.get<GetPartnerChargeMethodSalesResponse>(url, {});
    }

    const exportSales = (request: ExportSalesRequest) => {
        const queryParams = new URLSearchParams();

        if(request.from != undefined) {
            queryParams.set("from", request.from);
        }
        
        if(request.to != undefined) {
            queryParams.set("to", request.to);
        }

        queryParams.set("labels.date", request.labels.date);
        queryParams.set("labels.transactionId", request.labels.transactionId);
        queryParams.set("labels.invoice", request.labels.invoice);
        queryParams.set("labels.id", request.labels.id);
        queryParams.set("labels.method", request.labels.method);
        queryParams.set("labels.menuId", request.labels.menuId);
        queryParams.set("labels.item", request.labels.item);
        queryParams.set("labels.unitPrice", request.labels.unitPrice);
        queryParams.set("labels.quantity", request.labels.quantity);
        queryParams.set("labels.total", request.labels.total);

        const url = new URL(`api/reporting/sales/export/${ExportFileType[request.type]}?${queryParams}`, import.meta.env.VITE_API_URL).toString();
        return client.get<ExportSalesResponse>(url, {});
    }

    const state = useMemo(() => ({
        getSales,
        getProductSales,
        getCategorySales,
        getChargeMethodSales,
        getPartnerChargeMethodSales,
        exportSales,
    }), [client]);

    return state;
}