import { Pencil, Trash2, Merge } from 'lucide-react';
import { getClassificationText, CLASSIFICATION_COLORS } from '../constants/mapConstants';
import type { DossierRecordSearchResult, NamingCategory } from '../types';

interface DossierRecordItemProps {
  record: DossierRecordSearchResult;
  namingCategories: NamingCategory[];
  showFeatureCount?: boolean;
  compact?: boolean;
  isAdmin?: boolean;
  onEdit?: (record: DossierRecordSearchResult) => void;
  onDelete?: (record: DossierRecordSearchResult) => void;
  onMerge?: (record: DossierRecordSearchResult) => void;
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
  isAdmin = false,
  onEdit,
  onDelete,
  onMerge,
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
          <span
            className="text-xs truncate font-medium"
            style={{ color: CLASSIFICATION_COLORS[record.classification] }}
          >
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
          <span
            className="font-medium"
            style={{ color: CLASSIFICATION_COLORS[record.classification] }}
          >
            {getClassificationText(record.classification)}
          </span>
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

      {/* Admin actions */}
      {isAdmin && (onEdit || onDelete || onMerge) && (
        <div className="flex gap-2 pt-2 border-t border-black/10 mt-1">
          {onEdit && (
            <button
              onClick={(e) => {
                e.stopPropagation();
                onEdit(record);
              }}
              className="flex items-center gap-1.5 px-2.5 py-1.5 text-xs text-black/60 hover:text-primary hover:bg-primary/10 rounded-md transition-colors bg-transparent border-none cursor-pointer"
              title="Рэдагаваць"
            >
              <Pencil size={14} />
              <span>Рэдагаваць</span>
            </button>
          )}
          {onDelete && record.numFeatures === 0 && (
            <button
              onClick={(e) => {
                e.stopPropagation();
                onDelete(record);
              }}
              className="flex items-center gap-1.5 px-2.5 py-1.5 text-xs text-black/60 hover:text-red-600 hover:bg-red-50 rounded-md transition-colors bg-transparent border-none cursor-pointer"
              title="Выдаліць"
            >
              <Trash2 size={14} />
              <span>Выдаліць</span>
            </button>
          )}
          {onMerge && (
            <button
              onClick={(e) => {
                e.stopPropagation();
                onMerge(record);
              }}
              className="flex items-center gap-1.5 px-2.5 py-1.5 text-xs text-black/60 hover:text-amber-600 hover:bg-amber-50 rounded-md transition-colors bg-transparent border-none cursor-pointer"
              title="Аб'яднаць"
            >
              <Merge size={14} />
              <span>Аб'яднаць</span>
            </button>
          )}
        </div>
      )}
    </div>
  );
};

export default DossierRecordItem;
