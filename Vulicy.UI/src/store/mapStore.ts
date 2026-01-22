import { create } from 'zustand';
import type { FeatureProperties, Viewport } from '../types/feature';

interface MapState {
  // Selected feature state
  selectedFeature: FeatureProperties | null;
  isFeatureLoading: boolean;
  setSelectedFeature: (feature: FeatureProperties | null) => void;
  setFeatureLoading: (loading: boolean) => void;

  // Viewport state
  viewport: Viewport;
  setViewport: (viewport: Viewport) => void;

  // Forum links cache (featureId -> forumLink)
  forumLinksCache: Map<number, string>;
  cacheForumLink: (featureId: number, forumLink: string) => void;
  getForumLink: (featureId: number) => string | undefined;

  // Panel state
  isDossierPanelOpen: boolean;
  setDossierPanelOpen: (open: boolean) => void;

  // Copy link state
  isCopied: boolean;
  setIsCopied: (copied: boolean) => void;
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

  // Forum links cache
  forumLinksCache: new Map(),
  cacheForumLink: (featureId, forumLink) => {
    const cache = new Map(get().forumLinksCache);
    cache.set(featureId, forumLink);
    set({ forumLinksCache: cache });
  },
  getForumLink: (featureId) => get().forumLinksCache.get(featureId),

  // Panels
  isDossierPanelOpen: false,
  setDossierPanelOpen: (open) => set({ isDossierPanelOpen: open }),

  // Copy state
  isCopied: false,
  setIsCopied: (copied) => set({ isCopied: copied }),
}));
