import { useEffect, useRef, useState, useCallback } from 'react';
import 'maplibre-gl/dist/maplibre-gl.css';
import TopBar from './TopBar';
import FeatureInfoPanel from './FeatureInfoPanel';
import { useMapInitialization } from '../hooks/useMapInitialization';
import { useAuth } from '../hooks/useAuth';
import { useConfig } from '../hooks/useConfig';
import type { FeatureProperties, NamingCategory, Viewport, SearchResult } from '../types/feature';
import { api } from '../utils/api';

// Helper to update URL without page reload
const updateUrl = (params: Record<string, string | number | null | undefined>): void => {
  const url = new URL(window.location.href);
  Object.entries(params).forEach(([key, value]) => {
    if (value === null || value === undefined) {
      url.searchParams.delete(key);
    } else {
      url.searchParams.set(key, String(value));
    }
  });
  window.history.replaceState({}, '', url);
};

const MapComponent = () => {
  const mapContainer = useRef<HTMLDivElement>(null);

  const [selectedFeature, setSelectedFeature] = useState<FeatureProperties | null>(null);
  const selectedFeatureRef = useRef<FeatureProperties | null>(null);

  // Cache of forum links created during this session (featureId -> forumLink)
  const forumLinksCache = useRef<Map<number, string>>(new Map());

  const [namingCategories, setNamingCategories] = useState<NamingCategory[]>([]);
  const [isCopied, setIsCopied] = useState(false);

  const initialParams = new URLSearchParams(window.location.search);
  const [viewport, setViewport] = useState<Viewport>({
    lat: parseFloat(initialParams.get('lat') || '') || 53.9045,
    lng: parseFloat(initialParams.get('lng') || '') || 27.5615
  });

  // Auth state
  const { user, isLoading: authLoading, isAdmin, login, logout, clearAuthState } = useAuth();
  const { config } = useConfig();

  // Sync ref with state for animation loop access
  useEffect(() => {
    selectedFeatureRef.current = selectedFeature;
    window._selectedFeatureRef = selectedFeatureRef;
    setIsCopied(false);
  }, [selectedFeature]);

  // Wrapper for feature selection that enriches with cached forum links
  const handleFeatureSelect = useCallback((feature: FeatureProperties | null) => {
    if (feature && forumLinksCache.current.has(feature.Id)) {
      const cachedLink = forumLinksCache.current.get(feature.Id)!;
      setSelectedFeature({ ...feature, ForumRelativeLink: cachedLink });
    } else {
      setSelectedFeature(feature);
    }
  }, []);

  // Initialize map with custom hook
  const { flyTo } = useMapInitialization({
    containerRef: mapContainer,
    selectedFeatureRef,
    onFeatureSelect: handleFeatureSelect,
    onViewportChange: setViewport,
    updateUrl,
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
    flyTo(result.longitude, result.latitude);
    updateUrl({ featureId: result.id });
    window._selectFeature?.(result.id);
  }, [flyTo]);

  const handleCopyLink = useCallback(() => {
    navigator.clipboard.writeText(window.location.href);
    setIsCopied(true);
    setTimeout(() => setIsCopied(false), 2000);
  }, []);

  const handleClosePanel = useCallback(() => {
    setSelectedFeature(null);
    updateUrl({ featureId: null });
  }, []);

  // Handler for when a forum topic is created
  const handleForumLinkCreated = useCallback((featureId: number, forumLink: string) => {
    // Cache the link for future re-selections
    forumLinksCache.current.set(featureId, forumLink);

    // Update the current selected feature if it matches
    setSelectedFeature(prev =>
      prev && prev.Id === featureId
        ? { ...prev, ForumRelativeLink: forumLink }
        : prev
    );
  }, []);

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
      />

      <div className="map-container-with-topbar">
        <div className="map-container" ref={mapContainer} />

        {selectedFeature && (
          <FeatureInfoPanel
            feature={selectedFeature}
            namingCategories={namingCategories}
            isCopied={isCopied}
            onCopyLink={handleCopyLink}
            onClose={handleClosePanel}
            isAdmin={isAdmin}
            isAuthenticated={!!user}
            discourseBaseUrl={config?.discourseBaseUrl}
            onForumLinkCreated={handleForumLinkCreated}
          />
        )}
      </div>
    </div>
  );
};

export default MapComponent;
