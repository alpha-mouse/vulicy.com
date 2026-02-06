import { FEATURE_TYPE_LABELS } from '../constants/mapConstants';
import { SearchResult, getFeatureName } from '../types/feature';

interface FeatureListItemProps {
  feature: SearchResult;
  truncate?: boolean;
}

/**
 * Common component for displaying a feature name with type label and optional location.
 * Used in search results and dossier records panels.
 */
const FeatureListItem = ({ feature, truncate = false }: FeatureListItemProps) => {
  const truncateClass = truncate ? 'truncate' : '';

  return (
    <div className="flex flex-col min-w-0">
      <span className={`text-sm font-medium text-black ${truncateClass}`}>
        {FEATURE_TYPE_LABELS[feature.type] && (
          <span className="text-black/40 font-medium">{FEATURE_TYPE_LABELS[feature.type]} </span>
        )}
        {getFeatureName(feature)}
      </span>
      {feature.location && (
        <span className={`text-xs text-black/50 ${truncateClass}`}>{feature.location}</span>
      )}
    </div>
  );
};

export default FeatureListItem;
