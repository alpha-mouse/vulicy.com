import { useEffect, useRef, useCallback } from 'react';
import maplibregl, { Map as MapLibreMap, FilterSpecification } from 'maplibre-gl';
import 'maplibre-gl/dist/maplibre-gl.css';
import { SOURCES_CADASTRE_COLOR, SOURCES_OSM_COLOR } from '../constants/mapConstants';
import type { Viewport } from '../types';
import type { OsmFeature, CadastreFeature } from '../types/source-feature';
import type { SearchResult } from '../types/feature';
import { useConfig } from './useConfig';
import { useSourcesStore, selectOsmId, selectCadastreId } from '../store/sourcesStore';
import { createPulseAnimation, PulseLayerConfig } from '../utils/mapAnimations';

interface UseSourcesMapInitializationOptions {
  containerRef: React.RefObject<HTMLDivElement | null>;
  onViewportChange: (viewport: Viewport) => void;
  updateUrl: (params: Record<string, string | number | null | undefined>) => void;
  onCadastreFeatureSelect: (feature: CadastreFeature | null) => void;
  onOsmFeatureSelect: (feature: OsmFeature | null) => void;
  selectedFeature: SearchResult | null;
}

/**
 * Returns layer configs for pulse animation based on current OSM/Cadastre selection.
 * Uses Zustand store.getState() to access current selection without stale closures.
 */
const getSourcesLayerConfigs = (): PulseLayerConfig[] | null => {
  const state = useSourcesStore.getState();
  const selectedOsmId = selectOsmId(state);
  const selectedCadastreId = selectCadastreId(state);

  if (selectedOsmId === null && selectedCadastreId === null) {
    return null;
  }

  const configs: PulseLayerConfig[] = [];
  if (selectedOsmId !== null) {
    configs.push({ layerId: 'osm-selection-glow', baseColor: SOURCES_OSM_COLOR });
  }
  if (selectedCadastreId !== null) {
    configs.push({ layerId: 'cadastre-selection-glow', baseColor: SOURCES_CADASTRE_COLOR });
  }
  return configs;
};

// Common filters for OSM features
const OSM_HIGHWAY_EXCLUSION_FILTERS: FilterSpecification[] = [
  ['has', 'highway'],
  ['!=', ['get', 'highway'], 'footway'],
  ['!=', ['get', 'highway'], 'path'],
  ['!=', ['get', 'highway'], 'proposed'],
  [
    'any',
    ['has', 'name'],
    ['has', 'name:be'],
    ['has', 'name:be-tarask'],
    ['has', 'name:ru'],
  ],
];

