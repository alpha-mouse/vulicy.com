import { useEffect, useRef, useState, useCallback } from 'react';
import 'maplibre-gl/dist/maplibre-gl.css';
import './SourcesMap.css';
import { ArrowLeft } from 'lucide-react';
import TopBar from './TopBar';
import FeatureInfoPanel from './FeatureInfoPanel';
import { useMapInitialization } from '../hooks/useMapInitialization';
import { useConfig } from '../hooks/useConfig';
import { useUrlParams } from '../hooks/useUrlParams';
import { useAuth } from '../hooks/useAuth';
import { useMapStore } from '../store/mapStore';
import type { FeatureProperties, NamingCategory } from '../types';
import { api } from '../utils/api';

const BackButton = () => (
  <a
    href="/"
    className="p-2 hover:bg-black/5 rounded-lg transition-colors bg-transparent border-none cursor-pointer outline-none inline-flex"
    title="Вярнуцца да мапы"
  >
    <ArrowLeft size={20} className="text-black/60" />
  </a>
);

const ExplicitlyCategorizedMap = () => {
  const mapContainer = useRef<HTMLDivElement>(null);
  const selectedFeatureRef = useRef<FeatureProperties | null>(null);

  const { user, isAdmin } = useAuth();
  const {
    selectedFeature,
    setSelectedFeature,
    isFeatureLoading,
    setFeatureLoading,
    setViewport,
    getCachedFeature,
    isCopied,
    setIsCopied,
  } = useMapStore();

  const [namingCategories, setNamingCategories] = useState<NamingCategory[]>([]);

  const { config } = useConfig();
  const { updateParams } = useUrlParams();

  // Sync ref with state for animation loop access
  useEffect(() => {
    selectedFeatureRef.current = selectedFeature;
    setIsCopied(false);
  }, [selectedFeature]);

  const handleFeatureSelect = useCallback((feature: FeatureProperties | null) => {
    setFeatureLoading(false);
    if (feature) {
      const cached = getCachedFeature(feature.id);
      setSelectedFeature(cached ?? feature);
    } else {
      setSelectedFeature(null);
    }
  }, [setFeatureLoading, setSelectedFeature, getCachedFeature]);

  // Initialize map with explicitly-categorized-tile endpoint
  useMapInitialization({
    containerRef: mapContainer,
    selectedFeatureRef,
    onFeatureSelect: handleFeatureSelect,
    onViewportChange: setViewport,
    updateUrl: updateParams,
    isAdmin,
    tileEndpoint: 'explicitly-categorized-tile',
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

  return (
    <div className="flex flex-col h-full w-full">
      <TopBar leftContent={<BackButton />} />

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
          />
        )}
      </div>
    </div>
  );
};

export default ExplicitlyCategorizedMap;
