import React, { useEffect, useRef, useState } from 'react';
import maplibregl from 'maplibre-gl';
import 'maplibre-gl/dist/maplibre-gl.css';
import { Info, X, Map as MapIcon, Layers } from 'lucide-react';

const CLASSIFICATION_COLORS = {
  1: '#ff4d4f', // Priority - Vibrant Red
  2: '#ff7875', // Required - Lighter Red
  3: '#ffa940', // Suggested - Orange
  4: '#ffec3d', // Possible - Yellow
  5: '#73d13d', // Unneeded - Green
  0: '#d9d9d9', // None/Unknown - Grey
};

const FEATURE_TYPE_LABELS = {
  11: 'вул.',
  12: 'пр-т',
  14: 'пл.',
  15: 'бульв.',
  16: 'тракт',
  17: 'наб.',
  18: 'шаша',
  19: 'кальцо',
  21: 'зав.',
  22: 'пр-зд',
  23: 'тупік',
  24: 'спуск',
  25: 'заезд',
  34: 'парк',
  39: 'сквэр',
};

const Map = () => {
  const mapContainer = useRef(null);
  const map = useRef(null);
  const [selectedFeature, setSelectedFeature] = useState(null);
  const [hoveredFeature, setHoveredFeature] = useState(null);
  const [namingCategories, setNamingCategories] = useState([]);

  useEffect(() => {
    fetch('/api/map/naming-categories')
      .then(res => res.json())
      .then(data => setNamingCategories(data))
      .catch(err => console.error('Failed to fetch naming categories:', err));
  }, []);

  useEffect(() => {
    if (map.current) return;

    const MAPTILER_KEY = 'MmlCv2msuHGpnA8SG2Ko';

    map.current = new maplibregl.Map({
      container: mapContainer.current,
      style: `https://api.maptiler.com/maps/dataviz/style.json?key=${MAPTILER_KEY}`,
      center: [27.5615, 53.9045], // Minsk
      zoom: 12
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
          'line-width': [
            'interpolate',
            ['linear'],
            ['zoom'],
            10, 2,
            14, 5,
            18, 10
          ],
          'line-opacity': 0.6
        },
        filter: ['==', ['get', 'Id'], -1]
      });

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
          setSelectedFeature(e.features[0].properties);
        }
      });
    });

    map.current.on('click', (e) => {
      const features = map.current.queryRenderedFeatures(e.point, { layers: ['streets-layer'] });
      if (features.length === 0) {
        setSelectedFeature(null);
      }
    });

  }, []);

  return (
    <div className="map-container w-full h-full absolute inset-0" ref={mapContainer}>
      {/* Search Header (Placeholder) - Hidden for now
      <div className="absolute top-4 left-4 z-10 w-80 glass p-3 flex items-center gap-3 rounded-xl">
        <MapIcon className="text-primary w-5 h-5" />
        <input
          type="text"
          placeholder="Пошук вуліцы..."
          className="bg-transparent border-none outline-none w-full text-sm"
        />
      </div>
      */}

      {/* Selected Feature Info Panel */}
      {selectedFeature && (
        <div className="absolute right-4 top-4 h-fit max-h-panel w-96 glass z-20 overflow-y-auto p-6 flex flex-col gap-6 animate-in slide-in-from-right duration-300">
          <div className="flex justify-between items-start">
            <h2 className="text-2xl font-bold leading-tight m-0">
              {FEATURE_TYPE_LABELS[selectedFeature.Type] && (
                <span className="text-black/40 font-medium mr-2">{FEATURE_TYPE_LABELS[selectedFeature.Type]} </span>
              )}
              {selectedFeature.NameBeTarask || selectedFeature.NameBeNark || selectedFeature.NameRu}
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
    </div>
  );
};

function getClassificationText(lvl) {
  const mapping = {
    0: 'Статус невядомы',
    1: 'Перайменаваньне неабходнае ў прыярытэтным парадку',
    2: 'Перайменаваньне неабходнае',
    3: 'Перайменаваньне пажаданае',
    4: 'Перайменаваньне магчымае',
    5: 'Перайменаваньне не патрэбнае',
  };
  return mapping[lvl] || mapping[0];
}

export default Map;
