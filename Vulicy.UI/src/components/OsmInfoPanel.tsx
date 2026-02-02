import { X } from 'lucide-react';
import type { OsmFeatureProperties } from '../types/source-feature';
import { SOURCES_OSM_COLOR } from '../constants/mapConstants';

interface OsmInfoPanelProps {
  feature: OsmFeatureProperties;
  onClose: () => void;
}

const OsmInfoPanel = ({ feature, onClose }: OsmInfoPanelProps) => {
  // Get display name - prefer Belarusian, then general, then Russian
  const displayName = feature['name:be'] || feature.name || feature['name:ru'] || '—';

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
          <p className="text-sm font-medium text-black">{feature.Type}/{feature.Id}</p>
        </div>

        <div>
          <label className="text-xs text-black/50 uppercase tracking-wide">Назва</label>
          <p className="text-sm font-medium text-black">{displayName}</p>
        </div>

        {feature['name:be'] && feature.name && feature['name:be'] !== feature.name && (
          <div>
            <label className="text-xs text-black/50 uppercase tracking-wide">Назва (агульная)</label>
            <p className="text-sm font-medium text-black">{feature.name}</p>
          </div>
        )}

        {feature['name:ru'] && (
          <div>
            <label className="text-xs text-black/50 uppercase tracking-wide">Назва (рас.)</label>
            <p className="text-sm font-medium text-black">{feature['name:ru']}</p>
          </div>
        )}

        {feature.highway && (
          <div>
            <label className="text-xs text-black/50 uppercase tracking-wide">Тып дарогі</label>
            <p className="text-sm font-medium text-black">{feature.highway}</p>
          </div>
        )}

        {feature.Type && (
          <div>
            <label className="text-xs text-black/50 uppercase tracking-wide">Тып OSM</label>
            <p className="text-sm font-medium text-black">{feature.Type}</p>
          </div>
        )}
      </div>
    </div>
  );
};

export default OsmInfoPanel;
