import { create } from 'zustand';
import type { FeatureProperties, Viewport } from '../types';

interface MapState {
  // Selected feature state
  selectedFeature: FeatureProperties | null;
  isFeatureLoading: boolean;
  setSelectedFeature: (feature: FeatureProperties | null) => void;
  setFeatureLoading: (loading: boolean) => void;

  // Viewport state
  viewport: Viewport;
  setViewport: (viewport: Viewport) => void;

  // Feature cache (featureId -> full feature data)
  // Used for caching updated feature data (edits, forum links) to avoid stale tile data
  featureCache: Map<number, FeatureProperties>;
  cacheFeature: (feature: FeatureProperties) => void;
  getCachedFeature: (featureId: number) => FeatureProperties | undefined;

  // Panel state
  isDossierPanelOpen: boolean;
  setDossierPanelOpen: (open: boolean) => void;

  // Copy link state
  isCopied: boolean;
  setIsCopied: (copied: boolean) => void;

  // Sources mode (admin-only view of cadastre/OSM data)
  isSourcesMode: boolean;
  setSourcesMode: (mode: boolean) => void;
}

export const useMapStore = create<MapState>((set, get) => ({
  // Selected feature
  selectedFeature: null,
  isFeatureLoading: false,
  setSelectedFeature: (feature) => set({ selectedFeature: feature, isFeatureLoading: false }),
  setFeatureLoading: (loading) => set({ isFeatureLoading: loading }),

  // Viewport
  viewport: {
    lat: parseFloat(new URLSearchParams(window.location.search).get('lat') || '') || 53.9045,
    lng: parseFloat(new URLSearchParams(window.location.search).get('lng') || '') || 27.5615,
  },
  setViewport: (viewport) => set({ viewport }),

  // Feature cache
  featureCache: new Map(),
  cacheFeature: (feature) => {
    const cache = new Map(get().featureCache);
    cache.set(feature.Id, feature);
    set({ featureCache: cache });
  },
  getCachedFeature: (featureId) => get().featureCache.get(featureId),

  // Panels
  isDossierPanelOpen: false,
  setDossierPanelOpen: (open) => set({ isDossierPanelOpen: open }),

  // Copy state
  isCopied: false,
  setIsCopied: (copied) => set({ isCopied: copied }),

  // Sources mode
  isSourcesMode: window.location.pathname === '/sources',
  setSourcesMode: (mode) => set({ isSourcesMode: mode }),
}));
