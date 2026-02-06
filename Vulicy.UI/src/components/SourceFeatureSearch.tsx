import { useState, useRef, useCallback, useEffect } from 'react';
import { Search as SearchIcon, X } from 'lucide-react';
import './SourceFeatureSearch.css';
import { useClickOutside } from '../hooks/useClickOutside';
import { api } from '../utils/api';

interface SourceFeatureSearchProps<T> {
  placeholder: string;
  searchEndpoint: string;
  selectedItem: T | null;
  onSelect: (item: T) => void;
  onClear: () => void;
  renderResult: (item: T) => React.ReactNode;
  renderSelected: (item: T) => React.ReactNode;
  getItemKey: (item: T) => string | number;
  currentLat?: number;
  currentLng?: number;
}

function SourceFeatureSearch<T>({
  placeholder,
  searchEndpoint,
  selectedItem,
  onSelect,
  onClear,
  renderResult,
  renderSelected,
  getItemKey,
  currentLat,
  currentLng,
}: SourceFeatureSearchProps<T>) {
  const [query, setQuery] = useState('');
  const [results, setResults] = useState<T[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [isOpen, setIsOpen] = useState(false);
  const searchRef = useRef<HTMLDivElement>(null);

  // Close dropdown on click outside
  useClickOutside(searchRef, () => setIsOpen(false));

  // Memoize search function
  const performSearch = useCallback(async (searchQuery: string) => {
    setIsLoading(true);
    try {
      const params: Record<string, string | number | undefined> = {
        query: searchQuery,
      };
      if (currentLat !== undefined) params.lat = currentLat;
      if (currentLng !== undefined) params.lng = currentLng;

      const data = await api.get<T[]>(searchEndpoint, params);
      setResults(data);
      setIsOpen(true);
    } catch (error) {
      console.error('Search failed:', error);
      setResults([]);
    } finally {
      setIsLoading(false);
    }
  }, [searchEndpoint, currentLat, currentLng]);

  // Debounce search
  useEffect(() => {
    if (!query || query.length < 2) {
      setResults([]);
      return;
    }

    const timer = setTimeout(() => {
      performSearch(query);
    }, 300);

    return () => clearTimeout(timer);
  }, [query, performSearch]);

  const handleClear = () => {
    setQuery('');
    setResults([]);
    setIsOpen(false);
    onClear();
  };

  const handleSelect = (item: T) => {
    onSelect(item);
    setQuery('');
    setResults([]);
    setIsOpen(false);
  };

  // If there's a selected item, show it instead of the search field
  if (selectedItem) {
    return (
      <div className="source-search-container" ref={searchRef}>
        <div className="source-search-selected">
          <div className="source-search-selected-content">
            {renderSelected(selectedItem)}
          </div>
          <button
            onClick={handleClear}
            className="source-search-clear-btn"
            title="Прыбраць выбар"
          >
            <X size={14} className="text-black/40" />
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="source-search-container" ref={searchRef}>
      <div className="source-search-input-container">
        <SearchIcon className="text-black/40 w-4 h-4 shrink-0" />
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
          placeholder={placeholder}
          className="source-search-input"
        />
        {query && (
          <button
            onClick={() => {
              setQuery('');
              setResults([]);
              setIsOpen(false);
            }}
            className="source-search-input-clear"
          >
            <X size={14} className="text-black/40" />
          </button>
        )}
      </div>

      {isOpen && (results.length > 0 || isLoading) && (
        <div className="source-search-dropdown">
          {isLoading ? (
            <div className="source-search-loading">Шукаем...</div>
          ) : (
            <div className="source-search-results">
              {results.map((result) => (
                <div
                  key={getItemKey(result)}
                  onClick={(e) => {
                    e.stopPropagation();
                    handleSelect(result);
                  }}
                  className="source-search-result-item"
                >
                  {renderResult(result)}
                </div>
              ))}
            </div>
          )}
        </div>
      )}
    </div>
  );
}

export default SourceFeatureSearch;
