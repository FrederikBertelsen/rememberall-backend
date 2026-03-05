# Danish Translation Setup Guide

## Overview

This document outlines the complete setup and implementation of Danish (and English) language translations throughout the rememberall-frontend application. The translation system is built using a lightweight, custom i18n store that integrates seamlessly with Svelte 5's reactive runes.

## Architecture

### Translation System

The i18n system consists of:

1. **Translation Files** (`messages/` folder):
   - `en.json` - English translations
   - `da.json` - Danish translations

2. **i18n Store** (`src/lib/i18n/index.ts`):
   - Svelte writable store for reactive language switching
   - Functions for synchronous and reactive translation retrieval
   - localStorage persistence for user language preference
   - Automatic HTML lang attribute updates

3. **Language Switcher Component** (`src/lib/components/common/LanguageSwitcher.svelte`):
   - User-facing toggle between English and Danish
   - Displays current language in the app

## Files Modified

### Core i18n Files

#### `src/lib/i18n/index.ts` (New)
**Purpose**: Central hub for all translation logic

**Key Exports**:
- `languageTag` - Writable store containing current language ('en' or 'da')
- `setLanguage(lang: Language)` - Function to change language
- `tSync(lang: Language, key: string, params?: Record<string, string | number>)` - Synchronous translation
- `Language` - Type definition for 'en' | 'da'

**Features**:
- Auto-persists language choice to localStorage
- Auto-updates HTML `lang` attribute on language change
- Supports nested translation keys with dot notation (e.g., 'form.emailRequired')
- Parameter interpolation support (e.g., "Hello {name}")

### Translation Files

#### `messages/en.json` (New)
Contains all English strings organized by feature:
- `common` - Common UI elements (Sign in, Password, Email, etc.)
- `form` - Form validation messages
- `placeholders` - Input placeholders
- `buttons` - Button labels
- `dashboard` - Dashboard-specific text
- `invites` - Invite-related text
- `lists` - List-specific text
- `items` - Item-specific text
- `access` - Access control text
- `hints` - Helper text
- `auth` - Authentication-related text

#### `messages/da.json` (New)
Complete Danish translations of all strings in `en.json`

### Component Updates

#### Auth Pages

**`src/routes/login/+page.svelte`** - Updated with:
- i18n imports: `languageTag`, `tSync`
- LanguageSwitcher component display
- Form validation messages using translations
- Button labels using translations
- Input placeholders using translations
- Error messages using translations

**`src/routes/register/+page.svelte`** - Updated with:
- Same i18n setup as login page
- All form labels, placeholders, hints translated
- Dynamic button text based on loading state

#### Navigation

**`src/lib/components/common/Navbar.svelte`** - Updated with:
- i18n imports
- Logout button text translated
- Menu aria-label translated

**`src/lib/components/common/LanguageSwitcher.svelte`** (New) - Features:
- Language toggle buttons (English/Dansk)
- Visual indicator of current language
- Tailwind-styled with accent colors

#### List Management

**`src/lib/components/lists/ListForm.svelte`** - Updated with:
- "Create New List" button label
- Modal heading translated
- Input placeholder for list name
- Create/Cancel button labels
- Character count label

**`src/lib/components/lists/ShareForm.svelte`** - Updated with:
- "Share List" button label
- Modal heading translated
- Email validation messages
- Error handling messages
- Send Invite/Cancel buttons

#### Item Management

**`src/lib/components/items/ItemForm.svelte`** - Updated with:
- Item validation messages
- Input placeholder for item text
- Add/Cancel button labels
- Character count display
- Reuse completed item message

#### Dashboard

**`src/routes/+page.svelte`** - Updated with:
- "Invitations" section heading
- Accept/Decline buttons
- Revoke invitation button

## Usage Examples

### In Components (Synchronous)

```svelte
<script lang="ts">
  import { languageTag, tSync } from '$lib/i18n/index';
</script>

<button>
  {tSync($languageTag, 'buttons.add')}
</button>

<input placeholder={tSync($languageTag, 'placeholders.email')} />
```

### Getting Current Language

```svelte
<script lang="ts">
  import { languageTag } from '$lib/i18n/index';
</script>

<p>Current language: {$languageTag}</p>
```

### Changing Language

```svelte
<script lang="ts">
  import { setLanguage } from '$lib/i18n/index';
</script>

<button onclick={() => setLanguage('da')}>Dansk</button>
<button onclick={() => setLanguage('en')}>English</button>
```

### With Parameters

```svelte
{tSync($languageTag, 'messages.welcome', { name: 'Frederik' })}
```

## Adding New Translations

To add new translations:

1. **Add to `messages/en.json`**:
   ```json
   {
     "newSection": {
       "newKey": "New English text"
     }
   }
   ```

2. **Add to `messages/da.json`**:
   ```json
   {
     "newSection": {
       "newKey": "Ny dansk tekst"
     }
   }
   ```

3. **Use in component**:
   ```svelte
   {tSync($languageTag, 'newSection.newKey')}
   ```

## Current Translation Coverage

### English & Danish Available For:

- Authentication (login, register, password reset)
- Form validation messages
- Navigation & menus
- List creation & management
- List sharing & invitations
- Item management (create, edit, delete)
- Access control
- Error messages
- UI button labels
- Input placeholders
- Form hints

## Storage & Persistence

- Selected language is saved to `localStorage` with key `'lang'`
- Default language is English ('en')
- Language preference persists across sessions
- HTML document `lang` attribute updates automatically

## Browser Compatibility

Works with all modern browsers that support:
- Svelte 5 (with runes)
- localStorage API
- ES6+ JavaScript

## Performance Notes

- Translation lookups are O(1) - direct object lookups
- No runtime compilation needed
- Minimal bundle size impact (~15KB for messages + ~2KB for utilities)
- Synchronous access ensures no UI jank
- Fully tree-shakeable

## Future Enhancements

Possible improvements for the future:

1. **Plural forms**: Support for plural translations
2. **Date/Time localization**: Format dates based on language
3. **RTL support**: For Arabic, Hebrew, etc.
4. **Translation management UI**: Admin panel to edit translations
5. **Extract script**: Automated script to find untranslated strings
6. **Type-safe keys**: Use TypeScript to validate translation keys at compile time
7. **Lazy loading**: Load translation files on demand
8. **Translation file validation**: Ensure all keys exist in all languages

## Testing the Setup

### Manual Testing:

1. Navigate to login page (`/login` or `/register`)
2. Click language switcher in the top right
3. Switch between English and Dansk
4. Verify all text changes
5. Refresh page - language preference should persist
6. Check that form validation messages are translated
7. Create/update/delete items and lists in both languages

### Building:

```bash
pnpm build
```

Should complete without errors and include translation JSON files in the bundle.

## Related Files

- [messages/en.json](../../messages/en.json) - All English translations
- [messages/da.json](../../messages/da.json) - All Danish translations
- [src/lib/i18n/index.ts](../src/lib/i18n/index.ts) - Core i18n system
- [src/lib/components/common/LanguageSwitcher.svelte](../src/lib/components/common/LanguageSwitcher.svelte) - UI toggle component
