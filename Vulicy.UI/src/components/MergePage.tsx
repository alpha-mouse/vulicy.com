import { useState, useEffect, useCallback } from 'react';
import MergeTopBar from './MergeTopBar';
import MergeComparisonTable, { type Selections, type FieldKey, type Side } from './MergeComparisonTable';
import Button from './Button';
import { api } from '../utils/api';
import type { User, MergeSuggestion, MergeDossierRecordRequest, DossierRecordSearchResult, NamingCategory } from '../types';

interface MergePageProps {
  user: User | null;
  isLoading: boolean;
  onLogout: () => Promise<void>;
  onBack: () => void;
}

type FieldValue = string | number | null | undefined;

function isEmpty(value: FieldValue, field: FieldKey): boolean {
  if (field === 'classification') {
    return value === 0 || value === null || value === undefined;
  }
  return value === null || value === undefined || value === '';
}

function areEqual(left: FieldValue, right: FieldValue, field: FieldKey): boolean {
  const leftEmpty = isEmpty(left, field);
  const rightEmpty = isEmpty(right, field);

  if (leftEmpty && rightEmpty) return true;
  if (leftEmpty !== rightEmpty) return false;

  return left === right;
}

function computeInitialSelections(
  left: DossierRecordSearchResult,
  right: DossierRecordSearchResult
): Selections {
  const fields: FieldKey[] = ['nameBeTarask', 'nameBeNark', 'nameRu', 'descriptionBe', 'descriptionRu', 'classification', 'namingCategoryId'];
  const selections: Partial<Selections> = {};

  for (const field of fields) {
    const leftValue = left[field];
    const rightValue = right[field];
    const leftEmpty = isEmpty(leftValue, field);
    const rightEmpty = isEmpty(rightValue, field);
    const equal = areEqual(leftValue, rightValue, field);

    if (equal) {
      // Both equal (including both empty) - lock to left
      selections[field] = { side: 'left', locked: true };
    } else if (!leftEmpty && rightEmpty) {
      // Only left has value
      selections[field] = { side: 'left', locked: false };
    } else if (leftEmpty && !rightEmpty) {
      // Only right has value
      selections[field] = { side: 'right', locked: false };
    } else {
      // Both have different values
      // For text fields, prefer the longer value; for others, default to left
      const isTextField = ['nameBeTarask', 'nameBeNark', 'nameRu', 'descriptionBe', 'descriptionRu'].includes(field);
      if (isTextField) {
        const leftLen = String(leftValue ?? '').length;
        const rightLen = String(rightValue ?? '').length;
        selections[field] = { side: rightLen > leftLen ? 'right' : 'left', locked: false };
      } else {
        selections[field] = { side: 'left', locked: false };
      }
    }
  }

  return selections as Selections;
}

