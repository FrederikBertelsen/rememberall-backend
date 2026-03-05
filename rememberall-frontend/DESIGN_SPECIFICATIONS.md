# Design Specifications - RememberAll

## Overview
Mobile-first, minimalist dark theme web application. All designs prioritize simplicity, clarity, and mobile usability.

---

## Design Principles

1. **Mobile-First**: Design for mobile screens (375px - 540px). Ignore desktop layouts.
2. **Minimalist**: Remove all non-essential elements. Maximum clarity with minimum decoration.
3. **Dark Theme**: Dark background with white text for eye comfort and modern aesthetic.
4. **Accessible**: Ensure proper contrast and tap targets for mobile users.

---

## Color Palette

**⚠️ All colors are defined as CSS custom properties in [src/routes/layout.css](src/routes/layout.css)**

The design system uses CSS custom properties (variables) defined in the layout stylesheet. This is the recommended approach for managing colors alongside Tailwind CSS.

### Using the Color System

Colors are available as CSS variables throughout the application. No imports are needed.

**Color Variables:**

| Purpose | CSS Variable | RGB Value | Notes |
|---------|---|---|---|
| Background | `--color-bg-primary` | `rgb(9, 9, 11)` | Primary background for all pages |
| Card Background | `--color-bg-secondary` | `rgb(18, 18, 21)` | Card and input backgrounds |
| Text Primary | `--color-fg-primary` | `rgb(255, 255, 255)` | Main text, headings |
| Text Secondary | `--color-fg-secondary` | `rgb(163, 163, 163)` | Secondary text, hints, timestamps |
| Text Muted | `--color-fg-muted` | `rgb(107, 114, 128)` | Placeholder, disabled text |
| Accent | `--color-accent` | `rgb(20, 184, 166)` | Primary action buttons |
| Accent Hover | `--color-accent-hover` | `rgb(45, 212, 191)` | Hover state for buttons |
| Danger | `--color-danger` | `rgb(239, 68, 68)` | Error text and borders |
| Danger Background | `--color-error-light` | `rgba(239, 68, 68, 0.1)` | Error backgrounds |
| Success | `--color-success` | `rgb(34, 197, 94)` | Success messages |
| Success Background | `--color-success-light` | `rgba(34, 197, 94, 0.1)` | Success backgrounds |
| Border | `--color-border` | `rgb(39, 39, 42)` | Dividers, borders |
| Overlay | `--color-overlay-dark` | `rgba(0, 0, 0, 0.5)` | Modal overlays |

### Example Usage

In CSS:
```css
.my-element {
  background-color: var(--color-bg-primary);
  color: var(--color-fg-primary);
}
```

In Tailwind utilities (if using `@apply`):
```css
.my-class {
  @apply rounded px-4 py-2;
  background-color: var(--color-accent);
  color: var(--color-fg-primary);
}
```

To change colors globally, edit the `:root` selector in [src/routes/layout.css](src/routes/layout.css).

---

## Typography

| Element | Font Size | Font Weight | Line Height | Tailwind Classes | Notes |
|---------|-----------|-------------|-------------|------------------|-------|
| Page Title | 28px (1.75rem) | 700 Bold | 1.2 | `text-3xl font-bold` | Main page heading |
| Section Heading | 20px (1.25rem) | 600 SemiBold | 1.3 | `text-xl font-semibold` | Secondary headings |
| Body Text | 16px (1rem) | 400 Regular | 1.5 | `text-base leading-relaxed` | Default paragraph text |
| Small Text | 14px (0.875rem) | 400 Regular | 1.4 | `text-sm` | Labels, helper text, timestamps |
| Extra Small | 12px (0.75rem) | 400 Regular | 1.3 | `text-xs` | Captions, fine print |

**Font Family**: System font stack (San Francisco, Segoe UI, or fallback sans-serif)

---

## Spacing System

| Scale | Size | CSS Value | Tailwind |
|-------|------|-----------|----------|
| xs | 4px | 0.25rem | `p-1`, `m-1` |
| sm | 8px | 0.5rem | `p-2`, `m-2` |
| md | 16px | 1rem | `p-4`, `m-4` |
| lg | 24px | 1.5rem | `p-6`, `m-6` |
| xl | 32px | 2rem | `p-8`, `m-8` |
| 2xl | 48px | 3rem | `p-12`, `m-12` |

