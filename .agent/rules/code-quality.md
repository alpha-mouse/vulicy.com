---
trigger: glob
globs: Vulicy.UI/**/*.ts, Vulicy.UI/**/*.tsx
---

# Code Quality Guidelines

## Documentation
- Document non-obvious logic inline with regular comments
- No need for obvious comments like `// Set loading state`

## Code Organization
- Keep functions small and focused (max ~50 lines)
- Extract complex logic into utility functions or hooks
- Group related code together (all state declarations, then effects, then handlers)
