import { useEffect, useRef, useCallback } from 'react';
import maplibregl, { Map as MapLibreMap, MapMouseEvent, MapSourceDataEvent } from 'maplibre-gl';
import 'maplibre-gl/dist/maplibre-gl.css';
import { CLASSIFICATION_COLORS } from '../constants/mapConstants';
import type { FeatureProperties, Viewport } from '../types/feature';

// Extend Window interface for global state
declare global {
  interface Window {
    _selectedFeatureRef?: React.MutableRefObject<FeatureProperties | null>;
    _selectFeature?: (id: string | number) => boolean;
  }
}

interface UseMapInitializationOptions {
  containerRef: React.RefObject<HTMLDivElement | null>;
  selectedFeatureRef: React.MutableRefObject<FeatureProperties | null>;
  onFeatureSelect: (feature: FeatureProperties | null) => void;
  onViewportChange: (viewport: Viewport) => void;
  updateUrl: (params: Record<string, string | number | null | undefined>) => void;
}

// Helper function for color interpolation
const interpolateHex = (hex1: string, hex2: string, factor: number): string => {
  const r1 = parseInt(hex1.substring(1, 3), 16);
  const g1 = parseInt(hex1.substring(3, 5), 16);
  const b1 = parseInt(hex1.substring(5, 7), 16);

  const r2 = parseInt(hex2.substring(1, 3), 16);
  const g2 = parseInt(hex2.substring(3, 5), 16);
  const b2 = parseInt(hex2.substring(5, 7), 16);

  const r = Math.round(r1 + factor * (r2 - r1));
  const g = Math.round(g1 + factor * (g2 - g1));
  const b = Math.round(b1 + factor * (b2 - b1));

  return `rgb(${r}, ${g}, ${b})`;
};

