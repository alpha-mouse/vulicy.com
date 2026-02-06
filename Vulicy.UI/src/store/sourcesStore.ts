import { create } from 'zustand';
import type { SearchResult } from '../types/feature';
import type { OsmFeature, CadastreFeature } from '../types/source-feature';

interface SourcesMapState {
  // Selected features (unified - works for both map clicks and search selections)
  selectedOsmFeature: OsmFeature | null;
  selectedCadastreFeature: CadastreFeature | null;
  selectedFeatureSearch: SearchResult | null;  // For the Vulicy feature search

  // Panel visibility (separate from selection so closing panel keeps selection)
  osmPanelOpen: boolean;
  cadastrePanelOpen: boolean;
  createDialogOpen: boolean;

  // Hidden features (locally hidden after creation until tiles reload)
  hiddenOsmIds: number[];
  hiddenCadastreIds: string[];

  // Actions
  selectOsm: (feature: OsmFeature) => void;
  selectCadastre: (feature: CadastreFeature) => void;
  setSelectedFeatureSearch: (result: SearchResult | null) => void;

  closeOsmPanel: () => void;
  closeCadastrePanel: () => void;

  openCreateDialog: () => void;
  closeCreateDialog: () => void;

  clearOsmSelection: () => void;
  clearCadastreSelection: () => void;

  hideFeatures: (osmId?: number, cadastreId?: string) => void;
}

export const useSourcesStore = create<SourcesMapState>((set) => ({
  // Selection state
  selectedOsmFeature: null,
  selectedCadastreFeature: null,
  selectedFeatureSearch: null,

  // Panel visibility
  osmPanelOpen: false,
  cadastrePanelOpen: false,
  createDialogOpen: false,
  hiddenOsmIds: [],
  hiddenCadastreIds: [],

  // Select actions - set feature and open panel
  selectOsm: (feature) => set({
    selectedOsmFeature: feature,
    osmPanelOpen: true,
  }),

  selectCadastre: (feature) => set({
    selectedCadastreFeature: feature,
    cadastrePanelOpen: true,
  }),

  setSelectedFeatureSearch: (result) => set({ selectedFeatureSearch: result }),

  // Close panel actions - only close panel, keep selection
  closeOsmPanel: () => set({ osmPanelOpen: false }),
  closeCadastrePanel: () => set({ cadastrePanelOpen: false }),

  // Create dialog actions
  openCreateDialog: () => set({ createDialogOpen: true }),
  closeCreateDialog: () => set({ createDialogOpen: false }),

  // Clear actions - clear selection and close panel
  clearOsmSelection: () => set({
    selectedOsmFeature: null,
    osmPanelOpen: false,
  }),

  clearCadastreSelection: () => set({
    selectedCadastreFeature: null,
    cadastrePanelOpen: false,
  }),

  hideFeatures: (osmId, cadastreId) => set((state) => ({
    hiddenOsmIds: osmId ? [...state.hiddenOsmIds, osmId] : state.hiddenOsmIds,
    hiddenCadastreIds: cadastreId ? [...state.hiddenCadastreIds, cadastreId] : state.hiddenCadastreIds,
  })),
}));

// Derived selectors for animation (used by useSourcesMapInitialization)
export const selectOsmId = (state: SourcesMapState) => state.selectedOsmFeature?.id ?? null;
export const selectCadastreId = (state: SourcesMapState) => state.selectedCadastreFeature?.id ?? null;
