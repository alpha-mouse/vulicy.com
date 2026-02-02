import { X } from 'lucide-react';
import type { CadastreFeatureProperties } from '../types/source-feature';
import { SOURCES_CADASTRE_COLOR } from '../constants/mapConstants';

interface CadastreInfoPanelProps {
  feature: CadastreFeatureProperties;
  onClose: () => void;
}

const CadastreInfoPanel = ({ feature, onClose }: CadastreInfoPanelProps) => {
  return (
    <div className="absolute right-4 top-4 w-80 glass z-20 p-4 animate-in">
      <div className="flex items-center justify-between mb-3">
        <div className="flex items-center gap-2">
          <div
            className="w-3 h-3 rounded-full"
            style={{ backgroundColor: SOURCES_CADASTRE_COLOR }}
          />
          <h3 className="text-lg font-semibold text-black">Кадастр</h3>
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
          <p className="text-sm font-medium text-black">{feature.Id}</p>
        </div>

        {feature.ElementNameBel && (
          <div>
            <label className="text-xs text-black/50 uppercase tracking-wide">Назва (бел.)</label>
            <p className="text-sm font-medium text-black">{feature.ElementNameBel}</p>
          </div>
        )}

        {feature.ElementName && (
          <div>
            <label className="text-xs text-black/50 uppercase tracking-wide">Назва (рас.)</label>
            <p className="text-sm font-medium text-black">{feature.ElementName}</p>
          </div>
        )}

        {feature.ElementTypeShortNameBel && (
          <div>
            <label className="text-xs text-black/50 uppercase tracking-wide">Тып</label>
            <p className="text-sm font-medium text-black">{feature.ElementTypeShortNameBel}</p>
          </div>
        )}

        {feature.ShortInfo && (
          <div>
            <label className="text-xs text-black/50 uppercase tracking-wide">Дадатковая інфармацыя</label>
            <p className="text-sm font-medium text-black">{feature.ShortInfo}</p>
          </div>
        )}
      </div>
    </div>
  );
};

export default CadastreInfoPanel;
