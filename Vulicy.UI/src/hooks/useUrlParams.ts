import { useCallback } from 'react';

/**
 * Hook for managing URL search parameters without page reload.
 * Provides utilities to update URL params in place.
 *
 * @returns Object with utility functions for URL parameter management
 */
export function useUrlParams() {
  const updateParams = useCallback((params: Record<string, string | number | null | undefined>): void => {
    const url = new URL(window.location.href);
    Object.entries(params).forEach(([key, value]) => {
      if (value === null || value === undefined) {
        url.searchParams.delete(key);
      } else {
        url.searchParams.set(key, String(value));
      }
    });
    window.history.replaceState({}, '', url);
  }, []);

  const getParam = useCallback((key: string): string | null => {
    return new URLSearchParams(window.location.search).get(key);
  }, []);

  const getNumericParam = useCallback((key: string, defaultValue?: number): number | undefined => {
    const value = new URLSearchParams(window.location.search).get(key);
    if (!value) return defaultValue;
    const parsed = parseFloat(value);
    return isNaN(parsed) ? defaultValue : parsed;
  }, []);

  return {
    updateParams,
    getParam,
    getNumericParam,
  };
}
