import { useEffect, RefObject } from 'react';

/**
 * Hook that handles click events outside of the specified element.
 * Useful for closing dropdowns, modals, or menus when clicking outside.
 *
 * @param ref - React ref to the element to detect clicks outside of
 * @param handler - Callback function to execute when clicking outside
 */
export function useClickOutside<T extends HTMLElement>(
  ref: RefObject<T | null>,
  handler: (event: MouseEvent) => void
): void {
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (ref.current && !ref.current.contains(event.target as Node)) {
        handler(event);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, [ref, handler]);
}
