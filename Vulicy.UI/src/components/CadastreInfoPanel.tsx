import { X } from 'lucide-react';
import type { CadastreFeature } from '../types/source-feature';
import { SOURCES_CADASTRE_COLOR, getClassificationText } from '../constants/mapConstants';

interface CadastreInfoPanelProps {
  feature: CadastreFeature;
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
          <p className="text-sm font-medium text-black">{feature.id}</p>
        </div>

        {feature.elementNameBel && (
          <div>
            <label className="text-xs text-black/50 uppercase tracking-wide">Назва (бел.)</label>
            <p className="text-sm font-medium text-black">{feature.elementNameBel}</p>
          </div>
        )}

        {feature.elementName && (
          <div>
            <label className="text-xs text-black/50 uppercase tracking-wide">Назва (рас.)</label>
            <p className="text-sm font-medium text-black">{feature.elementName}</p>
          </div>
        )}

        {feature.elementTypeShortNameBel && (
          <div>
            <label className="text-xs text-black/50 uppercase tracking-wide">Тып</label>
            <p className="text-sm font-medium text-black">{feature.elementTypeShortNameBel}</p>
          </div>
        )}

        {feature.shortInfo && (
          <div>
            <label className="text-xs text-black/50 uppercase tracking-wide">Дадатковая інфармацыя</label>
            <p className="text-sm font-medium text-black">{feature.shortInfo}</p>
          </div>
        )}

        {/* Additional fields separator - only show if any extended fields are present */}
        {(feature.reason || feature.classification != null || feature.comment || feature.historicName || feature.historicPossible || feature.yearNamed || feature.nameCategory) && (
          <>
            <hr className="border-black/10 my-3" />

            {feature.reason && (
              <div>
                <label className="text-xs text-black/50 uppercase tracking-wide">Прычына наймення</label>
                <p className="text-sm font-medium text-black">{feature.reason}</p>
              </div>
            )}

            {feature.classification != null && (
              <div>
                <label className="text-xs text-black/50 uppercase tracking-wide">Класіфікацыя</label>
                <p className="text-sm font-medium text-black">{getClassificationText(feature.classification)}</p>
              </div>
            )}

            {feature.comment && (
              <div>
                <label className="text-xs text-black/50 uppercase tracking-wide">Каментар</label>
                <p className="text-sm font-medium text-black">{feature.comment}</p>
              </div>
            )}

            {feature.historicName && (
              <div>
                <label className="text-xs text-black/50 uppercase tracking-wide">Гістарычная назва</label>
                <p className="text-sm font-medium text-black">{feature.historicName}</p>
              </div>
            )}

            {feature.historicPossible && (
              <div>
                <label className="text-xs text-black/50 uppercase tracking-wide">Гістарычнасць магчыма</label>
                <p className="text-sm font-medium text-black">Так</p>
              </div>
            )}

            {feature.yearNamed && (
              <div>
                <label className="text-xs text-black/50 uppercase tracking-wide">Год наймення</label>
                <p className="text-sm font-medium text-black">{feature.yearNamed}</p>
              </div>
            )}

            {feature.nameCategory && (
              <div>
                <label className="text-xs text-black/50 uppercase tracking-wide">Катэгорыя назвы</label>
                <p className="text-sm font-medium text-black">{feature.nameCategory}</p>
              </div>
            )}
          </>
        )}
      </div>
    </div>
  );
};

export default CadastreInfoPanel;