const MergePage = ({
  user,
  isLoading,
  onLogout,
  onBack,
}: MergePageProps) => {
  const [suggestion, setSuggestion] = useState<MergeSuggestion | null>(null);
  const [selections, setSelections] = useState<Selections | null>(null);
  const [namingCategories, setNamingCategories] = useState<NamingCategory[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isSaving, setIsSaving] = useState(false);
  const [completed, setCompleted] = useState(false);

  // Fetch naming categories on mount
  useEffect(() => {
    api.get<NamingCategory[]>('/api/map/naming-categories')
      .then(setNamingCategories)
      .catch(err => console.error('Failed to fetch naming categories:', err));
  }, []);

  const fetchNextSuggestion = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await api.get<MergeSuggestion | null>('/api/dossier-records/merge-suggestions/next');
      if (data) {
        setSuggestion(data);
        setSelections(computeInitialSelections(data.leftRecord, data.rightRecord));
      } else {
        setSuggestion(null);
        setSelections(null);
        setCompleted(true);
      }
    } catch (err) {
      setError('Не ўдалося загрузіць прапанову');
      console.error('Failed to fetch merge suggestion:', err);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchNextSuggestion();
  }, [fetchNextSuggestion]);

  const handleSelect = useCallback((field: FieldKey, side: Side) => {
    setSelections(prev => {
      if (!prev) return prev;
      return {
        ...prev,
        [field]: { ...prev[field], side },
      };
    });
  }, []);

  const getSelectedValue = useCallback((field: FieldKey, left: DossierRecordSearchResult, right: DossierRecordSearchResult): FieldValue => {
    if (!selections) return left[field];
    return selections[field].side === 'left' ? left[field] : right[field];
  }, [selections]);

  const handleMerge = useCallback(async () => {
    if (!suggestion || !selections) return;

    const { leftRecord, rightRecord } = suggestion;

    const request: MergeDossierRecordRequest = {
      otherId: rightRecord.id,
      nameBeTarask: String(getSelectedValue('nameBeTarask', leftRecord, rightRecord) ?? ''),
      nameBeNark: String(getSelectedValue('nameBeNark', leftRecord, rightRecord) ?? ''),
      nameRu: getSelectedValue('nameRu', leftRecord, rightRecord) as string | null,
      descriptionBe: getSelectedValue('descriptionBe', leftRecord, rightRecord) as string | null,
      descriptionRu: getSelectedValue('descriptionRu', leftRecord, rightRecord) as string | null,
      classification: Number(getSelectedValue('classification', leftRecord, rightRecord) ?? 0),
      namingCategoryId: getSelectedValue('namingCategoryId', leftRecord, rightRecord) as number | null,
    };

    setIsSaving(true);
    setError(null);
    try {
      await api.put(`/api/dossier-records/${leftRecord.id}/merge-other`, request);
      await fetchNextSuggestion();
    } catch (err) {
      setError('Не ўдалося аб\'яднаць запісы');
      console.error('Failed to merge records:', err);
    } finally {
      setIsSaving(false);
    }
  }, [suggestion, selections, getSelectedValue, fetchNextSuggestion]);

  const handleSkip = useCallback(async () => {
    if (!suggestion) return;

    setIsSaving(true);
    setError(null);
    try {
      await api.delete(`/api/dossier-records/merge-suggestions/${suggestion.id}`);
      await fetchNextSuggestion();
    } catch (err) {
      setError('Не ўдалося ігнараваць');
      console.error('Failed to skip suggestion:', err);
    } finally {
      setIsSaving(false);
    }
  }, [suggestion, fetchNextSuggestion]);

  const handlePostpone = useCallback(async () => {
    if (!suggestion) return;

    setIsSaving(true);
    setError(null);
    try {
      await api.post(`/api/dossier-records/merge-suggestions/${suggestion.id}/postpone`);
      await fetchNextSuggestion();
    } catch (err) {
      setError('Не ўдалося адкласьці');
      console.error('Failed to postpone suggestion:', err);
    } finally {
      setIsSaving(false);
    }
  }, [suggestion, fetchNextSuggestion]);

  return (
    <div className="flex flex-col h-full w-full">
      <MergeTopBar
        user={user}
        isLoading={isLoading}
        onLogout={onLogout}
        onBack={onBack}
      />

      <div className="flex-1 overflow-auto p-6 bg-slate-50 dark:bg-slate-900">
        {loading ? (
          <div className="flex items-center justify-center h-full">
            <div className="text-lg text-black/50">Загрузка...</div>
          </div>
        ) : completed || !suggestion ? (
          <div className="flex flex-col items-center justify-center h-full gap-4">
            <div className="text-xl font-medium text-black/70">Няма прапановаў для аб'яднаньня</div>
            <Button variant="secondary" onClick={onBack}>
              Вярнуцца да мапы
            </Button>
          </div>
        ) : (
          <div className="flex flex-col gap-6">
            {error && (
              <div className="bg-red-100 border border-red-300 text-red-700 px-4 py-3 rounded-lg text-sm">
                {error}
              </div>
            )}

            <div className="text-center">
              <h2 className="text-lg font-semibold text-black/80 mb-1">
                Аб'яднаньне запісаў
              </h2>
              <p className="text-sm text-black/50">
                Выберыце, якія значэньні захаваць пасьля аб'яднаньня
              </p>
            </div>

            {selections && (
              <MergeComparisonTable
                leftRecord={suggestion.leftRecord}
                rightRecord={suggestion.rightRecord}
                selections={selections}
                onSelect={handleSelect}
                namingCategories={namingCategories}
              />
            )}

            <div className="flex justify-center gap-4 mt-4">
              <Button
                variant="secondary"
                onClick={handleSkip}
                disabled={isSaving}
              >
                Ігнараваць
              </Button>
              <Button
                variant="secondary"
                onClick={handlePostpone}
                disabled={isSaving}
              >
                Адкласьці
              </Button>
              <Button
                variant="primary"
                onClick={handleMerge}
                disabled={isSaving}
              >
                Аб'яднаць
              </Button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default MergePage;
