import { useRef, useCallback, useState } from 'react';
import 'maplibre-gl/dist/maplibre-gl.css';
import './SourcesMap.css';
import { ArrowLeft, Plus, Loader2 } from 'lucide-react';
import TopBar from './TopBar';
import CadastreInfoPanel from './CadastreInfoPanel';
import OsmInfoPanel from './OsmInfoPanel';
import SourceFeatureSearch from './SourceFeatureSearch';
import FeatureCreateDialog from './FeatureCreateDialog';
import { useSourcesMapInitialization } from '../hooks/useSourcesMapInitialization';
import { useUrlParams } from '../hooks/useUrlParams';
import { useNavigation } from '../hooks/useNavigation';
import { useMapStore } from '../store/mapStore';
import { useSourcesStore } from '../store/sourcesStore';
import { computeCentroid } from '../utils/geometry';
import { OsmFeature, CadastreFeature, getOsmName, getCadastreName } from '../types/source-feature';
import { SearchResult, getFeatureName } from '../types/feature';

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



// Search controls component
interface SourcesMapSearchControlsProps {
  currentLat: number;
  currentLng: number;
  selectedOsm: OsmFeature | null;
  selectedFeature: SearchResult | null;
  selectedCadastre: CadastreFeature | null;
  onOsmSelect: (result: OsmFeature) => void;
  onOsmClear: () => void;
  onFeatureSelect: (result: SearchResult) => void;
  onFeatureClear: () => void;
  onCadastreSelect: (result: CadastreFeature) => void;
  onCadastreClear: () => void;
  onCreateClick: () => void;
  onLinkClick: () => void;
  isLinking: boolean;
}

const SourcesMapSearchControls = ({
  currentLat,
  currentLng,
  selectedOsm,
  selectedFeature,
  selectedCadastre,
  onOsmSelect,
  onOsmClear,
  onFeatureSelect,
  onFeatureClear,
  onCadastreSelect,
  onCadastreClear,
  onCreateClick,
  onLinkClick,
  isLinking,
}: SourcesMapSearchControlsProps) => {
  // Show "Add Street" button when OSM and Cadastre are selected but Vulicy feature is not
  const showCreateButton = selectedOsm && selectedCadastre && !selectedFeature;
  // Show "Link OSM" button when OSM and Vulicy are selected but Cadastre is not
  const showLinkButton = selectedOsm && selectedFeature && !selectedCadastre;
  return (
    <div className="flex-1 flex items-center justify-center gap-2">
      {/* OSM Feature Search */}
      <SourceFeatureSearch<OsmFeature>
        placeholder="OSM..."
        searchEndpoint="/api/osm-features/search-unmatched"
        selectedItem={selectedOsm}
        onSelect={onOsmSelect}
        onClear={onOsmClear}
        currentLat={currentLat}
        currentLng={currentLng}
        getItemKey={(item) => item.id}
        renderResult={(result) => (
          <div>
            <div className="font-medium">{getOsmName(result)}</div>
          </div>
        )}
        renderSelected={(result) => (
          <span className="text-orange-600 font-medium">{getOsmName(result)}</span>
        )}
      />

      {/* Link OSM to Feature button - always takes space, hidden when inapplicable */}
      <button
        onClick={onLinkClick}
        disabled={isLinking}
        className="w-8 h-8 flex items-center justify-center bg-primary text-white hover:bg-primary/90 transition-colors cursor-pointer border-none rounded-lg disabled:cursor-wait"
        style={{ visibility: showLinkButton ? 'visible' : 'hidden' }}
        title="Прыяднаць OSM да вуліцы"
      >
        {isLinking ? <Loader2 size={18} className="animate-spin" /> : <Plus size={18} />}
      </button>

      {/* Feature Search or Create Button */}
      {showCreateButton ? (
        <button
          onClick={onCreateClick}
          className="flex-1 max-w-[22rem] h-8 flex items-center justify-center gap-2 bg-primary text-white hover:bg-primary/90 transition-colors cursor-pointer border-none rounded-lg font-medium"
        >
          <Plus size={18} />
          <span>Дадаць вуліцу...</span>
        </button>
      ) : (
        <SourceFeatureSearch<SearchResult>
          placeholder="Вуліца..."
          searchEndpoint="/api/features/search"
          selectedItem={selectedFeature}
          onSelect={onFeatureSelect}
          onClear={onFeatureClear}
          currentLat={currentLat}
          currentLng={currentLng}
          getItemKey={(item) => item.id}
          renderResult={(result) => (
            <div>
              <div className="font-medium">{getFeatureName(result)}</div>
              {result.location && <div className="text-xs text-black/50">{result.location}</div>}
            </div>
          )}
          renderSelected={(result) => (
            <span className="text-blue-600 font-medium">{getFeatureName(result)}</span>
          )}
        />
      )}

      {/* Cadastre Feature Search */}
      <SourceFeatureSearch<CadastreFeature>
        placeholder="Кадастр..."
        searchEndpoint="/api/cadastre-features/search-unmatched"
        selectedItem={selectedCadastre}
        onSelect={onCadastreSelect}
        onClear={onCadastreClear}
        currentLat={currentLat}
        currentLng={currentLng}
        getItemKey={(item) => item.id}
        renderResult={(result) => (
          <div>
            <div className="font-medium">{getCadastreName(result)}</div>
            {result.location && <div className="text-xs text-black/50">{result.location}</div>}
          </div>
        )}
        renderSelected={(result) => (
          <span className="text-fuchsia-700 font-medium">{getCadastreName(result)}</span>
        )}
      />
    </div>
  );
};

