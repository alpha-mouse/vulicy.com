import { useEffect, useRef, useState, useCallback } from 'react';
import 'maplibre-gl/dist/maplibre-gl.css';
import './SourcesMap.css';
import { Menu, FileUser, Database, ListTree } from 'lucide-react';
import TopBar from './TopBar';
import FeatureInfoPanel from './FeatureInfoPanel';
import DossierRecordsPanel from './DossierRecordsPanel';
import Search from './Search';
import { useMapInitialization } from '../hooks/useMapInitialization';
import { useConfig } from '../hooks/useConfig';
import { useUrlParams } from '../hooks/useUrlParams';
import { useClickOutside } from '../hooks/useClickOutside';
import { useAuth } from '../hooks/useAuth';
import { useMapStore } from '../store/mapStore';
import type { FeatureProperties, SearchResult, NamingCategory } from '../types';
import { api } from '../utils/api';
import { computeCentroid } from '../utils/geometry';

interface MapComponentProps {
  clearAuthState: () => void;
}

// Menu + Search content for the TopBar left side
const MapTopBarContent = () => {
  const { user, isAdmin } = useAuth();
  const { setDossierPanelOpen } = useMapStore();

  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);

  useClickOutside(menuRef, () => setIsMenuOpen(false));

  const handleMenuItemClick = (action: () => void) => {
    action();
    setIsMenuOpen(false);
  };

  return (
    <>
      {/* Menu button - visible to all users */}
      <div className="relative" ref={menuRef}>
        <button
          onClick={() => setIsMenuOpen(!isMenuOpen)}
          className="p-2 hover:bg-black/5 rounded-lg transition-colors bg-transparent border-none cursor-pointer outline-none"
          title="Меню"
        >
          <Menu size={20} className="text-black/60" />
        </button>

        {isMenuOpen && (
          <div className="absolute top-full left-0 mt-1 bg-white dark:bg-slate-800 rounded-lg shadow-2xl border border-black/10 dark:border-white/10 overflow-hidden z-50 min-w-60">
            <button
              onClick={() => handleMenuItemClick(() => setDossierPanelOpen(true))}
              className="w-full px-4 py-2.5 text-left text-sm font-medium text-black hover:bg-black/5 transition-colors bg-transparent border-none cursor-pointer outline-none flex items-center gap-2"
            >
              <FileUser size={18} className="text-black/60" />
              <span>Імёны</span>
            </button>
            {isAdmin && user && (
              <>
                <a
                  href="/sources"
                  className="w-full px-4 py-2.5 text-left text-sm font-medium text-black hover:bg-black/5 transition-colors bg-transparent border-none cursor-pointer outline-none flex items-center gap-2 no-underline"
                >
                  <Database size={18} className="text-black/60" />
                  <span>Крыніцы</span>
                </a>
                <a
                  href="/administrative"
                  className="w-full px-4 py-2.5 text-left text-sm font-medium text-black hover:bg-black/5 transition-colors bg-transparent border-none cursor-pointer outline-none flex items-center gap-2 no-underline"
                >
                  <ListTree size={18} className="text-black/60" />
                  <span>Адміністрацыйны падзел</span>
                </a>
              </>
            )}
          </div>
        )}
      </div>
    </>
  );
};

const MapComponent = ({
  clearAuthState,
}: MapComponentProps) => {
  const mapContainer = useRef<HTMLDivElement>(null);
  const selectedFeatureRef = useRef<FeatureProperties | null>(null);

  // Auth from hook
  const { user, isAdmin } = useAuth();

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
  const { config } = useConfig();
  const { updateParams, getNumericParam } = useUrlParams();

  // Deep link: read dossierRecordId from URL on mount
  const [initialDossierRecordId, setInitialDossierRecordId] = useState<number | null>(() => {
    const id = getNumericParam('dossierRecordId');
    return id !== undefined && Number.isInteger(id) ? id : null;
  });

  // Auto-open dossier panel when deep link param is present
  useEffect(() => {
    if (initialDossierRecordId !== null) {
      setDossierPanelOpen(true);
    }
  }, []); // only on mount

  // Sync ref with state for animation loop access
  useEffect(() => {
    selectedFeatureRef.current = selectedFeature;
    setIsCopied(false);
  }, [selectedFeature]);

  // Wrapper for feature selection that uses cached data if available
  const handleFeatureSelect = useCallback((feature: FeatureProperties | null) => {
    setFeatureLoading(false);
    if (feature) {
      // Use cached feature data if available (contains recent edits/forum links)
      const cached = getCachedFeature(feature.id);
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
    const [longitude, latitude] = computeCentroid(result.geometry);
    flyTo(longitude, latitude);
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

  const handleCloseDossierPanel = useCallback(() => {
    setDossierPanelOpen(false);
    if (initialDossierRecordId !== null) {
      setInitialDossierRecordId(null);
      updateParams({ dossierRecordId: null });
    }
  }, [setDossierPanelOpen, initialDossierRecordId, updateParams]);

  const handleInitialRecordHandled = useCallback(() => {
    setInitialDossierRecordId(null);
    updateParams({ dossierRecordId: null });
  }, [updateParams]);

  // Handler for when a forum topic is created
  const handleForumLinkCreated = useCallback((featureId: number, forumLink: string) => {
    // Update current feature and cache it
    if (selectedFeature && selectedFeature.id === featureId) {
      const updated = { ...selectedFeature, forumRelativeLink: forumLink };
      setSelectedFeature(updated);
      cacheFeature(updated);
    }
  }, [cacheFeature, selectedFeature, setSelectedFeature]);

  // Handler for when a feature is updated
  const handleFeatureUpdated = useCallback((featureId: number, updatedData?: Partial<FeatureProperties>) => {
    if (updatedData && selectedFeature && selectedFeature.id === featureId) {
      // Merge updates and cache the result
      const updated = Object.assign({}, selectedFeature, updatedData) as FeatureProperties;
      setSelectedFeature(updated);
      cacheFeature(updated);

      // Update map color if classification or dossier record changed
      // Use same logic as paint: if own classification is 0, use dossierRecordClassification
      if (updatedData.classification !== undefined || updatedData.dossierRecordClassification !== undefined) {
        const effectiveClassification = (updated.classification && updated.classification > 0)
          ? updated.classification
          : (updated.dossierRecordClassification ?? 0);
        setFeatureClassification(featureId, effectiveClassification);
      }
    }
  }, [cacheFeature, selectedFeature, setSelectedFeature, setFeatureClassification]);

  return (
    <div className="flex flex-col h-full w-full">
      <TopBar
        leftContent={
          <>
            <MapTopBarContent />
            <Search
              currentLat={viewport.lat}
              currentLng={viewport.lng}
              onResultClick={handleResultClick}
              embedded
            />
          </>
        }
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
          onClose={handleCloseDossierPanel}
          onFeatureClick={handleResultClick}
          namingCategories={namingCategories}
          isAuthenticated={!!user}
          isAdmin={isAdmin}
          discourseBaseUrl={config?.discourseBaseUrl}
          initialRecordId={initialDossierRecordId}
          onInitialRecordHandled={handleInitialRecordHandled}
        />
      </div>
    </div>
  );
};

export default MapComponent;
