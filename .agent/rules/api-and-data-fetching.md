---
trigger: glob
globs: Vulicy.UI/**/*.ts, Vulicy.UI/**/*.tsx
---

# API and Data Fetching Guidelines

## API Utility Pattern
- Use centralized `api` utility from `/src/utils/api.ts`
- All API calls go through `api.get()`, `api.post()`, `api.put()`
- Type API responses explicitly

## Loading States
- Use skeleton loaders or spinners
- Disable interactive elements during loading

## Caching Strategy
- Cache frequently accessed data in Zustand store
