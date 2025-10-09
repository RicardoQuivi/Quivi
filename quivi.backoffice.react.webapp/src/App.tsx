import { Routes, Route, Navigate, Outlet, BrowserRouter } from "react-router";
import { ScrollToTop } from "./components/common/ScrollToTop";
import { SignInPage } from "./pages/Auth/SignInPage";
import { SignUpPage } from "./pages/Auth/SignUpPage";
import { ConfirmEmailPage } from "./pages/Auth/ConfirmEmailPage";
import { ForgotPasswordPage } from "./pages/Auth/ForgotPasswordPage";
import { RecoverPasswordPage } from "./pages/Auth/RecoverPasswordPage";
import { SidebarProvider, useSidebar } from "./context/SidebarContext";
import AppSidebar from "./layout/AppSidebar";
import Backdrop from "./layout/Backdrop";
import { AppHeader } from "./layout/AppHeader";
import { DashboardPage } from "./pages/Dashboard/DashboardPage";
import { useAuth } from "./context/AuthContext";
import { ChannelProfilesPage } from "./pages/BusinessProfile/ChannelProfiles/ChannelProfilesPage";
import { ChannelProfileFormPage } from "./pages/BusinessProfile/ChannelProfiles/ChannelProfileFormPage";
import { ChannelsPage } from "./pages/BusinessProfile/Channels/ChannelsPage";
import { SetUpNewMerchantPage } from "./pages/BusinessProfile/Merchant/SetUpNewMerchantPage";
import { TermsAndConditionsPage } from "./pages/BusinessProfile/Merchant/TermsAndConditionsPage";
import { MerchantProfileInfo } from "./pages/BusinessProfile/Merchant/MerchantProfileInfo";
import { MenuManagementPage } from "./pages/BusinessProfile/Menus/MenuManagement";
import { MenuCategoryFormPage } from "./pages/BusinessProfile/Menus/MenuCategories/MenuCategoriesFormPage";
import { MenuItemFormPage } from "./pages/BusinessProfile/Menus/MenuItems/MenuItemFormPage";
import { LocalsPage } from "./pages/Settings/Locals/LocalsPage";
import { LocalFormPage } from "./pages/Settings/Locals/LocalFormPage";
import { EmployeesPage } from "./pages/Settings/Employees/EmployeesPage";
import { EmployeeFormPage } from "./pages/Settings/Employees/EmployeeFormPage";
import { ModifierGroupFormPage } from "./pages/BusinessProfile/Menus/ModifierGroups/ModifierGroupFormPage";
import { CustomChargeMethodsPage } from "./pages/Settings/CustomChargeMethods/CustomChargeMethodsPage";
import { CustomChargeMethodFormPage } from "./pages/Settings/CustomChargeMethods/CustomChargeMethodFormPage";
import { PrinterPage } from "./pages/Settings/Printers/PrintersPage";
import { PrinterWorkerFormPage } from "./pages/Settings/Printers/PrinterWorkers/PrinterWorkerFormPage";
import { PrinterFormPage } from "./pages/Settings/Printers/Printers/PrinterFormPage";
import { AcquirerConfigurationsPage } from "./pages/Admin/AcquirerConfigurations/AcquirerConfigurationsPage";
import { AcquirerConfigurationFormPage } from "./pages/Admin/AcquirerConfigurations/AcquirerConfigurationFormPage";
import { TransactionsPage } from "./pages/Transactions/TransactionsPage";
import { PosIntegrationsPage } from "./pages/Admin/PosIntegrations/PosIntegrationsPage";
import { PosIntegrationFormPage } from "./pages/Admin/PosIntegrations/PosIntegrationFormPage";
import { ConfigurableFieldsPage } from "./pages/BusinessProfile/ConfigurableFields/ConfigurableFieldsPage";
import { ConfigurableFieldFormPage } from "./pages/BusinessProfile/ConfigurableFields/ConfigurableFieldFormPage";
import { PayoutsPage } from "./pages/Payouts/PayoutsPage";
import { InvoicingPage } from "./pages/Invoicing/InvoicingPage";
import { AvailabilitiesPage } from "./pages/BusinessProfile/Availabilities/AvailabilitiesPage";
import { AvailabilityFormPage } from "./pages/BusinessProfile/Availabilities/AvailabilityFormPage";