const SourcesMap = () => {
  const mapContainer = useRef<HTMLDivElement>(null);
  const [isLinking, setIsLinking] = useState(false);

  // Get state from Zustand stores
  const { viewport, setViewport } = useMapStore();
  const {
    selectedOsmFeature,
    selectedCadastreFeature,
    selectedFeatureSearch,
    osmPanelOpen,
    cadastrePanelOpen,
    createDialogOpen,
    selectOsm,
    selectCadastre,
    setSelectedFeatureSearch,
    closeOsmPanel,
    closeCadastrePanel,
    openCreateDialog,
    closeCreateDialog,
    clearOsmSelection,
    clearCadastreSelection,
  } = useSourcesStore();

  // Hooks
  const { updateParams } = useUrlParams();

  // Callbacks for map click selections - just call selectOsm/selectCadastre
  const handleCadastreFeatureSelect = useCallback((feature: CadastreFeature | null) => {
    if (feature) {
      selectCadastre(feature);
    } else {
      clearCadastreSelection();
    }
  }, [selectCadastre, clearCadastreSelection]);

  const handleOsmFeatureSelect = useCallback((feature: OsmFeature | null) => {
    if (feature) {
      selectOsm(feature);
    } else {
      clearOsmSelection();
    }
  }, [selectOsm, clearOsmSelection]);

  // Initialize sources map
  const { flyTo } = useSourcesMapInitialization({
    containerRef: mapContainer,
    onViewportChange: setViewport,
    updateUrl: updateParams,
    onCadastreFeatureSelect: handleCadastreFeatureSelect,
    onOsmFeatureSelect: handleOsmFeatureSelect,
    selectedFeature: selectedFeatureSearch,
  });

  // Handle search result selections - select and fly to feature
  const handleOsmSearchSelect = useCallback((result: OsmFeature) => {
    selectOsm(result);
    if (result.geometry) {
      const [lng, lat] = computeCentroid(result.geometry);
      flyTo(lng, lat);
    }
  }, [flyTo, selectOsm]);

  const handleOsmClear = useCallback(() => {
    clearOsmSelection();
  }, [clearOsmSelection]);

  const handleFeatureSearchSelect = useCallback((result: SearchResult) => {
    setSelectedFeatureSearch(result);
    // Fly to the feature
    const [lng, lat] = computeCentroid(result.geometry);
    flyTo(lng, lat);
  }, [flyTo, setSelectedFeatureSearch]);

  const handleFeatureClear = useCallback(() => {
    setSelectedFeatureSearch(null);
  }, [setSelectedFeatureSearch]);

  const handleCadastreSearchSelect = useCallback((result: CadastreFeature) => {
    selectCadastre(result);
    if (result.geometry) {
      const [lng, lat] = computeCentroid(result.geometry);
      flyTo(lng, lat);
    }
  }, [flyTo, selectCadastre]);

  const handleCadastreClear = useCallback(() => {
    clearCadastreSelection();
  }, [clearCadastreSelection]);

  // Handle linking OSM feature to existing Vulicy feature
  const handleLinkOsm = useCallback(async () => {
    const osmFeature = selectedOsmFeature;
    const vulicyFeature = selectedFeatureSearch;
    if (!osmFeature || !vulicyFeature) return;

    setIsLinking(true);
    try {
      const response = await fetch(`/api/features/${vulicyFeature.id}/link-osm`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ id: osmFeature.id, type: osmFeature.type }),
      });

      if (!response.ok) {
        throw new Error('Failed to link OSM feature');
      }

      const updatedGeometry = await response.json();

      // Update the Vulicy feature with the new geometry
      setSelectedFeatureSearch({ ...vulicyFeature, geometry: updatedGeometry });

      // Hide the OSM feature from the map and clear selection
      useSourcesStore.getState().hideFeatures(osmFeature.id, undefined);
      clearOsmSelection();
    } catch (error) {
      console.error('Error linking OSM feature:', error);
      // TODO: Show error toast
    } finally {
      setIsLinking(false);
    }
  }, [selectedOsmFeature, selectedFeatureSearch, setSelectedFeatureSearch, clearOsmSelection]);

  // Handle feature creation success
  const handleFeatureCreated = useCallback((feature: SearchResult) => {
    // Set the new feature as selected
    setSelectedFeatureSearch(feature);

    // Hide the used source features from the map
    // We need to capture the IDs before clearing selection
    const osmId = useSourcesStore.getState().selectedOsmFeature?.id;
    const cadastreId = useSourcesStore.getState().selectedCadastreFeature?.id;

    // Call hideFeatures which appends to the hidden lists
    // Note: We use the hook selector or getState inside component, but here we can just use the store's action
    useSourcesStore.getState().hideFeatures(osmId, cadastreId);

    // Clear OSM and Cadastre selections
    clearOsmSelection();
    clearCadastreSelection();
    // Close the dialog
    closeCreateDialog();
  }, [setSelectedFeatureSearch, clearOsmSelection, clearCadastreSelection, closeCreateDialog]);

  return (
    <div className="flex flex-col h-full w-full">
      <TopBar
        leftContent={<BackButton />}
        centerContent={
          <SourcesMapSearchControls
            currentLat={viewport.lat}
            currentLng={viewport.lng}
            selectedOsm={selectedOsmFeature}
            selectedFeature={selectedFeatureSearch}
            selectedCadastre={selectedCadastreFeature}
            onOsmSelect={handleOsmSearchSelect}
            onOsmClear={handleOsmClear}
            onFeatureSelect={handleFeatureSearchSelect}
            onFeatureClear={handleFeatureClear}
            onCadastreSelect={handleCadastreSearchSelect}
            onCadastreClear={handleCadastreClear}
            onCreateClick={openCreateDialog}
            onLinkClick={handleLinkOsm}
            isLinking={isLinking}
          />
        }
      />

      <div className="map-container-with-topbar">
        <div className="map-container" ref={mapContainer} />

        {/* OSM panel - left side */}
        {osmPanelOpen && selectedOsmFeature && (
          <OsmInfoPanel
            feature={selectedOsmFeature}
            onClose={closeOsmPanel}
          />
        )}

        {/* Cadastre panel - right side */}
        {cadastrePanelOpen && selectedCadastreFeature && (
          <CadastreInfoPanel
            feature={selectedCadastreFeature}
            onClose={closeCadastrePanel}
          />
        )}

        {/* Feature creation dialog - center */}
        {createDialogOpen && selectedOsmFeature && selectedCadastreFeature && (
          <FeatureCreateDialog
            osmFeature={selectedOsmFeature}
            cadastreFeature={selectedCadastreFeature}
            onClose={closeCreateDialog}
            onCreated={handleFeatureCreated}
          />
        )}
      </div>
    </div>
  );
};

export default SourcesMap;
