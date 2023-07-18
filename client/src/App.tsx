/* eslint-disable @shopify/strict-component-boundaries */
import {Suspense} from 'react';
import {CssBaseline} from '@mui/material';
import {Route, Routes} from 'react-router-dom';
import {ThemeContextProvider} from '@shared/hooks/ThemeContextProvider';
import {RequireAuth} from '@shared/components/RequireAuth';
import {QueryClient, QueryClientProvider} from 'react-query';
import {ReactQueryDevtools} from 'react-query/devtools';
import {AlertProvider} from '@shared/hooks/Alerts/AlertContext';
import AlertSystem from '@shared/hooks/Alerts/AlertSystem';
import {NotifiSignalRContext} from '@shared/hooks/NotificationSignalRContext/NotificationSignalRContext';
import {HubConnectionBuilder} from '@microsoft/signalr';
import {NavBar} from '@shared/components/NavBar';
import {MarketplaceProvider} from '@shared/hooks/MarketplaceContext';
import {ScrollToTop} from '@shared/components/ScrollToTop';

import './App.css';
import Role from './pages/roles';
import {
  routeAdmin,
  routeAuthentication,
  routeCheckout,
  routeHome,
  routeLandingPage,
  routeMarket,
  routeOrders,
  routeProduct,
  routeProfile,
  routeSettings,
  routeShoppingCart,
  routeStyleGuide,
  routeWallet,
} from './pages/routes';
import {StyleGuide} from './pages/StyleGuide';
import {AuthenticationPage} from './pages/Authentication';
import {MarketPlace} from './pages/MarketPlace';
import {ProductPage} from './pages/MarketPlace/components/ProductPage';
import {HomePage} from './pages/HomePage';
import {Wallet} from './pages/Wallet';
import {ShoppingCart} from './pages/ShoppingCart';
import {Checkout} from './pages/Checkout';
import {Profile} from './pages/Profile';
import {Orders} from './pages/Orders';
import {Settings} from './pages/Settings';
import {Admin} from './pages/Admin';
import {LandingPage} from './pages/LandingPage';
import {apiEndpoint} from './data/api/routes';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 1000 * 60 * 5,
      cacheTime: 1000 * 60 * 5,
    },
  },
});

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AlertProvider>
        <Suspense fallback="Loading...">
          <NotifiSignalRContext.Provider
            value={new HubConnectionBuilder()
              .withUrl(`${apiEndpoint}hub/notifications`, {
                withCredentials: false,
              })
              .withAutomaticReconnect()
              .build()}
          >
            <ThemeContextProvider>
              <CssBaseline />
              <AlertSystem />
              <NavBar />
              <ScrollToTop />
              <Routes>
                <Route path={routeLandingPage} element={<LandingPage />} />
                <Route element={<RequireAuth {...[]} />}>
                  <Route path={routeHome} element={<HomePage />} />
                  <Route path={routeWallet} element={<Wallet />} />
                  <Route
                    path={routeMarket}
                    element={
                      <MarketplaceProvider>
                        <MarketPlace />
                      </MarketplaceProvider>
                    }
                  />
                  <Route
                    path={routeProduct}
                    element={
                      <MarketplaceProvider>
                        <ProductPage />
                      </MarketplaceProvider>
                    }
                  />
                  <Route
                    path={routeShoppingCart}
                    element={
                      <MarketplaceProvider>
                        <ShoppingCart />
                      </MarketplaceProvider>
                    }
                  />
                  <Route
                    path={routeCheckout}
                    element={
                      <MarketplaceProvider>
                        <Checkout />
                      </MarketplaceProvider>
                    }
                  />
                  <Route path={routeProfile} element={<Profile />} />
                  <Route path={routeOrders} element={<Orders />} />
                  <Route path={routeSettings} element={<Settings />} />
                </Route>
                <Route element={<RequireAuth {...[Role.Admin]} />}>
                  <Route path={routeStyleGuide} element={<StyleGuide />} />
                  <Route path={routeAdmin} element={<Admin />} />
                </Route>
                <Route
                  path={routeAuthentication}
                  element={<AuthenticationPage />}
                />
              </Routes>
            </ThemeContextProvider>
          </NotifiSignalRContext.Provider>
        </Suspense>
        <ReactQueryDevtools />
      </AlertProvider>
    </QueryClientProvider>
  );
}

export default App;