export const App = () => {
    return <>
        <BrowserRouter>
            <ScrollToTop />
            <Routes>
                <Route path="/signup" element={<SignUpPage />} />
                <Route path="/signup/confirmEmail/" element={<ConfirmEmailPage />} />
                
                <Route path="/signin" element={<SignInPage />} />

                <Route path="/forgotPassword" element={<ForgotPasswordPage />} />
                <Route path="/forgotPassword/reset" element={<RecoverPasswordPage />} />

                <Route element={<AuthLayoutRoute />}>
                    <Route path="/" element={<DashboardPage />} />

                    {/* Admin - Integrations */}
                    <Route path="/admin/integrations" element={<PosIntegrationsPage />} />
                    <Route path="/admin/integrations/add" element={<PosIntegrationFormPage />} />
                    <Route path="/admin/integrations/:id/edit" element={<PosIntegrationFormPage />} />
                    
                    {/* Admin - Acquirer Configurations */}
                    <Route path="/admin/acquirerConfigurations" element={<AcquirerConfigurationsPage />} />
                    <Route path="/admin/acquirerConfigurations/add" element={<AcquirerConfigurationFormPage />} />
                    <Route path="/admin/acquirerConfigurations/:id/edit" element={<AcquirerConfigurationFormPage />} />

                    {/* Admin - Transactions */}
                    <Route path="/admin/transactions" element={<TransactionsPage isAdmin />} />
                    <Route path="/admin/transactions/:id" element={<TransactionsPage isAdmin />} />

                    {/* Merchant */}
                    <Route path="/businessProfile/merchant/setup" element={<SetUpNewMerchantPage />} />
                    <Route path="/businessProfile/merchant" element={<MerchantProfileInfo />} />

                    {/* Channel Profile */}
                    <Route path="/businessProfile/channelprofiles" element={<ChannelProfilesPage />} />
                    <Route path="/businessProfile/channelprofiles/add" element={<ChannelProfileFormPage />} />
                    <Route path="/businessProfile/channelprofiles/:id/edit" element={<ChannelProfileFormPage />} />

                    {/* Configurable Fields */}
                    <Route path="/businessProfile/configurablefields" element={<ConfigurableFieldsPage />} />
                    <Route path="/businessProfile/configurablefields/add" element={<ConfigurableFieldFormPage />} />
                    <Route path="/businessProfile/configurablefields/:id/edit" element={<ConfigurableFieldFormPage />} />

                    {/* Channel */}
                    <Route path="/businessProfile/channels" element={<ChannelsPage />} />

                    {/* Menu Management */}
                    <Route path="/businessProfile/menumanagement" element={<MenuManagementPage categories="All" />} />
                    <Route path="/businessProfile/menumanagement/categories/none" element={<MenuManagementPage categories="None" />} />
                    <Route path="/businessProfile/menumanagement/categories/:categoryId" element={<MenuManagementPage />} />
                    <Route path="/businessProfile/menumanagement/categories/add" element={<MenuCategoryFormPage />} />
                    <Route path="/businessProfile/menumanagement/categories/:id/edit" element={<MenuCategoryFormPage />} />
                    <Route path="/businessProfile/menumanagement/items/add" element={<MenuItemFormPage />} />
                    <Route path="/businessProfile/menumanagement/items/:id/edit" element={<MenuItemFormPage />} />
                    <Route path="/businessProfile/menumanagement/items/:id/clone" element={<MenuItemFormPage clone />} />
                    <Route path="/businessProfile/menumanagement/modifiers/add" element={<ModifierGroupFormPage />} />
                    <Route path="/businessProfile/menumanagement/modifiers/:id/edit" element={<ModifierGroupFormPage />} />

                    {/* Availabilities */}
                    <Route path="/businessProfile/availabilities" element={<AvailabilitiesPage />} />
                    <Route path="/businessProfile/availabilities/add" element={<AvailabilityFormPage />} />
                    <Route path="/businessProfile/availabilities/:id/edit" element={<AvailabilityFormPage />} />

                    {/* Transactions */}
                    <Route path="/transactions" element={<TransactionsPage />} />
                    <Route path="/transactions/:id" element={<TransactionsPage />} />

                    {/* Payouts */}
                    <Route path="/payouts" element={<PayoutsPage />} />

                    {/* Invoicing */}
                    <Route path="/invoicing" element={<InvoicingPage />} />

                    {/* Locals */}
                    <Route path="/settings/locals" element={<LocalsPage />} />
                    <Route path="/settings/locals/add" element={<LocalFormPage />} />
                    <Route path="/settings/locals/:id/edit" element={<LocalFormPage />} />

                    {/* Custom Charge Methods */}
                    <Route path="/settings/chargemethods" element={<CustomChargeMethodsPage />} />
                    <Route path="/settings/chargemethods/add" element={<CustomChargeMethodFormPage />} />
                    <Route path="/settings/chargemethods/:id/edit" element={<CustomChargeMethodFormPage />} />

                    {/* Employees */}
                    <Route path="/settings/employees" element={<EmployeesPage />} />
                    <Route path="/settings/employees/add" element={<EmployeeFormPage />} />
                    <Route path="/settings/employees/:id/edit" element={<EmployeeFormPage />} />

                    {/* Printers */}
                    <Route path="/settings/printersmanagement" element={<PrinterPage />} />
                    <Route path="/settings/printersmanagement/workers/:workerId" element={<PrinterPage />} />
                    <Route path="/settings/printersmanagement/workers/add" element={<PrinterWorkerFormPage />} />
                    <Route path="/settings/printersmanagement/workers/:id/edit" element={<PrinterWorkerFormPage />} />
                    <Route path="/settings/printersmanagement/printers/add" element={<PrinterFormPage />} />
                    <Route path="/settings/printersmanagement/printers/:id/edit" element={<PrinterFormPage />} />

                    {/* Terms And Conditions */}
                    <Route path="/termsAndConditions" element={<TermsAndConditionsPage />} />
                </Route>
            </Routes>
        </BrowserRouter>
    </>
}

const LayoutContent = () => {
    const { isExpanded, isHovered, isMobileOpen } = useSidebar();
  
    return (
    <div className="min-h-screen xl:flex">
        <AppSidebar />
        <Backdrop />

        <div
            className={`flex-1 transition-all duration-300 ease-in-out ${isExpanded || isHovered ? "lg:ml-[290px]" : "lg:ml-[90px]"} ${isMobileOpen ? "ml-0" : ""}`}
        >
            <AppHeader />
            <div className="p-4 mx-auto md:p-6">
                <Outlet />
            </div>
        </div>
    </div>
    );
}

const AuthLayoutRoute = () => {
    const auth = useAuth();

    if(auth.user == undefined) {
        return <Navigate to="/signin" />
    }
    
    return (
    <SidebarProvider>
        <LayoutContent />
    </SidebarProvider>
    )
}