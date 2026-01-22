import { useEffect, useRef, useState, useCallback } from 'react';
import 'maplibre-gl/dist/maplibre-gl.css';
import TopBar from './TopBar';
import FeatureInfoPanel from './FeatureInfoPanel';
import DossierRecordsPanel from './DossierRecordsPanel';
import { useMapInitialization } from '../hooks/useMapInitialization';
import { useAuth } from '../hooks/useAuth';
import { useConfig } from '../hooks/useConfig';
import { useUrlParams } from '../hooks/useUrlParams';
import { useMapStore } from '../store/mapStore';
import type { FeatureProperties, NamingCategory, SearchResult } from '../types/feature';
import { api } from '../utils/api';

const MapComponent = () => {
  const mapContainer = useRef<HTMLDivElement>(null);
  const selectedFeatureRef = useRef<FeatureProperties | null>(null);

  // Zustand store
  const {
    selectedFeature,
    setSelectedFeature,
    isFeatureLoading,
    setFeatureLoading,
    viewport,
    setViewport,
    cacheForumLink,
    getForumLink,
    isDossierPanelOpen,
    setDossierPanelOpen,
    isCopied,
    setIsCopied,
  } = useMapStore();

  const [namingCategories, setNamingCategories] = useState<NamingCategory[]>([]);

  // Hooks
  const { user, isLoading: authLoading, isAdmin, login, logout, clearAuthState } = useAuth();
  const { config } = useConfig();
  const { updateParams } = useUrlParams();

  // Sync ref with state for animation loop access
  useEffect(() => {
    selectedFeatureRef.current = selectedFeature;
    window._selectedFeatureRef = selectedFeatureRef;
    setIsCopied(false);
  }, [selectedFeature]);

  // Wrapper for feature selection that enriches with cached forum links
  const handleFeatureSelect = useCallback((feature: FeatureProperties | null) => {
    setFeatureLoading(false);
    if (feature) {
      const cachedLink = getForumLink(feature.Id);
      setSelectedFeature(cachedLink ? { ...feature, ForumRelativeLink: cachedLink } : feature);
    } else {
      setSelectedFeature(null);
    }
  }, [setFeatureLoading, setSelectedFeature, getForumLink]);

  // Initialize map with custom hook
  const { flyTo } = useMapInitialization({
    containerRef: mapContainer,
    selectedFeatureRef,
    onFeatureSelect: handleFeatureSelect,
    onViewportChange: setViewport,
    updateUrl: updateParams,
    isAdmin,
    onAdminFallback: clearAuthState,
  });

  // Fetch naming categories
  useEffect(() => {
    api.get<NamingCategory[]>('/api/map/naming-categories')
      .then((data) => setNamingCategories(data))
      .catch(err => console.error('Failed to fetch naming categories:', err));
  }, []);

  // Handle popstate (back/forward browser buttons)
  useEffect(() => {
    const handlePopState = () => {
      const params = new URLSearchParams(window.location.search);
      const featureId = params.get('featureId');
      if (!featureId) {
        setSelectedFeature(null);
      }
    };

    window.addEventListener('popstate', handlePopState);
    return () => window.removeEventListener('popstate', handlePopState);
  }, []);

  // Update selection glow filter when selection changes
  useEffect(() => {
    // This is handled in the hook now via selectedFeatureRef
  }, [selectedFeature]);

  const handleResultClick = useCallback((result: SearchResult) => {
    setFeatureLoading(true);
    flyTo(result.longitude, result.latitude);
    updateParams({ featureId: result.id });
    window._selectFeature?.(result.id);
  }, [flyTo, setFeatureLoading, updateParams]);

  const handleCopyLink = useCallback(() => {
    navigator.clipboard.writeText(window.location.href);
    setIsCopied(true);
    setTimeout(() => setIsCopied(false), 2000);
  }, [setIsCopied]);

  const handleClosePanel = useCallback(() => {
    setSelectedFeature(null);
    setFeatureLoading(false);
    updateParams({ featureId: null });
  }, [setSelectedFeature, setFeatureLoading, updateParams]);

  // Handler for when a forum topic is created
  const handleForumLinkCreated = useCallback((featureId: number, forumLink: string) => {
    // Cache the link for future re-selections
    cacheForumLink(featureId, forumLink);

    // Update the current selected feature if it matches
    if (selectedFeature && selectedFeature.Id === featureId) {
      setSelectedFeature({ ...selectedFeature, ForumRelativeLink: forumLink });
    }
  }, [cacheForumLink, selectedFeature, setSelectedFeature]);

  // Handler for when a feature is updated - re-select to get fresh data
  const handleFeatureUpdated = useCallback((featureId: number, updatedData?: Partial<FeatureProperties>) => {
    if (updatedData && selectedFeature && selectedFeature.Id === featureId) {
      // Use Object.assign instead of spread to properly overwrite with undefined values
      setSelectedFeature(Object.assign({}, selectedFeature, updatedData) as FeatureProperties);
    }
  }, [selectedFeature, setSelectedFeature]);

  const handleLogin = useCallback(() => {
    login(window.location.href);
  }, [login]);

  return (
    <div className="flex flex-col h-full w-full">
      <TopBar
        user={user}
        isLoading={authLoading}
        onLogin={handleLogin}
        onLogout={logout}
        currentLat={viewport.lat}
        currentLng={viewport.lng}
        onResultClick={handleResultClick}
        isAdmin={isAdmin}
        onOpenDossierPanel={() => setDossierPanelOpen(true)}
      />

      <div className="map-container-with-topbar">
        <div className="map-container" ref={mapContainer} />

        {(selectedFeature || isFeatureLoading) && (
          <FeatureInfoPanel
            feature={selectedFeature}
            isLoading={isFeatureLoading}
            namingCategories={namingCategories}
            isCopied={isCopied}
            onCopyLink={handleCopyLink}
            onClose={handleClosePanel}
            isAdmin={isAdmin}
            isAuthenticated={!!user}
            discourseBaseUrl={config?.discourseBaseUrl}
            onForumLinkCreated={handleForumLinkCreated}
            onFeatureUpdated={handleFeatureUpdated}
          />
        )}

        <DossierRecordsPanel
          isOpen={isDossierPanelOpen}
          onClose={() => setDossierPanelOpen(false)}
          onFeatureClick={handleResultClick}
          namingCategories={namingCategories}
        />
      </div>
    </div>
  );
};

export default MapComponent;
