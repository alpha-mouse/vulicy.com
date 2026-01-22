---
trigger: glob
globs: Vulicy.UI/**/*.ts, Vulicy.UI/**/*.tsx
---

## Common Patterns

### ✅ DO
```typescript
// Debounce search
const { query, setQuery, results } = useDebounceSearch({
  searchFn: api.search,
  debounceMs: 300,
});

// Click outside
const ref = useRef<HTMLDivElement>(null);
useClickOutside(ref, () => setIsOpen(false));

// Copy to clipboard
const handleCopy = () => {
  navigator.clipboard.writeText(text);
  setIsCopied(true);
  setTimeout(() => setIsCopied(false), 2000);
};

```