export const useMapInitialization = ({
  containerRef,
  selectedFeatureRef,
  onFeatureSelect,
  onViewportChange,
  updateUrl,
}: UseMapInitializationOptions) => {
  const map = useRef<MapLibreMap | null>(null);
  const animationFrameId = useRef<number | null>(null);

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

  useEffect(() => {
    if (map.current || !containerRef.current) return;

    const MAPTILER_KEY = 'MmlCv2msuHGpnA8SG2Ko';

    const params = new URLSearchParams(window.location.search);
    const lat = parseFloat(params.get('lat') || '');
    const lng = parseFloat(params.get('lng') || '');
    const zoom = parseFloat(params.get('z') || '');

    map.current = new maplibregl.Map({
      container: containerRef.current,
      style: `https://api.maptiler.com/maps/dataviz/style.json?key=${MAPTILER_KEY}`,
      center: (lat && lng) ? [lng, lat] : [27.5615, 53.9045],
      zoom: zoom || 12
    });

    map.current.addControl(new maplibregl.NavigationControl(), 'bottom-right');

    map.current.on('load', () => {
      if (!map.current) return;

      // Add vector tile source
      map.current.addSource('vulicy-streets', {
        type: 'vector',
        tiles: [window.location.origin + '/api/map/tile/{z}/{x}/{y}.mvt'],
        minzoom: 0,
        maxzoom: 20
      });

      // Main streets layer
      map.current.addLayer({
        id: 'streets-layer',
        type: 'line',
        source: 'vulicy-streets',
        'source-layer': 'streets',
        paint: {
          'line-color': [
            'match',
            ['to-string', ['get', 'Classification']],
            '1', CLASSIFICATION_COLORS[1],
            '2', CLASSIFICATION_COLORS[2],
            '3', CLASSIFICATION_COLORS[3],
            '4', CLASSIFICATION_COLORS[4],
            '5', CLASSIFICATION_COLORS[5],
            CLASSIFICATION_COLORS[0]
          ],
          'line-width': ['interpolate', ['linear'], ['zoom'], 10, 1, 14, 3, 18, 6],
          'line-opacity': 0.8
        }
      });

      // Highlight layer (hover)
      map.current.addLayer({
        id: 'streets-highlight',
        type: 'line',
        source: 'vulicy-streets',
        'source-layer': 'streets',
        paint: {
          'line-color': '#ffffff',
          'line-width': ['interpolate', ['linear'], ['zoom'], 10, 2, 14, 5, 18, 10],
          'line-opacity': 0.6
        },
        filter: ['==', ['get', 'Id'], -1]
      });

      // Selection glow layer
      map.current.addLayer({
        id: 'streets-selection-glow',
        type: 'line',
        source: 'vulicy-streets',
        'source-layer': 'streets',
        paint: {
          'line-color': '#ffffff',
          'line-width': ['interpolate', ['linear'], ['zoom'], 10, 4, 14, 10, 18, 20],
          'line-opacity': 0.4,
          'line-blur': 5
        },
        filter: ['==', ['get', 'Id'], -1]
      });

      // Pulse animation loop
      const startTime = Date.now();
      const pulse = () => {
        if (!map.current) return;

        const selected = window._selectedFeatureRef?.current;
        if (!selected) {
          animationFrameId.current = requestAnimationFrame(pulse);
          return;
        }

        const duration = 1500;
        const time = (Date.now() - startTime) % duration;
        const t = (Math.sin((time / duration) * Math.PI * 2) + 1) / 2;

        const baseColor = CLASSIFICATION_COLORS[selected.Classification] || CLASSIFICATION_COLORS[0];
        const interpolatedColor = interpolateHex(baseColor, '#ffffff', t);

        if (map.current.getLayer('streets-selection-glow')) {
          map.current.setPaintProperty('streets-selection-glow', 'line-color', interpolatedColor);
          map.current.setPaintProperty('streets-selection-glow', 'line-opacity', 0.4 + t * 0.4);
          map.current.setPaintProperty('streets-selection-glow', 'line-blur', 2 + t * 8);
        }

        animationFrameId.current = requestAnimationFrame(pulse);
      };
      animationFrameId.current = requestAnimationFrame(pulse);

      // Mouse events
      map.current.on('mousemove', 'streets-layer', (e: MapMouseEvent) => {
        if (!map.current || !e.features || e.features.length === 0) return;
        map.current.getCanvas().style.cursor = 'pointer';
        const id = (e.features[0].properties as FeatureProperties).Id;
        map.current.setFilter('streets-highlight', ['==', ['get', 'Id'], id]);
      });

      map.current.on('mouseleave', 'streets-layer', () => {
        if (!map.current) return;
        map.current.getCanvas().style.cursor = '';
        map.current.setFilter('streets-highlight', ['==', ['get', 'Id'], -1]);
      });

      map.current.on('click', 'streets-layer', (e: MapMouseEvent) => {
        if (!e.features || e.features.length === 0) return;
        const props = e.features[0].properties as FeatureProperties;
        onFeatureSelect(props);
        updateUrl({ featureId: props.Id });
      });

      // Feature selection function
      window._selectFeature = (id: string | number): boolean => {
        if (!map.current) return false;
        const features = map.current.querySourceFeatures('vulicy-streets', {
          sourceLayer: 'streets',
          filter: ['==', ['get', 'Id'], typeof id === 'string' ? parseInt(id) : id]
        });

        if (features.length > 0) {
          onFeatureSelect(features[0].properties as FeatureProperties);
          return true;
        }
        return false;
      };

      // Initial feature selection from URL
      const selectInitialFeature = () => {
        const featureId = new URLSearchParams(window.location.search).get('featureId');
        if (featureId && window._selectFeature) {
          window._selectFeature(featureId);
        }
      };

      map.current.on('sourcedata', (e: MapSourceDataEvent) => {
        if (e.sourceId === 'vulicy-streets' && e.isSourceLoaded) {
          selectInitialFeature();
        }
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
    map.current.on('click', (e: MapMouseEvent) => {
      if (!map.current) return;
      const features = map.current.queryRenderedFeatures(e.point, { layers: ['streets-layer'] });
      if (features.length === 0) {
        onFeatureSelect(null);
        updateUrl({ featureId: null });
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
  }, [containerRef, onFeatureSelect, onViewportChange, updateUrl]);

  // Update selection glow filter when selection changes
  useEffect(() => {
    if (!map.current) return;
    const id = selectedFeatureRef.current?.Id ?? -1;
    if (map.current.getLayer('streets-selection-glow')) {
      map.current.setFilter('streets-selection-glow', ['==', ['get', 'Id'], id]);
    }
  });

  return { map, flyTo };
};
