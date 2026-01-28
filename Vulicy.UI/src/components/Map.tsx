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
    cacheFeature,
    getCachedFeature,
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

  // Wrapper for feature selection that uses cached data if available
  const handleFeatureSelect = useCallback((feature: FeatureProperties | null) => {
    setFeatureLoading(false);
    if (feature) {
      // Use cached feature data if available (contains recent edits/forum links)
      const cached = getCachedFeature(feature.Id);
      setSelectedFeature(cached ?? feature);
    } else {
      setSelectedFeature(null);
    }
  }, [setFeatureLoading, setSelectedFeature, getCachedFeature]);

  // Initialize map with custom hook
  const { flyTo, setFeatureClassification } = useMapInitialization({
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
    // Update current feature and cache it
    if (selectedFeature && selectedFeature.Id === featureId) {
      const updated = { ...selectedFeature, ForumRelativeLink: forumLink };
      setSelectedFeature(updated);
      cacheFeature(updated);
    }
  }, [cacheFeature, selectedFeature, setSelectedFeature]);

  // Handler for when a feature is updated
  const handleFeatureUpdated = useCallback((featureId: number, updatedData?: Partial<FeatureProperties>) => {
    if (updatedData && selectedFeature && selectedFeature.Id === featureId) {
      // Merge updates and cache the result
      const updated = Object.assign({}, selectedFeature, updatedData) as FeatureProperties;
      setSelectedFeature(updated);
      cacheFeature(updated);

      // Update map color if classification or dossier record changed
      // Use same logic as paint: if own Classification is 0, use DossierRecordClassification
      if (updatedData.Classification !== undefined || updatedData.DossierRecordClassification !== undefined) {
        const effectiveClassification = (updated.Classification && updated.Classification > 0)
          ? updated.Classification
          : (updated.DossierRecordClassification ?? 0);
        setFeatureClassification(featureId, effectiveClassification);
      }
    }
  }, [cacheFeature, selectedFeature, setSelectedFeature, setFeatureClassification]);

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