export const useSourcesMapInitialization = ({
  containerRef,
  onViewportChange,
  updateUrl,
  onCadastreFeatureSelect,
  onOsmFeatureSelect,
  selectedFeature,
}: UseSourcesMapInitializationOptions) => {
  const { config } = useConfig();
  const map = useRef<MapLibreMap | null>(null);
  const animationFrameId = useRef<number | null>(null);

  // Get derived IDs for layer filter updates
  const selectedOsmId = useSourcesStore(selectOsmId);
  const selectedCadastreId = useSourcesStore(selectCadastreId);
  // Subscribe to hidden IDs to trigger effect when they change
  const hiddenOsmIds = useSourcesStore(state => state.hiddenOsmIds);
  const hiddenCadastreIds = useSourcesStore(state => state.hiddenCadastreIds);

  const flyTo = useCallback((longitude: number, latitude: number, zoom = 16) => {
    if (map.current) {
      map.current.flyTo({
        center: [longitude, latitude],
        zoom,
        essential: true,
        speed: 2.4
      });
    }
  }, []);

  // Get tile URLs for cadastre and OSM
  const getCadastreTileUrl = useCallback(() => {
    return window.location.origin + '/api/map/cadastre-unmatched-tile/{z}/{x}/{y}.mvt';
  }, []);

  const getOsmTileUrl = useCallback(() => {
    return window.location.origin + '/api/map/osm-unmatched-tile/{z}/{x}/{y}.mvt';
  }, []);

  useEffect(() => {
    if (map.current || !containerRef.current || !config?.mapKey) return;

    const params = new URLSearchParams(window.location.search);
    const lat = parseFloat(params.get('lat') || '');
    const lng = parseFloat(params.get('lng') || '');
    const zoom = parseFloat(params.get('z') || '');

    map.current = new maplibregl.Map({
      container: containerRef.current,
      style: `https://api.maptiler.com/maps/dataviz/style.json?key=${config.mapKey}`,
      center: (lat && lng) ? [lng, lat] : [27.5615, 53.9045],
      zoom: zoom || 12,
      transformRequest: (url) => {
        // Include credentials for API requests
        if (url.includes('/api/map/')) {
          return {
            url,
            credentials: 'same-origin' as const,
          };
        }
        return { url };
      }
    });

    map.current.addControl(new maplibregl.NavigationControl(), 'bottom-right');

    map.current.on('load', () => {
      if (!map.current) return;

      // Add cadastre vector tile source
      map.current.addSource('vulicy-cadastre', {
        type: 'vector',
        tiles: [getCadastreTileUrl()],
        minzoom: 0,
        maxzoom: 20,
      });

      // Add OSM vector tile source
      map.current.addSource('vulicy-osm', {
        type: 'vector',
        tiles: [getOsmTileUrl()],
        minzoom: 0,
        maxzoom: 20,
      });

      // Cadastre layer - beetroot color, only unlinked features (featureId is null)
      map.current.addLayer({
        id: 'cadastre-layer',
        type: 'line',
        source: 'vulicy-cadastre',
        'source-layer': 'streets',
        paint: {
          'line-color': SOURCES_CADASTRE_COLOR,
          'line-width': ['interpolate', ['linear'], ['zoom'], 10, 1, 14, 3, 18, 6],
          'line-opacity': 0.8
        }
      });

      // Cadastre selection glow layer
      map.current.addLayer({
        id: 'cadastre-selection-glow',
        type: 'line',
        source: 'vulicy-cadastre',
        'source-layer': 'streets',
        paint: {
          'line-color': SOURCES_CADASTRE_COLOR,
          'line-width': ['interpolate', ['linear'], ['zoom'], 10, 4, 14, 10, 18, 20],
          'line-opacity': 0.4,
          'line-blur': 5
        },
        filter: ['==', ['get', 'id'], '__none__']
      });

      // OSM layer - dark orange color, only unlinked features (featureId is null)
      map.current.addLayer({
        id: 'osm-layer',
        type: 'line',
        source: 'vulicy-osm',
        'source-layer': 'streets',
        filter: [
          'all',
          ...OSM_HIGHWAY_EXCLUSION_FILTERS
        ] as FilterSpecification,
        paint: {
          'line-color': SOURCES_OSM_COLOR,
          'line-width': ['interpolate', ['linear'], ['zoom'], 10, 1, 14, 3, 18, 6],
          'line-opacity': 0.8
        }
      });

      // OSM selection glow layer
      map.current.addLayer({
        id: 'osm-selection-glow',
        type: 'line',
        source: 'vulicy-osm',
        'source-layer': 'streets',
        paint: {
          'line-color': SOURCES_OSM_COLOR,
          'line-width': ['interpolate', ['linear'], ['zoom'], 10, 4, 14, 10, 18, 20],
          'line-opacity': 0.4,
          'line-blur': 5
        },
        filter: ['==', ['get', 'id'], -1]
      });

      // Start pulse animation
      const pulse = createPulseAnimation(map, animationFrameId, getSourcesLayerConfigs);
      animationFrameId.current = requestAnimationFrame(pulse);

      // Selected Feature (Vulicy) Source - for search results
      map.current.addSource('vulicy-selected-feature', {
        type: 'geojson',
        data: {
          type: 'FeatureCollection',
          features: []
        }
      });

      // Selected Feature Layer
      map.current.addLayer({
        id: 'vulicy-selected-feature-layer',
        type: 'line',
        source: 'vulicy-selected-feature',
        layout: {
          'line-join': 'round',
          'line-cap': 'round'
        },
        paint: {
          'line-color': '#2563eb', // blue-600 to match the search result
          'line-width': 4,
          'line-opacity': 0.8
        }
      });

      // Mouse events for cadastre layer
      map.current.on('mousemove', 'cadastre-layer', () => {
        if (map.current) {
          map.current.getCanvas().style.cursor = 'pointer';
        }
      });

      map.current.on('mouseleave', 'cadastre-layer', () => {
        if (map.current) {
          map.current.getCanvas().style.cursor = '';
        }
      });

      // Mouse events for OSM layer
      map.current.on('mousemove', 'osm-layer', () => {
        if (map.current) {
          map.current.getCanvas().style.cursor = 'pointer';
        }
      });

      map.current.on('mouseleave', 'osm-layer', () => {
        if (map.current) {
          map.current.getCanvas().style.cursor = '';
        }
      });

      // Click handler for cadastre layer
      map.current.on('click', 'cadastre-layer', (e: any) => {
        if (!e.features || e.features.length === 0) return;
        const props = e.features[0].properties as CadastreFeature;
        console.log('Cadastre feature clicked:', props);
        onCadastreFeatureSelect(props);
      });

      // Click handler for OSM layer
      map.current.on('click', 'osm-layer', (e: any) => {
        if (!e.features || e.features.length === 0) return;
        // Parse tags from JSON string (MVT serializes nested objects as strings)
        const rawProps = e.features[0].properties;
        const feature: OsmFeature = {
          ...rawProps,
          tags: typeof rawProps.tags === 'string' ? JSON.parse(rawProps.tags) : rawProps.tags,
        };
        console.log('OSM feature clicked:', feature);
        onOsmFeatureSelect(feature);
      });
    });

    // Viewport change tracking
    map.current.on('moveend', () => {
      if (!map.current) return;
      const { lng, lat } = map.current.getCenter();
      onViewportChange({ lat, lng });
      updateUrl({
        lat: lat.toFixed(6),
        lng: lng.toFixed(6),
        z: map.current.getZoom().toFixed(2)
      });
    });

    // Click on empty area clears selection
    map.current.on('click', (e) => {
      if (!map.current) return;
      const cadastreFeatures = map.current.queryRenderedFeatures(e.point, { layers: ['cadastre-layer'] });
      const osmFeatures = map.current.queryRenderedFeatures(e.point, { layers: ['osm-layer'] });
      if (cadastreFeatures.length === 0 && osmFeatures.length === 0) {
        onCadastreFeatureSelect(null);
        onOsmFeatureSelect(null);
      }
    });

    // Cleanup
    return () => {
      if (animationFrameId.current) {
        cancelAnimationFrame(animationFrameId.current);
      }
      if (map.current) {
        map.current.remove();
        map.current = null;
      }
    };
  }, [containerRef, onViewportChange, updateUrl, getCadastreTileUrl, getOsmTileUrl, config?.mapKey, onCadastreFeatureSelect, onOsmFeatureSelect]);

  // Update layer filters (selection glow + hidden features)
  useEffect(() => {
    if (!map.current) return;

    const currentOsmIds = useSourcesStore.getState().hiddenOsmIds;
    const currentCadastreIds = useSourcesStore.getState().hiddenCadastreIds;

    // Update OSM filters - merge highway exclusion with hidden IDs
    if (map.current.getLayer('osm-layer')) {
      map.current.setFilter('osm-layer', [
        'all',
        ...OSM_HIGHWAY_EXCLUSION_FILTERS,
        ['!', ['in', ['get', 'id'], ['literal', currentOsmIds]]]
      ] as FilterSpecification);
    }

    // Update Cadastre filters - just hidden IDs
    if (map.current.getLayer('cadastre-layer')) {
      map.current.setFilter('cadastre-layer', [
        '!', ['in', ['get', 'id'], ['literal', currentCadastreIds]]
      ]);
    }

    // Update OSM selection filter
    if (map.current.getLayer('osm-selection-glow')) {
      const osmFilter: FilterSpecification = selectedOsmId !== null
        ? ['==', ['get', 'id'], selectedOsmId]
        : ['==', ['get', 'id'], -1];
      map.current.setFilter('osm-selection-glow', osmFilter);
    }

    // Update Cadastre selection filter
    if (map.current.getLayer('cadastre-selection-glow')) {
      const cadastreFilter: FilterSpecification = selectedCadastreId !== null
        ? ['==', ['get', 'id'], selectedCadastreId]
        : ['==', ['get', 'id'], '__none__'];
      map.current.setFilter('cadastre-selection-glow', cadastreFilter);
    }

    // Restart animation loop if there's a selection and animation isn't running
    if ((selectedOsmId !== null || selectedCadastreId !== null) && !animationFrameId.current) {
      const pulse = createPulseAnimation(map, animationFrameId, getSourcesLayerConfigs);
      animationFrameId.current = requestAnimationFrame(pulse);
    }
  }, [selectedOsmId, selectedCadastreId, hiddenOsmIds, hiddenCadastreIds]);

  // Update selected feature layer data
  useEffect(() => {
    if (!map.current) return;

    const source = map.current.getSource('vulicy-selected-feature') as maplibregl.GeoJSONSource;
    if (source) {
      if (selectedFeature && selectedFeature.geometry) {
        source.setData({
          type: 'Feature',
          geometry: selectedFeature.geometry,
          properties: {}
        });
      } else {
        source.setData({
          type: 'FeatureCollection',
          features: []
        });
      }
    }
  }, [selectedFeature]);

  return { map, flyTo };
};
