import { useState, useEffect, useCallback, useRef } from 'react';
import { createPortal } from 'react-dom';
import { X, Search as SearchIcon, Loader2, ArrowLeft } from 'lucide-react';
import MergeComparisonTable, { type Selections, type FieldKey, type Side } from './MergeComparisonTable';
import DossierRecordItem from './DossierRecordItem';
import Button from './Button';
import { api } from '../utils/api';
import type { DossierRecordSearchResult, MergeDossierRecordRequest, NamingCategory } from '../types';

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
      selections[field] = { side: 'left', locked: true };
    } else if (!leftEmpty && rightEmpty) {
      selections[field] = { side: 'left', locked: false };
    } else if (leftEmpty && !rightEmpty) {
      selections[field] = { side: 'right', locked: false };
    } else {
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

const PAGE_SIZE = 20;

interface MergeDialogProps {
  record: DossierRecordSearchResult;
  namingCategories: NamingCategory[];
  onClose: () => void;
  onMerged: () => void;
}

const MergeDialog = ({
  record,
  namingCategories,
  onClose,
  onMerged,
}: MergeDialogProps) => {
  // Phase 1: search for second record
  const [rightRecord, setRightRecord] = useState<DossierRecordSearchResult | null>(null);
  const [query, setQuery] = useState('');
  const [searchResults, setSearchResults] = useState<DossierRecordSearchResult[]>([]);
  const [isSearching, setIsSearching] = useState(false);
  const inputRef = useRef<HTMLInputElement>(null);

  // Phase 2: comparison & merge
  const [selections, setSelections] = useState<Selections | null>(null);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Focus search input on mount
  useEffect(() => {
    const timer = setTimeout(() => inputRef.current?.focus(), 50);
    return () => clearTimeout(timer);
  }, []);

  // Search with debounce
  useEffect(() => {
    if (rightRecord) return; // don't search in phase 2

    if (!query.trim()) {
      setSearchResults([]);
      return;
    }

    const timer = setTimeout(async () => {
      setIsSearching(true);
      try {
        const data = await api.get<DossierRecordSearchResult[]>('/api/dossier-records/search', {
          query: query,
          skip: 0,
          take: PAGE_SIZE,
        });
        // Exclude the left record
        setSearchResults(data.filter(r => r.id !== record.id));
      } catch (err) {
        console.error('Failed to search dossier records:', err);
      } finally {
        setIsSearching(false);
      }
    }, 300);

    return () => clearTimeout(timer);
  }, [query, rightRecord, record.id]);

  const handleSelectRight = useCallback((selected: DossierRecordSearchResult) => {
    setRightRecord(selected);
    setSelections(computeInitialSelections(record, selected));
    setError(null);
  }, [record]);

  const handleBackToSearch = useCallback(() => {
    setRightRecord(null);
    setSelections(null);
    setError(null);
    setTimeout(() => inputRef.current?.focus(), 50);
  }, []);

  const handleFieldSelect = useCallback((field: FieldKey, side: Side) => {
    setSelections(prev => {
      if (!prev) return prev;
      return {
        ...prev,
        [field]: { ...prev[field], side },
      };
    });
  }, []);

  const getSelectedValue = useCallback((field: FieldKey): FieldValue => {
    if (!selections || !rightRecord) return record[field];
    return selections[field].side === 'left' ? record[field] : rightRecord[field];
  }, [selections, record, rightRecord]);

  const handleMerge = useCallback(async () => {
    if (!rightRecord || !selections) return;

    const request: MergeDossierRecordRequest = {
      otherId: rightRecord.id,
      nameBeTarask: String(getSelectedValue('nameBeTarask') ?? ''),
      nameBeNark: String(getSelectedValue('nameBeNark') ?? ''),
      nameRu: getSelectedValue('nameRu') as string | null,
      descriptionBe: getSelectedValue('descriptionBe') as string | null,
      descriptionRu: getSelectedValue('descriptionRu') as string | null,
      classification: Number(getSelectedValue('classification') ?? 0),
      namingCategoryId: getSelectedValue('namingCategoryId') as number | null,
    };

    setIsSaving(true);
    setError(null);
    try {
      await api.put(`/api/dossier-records/${record.id}/merge-other`, request);
      onMerged();
    } catch (err) {
      setError('Не ўдалося аб\'яднаць запісы');
      console.error('Failed to merge records:', err);
    } finally {
      setIsSaving(false);
    }
  }, [rightRecord, selections, getSelectedValue, record.id, onMerged]);

  return createPortal(
    <div
      className="fixed inset-0 z-[100] flex items-center justify-center bg-black/60 backdrop-blur-sm"
      style={{ animation: 'fadeIn 150ms ease-out' }}
      onClick={onClose}
    >
      <div
        className="glass w-full max-w-4xl max-h-[90vh] flex flex-col overflow-hidden animate-in zoom-in-95 duration-200"
        onClick={(e) => e.stopPropagation()}
      >
        {/* Header */}
        <div className="flex items-center justify-between p-5 border-b border-black/10">
          <div className="flex items-center gap-3">
            {rightRecord && (
              <button
                onClick={handleBackToSearch}
                className="p-1.5 hover:bg-black/5 rounded-lg transition-colors bg-transparent border-none cursor-pointer outline-none"
                title="Назад да пошуку"
              >
                <ArrowLeft size={18} className="text-black/60" />
              </button>
            )}
            <h3 className="text-xl font-bold m-0">Аб'яднаньне запісаў</h3>
          </div>
          <button
            onClick={onClose}
            className="p-1.5 hover:bg-black/5 rounded-full transition-colors bg-transparent border-none cursor-pointer outline-none"
          >
            <X size={22} className="text-black/40" />
          </button>
        </div>

        {error && (
          <div className="mx-5 mt-4 bg-red-100 border border-red-300 text-red-700 px-4 py-3 rounded-lg text-sm">
            {error}
          </div>
        )}

        {!rightRecord ? (
          /* Phase 1: Search for second record */
          <div className="flex-1 flex flex-col overflow-hidden">
            {/* Left record summary */}
            <div className="px-5 pt-4 pb-3 border-b border-black/5">
              <div className="text-xs uppercase tracking-wider font-bold text-black/30 mb-2">Запіс А (абраны)</div>
              <DossierRecordItem record={record} namingCategories={namingCategories} compact />
            </div>

            {/* Search bar */}
            <div className="p-5 border-b border-black/5">
              <div className="text-xs uppercase tracking-wider font-bold text-black/30 mb-2">Знайсьці Запіс Б</div>
              <div className="bg-black/5 dark:bg-white/5 h-10 flex items-center gap-3 rounded-xl border border-black/10 px-4 box-border focus-within:border-primary/50 focus-within:bg-white dark:focus-within:bg-white/10 transition-all">
                <SearchIcon className="text-black/30 w-4 h-4 shrink-0" />
                <input
                  ref={inputRef}
                  type="text"
                  value={query}
                  onChange={(e) => setQuery(e.target.value)}
                  placeholder="Пошук імені (напрыклад: Купала)..."
                  className="bg-transparent border-none outline-none w-full text-sm text-black placeholder:text-black/25"
                />
                {query && (
                  <button
                    onClick={() => { setQuery(''); inputRef.current?.focus(); }}
                    className="p-1 hover:bg-black/5 rounded-full transition-colors border-none bg-transparent cursor-pointer outline-none"
                  >
                    <X size={14} className="text-black/40" />
                  </button>
                )}
              </div>
            </div>

            {/* Search results */}
            <div className="flex-1 overflow-y-auto p-2">
              {searchResults.map((r) => (
                <div
                  key={r.id}
                  onClick={() => handleSelectRight(r)}
                  className="p-4 rounded-xl cursor-pointer transition-all flex items-center gap-4 group mb-1 hover:bg-black/5 border border-transparent"
                >
                  <div className="flex-1 min-w-0">
                    <DossierRecordItem record={r} namingCategories={namingCategories} compact />
                  </div>
                  <span className="text-[10px] uppercase tracking-wider font-bold text-black/30 bg-black/5 dark:bg-white/10 px-2 py-0.5 rounded-md shrink-0">
                    {r.numFeatures}
                  </span>
                </div>
              ))}

              {isSearching && (
                <div className="p-8 flex items-center justify-center gap-2 text-sm text-black/40">
                  <Loader2 size={20} className="animate-spin text-primary" />
                  <span>Загрузка...</span>
                </div>
              )}

              {!isSearching && searchResults.length === 0 && (
                <div className="p-12 text-center flex flex-col gap-2">
                  <div className="text-sm text-black/40 font-medium">
                    {query ? 'Нічога ня знойдзена' : 'Увядзіце пошукавы запыт'}
                  </div>
                  <div className="text-xs text-black/20">
                    Паспрабуйце іншую назву або скарочаны варыянт
                  </div>
                </div>
              )}
            </div>
          </div>
        ) : (
          /* Phase 2: Comparison table and merge */
          <div className="flex-1 overflow-auto p-6">
            <div className="text-center mb-4">
              <p className="text-sm text-black/50">
                Выберыце, якія значэньні захаваць пасьля аб'яднаньня
              </p>
            </div>

            {selections && (
              <MergeComparisonTable
                leftRecord={record}
                rightRecord={rightRecord}
                selections={selections}
                onSelect={handleFieldSelect}
                namingCategories={namingCategories}
              />
            )}

            <div className="flex justify-center gap-4 mt-6">
              <Button
                variant="secondary"
                onClick={onClose}
                disabled={isSaving}
              >
                Скасаваць
              </Button>
              <Button
                variant="primary"
                onClick={handleMerge}
                disabled={isSaving}
                loading={isSaving}
              >
                Аб'яднаць
              </Button>
            </div>
          </div>
        )}
      </div>
    </div>,
    document.body
  );
};

export default MergeDialog;
