import { Check } from 'lucide-react';
import type { DossierRecordSearchResult, NamingCategory } from '../types';
import { CLASSIFICATION_OPTIONS } from '../constants/mapConstants';

type FieldKey = 'nameBeTarask' | 'nameBeNark' | 'nameRu' | 'descriptionBe' | 'descriptionRu' | 'classification' | 'namingCategoryId';
type Side = 'left' | 'right';

interface FieldSelection {
  side: Side;
  locked: boolean; // true when both values are equal
}

export type Selections = Record<FieldKey, FieldSelection>;

interface MergeComparisonTableProps {
  leftRecord: DossierRecordSearchResult;
  rightRecord: DossierRecordSearchResult;
  selections: Selections;
  onSelect: (field: FieldKey, side: Side) => void;
  namingCategories: NamingCategory[];
}

const FIELD_LABELS: Record<FieldKey, string> = {
  nameBeTarask: 'Назва (клясычны)',
  nameBeNark: 'Назва (акадэмічны)',
  nameRu: 'Назва (расейская)',
  descriptionBe: 'Апісаньне (бел.)',
  descriptionRu: 'Апісаньне (рас.)',
  classification: 'Клясыфікацыя',
  namingCategoryId: 'Катэгорыя',
};

function getClassificationLabel(value: number): string {
  const option = CLASSIFICATION_OPTIONS.find(o => o.value === value);
  return option?.label ?? '—';
}

function getDisplayValue(
  value: string | number | null | undefined,
  field: FieldKey,
  namingCategories: NamingCategory[]
): string {
  if (field === 'classification') {
    const numValue = typeof value === 'number' ? value : 0;
    return numValue === 0 ? '—' : getClassificationLabel(numValue);
  }
  if (field === 'namingCategoryId') {
    if (value === null || value === undefined) return '—';
    const category = namingCategories.find(c => c.id === value);
    return category?.name ?? `#${value}`;
  }
  if (value === null || value === undefined || value === '') {
    return '—';
  }
  return String(value);
}

function isEmpty(value: string | number | null | undefined, field: FieldKey): boolean {
  if (field === 'classification') {
    return value === 0 || value === null || value === undefined;
  }
  return value === null || value === undefined || value === '';
}

const MergeComparisonTable = ({
  leftRecord,
  rightRecord,
  selections,
  onSelect,
  namingCategories,
}: MergeComparisonTableProps) => {
  const fields: FieldKey[] = ['nameBeTarask', 'nameBeNark', 'nameRu', 'descriptionBe', 'descriptionRu', 'classification', 'namingCategoryId'];

  return (
    <div className="w-full max-w-4xl mx-auto">
      <table className="w-full border-collapse">
        <thead>
          <tr className="border-b border-black/10">
            <th className="text-left py-3 px-4 text-sm font-semibold text-black/70 w-1/4">Поле</th>
            <th className="text-left py-3 px-4 text-sm font-semibold text-black/70 w-[37.5%]">Запіс А</th>
            <th className="text-left py-3 px-4 text-sm font-semibold text-black/70 w-[37.5%]">Запіс Б</th>
          </tr>
        </thead>
        <tbody>
          {fields.map((field) => {
            const leftValue = leftRecord[field];
            const rightValue = rightRecord[field];
            const selection = selections[field];
            const isLeftSelected = selection.side === 'left';
            const isRightSelected = selection.side === 'right';
            const bothSelected = selection.locked;
            const leftEmpty = isEmpty(leftValue, field);
            const rightEmpty = isEmpty(rightValue, field);

            return (
              <tr key={field} className="border-b border-black/5 hover:bg-black/2">
                <td className="py-3 px-4 text-sm font-medium text-black/60">
                  {FIELD_LABELS[field]}
                </td>
                <td className="py-2 px-2">
                  <button
                    onClick={() => !selection.locked && onSelect(field, 'left')}
                    disabled={selection.locked || leftEmpty}
                    className={`relative w-full text-left py-2 pl-3 pr-8 rounded-lg transition-all text-sm border-2 ${bothSelected
                      ? 'bg-green-100 border-green-400 dark:bg-green-900/30 dark:border-green-500/50 dark:text-green-100 cursor-default'
                      : isLeftSelected
                        ? 'bg-primary/10 border-primary dark:bg-primary/20 dark:border-primary/50 dark:text-blue-100 cursor-default'
                        : leftEmpty
                          ? 'bg-black/5 text-black/30 dark:bg-white/5 dark:text-white/20 cursor-not-allowed border-transparent'
                          : 'bg-black/5 hover:bg-black/10 dark:bg-white/5 dark:hover:bg-white/10 cursor-pointer border-transparent hover:border-black/20 dark:hover:border-white/20'
                      }`}
                  >
                    <div className="flex items-center justify-between gap-2">
                      <span className="break-words">{getDisplayValue(leftValue, field, namingCategories)}</span>
                      {(isLeftSelected || bothSelected) && !leftEmpty && (
                        <Check size={16} className={`absolute right-2 top-1/2 -translate-y-1/2 flex-shrink-0 ${bothSelected ? 'text-green-600 dark:text-green-400' : 'text-primary dark:text-primary'}`} />
                      )}
                    </div>
                  </button>
                </td>
                <td className="py-2 px-2">
                  <button
                    onClick={() => !selection.locked && onSelect(field, 'right')}
                    disabled={selection.locked || rightEmpty}
                    className={`relative w-full text-left py-2 pl-3 pr-8 rounded-lg transition-all text-sm border-2 ${bothSelected
                      ? 'bg-green-100 border-green-400 dark:bg-green-900/30 dark:border-green-500/50 dark:text-green-100 cursor-default'
                      : isRightSelected
                        ? 'bg-primary/10 border-primary dark:bg-primary/20 dark:border-primary/50 dark:text-blue-100 cursor-default'
                        : rightEmpty
                          ? 'bg-black/5 text-black/30 dark:bg-white/5 dark:text-white/20 cursor-not-allowed border-transparent'
                          : 'bg-black/5 hover:bg-black/10 dark:bg-white/5 dark:hover:bg-white/10 cursor-pointer border-transparent hover:border-black/20 dark:hover:border-white/20'
                      }`}
                  >
                    <div className="flex items-center justify-between gap-2">
                      <span className="break-words">{getDisplayValue(rightValue, field, namingCategories)}</span>
                      {(isRightSelected || bothSelected) && !rightEmpty && (
                        <Check size={16} className={`absolute right-2 top-1/2 -translate-y-1/2 flex-shrink-0 ${bothSelected ? 'text-green-600 dark:text-green-400' : 'text-primary dark:text-primary'}`} />
                      )}
                    </div>
                  </button>
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
};

export default MergeComparisonTable;
export type { FieldKey, Side };
