import { X, Link, Check } from 'lucide-react';
import { FEATURE_TYPE_LABELS, getClassificationText } from '../constants/mapConstants';
import type { FeatureProperties, NamingCategory } from '../types/feature';

interface FeatureInfoPanelProps {
  feature: FeatureProperties;
  namingCategories: NamingCategory[];
  isCopied: boolean;
  onCopyLink: () => void;
  onClose: () => void;
}

const FeatureInfoPanel = ({
  feature,
  namingCategories,
  isCopied,
  onCopyLink,
  onClose,
}: FeatureInfoPanelProps) => {
  return (
    <div className="absolute right-4 top-4 h-fit max-h-panel w-96 glass z-20 overflow-y-auto p-6 flex flex-col gap-6 animate-in">
      <div className="flex justify-between items-start">
        <h2 className="text-2xl font-bold leading-tight m-0">
          {FEATURE_TYPE_LABELS[feature.Type] && (
            <span className="text-black/40 font-medium mr-2">{FEATURE_TYPE_LABELS[feature.Type]} </span>
          )}
          {feature.NameBeTarask || feature.NameBeNark || feature.NameRu}
          <button
            onClick={onCopyLink}
            className="ml-2 p-1 inline-flex items-center justify-center text-black/30 hover:text-black/60 transition-colors bg-transparent border-none cursor-pointer outline-none align-middle"
            title="Скапіяваць спасылку"
          >
            {isCopied ? <Check size={16} className="text-green-500" /> : <Link size={16} />}
          </button>
        </h2>
        <button
          onClick={onClose}
          className="text-black/30 hover:text-black/60 transition-colors p-0 appearance-none bg-transparent border-none cursor-pointer outline-none"
        >
          <X size={20} strokeWidth={1.2} />
        </button>
      </div>

      <div className="flex flex-col gap-2">
        <div className="text-sm leading-relaxed">
          <span className="text-black/50">Клясыфікацыя: </span>
          <span className="font-medium text-black">{getClassificationText(feature.Classification)}</span>
        </div>

        {feature.EtymologyBeTarask && (
          <div className="text-sm leading-relaxed">
            <span className="text-black/50">Этымалёгія: </span>
            <span className="text-black">{feature.EtymologyBeTarask}</span>
          </div>
        )}

        {feature.RenamingReason && (
          <div className="text-sm leading-relaxed">
            <span className="text-black/50">Абгрунтаваньне: </span>
            <span className="text-black">{feature.RenamingReason}</span>
          </div>
        )}

        {feature.NamingCategoryId && (
          <div className="text-sm leading-relaxed">
            <span className="text-black/50">Катэгорыя: </span>
            <span className="text-black">
              {namingCategories.find(c => c.id === feature.NamingCategoryId)?.name || '...'}
            </span>
          </div>
        )}

        {feature.HistoricNames && (
          <div className="text-sm leading-relaxed">
            <span className="text-black/50">Гістарычная(-ыя) назва(-ы): </span>
            <span className="text-black italic">{feature.HistoricNames}</span>
          </div>
        )}

        {feature.YearNamed && (
          <div className="text-sm leading-relaxed">
            <span className="text-black/50">Год назвы: </span>
            <span className="text-black">{feature.YearNamed}</span>
          </div>
        )}
      </div>

      {feature.ForumRelativeLink && (
        <a
          href={feature.ForumRelativeLink}
          target="_blank"
          rel="noopener noreferrer"
          className="mt-auto bg-primary text-white py-3 px-4 rounded-xl text-center font-semibold hover:bg-primary-hover transition-all shadow-lg shadow-primary/20"
        >
          Абмеркаваць на форуме
        </a>
      )}
    </div>
  );
};

export default FeatureInfoPanel;
