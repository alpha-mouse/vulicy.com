import { useState, useRef } from 'react';
import 'maplibre-gl/dist/maplibre-gl.css';
import { ArrowLeft } from 'lucide-react';
import TopBar from './TopBar';
import CadastreInfoPanel from './CadastreInfoPanel';
import OsmInfoPanel from './OsmInfoPanel';
import { useSourcesMapInitialization } from '../hooks/useSourcesMapInitialization';
import { useUrlParams } from '../hooks/useUrlParams';
import { useNavigation } from '../hooks/useNavigation';
import { useMapStore } from '../store/mapStore';
import type { CadastreFeatureProperties, OsmFeatureProperties } from '../types/source-feature';

// Back button for the TopBar left side
const BackButton = () => {
  const { navigateToMap } = useNavigation();

  return (
    <button
      onClick={navigateToMap}
      className="p-2 hover:bg-black/5 rounded-lg transition-colors bg-transparent border-none cursor-pointer outline-none"
      title="Вярнуцца да мапы"
    >
      <ArrowLeft size={20} className="text-black/60" />
    </button>
  );
};

const SourcesMap = () => {
  const mapContainer = useRef<HTMLDivElement>(null);

  // Selected features state
  const [selectedCadastre, setSelectedCadastre] = useState<CadastreFeatureProperties | null>(null);
  const [selectedOsm, setSelectedOsm] = useState<OsmFeatureProperties | null>(null);

  // Zustand store
  const { setViewport } = useMapStore();

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

  return (
    <div className="flex flex-col h-full w-full">
      <TopBar leftContent={<BackButton />} />

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
