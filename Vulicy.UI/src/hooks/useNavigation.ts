import { useCallback, useSyncExternalStore } from 'react';

/**
 * Subscribe to browser location changes (for useSyncExternalStore)
 */
function subscribeToLocation(callback: () => void) {
  window.addEventListener('popstate', callback);
  return () => window.removeEventListener('popstate', callback);
}

function getLocationSnapshot() {
  return window.location.pathname;
}

/**
 * Navigation hook for app-wide navigation without prop drilling.
 * Uses URL as the single source of truth.
 */
export function useNavigation() {
  // useSyncExternalStore ensures re-render on popstate events
  const pathname = useSyncExternalStore(subscribeToLocation, getLocationSnapshot);

  const navigateTo = useCallback((path: string) => {
    window.history.pushState(null, '', path);
    // Dispatch a custom event to trigger re-renders in all useNavigation consumers
    window.dispatchEvent(new PopStateEvent('popstate'));
  }, []);

  const navigateToMap = useCallback(() => navigateTo('/'), [navigateTo]);
  const navigateToMerge = useCallback(() => navigateTo('/dossier-deduplication'), [navigateTo]);
  const navigateToSources = useCallback(() => navigateTo('/sources'), [navigateTo]);
  const navigateToAdministrative = useCallback(() => navigateTo('/administrative'), [navigateTo]);
  const navigateToExplicitlyCategorized = useCallback(() => navigateTo('/explicitly-categorized'), [navigateTo]);

  return {
    pathname,
    isSourcesMode: pathname === '/sources',
    isMergePage: pathname === '/dossier-deduplication',
    isAdministrativePage: pathname === '/administrative',
    isExplicitlyCategorizedPage: pathname === '/explicitly-categorized',
    navigateToMap,
    navigateToMerge,
    navigateToSources,
    navigateToAdministrative,
    navigateToExplicitlyCategorized,
  };
}
