import { useState, useEffect, useCallback } from 'react';

interface UseDebounceSearchOptions<T> {
  searchFn: (query: string, ...args: any[]) => Promise<T[]>;
  debounceMs?: number;
  minQueryLength?: number;
}

interface UseDebounceSearchResult<T> {
  query: string;
  setQuery: (query: string) => void;
  results: T[];
  isLoading: boolean;
  clearSearch: () => void;
}

/**
 * Reusable hook for debounced search functionality.
 * Handles query state, loading state, and debounced API calls.
 *
 * @param searchFn - Async function that performs the search
 * @param debounceMs - Debounce delay in milliseconds (default: 300)
 * @param minQueryLength - Minimum query length to trigger search (default: 2)
 * @returns Search state and control functions
 */
export function useDebounceSearch<T>({
  searchFn,
  debounceMs = 300,
  minQueryLength = 2,
}: UseDebounceSearchOptions<T>): UseDebounceSearchResult<T> {
  const [query, setQuery] = useState('');
  const [results, setResults] = useState<T[]>([]);
  const [isLoading, setIsLoading] = useState(false);

  const performSearch = useCallback(async (searchQuery: string, ...args: any[]) => {
    setIsLoading(true);
    try {
      const data = await searchFn(searchQuery, ...args);
      setResults(data);
    } catch (error) {
      console.error('Search failed:', error);
      setResults([]);
    } finally {
      setIsLoading(false);
    }
  }, [searchFn]);

  useEffect(() => {
    if (!query || query.length < minQueryLength) {
      setResults([]);
      return;
    }

    const timer = setTimeout(() => {
      performSearch(query);
    }, debounceMs);

    return () => clearTimeout(timer);
  }, [query, debounceMs, minQueryLength, performSearch]);

  const clearSearch = useCallback(() => {
    setQuery('');
    setResults([]);
  }, []);

  return {
    query,
    setQuery,
    results,
    isLoading,
    clearSearch,
  };
}
