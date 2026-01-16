import React, { useEffect, useRef, useState } from 'react';
import maplibregl from 'maplibre-gl';
import 'maplibre-gl/dist/maplibre-gl.css';
import { Info, X, Map as MapIcon, Layers, Link, Check, Copy } from 'lucide-react';
import Search from './Search';
import { CLASSIFICATION_COLORS, FEATURE_TYPE_LABELS, getClassificationText } from '../constants/mapConstants';


const Map = () => {
  const mapContainer = useRef(null);
  const map = useRef(null);
  const [selectedFeature, setSelectedFeature] = useState(null);
  const selectedFeatureRef = useRef(null);
  // Export ref for the pulse loop which is defined in the initial useEffect
  useEffect(() => {
    selectedFeatureRef.current = selectedFeature;
    window._selectedFeatureRef = selectedFeatureRef;
  }, [selectedFeature]);

  const [hoveredFeature, setHoveredFeature] = useState(null);
  const [namingCategories, setNamingCategories] = useState([]);
  const [isCopied, setIsCopied] = useState(false);
  const initialParams = new URLSearchParams(window.location.search);
  const [viewport, setViewport] = useState({
    lat: parseFloat(initialParams.get('lat')) || 53.9045,
    lng: parseFloat(initialParams.get('lng')) || 27.5615
  });
  const initialFeatureId = useRef(initialParams.get('featureId'));

  // Helper to update URL without page reload
  const updateUrl = (params) => {
    const url = new URL(window.location);
    Object.entries(params).forEach(([key, value]) => {
      if (value === null || value === undefined) {
        url.searchParams.delete(key);
      } else {
        url.searchParams.set(key, value);
      }
    });
    window.history.replaceState({}, '', url);
  };

  useEffect(() => {
    fetch('/api/map/naming-categories')
      .then(res => res.json())
      .then(data => setNamingCategories(data))
      .catch(err => console.error('Failed to fetch naming categories:', err));
  }, []);

  useEffect(() => {
    if (map.current) return;

    const MAPTILER_KEY = 'MmlCv2msuHGpnA8SG2Ko';

    const params = new URLSearchParams(window.location.search);
    const lat = parseFloat(params.get('lat'));
    const lng = parseFloat(params.get('lng'));
    const zoom = parseFloat(params.get('z'));

    map.current = new maplibregl.Map({
      container: mapContainer.current,
      style: `https://api.maptiler.com/maps/dataviz/style.json?key=${MAPTILER_KEY}`,
      center: (lat && lng) ? [lng, lat] : [27.5615, 53.9045], // Minsk
      zoom: zoom || 12
    });

    map.current.addControl(new maplibregl.NavigationControl(), 'bottom-right');

    map.current.on('load', () => {
      map.current.addSource('vulicy-streets', {
        type: 'vector',
        tiles: [window.location.origin + '/api/map/tile/{z}/{x}/{y}.mvt'],
        minzoom: 0,
        maxzoom: 20
      });

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
          'line-width': [
            'interpolate',
            ['linear'],
            ['zoom'],
            10, 1,
            14, 3,
            18, 6
          ],
          'line-opacity': 0.8
        }
      });

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

      // Animation loop for pulse
      let startTime = Date.now();
      const pulse = () => {
        if (!map.current) return;

        const selected = window._selectedFeatureRef?.current;
        if (!selected) {
          requestAnimationFrame(pulse);
          return;
        }

        const duration = 1500; // Slightly faster pulse
        const time = (Date.now() - startTime) % duration;
        const t = (Math.sin((time / duration) * Math.PI * 2) + 1) / 2; // 0 to 1

        const baseColor = CLASSIFICATION_COLORS[selected.Classification] || CLASSIFICATION_COLORS[0];

        // Simple hex interpolation helper
        const interpolateHex = (hex1, hex2, factor) => {
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

        const targetColor = '#ffffff'; // White for glow
        const interpolatedColor = interpolateHex(baseColor, targetColor, t);
        const opacity = 0.4 + t * 0.4; // 0.4 to 0.8
        const blur = 2 + t * 8; // 2 to 10

        if (map.current.getLayer('streets-selection-glow')) {
          map.current.setPaintProperty('streets-selection-glow', 'line-color', interpolatedColor);
          map.current.setPaintProperty('streets-selection-glow', 'line-opacity', opacity);
          map.current.setPaintProperty('streets-selection-glow', 'line-blur', blur);
        }

        requestAnimationFrame(pulse);
      };
      pulse();

      // Events
      map.current.on('mousemove', 'streets-layer', (e) => {
        if (e.features.length > 0) {
          map.current.getCanvas().style.cursor = 'pointer';
          const feature = e.features[0];
          setHoveredFeature(feature.properties);

          const id = feature.properties.Id;
          map.current.setFilter('streets-highlight', ['==', ['get', 'Id'], id]);
        }
      });

      map.current.on('mouseleave', 'streets-layer', () => {
        map.current.getCanvas().style.cursor = '';
        setHoveredFeature(null);
        map.current.setFilter('streets-highlight', ['==', ['get', 'Id'], -1]);
      });

      map.current.on('click', 'streets-layer', (e) => {
        if (e.features.length > 0) {
          const feature = e.features[0];
          setSelectedFeature(feature.properties);
          updateUrl({ featureId: feature.properties.Id });
        }
      });

      // Export selection logic to window for internal use or ref-like access
      window._selectFeature = (id) => {
        const features = map.current.querySourceFeatures('vulicy-streets', {
          sourceLayer: 'streets',
          filter: ['==', ['get', 'Id'], parseInt(id)]
        });

        if (features.length > 0) {
          setSelectedFeature(features[0].properties);
          return true;
        }
        return false;
      };

      // Try to select initial feature when tiles are loaded
      const selectInitialFeature = () => {
        const params = new URLSearchParams(window.location.search);
        const featureId = params.get('featureId');
        if (featureId) {
          if (window._selectFeature(featureId)) {
            // Found it
          }
        }
      };

      map.current.on('sourcedata', (e) => {
        if (e.sourceId === 'vulicy-streets' && e.isSourceLoaded) {
          selectInitialFeature();
        }
      });
    });

    map.current.on('moveend', () => {
      const { lng, lat } = map.current.getCenter();
      setViewport({ lat, lng });
      updateUrl({
        lat: lat.toFixed(6),
        lng: lng.toFixed(6),
        z: map.current.getZoom().toFixed(2)
      });
    });

    map.current.on('click', (e) => {
      const features = map.current.queryRenderedFeatures(e.point, { layers: ['streets-layer'] });
      if (features.length === 0) {
        setSelectedFeature(null);
        updateUrl({ featureId: null });
      }
    });

    // Handle back/forward buttons (optional but good practice)
    window.onpopstate = () => {
      const params = new URLSearchParams(window.location.search);
      const featureId = params.get('featureId');
      if (!featureId) {
        setSelectedFeature(null);
      }
      // Note: Map view state restoration could be added here if desired
    };

  }, []);

  useEffect(() => {
    if (!map.current) return;

    const id = selectedFeature?.Id || -1;
    if (map.current.getLayer('streets-selection-glow')) {
      map.current.setFilter('streets-selection-glow', ['==', ['get', 'Id'], id]);
    }
  }, [selectedFeature]);

  return (
    <>
      <div className="map-container w-full h-full absolute inset-0" ref={mapContainer} />

      <Search
        currentLat={viewport.lat}
        currentLng={viewport.lng}
        onResultClick={(result) => {
          if (map.current) {
            map.current.flyTo({
              center: [result.longitude, result.latitude],
              zoom: 16,
              essential: true,
              speed: 2.4
            });
            updateUrl({ featureId: result.id });

            // Try to select immediately, if not found, it will be picked up by sourcedata event
            if (!window._selectFeature(result.id)) {
              // Not in current tiles, will be selected when tiles load
            }
          }
        }} />

      {/* Selected Feature Info Panel */}
      {selectedFeature && (
        <div className="absolute right-4 top-4 h-fit max-h-panel w-96 glass z-20 overflow-y-auto p-6 flex flex-col gap-6 animate-in slide-in-from-right duration-300">
          <div className="flex justify-between items-start">
            <h2 className="text-2xl font-bold leading-tight m-0">
              {FEATURE_TYPE_LABELS[selectedFeature.Type] && (
                <span className="text-black/40 font-medium mr-2">{FEATURE_TYPE_LABELS[selectedFeature.Type]} </span>
              )}
              {selectedFeature.NameBeTarask || selectedFeature.NameBeNark || selectedFeature.NameRu}
              <button
                onClick={() => {
                  navigator.clipboard.writeText(window.location.href);
                  setIsCopied(true);
                  setTimeout(() => setIsCopied(false), 2000);
                }}
                className="ml-2 p-1 inline-flex items-center justify-center text-black/30 hover:text-black/60 transition-colors bg-transparent border-none cursor-pointer outline-none align-middle"
                title="Скапіяваць спасылку"
              >
                {isCopied ? <Check size={16} className="text-green-500" /> : <Link size={16} />}
              </button>
            </h2>
            <button
              onClick={() => setSelectedFeature(null)}
              className="text-black/30 hover:text-black/60 transition-colors p-0 appearance-none bg-transparent border-none cursor-pointer outline-none"
            >
              <X size={20} strokeWidth={1.2} />
            </button>
          </div>

          <div className="flex flex-col gap-2">
            <div className="text-sm leading-relaxed">
              <span className="text-black/50">Клясыфікацыя: </span>
              <span className="font-medium text-black">{getClassificationText(selectedFeature.Classification)}</span>
            </div>

            {selectedFeature.EtymologyBeTarask && (
              <div className="text-sm leading-relaxed">
                <span className="text-black/50">Этымалёгія: </span>
                <span className="text-black">{selectedFeature.EtymologyBeTarask}</span>
              </div>
            )}

            {selectedFeature.RenamingReason && (
              <div className="text-sm leading-relaxed">
                <span className="text-black/50">Абгрунтаваньне: </span>
                <span className="text-black">{selectedFeature.RenamingReason}</span>
              </div>
            )}

            {selectedFeature.NamingCategoryId && (
              <div className="text-sm leading-relaxed">
                <span className="text-black/50">Катэгорыя: </span>
                <span className="text-black">
                  {namingCategories.find(c => c.id === selectedFeature.NamingCategoryId)?.name || '...'}
                </span>
              </div>
            )}

            {selectedFeature.HistoricNames && (
              <div className="text-sm leading-relaxed">
                <span className="text-black/50">Гістарычная(-ыя) назва(-ы): </span>
                <span className="text-black italic">{selectedFeature.HistoricNames}</span>
              </div>
            )}

            {selectedFeature.YearNamed && (
              <div className="text-sm leading-relaxed">
                <span className="text-black/50">Год назвы: </span>
                <span className="text-black">{selectedFeature.YearNamed}</span>
              </div>
            )}
          </div>

          {selectedFeature.ForumRelativeLink && (
            <a
              href={selectedFeature.ForumRelativeLink}
              target="_blank"
              rel="noopener noreferrer"
              className="mt-auto bg-primary text-white py-3 px-4 rounded-xl text-center font-semibold hover:bg-primary-hover transition-all shadow-lg shadow-primary/20"
            >
              Абмеркаваць на форуме
            </a>
          )}
        </div>
      )}

      {/* Legend */}
      <div className="absolute left-4 bottom-10 glass p-4 rounded-xl z-10 w-64 space-y-2">
        <div className="flex items-center gap-2">
          <Layers size={16} className="text-black/60" />
          <span className="text-xs font-bold uppercase tracking-widest text-black/60">Легенда</span>
        </div>
        {Object.entries(CLASSIFICATION_COLORS).map(([lvl, color]) => (
          lvl !== '0' && (
            <div key={lvl} className="flex items-center gap-3">
              <div className="w-3 h-3 rounded-full shrink-0" style={{ backgroundColor: color }} />
              <span className="text-xs font-medium text-black/80">{getClassificationText(lvl)}</span>
            </div>
          )
        ))}
      </div>
    </>
  );
};


export default Map;
