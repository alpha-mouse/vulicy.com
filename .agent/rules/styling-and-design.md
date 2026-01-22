---
trigger: glob
globs: Vulicy.UI/**/*.css, Vulicy.UI/**/*.tsx
---

# Styling and Design Guidelines

## Tailwind CSS
- Use Tailwind CSS v4 utility classes
- Custom theme in `/src/index.css` using `@theme` directive

## Design System
- **Colors**:
  - Primary: `bg-primary`, `text-primary`, `border-primary`
  - Secondary: `bg-secondary`, `bg-secondary-hover`
- **Spacing**: Use Tailwind spacing scale (rem-based)
- **Borders**: `rounded-lg` for cards/buttons, `rounded-xl` for large panels
- **Shadows**: Use custom `--shadow` CSS variable or Tailwind shadows

## Responsive Design
- Mobile-first approach with Tailwind breakpoints
- Use flexbox: `flex`, `flex-col`, `items-center`, `justify-between`
- Panels: fixed positioning with proper z-index layering

## Dark Mode
- Automatic dark mode via `prefers-color-scheme`
- CSS variables adapt automatically (defined in `:root` and `@media (prefers-color-scheme: dark)`)
- No manual dark mode toggle needed
