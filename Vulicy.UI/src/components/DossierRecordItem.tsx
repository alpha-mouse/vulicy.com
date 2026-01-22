import { getClassificationText } from '../constants/mapConstants';
import type { DossierRecordSearchResult, NamingCategory } from '../types/feature';

interface DossierRecordItemProps {
  record: DossierRecordSearchResult;
  namingCategories: NamingCategory[];
  showFeatureCount?: boolean;
  compact?: boolean;
}

/**
 * Reusable component for displaying dossier record details.
 * Used in DossierRecordsPanel and DossierRecordPicker.
 */
const DossierRecordItem = ({
  record,
  namingCategories,
  showFeatureCount = false,
  compact = false,
}: DossierRecordItemProps) => {
  const categoryName = record.namingCategoryId
    ? namingCategories.find(c => c.id === record.namingCategoryId)?.name || '...'
    : null;

  if (compact) {
    return (
      <div className="flex flex-col min-w-0">
        <span className="text-sm font-medium text-black truncate">
          {record.nameBeTarask || record.nameBeNark || record.nameRu || '(без назвы)'}
        </span>
        {record.classification !== 0 && (
          <span className="text-xs text-black/50 truncate">
            {getClassificationText(record.classification)}
          </span>
        )}
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-1.5 text-sm">
      {record.nameBeNark && (
        <div className="leading-relaxed">
          <span className="text-black/50">Акадэмічны: </span>
          <span className="text-black">{record.nameBeNark}</span>
        </div>
      )}
      {record.nameRu && (
        <div className="leading-relaxed">
          <span className="text-black/50">Расейская: </span>
          <span className="text-black">{record.nameRu}</span>
        </div>
      )}
      {record.descriptionBe && (
        <div className="leading-relaxed">
          <span className="text-black/50">Апісаньне: </span>
          <span className="text-black">{record.descriptionBe}</span>
        </div>
      )}
      {record.descriptionRu && (
        <div className="leading-relaxed">
          <span className="text-black/50">Апісаньне КА: </span>
          <span className="text-black">{record.descriptionRu}</span>
        </div>
      )}
      {record.classification !== 0 && (
        <div className="leading-relaxed">
          <span className="text-black/50">Клясыфікацыя: </span>
          <span className="font-medium text-black">{getClassificationText(record.classification)}</span>
        </div>
      )}
      {categoryName && (
        <div className="leading-relaxed">
          <span className="text-black/50">Катэгорыя: </span>
          <span className="text-black">{categoryName}</span>
        </div>
      )}
      {showFeatureCount && (
        <div className="leading-relaxed">
          <span className="text-black/50">Аб'ектаў: </span>
          <span className="text-black">{record.numFeatures}</span>
        </div>
      )}
    </div>
  );
};

export default DossierRecordItem;
