# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in Vulicy.UI — the React SPA frontend. See the root CLAUDE.md for backend and full-stack setup.

## Commands

```bash
npm run dev        # dev server at http://localhost:5173; proxies /api → backend at :5165 (backend must be running)
npm run build      # tsc-free vite build INTO ../Vulicy.Web/wwwroot (empties that dir first)
npm run lint       # ESLint (flat config, typescript-eslint)
npx tsc            # typecheck (noEmit is set in tsconfig; build does NOT typecheck)
```

There is no test runner or test suite in this project.

## Stack & architecture

React 19 + TypeScript (strict) + Vite 7 + Tailwind CSS 4 (via `@tailwindcss/vite`; a few components also have their own `.css` files) + MapLibre GL + Zustand.

- **No router library.** [useNavigation.ts](src/hooks/useNavigation.ts) treats `window.location.pathname` as the source of truth (`pushState` + a synthetic `popstate` event); [App.tsx](src/App.tsx) switches on it to render the page. Routes: `/` (main map), `/sources` and `/dossier-deduplication` (admin-only), `/administrative`, `/explicitly-categorized`. A new page means a new `navigateTo*`/`is*Page` pair in the hook plus a branch in `App.tsx`.
- **State**: Zustand stores in `src/store/` (`mapStore` for the main map, `sourcesStore` for the sources view). App-wide config and auth are React contexts: [useConfig.tsx](src/hooks/useConfig.tsx) fetches `/api/config` (cached in localStorage, so UI renders before the fetch resolves) and [useAuth.tsx](src/hooks/useAuth.tsx) provides the current user / `isAdmin`.
- **API calls** go through the `api` fetch wrapper in [api.ts](src/utils/api.ts) with relative `/api/...` URLs — same-origin, so the auth cookie is sent automatically; the Vite dev proxy forwards to the backend. Error responses are surfaced as thrown `Error`s with the backend's `message`.
- **Map**: MapLibre initialization lives in `src/hooks/useMapInitialization.ts` / `useSourcesMapInitialization.ts`; the MapTiler key and other runtime settings come from the `/api/config` response, not from env vars or build-time constants.
