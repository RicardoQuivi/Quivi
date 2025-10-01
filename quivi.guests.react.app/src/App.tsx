import { Route, BrowserRouter, Routes, Navigate, Outlet, useLocation } from "react-router";
import './App.scss'
import SplashScreen from './pages/SplashScreen'
import { AnimatePresence } from 'framer-motion'
import { StrictMode, Suspense, type CSSProperties } from "react";
import { ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import { useQuiviTheme } from "./hooks/theme/useQuiviTheme";
import { ChannelProfilePage } from "./pages/ChannelProfile/ChannelProfilePage";
import { ScanQrCode } from "./pages/ScanQrCode/ScanQrCode";
import { AppContextProvider, useAppContext } from "./context/AppContextProvider";
import { MenuPage } from "./pages/Menu/MenuPage";
import { CartPage } from "./pages/Cart/CartPage";
import { SessionSummaryPage } from "./pages/SessionSummary/SessionSummaryPage";
import { PayPage } from "./pages/Pay/PayPage";
import { OrderCheckoutPage } from "./pages/Checkout/OrderCheckoutPage";
import { PaymentMethodsPage } from "./pages/PaymentMethods/PaymentMethodsPage";
import { PayOrderPage } from "./pages/PaymentForm/PayOrderPage";
import { TrackOrderPage } from "./pages/PaymentResult/TrackOrderPage";
import { PaySessionPage } from "./pages/PaymentForm/PaySessionPage";
import { SessionChargeCompletedPage } from "./pages/PaymentResult/SessionChargeCompletedPage";
import { OrdersPage } from "./pages/Orders/OrdersPage";
import { OrderingContextProvider } from "./context/OrderingContextProvider";
import RegisterPage from "./pages/Authentication/RegisterPage";
import LoginPage from "./pages/Authentication/LoginPage";
import { ConfirmEmailPage } from "./pages/Authentication/ConfirmEmailPage";
import { ForgotPasswordPage } from "./pages/Authentication/ForgotPasswordPage";
import { RecoverPasswordPage } from "./pages/Authentication/RecoverPasswordPage";
import { UserHomePage } from "./pages/Authentication/UserHomePage";

export const App = () => {
    const theme = useQuiviTheme();

    const toastStyle: CSSProperties & { [key: string]: string } = {
        '--toastify-color-progress-light': theme.primaryColor.hex,
    };

    return (
        <>
            <AnimatePresence mode="wait">
                <Suspense fallback={<SplashScreen />}>
                    <BrowserRouter>
                        <Routes>
                            <Route element={<AppContextsRoute/>}>
                                <Route path="/scancode" element={<ScanQrCode />} />

                                <Route path="/user/register" element={<RegisterPage />} />
                                <Route path="/user/confirm" element={<ConfirmEmailPage />} />
                                <Route path="/user/login" element={<LoginPage />} />
                                <Route path="/user/forgotpassword" element={<ForgotPasswordPage />} />
                                <Route path="/user/home" element={<UserHomePage />} />
                                <Route path="/user/recover" element={<RecoverPasswordPage />} />

                                <Route element={<ChannelLayoutRoute />}>
                                    <Route path="/c/:id" element={<ChannelProfilePage />} />
                                    <Route path="/c/:id/menu" element={<MenuPage />} />
                                    <Route path="/c/:id/cart" element={<CartPage />} />
                                    <Route path="/c/:id/session/summary" element={<SessionSummaryPage />} />

                                    <Route path="/c/:id/orders" element={<OrdersPage />} />

                                    <Route path="/c/:id/session/pay/total" element={<PayPage paymentSplit="total" />} />
                                    <Route path="/c/:id/session/pay/equal" element={<PayPage paymentSplit="equal" />} />
                                    <Route path="/c/:id/session/pay/items" element={<PayPage paymentSplit="items" />} />
                                    <Route path="/c/:id/session/pay/custom" element={<PayPage paymentSplit="custom" />} />
                                    <Route path="/c/:id/session/pay/free" element={<PayPage paymentSplit="freepayment" />} />

                                    <Route path="/c/:id/pay/checkout" element={<OrderCheckoutPage />} />
                                    <Route path="/c/:id/session/pay/methods/free" element={<PaymentMethodsPage isFreePayment />} />
                                    <Route path="/c/:id/session/pay/methods" element={<PaymentMethodsPage />}/>
                                    <Route path="/c/:id/session/pay/:chargeId" element={<PaySessionPage />} />
                                    <Route path="/c/:id/session/pay/:chargeId/complete" element={<SessionChargeCompletedPage />} />

                                    <Route path="/c/:id/pay/orders/:orderId/:chargeId" element={<PayOrderPage />} />
                                    <Route path="/c/:id/orders/:orderId/track" element={<TrackOrderPage />} />
                                </Route>
                            </Route>
                        </Routes>
                    </BrowserRouter>
                </Suspense>
            </AnimatePresence>
            <ToastContainer
                position="top-center"
                autoClose={10000}
                hideProgressBar={false}
                newestOnTop={false}
                closeOnClick
                rtl={false}
                pauseOnFocusLoss={true}
                draggable={false}
                pauseOnHover={false}
                theme="light"
                toastStyle={toastStyle}
            />
        </>
    );
}

const AppContextsRoute = () => {
    return <AppContextProvider>
        <Outlet />
    </AppContextProvider>
}

const ChannelLayoutRoute = () => {
    const appContext = useAppContext();
    const location = useLocation();

    if(appContext == undefined) {
        return <Navigate to="/" />
    }

    if(location.pathname == "/") {
        return <Navigate to={`/c/${appContext.channelId}`} replace />
    }
    
    return (
    <OrderingContextProvider>
        <StrictMode>
            <Outlet />
        </StrictMode>
    </OrderingContextProvider>
    )
}