import { X } from 'lucide-react';
import type { OsmFeature } from '../types/source-feature';
import { SOURCES_OSM_COLOR } from '../constants/mapConstants';

interface OsmInfoPanelProps {
  feature: OsmFeature;
  onClose: () => void;
}

const OsmInfoPanel = ({ feature, onClose }: OsmInfoPanelProps) => {
  const tags = feature.tags;

  // Get display name - prefer Belarusian, then general, then Russian
  const displayName = tags['name:be'] || tags['name'] || tags['name:ru'] || '—';
  const generalName = tags['name'];
  const russianName = tags['name:ru'];
  const highway = tags['highway'];

  return (
    <div className="absolute left-4 top-4 w-80 glass z-20 p-4 animate-slide-in-left">
      <div className="flex items-center justify-between mb-3">
        <div className="flex items-center gap-2">
          <div
            className="w-3 h-3 rounded-full"
            style={{ backgroundColor: SOURCES_OSM_COLOR }}
          />
          <h3 className="text-lg font-semibold text-black">OSM</h3>
        </div>
        <button
          onClick={onClose}
          className="p-1 hover:bg-black/5 rounded-lg transition-colors bg-transparent border-none cursor-pointer outline-none"
          title="Зачыніць"
        >
          <X size={20} className="text-black/60" />
        </button>
      </div>

      <div className="space-y-3">
        <div>
          <label className="text-xs text-black/50 uppercase tracking-wide">ID</label>
          <p className="text-sm font-medium text-black">{feature.type}/{feature.id}</p>
        </div>

        <div>
          <label className="text-xs text-black/50 uppercase tracking-wide">Назва</label>
          <p className="text-sm font-medium text-black">{displayName}</p>
        </div>

        {tags['name:be'] && generalName && tags['name:be'] !== generalName && (
          <div>
            <label className="text-xs text-black/50 uppercase tracking-wide">Назва (агульная)</label>
            <p className="text-sm font-medium text-black">{generalName}</p>
          </div>
        )}

        {russianName && (
          <div>
            <label className="text-xs text-black/50 uppercase tracking-wide">Назва (рас.)</label>
            <p className="text-sm font-medium text-black">{russianName}</p>
          </div>
        )}

        {highway && (
          <div>
            <label className="text-xs text-black/50 uppercase tracking-wide">Тып дарогі</label>
            <p className="text-sm font-medium text-black">{highway}</p>
          </div>
        )}

        {feature.type && (
          <div>
            <label className="text-xs text-black/50 uppercase tracking-wide">Тып OSM</label>
            <p className="text-sm font-medium text-black">{feature.type}</p>
          </div>
        )}
      </div>
    </div>
  );
};

export default OsmInfoPanel;
