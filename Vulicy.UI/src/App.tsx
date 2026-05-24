import Map from './components/Map'
import SourcesMap from './components/SourcesMap'
import MergePage from './components/MergePage'
import AdministrativePage from './components/AdministrativePage'
import ExplicitlyCategorizedMap from './components/ExplicitlyCategorizedMap'
import { ConfigProvider, useConfig } from './hooks/useConfig'
import { useAuth, AuthProvider } from './hooks/useAuth'
import { useNavigation } from './hooks/useNavigation'
import * as Sentry from "@sentry/react";

function AppContent() {
  const { isAdmin, clearAuthState } = useAuth();
  const { isSourcesMode, isMergePage, isAdministrativePage, isExplicitlyCategorizedPage } = useNavigation();

  if (isMergePage && isAdmin) {
    return <MergePage />;
  }

  if (isAdministrativePage) {
    return <AdministrativePage />;
  }

  if (isExplicitlyCategorizedPage) {
    return <ExplicitlyCategorizedMap />;
  }

  // Sources mode is admin-only
  if (isSourcesMode && isAdmin) {
    return <SourcesMap />;
  }

  return <Map clearAuthState={clearAuthState} />;
}

function AppContainer() {
  const { config } = useConfig();

  if (config?.environment === 'development') {
    return (
      <div className="w-full h-full relative">
        <AppContent />
      </div>
    );
  }

  return (
    <div className="w-full h-full relative">
      <Sentry.ErrorBoundary fallback={<p>An error occurred</p>}>
        <AppContent />
      </Sentry.ErrorBoundary>
    </div>
  );
}

function App() {
  return (
    <ConfigProvider>
      <AuthProvider>
        <AppContainer />
      </AuthProvider>
    </ConfigProvider>
  )
}

export default App
