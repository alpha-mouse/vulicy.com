---
trigger: glob
globs: Vulicy.UI/**/*.ts, Vulicy.UI/**/*.tsx
---

# File Structure and Organization

## Project Structure
```
Vulicy.UI/
├── src/
│   ├── components/     # React components
│   ├── hooks/          # Custom React hooks
│   ├── store/          # Zustand stores
│   ├── types/          # TypeScript interfaces/types
│   ├── utils/          # Utility functions
│   ├── constants/      # Constants and config values
│   ├── App.tsx         # Root component
│   ├── main.tsx        # Entry point
│   └── index.css       # Global styles and theme
├── public/             # Static assets
└── package.json
```