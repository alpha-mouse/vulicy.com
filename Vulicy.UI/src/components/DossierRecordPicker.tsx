import { useState, useEffect, useCallback, useRef } from 'react';
import { createPortal } from 'react-dom';
import { Search as SearchIcon, X, Loader2 } from 'lucide-react';
import type { DossierRecordSearchResult, NamingCategory } from '../types';
import { api } from '../utils/api';
import DossierRecordItem from './DossierRecordItem';

interface DossierRecordPickerProps {
  isOpen: boolean;
  onClose: () => void;
  onSelect: (record: DossierRecordSearchResult | null) => void;
  namingCategories: NamingCategory[];
  currentRecordId?: number | null;
  initialQuery?: string;
}

const PAGE_SIZE = 20;

/**
 * Modal for searching and selecting a dossier record to link to a feature.
 * Shows top 20 results without infinite scroll.
 */
const DossierRecordPicker = ({
  isOpen,
  onClose,
  onSelect,
  namingCategories,
  currentRecordId,
  initialQuery,
}: DossierRecordPickerProps) => {
  const [query, setQuery] = useState('');
  const [records, setRecords] = useState<DossierRecordSearchResult[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const inputRef = useRef<HTMLInputElement>(null);

  // Search for records
  const searchRecords = useCallback(async (searchQuery: string) => {
    setIsLoading(true);
    try {
      const data = await api.get<DossierRecordSearchResult[]>('/api/dossier-records/search', {
        query: searchQuery || undefined,
        skip: 0,
        take: PAGE_SIZE,
      });
      setRecords(data);
    } catch (error) {
      console.error('Failed to search dossier records:', error);
    } finally {
      setIsLoading(false);
    }
  }, []);

  // Initial load and search on query change
  useEffect(() => {
    if (!isOpen) return;

    const timer = setTimeout(() => {
      searchRecords(query);
    }, 300);

    return () => clearTimeout(timer);
  }, [query, isOpen, searchRecords]);

  // Handle focus and reset state when opening/closing
  useEffect(() => {
    if (isOpen) {
      setQuery(initialQuery || '');
      // Small delay to ensure modal is rendered for focus
      const timer = setTimeout(() => inputRef.current?.focus(), 50);
      return () => clearTimeout(timer);
    } else {
      setQuery('');
      setRecords([]);
    }
  }, [isOpen, initialQuery]);

  const handleClear = () => {
    setQuery('');
    inputRef.current?.focus();
  };

  const handleSelect = (record: DossierRecordSearchResult) => {
    onSelect(record);
    onClose();
  };

  const handleUnlink = () => {
    onSelect(null);
    onClose();
  };

  if (!isOpen) return null;

  return createPortal(
    <div className="fixed inset-0 z-[100] flex items-center justify-center bg-black/60 backdrop-blur-sm animate-in fade-in duration-200" onClick={onClose}>
      <div
        className="glass w-full max-w-lg max-h-[80vh] flex flex-col overflow-hidden animate-in zoom-in-95 duration-200"
        onClick={(e) => e.stopPropagation()}
      >
        {/* Header */}
        <div className="flex items-center justify-between p-5 border-b border-black/10">
          <h3 className="text-xl font-bold m-0">Выбраць імя</h3>
          <button
            onClick={onClose}
            className="p-1.5 hover:bg-black/5 rounded-full transition-colors bg-transparent border-none cursor-pointer outline-none"
          >
            <X size={22} className="text-black/40" />
          </button>
        </div>

        {/* Search bar */}
        <div className="p-5 border-b border-black/5">
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
                onClick={handleClear}
                className="p-1 hover:bg-black/5 rounded-full transition-colors border-none bg-transparent cursor-pointer outline-none"
              >
                <X size={14} className="text-black/40" />
              </button>
            )}
          </div>
        </div>

        {/* Unlink button - only show if there's a current record */}
        {currentRecordId && (
          <div className="px-5 pt-3">
            <button
              onClick={handleUnlink}
              className="text-sm font-medium text-red-500 hover:text-red-600 hover:underline bg-transparent border-none cursor-pointer p-1 rounded-lg hover:bg-red-50 dark:hover:bg-red-500/10 transition-colors"
            >
              Адвязаць імя
            </button>
          </div>
        )}

        {/* Records list */}
        <div className="flex-1 overflow-y-auto p-2">
          {records.map((record) => (
            <div
              key={record.id}
              onClick={() => handleSelect(record)}
              className={`p-4 rounded-xl cursor-pointer transition-all flex items-center gap-4 group mb-1 ${record.id === currentRecordId
                ? 'bg-primary/10 border border-primary/20'
                : 'hover:bg-black/5 border border-transparent'
                }`}
            >
              <div className="flex-1 min-w-0">
                <DossierRecordItem record={record} namingCategories={namingCategories} compact />
              </div>
              <div className="flex flex-col items-end gap-1 shrink-0">
                <span className="text-[10px] uppercase tracking-wider font-bold text-black/30 bg-black/5 dark:bg-white/10 px-2 py-0.5 rounded-md">
                  {record.numFeatures}
                </span>
              </div>
            </div>
          ))}

          {/* Loading indicator */}
          {isLoading && (
            <div className="p-8 flex items-center justify-center gap-2 text-sm text-black/40">
              <Loader2 size={20} className="animate-spin text-primary" />
              <span>Загрузка...</span>
            </div>
          )}

          {/* Empty state */}
          {!isLoading && records.length === 0 && (
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
    </div>,
    document.body
  );
};

export default DossierRecordPicker;
