import { useState, useRef, useCallback } from 'react';
import 'maplibre-gl/dist/maplibre-gl.css';
import TopBar from './TopBar';
import CadastreInfoPanel from './CadastreInfoPanel';
import OsmInfoPanel from './OsmInfoPanel';
import { useSourcesMapInitialization } from '../hooks/useSourcesMapInitialization';
import { useUrlParams } from '../hooks/useUrlParams';
import { useMapStore } from '../store/mapStore';
import type { User } from '../types';
import type { CadastreFeatureProperties, OsmFeatureProperties } from '../types/source-feature';

interface SourcesMapProps {
  user: User | null;
  isLoading: boolean;
  isAdmin: boolean;
  login: (returnUrl?: string) => void;
  logout: () => Promise<void>;
  onToggleSourcesMode: () => void;
}

const SourcesMap = ({
  user,
  isLoading: authLoading,
  isAdmin,
  login,
  logout,
  onToggleSourcesMode,
}: SourcesMapProps) => {
  const mapContainer = useRef<HTMLDivElement>(null);

  // Selected features state
  const [selectedCadastre, setSelectedCadastre] = useState<CadastreFeatureProperties | null>(null);
  const [selectedOsm, setSelectedOsm] = useState<OsmFeatureProperties | null>(null);

  // Zustand store
  const {
    viewport,
    setViewport,
  } = useMapStore();

  // Hooks
  const { updateParams } = useUrlParams();

  // Initialize sources map with feature selection callbacks
  useSourcesMapInitialization({
    containerRef: mapContainer,
    onViewportChange: setViewport,
    updateUrl: updateParams,
    onCadastreFeatureSelect: setSelectedCadastre,
    onOsmFeatureSelect: setSelectedOsm,
  });

  const handleLogin = useCallback(() => {
    login(window.location.href);
  }, [login]);

  const handleResultClick = useCallback(() => {
    // Sources view doesn't support search-based feature selection
  }, []);

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
        isSourcesMode={true}
        onToggleSourcesMode={onToggleSourcesMode}
      />

      <div className="map-container-with-topbar">
        <div className="map-container" ref={mapContainer} />

        {/* OSM panel - left side */}
        {selectedOsm && (
          <OsmInfoPanel
            feature={selectedOsm}
            onClose={() => setSelectedOsm(null)}
          />
        )}

        {/* Cadastre panel - right side */}
        {selectedCadastre && (
          <CadastreInfoPanel
            feature={selectedCadastre}
            onClose={() => setSelectedCadastre(null)}
          />
        )}
      </div>
    </div>
  );
};

export default SourcesMap;
