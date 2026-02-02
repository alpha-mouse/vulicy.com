import { useEffect, useRef, useCallback } from 'react';
import maplibregl, { Map as MapLibreMap } from 'maplibre-gl';
import 'maplibre-gl/dist/maplibre-gl.css';
import { SOURCES_CADASTRE_COLOR, SOURCES_OSM_COLOR } from '../constants/mapConstants';
import type { Viewport } from '../types';
import type { CadastreFeatureProperties, OsmFeatureProperties } from '../types/source-feature';
import { useConfig } from './useConfig';

interface UseSourcesMapInitializationOptions {
  containerRef: React.RefObject<HTMLDivElement | null>;
  onViewportChange: (viewport: Viewport) => void;
  updateUrl: (params: Record<string, string | number | null | undefined>) => void;
  onCadastreFeatureSelect: (feature: CadastreFeatureProperties | null) => void;
  onOsmFeatureSelect: (feature: OsmFeatureProperties | null) => void;
}

export const useSourcesMapInitialization = ({
  containerRef,
  onViewportChange,
  updateUrl,
  onCadastreFeatureSelect,
  onOsmFeatureSelect,
}: UseSourcesMapInitializationOptions) => {
  const { config } = useConfig();
  const map = useRef<MapLibreMap | null>(null);

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
    return window.location.origin + '/api/map/cadastre-tile/{z}/{x}/{y}.mvt';
  }, []);

  const getOsmTileUrl = useCallback(() => {
    return window.location.origin + '/api/map/osm-tile/{z}/{x}/{y}.mvt';
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

      // Cadastre layer - beetroot color, only unlinked features (FeatureId is null)
      map.current.addLayer({
        id: 'cadastre-layer',
        type: 'line',
        source: 'vulicy-cadastre',
        'source-layer': 'streets',
        filter: ['==', ['get', 'FeatureId'], null],
        paint: {
          'line-color': SOURCES_CADASTRE_COLOR,
          'line-width': ['interpolate', ['linear'], ['zoom'], 10, 1, 14, 3, 18, 6],
          'line-opacity': 0.8
        }
      });

      // OSM layer - dark orange color, only unlinked features (FeatureId is null)
      map.current.addLayer({
        id: 'osm-layer',
        type: 'line',
        source: 'vulicy-osm',
        'source-layer': 'streets',
        filter: [
          'all',
          ['==', ['get', 'FeatureId'], null],
          ['!=', ['get', 'highway'], 'footway'],
          ['!=', ['get', 'highway'], 'path'],
          ['!=', ['get', 'highway'], 'proposed'],
        ],
        paint: {
          'line-color': SOURCES_OSM_COLOR,
          'line-width': ['interpolate', ['linear'], ['zoom'], 10, 1, 14, 3, 18, 6],
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
        const props = e.features[0].properties as CadastreFeatureProperties;
        console.log('Cadastre feature clicked:', props);
        onCadastreFeatureSelect(props);
      });

      // Click handler for OSM layer
      map.current.on('click', 'osm-layer', (e: any) => {
        if (!e.features || e.features.length === 0) return;
        const props = e.features[0].properties as OsmFeatureProperties;
        console.log('OSM feature clicked:', props);
        onOsmFeatureSelect(props);
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
      if (map.current) {
        map.current.remove();
        map.current = null;
      }
    };
  }, [containerRef, onViewportChange, updateUrl, getCadastreTileUrl, getOsmTileUrl, config?.mapKey, onCadastreFeatureSelect, onOsmFeatureSelect]);

  return { map, flyTo };
};