**Padding**: Use `md` (16px) for standard container padding
**Margin**: Use multiples of `md` (16px, 32px, 48px)
**Gap**: Use `md` (16px) for spacing between elements

---

## Layout Rules

### Mobile Viewport
- **Width**: Full viewport width
- **Container Padding**: 16px on all sides (`p-4` in Tailwind)
- **Max Width**: Ignore—full width is intentional
- **Safe Area**: Use safe area insets for notches/bottom bars on mobile

### Spacing Between Sections
- **Vertical Gap**: 32px (`gap-8` / `space-y-8`)
- **Horizontal Gap**: 16px (`gap-4` / `space-x-4`)

### Cards/Containers
- **Padding**: 16px (`p-4`)
- **Border**: 1px solid `var(--color-border)` OR no border if dividing with spacing only
- **Border Radius**: 8px (`rounded-lg`) OR no radius for minimal look
- **Background**: Same as page background (`var(--color-bg-primary)`) for seamless look OR `var(--color-bg-secondary)` for subtle contrast

### Forms & Inputs
- **Input Height**: 40px minimum (`min-h-[40px]`)
- **Padding**: 12px horizontal, 8px vertical (`px-3 py-2`)
- **Border**: 1px solid `var(--color-border)` with focus on `var(--color-accent)`
- **Border Radius**: 6px (`rounded`)
- **Background**: `var(--color-bg-secondary)`
- **Text**: `var(--color-fg-primary)`
- **Placeholder**: `var(--color-fg-muted)`
- **Focus State**: Border color `var(--color-accent)`, no box shadow (`focus:outline-none`)
- **Disabled State**: 50% opacity (`disabled:opacity-50`)

---

## Component Patterns

### Button
- **Height**: 40px minimum (`min-h-[40px]`)
- **Padding**: 12px horizontal, 8px vertical (`px-4 py-2`)
- **Border Radius**: 6px (`rounded`)
- **CSS Classes**: Use `.btn` base class with `.btn-primary` or `.btn-secondary` modifiers
- **Primary**: `.btn .btn-primary` - `var(--color-accent)` background, `var(--color-fg-primary)` text
- **Primary Hover**: `.btn-primary:hover:not(:disabled)` - `var(--color-accent-hover)` background
- **Secondary**: `.btn .btn-secondary` - Transparent background, `var(--color-border)` border, `var(--color-fg-primary)` text
- **Disabled**: 50% opacity (`disabled:opacity-50 disabled:cursor-not-allowed`)
- **Font Weight**: 500 (medium) (`font-medium`)

### Form Elements
- **Container**: Use `.form` class for form wrapper with `space-y-6` spacing
- **Form Group**: Use `.form-group` for input+label pairs with `space-y-2` spacing
- **Labels**: Use `.form-label` for consistent styling
- **Inputs**: Use `.form-input` for all text inputs with proper height, padding, borders, and focus states
- **Hints**: Use `.form-hint` for helper text below inputs

### Alerts & Messages
- **Error Alert**: Use `.alert` class for error messages
- **Success Alert**: Use `.alert .alert-success` for success messages
- **Structure**: Use `<div class="alert"><p>Message</p></div>`

### Page Containers
- **Centered Layout** (forms, auth): Use `.page-container` for full-height centered content with `.card` wrapper
- **Scrollable Layout** (lists, content): Use `.page-container-scroll` for full-height scrollable pages
- **Card Sections**: Use `.card-section` for grouped content within scrollable pages

### Page Heading
- **Use `.page-heading`** for centered heading sections
- **Structure**: Wrap `<h1>` or `<h2>` with optional `<p>` subtitle in `.page-heading`

### Navigation Bar
- **Position**: Fixed at top or bottom (`fixed top-0` or `fixed bottom-0`)
- **Height**: 56px (`h-14`)
- **Background**: `var(--color-bg-primary)` with top border using `var(--color-border)`
- **Icon Size**: 24px (`w-6 h-6`)
- **Text Size**: 12px (`text-xs`)
- **Spacing**: Centered items with even distribution (`flex justify-center items-center gap-4`)

### List Item / Row
- **Height**: 56px minimum (`min-h-14`)
- **Padding**: 12px horizontal, 8px vertical (`px-3 py-2`)
- **Border**: Bottom border using `var(--color-border)`
- **Gap Between Items**: 1px border (visual separation)

