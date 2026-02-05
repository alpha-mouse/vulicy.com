import Map from './components/Map'
import SourcesMap from './components/SourcesMap'
import MergePage from './components/MergePage'
import { ConfigProvider, useConfig } from './hooks/useConfig'
import { useAuth } from './hooks/useAuth'
import { useNavigation } from './hooks/useNavigation'
import * as Sentry from "@sentry/react";

function AppContent() {
  const { isAdmin, clearAuthState } = useAuth();
  const { isSourcesMode, isMergePage } = useNavigation();

  if (isMergePage && isAdmin) {
    return <MergePage />;
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
      <AppContainer />
    </ConfigProvider>
  )
}

export default App
