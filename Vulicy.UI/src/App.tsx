import { useState, useEffect } from 'react'
import Map from './components/Map'
import SourcesMap from './components/SourcesMap'
import MergePage from './components/MergePage'
import { ConfigProvider, useConfig } from './hooks/useConfig'
import { useAuth } from './hooks/useAuth'
import { useMapStore } from './store/mapStore'
import * as Sentry from "@sentry/react";

type Page = 'map' | 'merge';

function AppContent() {
  const [currentPage, setCurrentPage] = useState<Page>(window.location.pathname === '/dossier-deduplication' ? 'merge' : 'map');
  const { user, isLoading, isAdmin, login, logout, clearAuthState } = useAuth();
  const { isSourcesMode, setSourcesMode } = useMapStore();

  // Sync state with URL path
  useEffect(() => {
    const handleLocationChange = () => {
      const path = window.location.pathname;
      if (path === '/sources') {
        setCurrentPage('map');
        setSourcesMode(true);
      } else if (path === '/dossier-deduplication') {
        setCurrentPage('merge');
        setSourcesMode(false);
      } else {
        setCurrentPage('map');
        setSourcesMode(false);
      }
    };

    window.addEventListener('popstate', handleLocationChange);
    handleLocationChange(); // Initial check

    return () => window.removeEventListener('popstate', handleLocationChange);
  }, [setSourcesMode]);

  const navigateTo = (path: string) => {
    window.history.pushState(null, '', path);
    // Manually trigger popstate or just call state updates
    if (path === '/sources') {
      setCurrentPage('map');
      setSourcesMode(true);
    } else if (path === '/dossier-deduplication') {
      setCurrentPage('merge');
      setSourcesMode(false);
    } else {
      setCurrentPage('map');
      setSourcesMode(false);
    }
  };

  const handleNavigateToMerge = () => navigateTo('/dossier-deduplication');
  const handleNavigateToMap = () => navigateTo('/');
  const handleToggleSources = () => navigateTo(isSourcesMode ? '/' : '/sources');

  if (currentPage === 'merge' && isAdmin) {
    return (
      <MergePage
        user={user}
        isLoading={isLoading}
        onLogout={logout}
        onBack={handleNavigateToMap}
      />
    );
  }

  // Sources mode is admin-only
  if (isSourcesMode && isAdmin) {
    return (
      <SourcesMap
        user={user}
        isLoading={isLoading}
        isAdmin={isAdmin}
        login={login}
        logout={logout}
        onToggleSourcesMode={handleToggleSources}
      />
    );
  }

  return (
    <Map
      user={user}
      isLoading={isLoading}
      isAdmin={isAdmin}
      login={login}
      logout={logout}
      clearAuthState={clearAuthState}
      onNavigateToMerge={handleNavigateToMerge}
      onToggleSourcesMode={handleToggleSources}
    />
  );
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