### Modal / Overlay
- **Background**: `var(--color-overlay-dark)`
- **Content Background**: `var(--color-bg-secondary)`
- **Padding**: 16px (`p-4`)
- **Border Radius**: 8px (`rounded-lg`)
- **Position**: Center on screen, full width with padding (`fixed inset-0 flex items-center justify-center px-4`)
- **Use Sparingly**: Prefer inline forms or page-level changes over modals

---

## States

### Hover (Desktop - Ignore for Mobile)
Not applicable for mobile-only design. Touch devices do not support hover states. Do not implement hover-only interactions.

### Active / Pressed
- **Visual Change**: 10% opacity reduction OR slight background color change
- **Feedback**: Instant (no delay)

### Disabled
- **Opacity**: 50%
- **Cursor**: Not allowed (if applicable)
- **No Interaction**: Prevent clicks

### Loading
- **Visual**: Text change (e.g., "Loading...") or subtle spinner
- **Disabled State**: Button becomes disabled during loading
- **Duration**: Minimal delay expected (<2 seconds typical)

### Focus (Accessibility)
- **Keyboard Focus**: `var(--color-accent)` border or outline
- **Mobile**: Not typically visible (touch-based interaction)

---

## Responsive Behavior

### Single Column Layout
- All pages use **single column** design
- No multi-column grids
- Content stacks vertically

### Content Width
- Full viewport width (minus safe padding)
- No horizontal scrolling
- Text wraps naturally

### Images & Media
- Scale to full container width
- Maintain aspect ratio
- Max width: container width

---

## Styling with CSS Variables

**All colors must use CSS custom properties defined in [src/routes/layout.css](src/routes/layout.css).**

### In CSS or Component Styles
```css
.component {
  background-color: var(--color-bg-primary);
  color: var(--color-fg-primary);
  border-color: var(--color-border);
}
```

### In Svelte Components (Inline Styles)
```svelte
<div style="background-color: var(--color-bg-secondary); color: var(--color-fg-primary);">
  Content
</div>
```

### Tailwind Utility Classes
- `rounded-lg` (8px) for containers/cards
- `rounded` (6px) for inputs/buttons
- `p-4` (16px) for padding
- `space-y-8` (32px gap) for section spacing
- `space-y-3` (12px gap) for button stacks
- `min-h-[40px]` for minimum button/input heights
- `text-xs`, `text-sm`, `text-base`, `text-xl`, `text-3xl` for typography
- `font-bold`, `font-semibold`, `font-medium` for font weights
- `w-full`, `min-h-screen`, `flex`, `items-center`, `justify-center`, etc. for layout
- `disabled:opacity-50`, `disabled:cursor-not-allowed` for disabled states

### Avoid Using
- `shadow-*` (no box shadows)
- `hover:` (no hover effects on mobile)
- `transition-*` (no animations)
- `bg-gradient-*` (no gradients)
- `opacity-*` except for `disabled:opacity-50`
- **Arbitrary color classes** like `bg-[rgb(...)]`, `border-gray-800`, `text-red-600`, `bg-teal-600`
- Any arbitrary spacing beyond the defined system

### Color CSS Variables Reference

Instead of using arbitrary Tailwind colors, always use these CSS variables:

- **Primary Background**: `var(--color-bg-primary)` (page background)
- **Secondary Background**: `var(--color-bg-secondary)` (cards, inputs)
- **Primary Text**: `var(--color-fg-primary)` (headings, main content)
- **Secondary Text**: `var(--color-fg-secondary)` (hints, metadata)
- **Muted Text**: `var(--color-fg-muted)` (placeholders, disabled text)
- **Border**: `var(--color-border)` (dividers, input borders)
- **Accent**: `var(--color-accent)` (primary buttons, active states)
- **Accent Hover**: `var(--color-accent-hover)` (button hover states)
- **Danger**: `var(--color-danger)` (error text, error borders)
- **Error Background**: `var(--color-error-light)` (error message backgrounds)
- **Success**: `var(--color-success)` (success messages)
- **Success Background**: `var(--color-success-light)` (success message backgrounds)
- **Overlay**: `var(--color-overlay-dark)` (modal backgrounds)

