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

const Map = () => {
  const mapContainer = useRef(null);
  const map = useRef(null);
  const [selectedFeature, setSelectedFeature] = useState(null);
  const [hoveredFeature, setHoveredFeature] = useState(null);

  useEffect(() => {
    if (map.current) return;

    map.current = new maplibregl.Map({
      container: mapContainer.current,
      style: {
        version: 8,
        sources: {
          'stadia-tiles': {
            type: 'raster',
            tiles: [
              'https://tiles.stadiamaps.com/tiles/alidade_smooth/{z}/{x}/{y}.png'
            ],
            tileSize: 256,
            attribution: '&copy; <a href="https://stadiamaps.com/">Stadia Maps</a>, &copy; <a href="https://openmaptiles.org/">OpenMapTiles</a> &copy; <a href="http://openstreetmap.org/copyright">OpenStreetMap</a> contributors',
          },
          'vulicy-streets': {
            type: 'vector',
            tiles: [window.location.origin + '/api/map/tile/{z}/{x}/{y}.mvt'],
            minzoom: 0,
            maxzoom: 20
          }
        },
        layers: [
          {
            id: 'base-layer',
            type: 'raster',
            source: 'stadia-tiles',
          },
          {
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
          },
          {
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
            filter: ['==', ['get', 'NameBeTarask'], ''] // Initially hide
          }
        ]
      },
      center: [27.5615, 53.9045], // Minsk
      zoom: 12
    });

    map.current.addControl(new maplibregl.NavigationControl(), 'bottom-right');

    map.current.on('mousemove', 'streets-layer', (e) => {
      if (e.features.length > 0) {
        map.current.getCanvas().style.cursor = 'pointer';
        const feature = e.features[0];
        setHoveredFeature(feature.properties);

        // Highlight logic - use a unique identifier if available, or NameBeTarask as fallback for grouping
        const id = feature.properties.NameBeTarask || '';
        map.current.setFilter('streets-highlight', ['==', ['get', 'NameBeTarask'], id]);
      }
    });

    map.current.on('mouseleave', 'streets-layer', () => {
      map.current.getCanvas().style.cursor = '';
      setHoveredFeature(null);
      map.current.setFilter('streets-highlight', ['==', ['get', 'NameBeTarask'], '']);
    });

    map.current.on('click', 'streets-layer', (e) => {
      if (e.features.length > 0) {
        setSelectedFeature(e.features[0].properties);
      }
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
      {/* Search Header (Placeholder) */}
      <div className="absolute top-4 left-4 z-10 w-80 glass p-3 flex items-center gap-3 rounded-xl">
        <MapIcon className="text-primary w-5 h-5" />
        <input
          type="text"
          placeholder="Пошук вуліцы..."
          className="bg-transparent border-none outline-none w-full text-sm"
        />
      </div>

      {/* Selected Feature Info Panel */}
      {selectedFeature && (
        <div className="absolute right-4 top-4 bottom-4 w-96 glass z-20 overflow-y-auto p-6 flex flex-col gap-6 animate-in slide-in-from-right duration-300">
          <div className="flex justify-between items-start">
            <h2 className="text-2xl font-bold leading-tight">
              {selectedFeature.NameBeTarask || selectedFeature.NameBeNark || selectedFeature.NameRu}
            </h2>
            <button
              onClick={() => setSelectedFeature(null)}
              className="p-2 hover:bg-black/5 rounded-full transition-colors"
            >
              <X size={20} />
            </button>
          </div>

          <div className="space-y-4">
            <section>
              <h3 className="text-xs font-semibold uppercase tracking-wider text-black/50 mb-1">Статус перайменаваньня</h3>
              <div className="flex items-center gap-2">
                <div
                  className="w-3 h-3 rounded-full"
                  style={{ backgroundColor: CLASSIFICATION_COLORS[selectedFeature.Classification] || CLASSIFICATION_COLORS[0] }}
                />
                <span className="font-medium text-lg">
                  {getClassificationText(selectedFeature.Classification)}
                </span>
              </div>
            </section>

            {selectedFeature.RenamingReason && (
              <section>
                <h3 className="text-xs font-semibold uppercase tracking-wider text-black/50 mb-1">Абгрунтаваньне</h3>
                <p className="text-sm leading-relaxed">{selectedFeature.RenamingReason}</p>
              </section>
            )}

            {selectedFeature.HistoricNames && (
              <section>
                <h3 className="text-xs font-semibold uppercase tracking-wider text-black/50 mb-1">Гістарычныя назвы</h3>
                <p className="text-sm italic">{selectedFeature.HistoricNames}</p>
              </section>
            )}

            <div className="pt-4 border-t border-black/10 grid grid-cols-2 gap-4">
              <div>
                <h4 className="text-[10px] font-bold uppercase text-black/40">Тып</h4>
                <p className="text-sm">{selectedFeature.Type || 'Невядома'}</p>
              </div>
              <div>
                <h4 className="text-[10px] font-bold uppercase text-black/40">Год назвы</h4>
                <p className="text-sm">{selectedFeature.YearNamed || '—'}</p>
              </div>
            </div>
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
        <div className="flex items-center gap-2 mb-2">
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
    1: 'Прыярытэтнае перайменаваньне',
    2: 'Неабходнае перайменаваньне',
    3: 'Пажаданае перайменаваньне',
    4: 'Магчымае перайменаваньне',
    5: 'Не патрэбнае перайменаваньне',
  };
  return mapping[lvl] || 'Статус невядомы';
}

export default Map;
