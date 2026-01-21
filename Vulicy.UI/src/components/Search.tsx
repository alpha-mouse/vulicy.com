import { useState, useEffect, useRef, useCallback } from 'react';
import { Search as SearchIcon, X, MapPin } from 'lucide-react';
import { FEATURE_TYPE_LABELS } from '../constants/mapConstants';
import type { SearchResult } from '../types/feature';
import { api } from '../utils/api';

interface SearchProps {
  onResultClick: (result: SearchResult) => void;
  currentLat: number;
  currentLng: number;
  embedded?: boolean;
}

const Search = ({ onResultClick, currentLat, currentLng, embedded = false }: SearchProps) => {
  const [query, setQuery] = useState('');
  const [results, setResults] = useState<SearchResult[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [isOpen, setIsOpen] = useState(false);
  const searchRef = useRef<HTMLDivElement>(null);

  // Memoize search function to use in dependency array
  const performSearch = useCallback(async (searchQuery: string, lat: number, lng: number) => {
    setIsLoading(true);
    try {
      const data = await api.get<SearchResult[]>('/api/features/search', {
        query: searchQuery,
        lat: lat || undefined,
        lng: lng || undefined,
      });
      setResults(data);
      setIsOpen(true);
    } catch (error) {
      console.error('Search failed:', error);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    if (!query || query.length < 2) {
      setResults([]);
      return;
    }

    const timer = setTimeout(() => {
      performSearch(query, currentLat, currentLng);
    }, 300);

    return () => clearTimeout(timer);
  }, [query, currentLat, currentLng, performSearch]);

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (searchRef.current && !searchRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const handleClear = () => {
    setQuery('');
    setResults([]);
    setIsOpen(false);
  };

  const containerClasses = embedded
    ? "relative flex-1 max-w-md flex flex-col gap-2"
    : "absolute top-4 left-4 z-30 w-80 flex flex-col gap-2";

  const inputContainerClasses = embedded
    ? "bg-white/10 h-9 flex items-center gap-3 rounded-lg border border-white/10 px-3 box-border"
    : "glass p-3 h-10 flex items-center gap-3 rounded-xl shadow-lg border border-white/20 box-border";

  const dropdownClasses = embedded
    ? "absolute top-full left-0 right-0 mt-1 bg-white dark:bg-slate-800 rounded-lg shadow-2xl border border-black/10 dark:border-white/10 overflow-hidden z-50"
    : "glass rounded-xl shadow-2xl border border-white/20 overflow-hidden animate-in fade-in slide-in-from-top-2 duration-200";

  return (
    <div className={containerClasses} ref={searchRef}>
      <div className={inputContainerClasses}>
        <SearchIcon className="text-black/40 w-5 h-5 shrink-0" />
        <input
          type="text"
          value={query}
          onChange={(e) => {
            setQuery(e.target.value);
            setIsOpen(true);
          }}
          onFocus={() => {
            if (query.length >= 2) {
              setIsOpen(true);
            }
          }}
          placeholder="Пошук вуліцы..."
          className="bg-transparent border-none outline-none w-full text-sm text-black placeholder:text-black/30"
        />
        {query && (
          <button
            onClick={handleClear}
            className="p-1 hover:bg-black/5 rounded-full transition-colors border-none bg-transparent cursor-pointer outline-none"
          >
            <X size={16} className="text-black/40" />
          </button>
        )}
      </div>

      {isOpen && (results.length > 0 || isLoading) && (
        <div className={dropdownClasses}>
          {isLoading ? (
            <div className="p-4 text-center text-sm text-black/40">Шукаем...</div>
          ) : (
            <div className="max-h-80 overflow-y-auto">
              {results.map((result) => (
                <div
                  key={result.id}
                  onClick={(e) => {
                    e.stopPropagation();
                    onResultClick(result);
                    setIsOpen(false);
                  }}
                  className="p-3 hover:bg-white/50 cursor-pointer transition-colors flex items-center gap-3 group"
                >
                  <MapPin size={16} className="text-black/20 group-hover:text-primary transition-colors shrink-0" />
                  <div className="flex flex-col">
                    <span className="text-sm font-medium text-black">
                      {FEATURE_TYPE_LABELS[result.type] && (
                        <span className="text-black/40 font-medium mr-1.5">{FEATURE_TYPE_LABELS[result.type]} </span>
                      )}
                      {result.nameBeTarask || result.nameBeNark || result.nameRu}
                    </span>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      )}
    </div>
  );
};

export default Search;