To change any color globally, edit the `:root` selector in [src/routes/layout.css](src/routes/layout.css).

---

## Accessibility Requirements

### Text Contrast
- White text on black background: **WCAG AAA compliant** (21:1 ratio)
- Secondary gray text on black: **WCAG AA compliant** (7:1 ratio)

### Touch Targets
- Minimum 44x44px for interactive elements
- Button height: 40px minimum
- Spacing between targets: 8px minimum

### Semantic HTML
- Use proper HTML elements (`<button>`, `<input>`, `<nav>`, etc.)
- Include `<label>` elements for form inputs
- Use `aria-label` for icon-only buttons

### Focus Management
- Logical tab order
- Visible focus indicators (though less critical on mobile)

---

## Page Structure Templates

### Centered Layout (Forms, Auth, etc.)

```svelte
<script lang="ts">
  // Reactive state using Svelte 5 runes
  let isLoading = $state(false);
  let error = $state<string | null>(null);
</script>

<div class="page-container">
  <div class="card">
    <div class="page-heading">
      <h2>Page Title</h2>
      <p>Subtitle or description</p>
    </div>

    <form class="form">
      {#if error}
        <div class="alert">
          <p>{error}</p>
        </div>
      {/if}

      <div class="form-group">
        <label for="input" class="form-label">Label</label>
        <input id="input" type="text" class="form-input" placeholder="Placeholder" />
      </div>

      <button type="submit" disabled={isLoading} class="btn btn-primary">
        {isLoading ? 'Loading...' : 'Submit'}
      </button>

      <p class="page-footer">
        Link text? <a href="/">Go here</a>
      </p>
    </form>
  </div>
</div>
```

### Scrollable Content Layout (Lists, Pages, etc.)

```svelte
<script lang="ts">
  let isLoading = $state(false);
  let items = $state<string[]>([]);
</script>

<div class="page-container-scroll">
  <h1 class="text-3xl font-bold mb-8">Page Title</h1>

  <div class="space-y-8">
    <!-- Content sections -->
    <div class="card-section">
      <h2 class="text-xl font-semibold mb-4">Section Title</h2>
      <p style="color: var(--color-fg-secondary);">Body text here</p>
    </div>

    <!-- List or repeated items -->
    {#each items as item (item.id)}
      <div class="pb-4" style="border-bottom: 1px solid var(--color-border);">
        <p>{item}</p>
      </div>
    {/each}
  </div>

  <!-- Action buttons -->
  <div class="mt-8 space-y-3">
    <button disabled={isLoading} class="btn btn-primary">
      {isLoading ? 'Loading...' : 'Primary Action'}
    </button>
    <button class="btn btn-secondary">Secondary Action</button>
  </div>
</div>
```

---

## What NOT to Include (Minimalism Violations)

- ❌ Box shadows or drop shadows
- ❌ Hover effects (use active/pressed states only)
- ❌ Animations or transitions (except loading spinners)
- ❌ Decorative icons or illustrations
- ❌ Gradients or complex background patterns
- ❌ Multiple text colors (use white, gray-400, red, green, blue only)
- ❌ Sidebars or multi-column layouts
- ❌ Unnecessary padding or whitespace
- ❌ Overlay effects or glass morphism
- ❌ Complex form validation messages
- ❌ Tooltips or popovers
- ❌ Breadcrumbs or navigation hierarchy
- ❌ Desktop-specific features

## Implementation Checklist

- [ ] Update global CSS: Set background to `rgb(9, 9, 11)`, text to white
- [ ] Configure Tailwind for dark mode
- [ ] Remove all non-mobile viewport styles and media queries
- [ ] Audit all components for minimalism (remove unnecessary elements)
- [ ] Check button/input heights are minimum 40px
- [ ] Verify color contrast ratios
- [ ] Test on actual mobile devices (not just browser DevTools)
- [ ] Remove desktop-specific features (hover states, sidebars, etc.)
- [ ] Remove all box shadows and decorative effects
- [ ] Use only specified colors—no arbitrary color additions
- [ ] Ensure every component serves a functional purpose

---

## Revision History

| Date | Author | Changes |
|------|--------|---------|
| Feb 4, 2026 | System | Updated for Svelte 5 standards and strict minimalism |
| Feb 4, 2026 | System | Initial design specification |

