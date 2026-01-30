import { useState } from 'react'
import Map from './components/Map'
import MergePage from './components/MergePage'
import { ConfigProvider } from './hooks/useConfig'
import { useAuth } from './hooks/useAuth'
import * as Sentry from "@sentry/react";

type Page = 'map' | 'merge';

function AppContent() {
  const [currentPage, setCurrentPage] = useState<Page>('map');
  const { user, isLoading, isAdmin, login, logout, clearAuthState } = useAuth();

  const handleNavigateToMerge = () => setCurrentPage('merge');
  const handleNavigateToMap = () => setCurrentPage('map');

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

  return (
    <Map
      user={user}
      isLoading={isLoading}
      isAdmin={isAdmin}
      login={login}
      logout={logout}
      clearAuthState={clearAuthState}
      onNavigateToMerge={handleNavigateToMerge}
    />
  );
}

function App() {
  return (
    <ConfigProvider>
      <div className="w-full h-full relative">
        <Sentry.ErrorBoundary fallback={<p>An error occurred</p>}>
          <AppContent />
        </Sentry.ErrorBoundary>
      </div>
    </ConfigProvider>
  )
}

export default App

