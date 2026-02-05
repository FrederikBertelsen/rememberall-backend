# RememberAll Frontend

A modern, collaborative todo list application built with **SvelteKit** and **Svelte 5**. Share lists with others, manage tasks together, and stay organized with a clean, intuitive interface.

## Features

- **User Authentication** - Secure login/registration with HttpOnly cookie-based sessions
- **Todo List Management** - Create, edit, and organize multiple lists
- **Collaborative Sharing** - Share lists with other users via an invite system
- **Todo Items** - Add, complete, and track items within lists
- **Multi-language Support** - English and Danish localization built-in
- **Responsive Design** - Works seamlessly on desktop and mobile devices
- **Secure Architecture** - Backend API hidden behind frontend proxy for enhanced security

## Technology Stack

- **Framework**: SvelteKit 2.x
- **Language**: TypeScript
- **State Management**: Svelte 5 runes (`$state`, `$derived`, `$effect`)
- **Styling**: Tailwind CSS
- **HTTP Client**: Native Fetch API
- **Internationalization**: Custom i18n system
- **Icons & UI**: Custom components

## Architecture

### Proxy Pattern

This frontend uses a **secure proxy pattern** for all backend communication:

```
Browser → Frontend (/api) → Backend (http://localhost:5000)
```

**Benefits:**
- Backend URL never exposed to the browser
- HttpOnly cookies work securely
- Centralized API communication point
- Easy to change backend URL without updating client code

### Project Structure

```
src/
├── lib/
│   ├── api/
│   │   ├── client.ts              # HTTP client wrapper
│   │   ├── types.ts               # All TypeScript DTOs
│   │   └── services/              # API service layer
│   │       ├── auth.ts
│   │       ├── lists.ts
│   │       ├── items.ts
│   │       ├── invites.ts
│   │       └── access.ts
│   ├── stores/                    # Svelte 5 rune-based stores
│   │   ├── auth.svelte.ts
│   │   ├── lists.svelte.ts
│   │   ├── items.svelte.ts
│   │   ├── invites.svelte.ts
│   │   └── access.svelte.ts
│   ├── components/                # Reusable UI components
│   │   ├── common/
│   │   ├── auth/
│   │   ├── lists/
│   │   └── items/
│   ├── i18n/                      # Internationalization
│   ├── utils/                     # Shared utilities
│   └── styles/
├── routes/                        # SvelteKit page routes
│   ├── +layout.svelte             # Root layout (auth guard, navbar)
│   ├── +page.svelte               # Dashboard
│   ├── login/
│   ├── register/
│   ├── lists/[id]/                # Individual list view
│   ├── invites/                   # Invite management
│   └── api/[...path]/+server.ts   # Backend proxy
└── app.html
```

### State Management

This project uses **Svelte 5 runes** for reactive state management without external dependencies:

```typescript
// Example store pattern (lib/stores/auth.svelte.ts)
class AuthStore {
  currentUser = $state<UserDto | null>(null);
  isLoading = $state(false);
  isAuthenticated = $derived(!!this.currentUser);

  async login(credentials: LoginDto) {
    // State updates trigger reactivity automatically
  }
}

export const authStore = new AuthStore();
```

Components access stores directly:

```svelte
<script lang="ts">
  import { authStore } from '$lib/stores/auth.svelte';
  const { currentUser, isAuthenticated } = authStore;
</script>

{#if isAuthenticated}
  <p>Hello {currentUser?.name}</p>
{/if}
```

## Getting Started

### Prerequisites

- **Node.js** 18+ and npm/pnpm
- **Backend API** running on `http://localhost:5000` (see [backend repository](../rememberall-backend) for setup)

### Installation

```bash
# Install dependencies
npm install

# Start development server
npm run dev
```

The frontend will be available at `http://localhost:5173`.

### Environment Setup

The frontend communicates with the backend through a proxy at `/api`. Ensure your backend API is running on `http://localhost:5000`.

**Backend proxy configuration:** [src/routes/api/[...path]/+server.ts](src/routes/api/[...path]/+server.ts)

### Build for Production

```bash
npm run build
npm run preview
```

## Development

### Available Scripts

```bash
npm run dev       # Start dev server
npm run build     # Build for production
npm run preview   # Preview production build
npm run check     # Run svelte-check for type errors
```

### Key Development Patterns

**Creating a Service:**
```typescript
// lib/api/services/example.ts
export async function getExample(id: string) {
  const response = await fetch(`/api/example/${id}`);
  if (!response.ok) throw new Error('Failed to fetch');
  return response.json() as Promise<ExampleDto>;
}
```

**Creating a Store:**
```typescript
// lib/stores/example.svelte.ts
export class ExampleStore {
  items = $state<ExampleDto[]>([]);
  isLoading = $state(false);
  error = $state<string | null>(null);

  async loadItems() {
    this.isLoading = true;
    try {
      this.items = await getExamples();
    } catch (e) {
      this.error = e instanceof Error ? e.message : 'Unknown error';
    } finally {
      this.isLoading = false;
    }
  }
}

export const exampleStore = new ExampleStore();
```

**Creating a Component:**
```svelte
<script lang="ts">
  import type { ExampleDto } from '$lib/api/types';

  interface Props {
    example: ExampleDto;
    onDelete?: (id: string) => void;
  }

  const { example, onDelete } = $props();
</script>

<div>
  <h3>{example.name}</h3>
  {#if onDelete}
    <button onclick={() => onDelete(example.id)}>Delete</button>
  {/if}
</div>
```

### File Naming Conventions

- **Svelte components**: PascalCase (e.g., `TodoItem.svelte`)
- **Store files**: lowercase with `.svelte.ts` extension (e.g., `auth.svelte.ts`)
- **Services**: lowercase (e.g., `lists.ts`)
- **Routes**: lowercase with `+` prefix (e.g., `+page.svelte`, `+layout.svelte`)

## API Integration

The frontend communicates with a .NET backend API. All requests go through the proxy at `/api`:

### Key Endpoints

**Authentication:**
- `POST /api/auth/register` - Create account
- `POST /api/auth/login` - Login
- `POST /api/auth/logout` - Logout
- `GET /api/auth/me` - Get current user

**Lists:**
- `POST /api/lists` - Create list
- `GET /api/lists` - Get all lists
- `GET /api/lists/{id}` - Get single list
- `PATCH /api/lists` - Update list
- `DELETE /api/lists/{id}` - Delete list

**Items:**
- `POST /api/todoitems` - Create item
- `GET /api/todoitems/bylist/{listId}` - Get items for list
- `PATCH /api/todoitems` - Update item
- `PATCH /api/todoitems/{id}/complete` - Mark complete
- `DELETE /api/todoitems/{id}` - Delete item

**Sharing:**
- `POST /api/invites` - Send invite
- `GET /api/invites/received` - Get received invites
- `PATCH /api/invites/{id}/accept` - Accept invite

See [PROJECT_SOURCE_OF_TRUTH.md](PROJECT_SOURCE_OF_TRUTH.md) for complete API documentation.

## Internationalization

This project supports multiple languages via the i18n system in [src/lib/i18n/](src/lib/i18n/).

**Supported languages:**
- English (en)
- Danish (da)

To add a new language:
1. Create a new JSON file in `src/lib/messages/` (e.g., `de.json`)
2. Mirror the structure from existing language files
3. Update the i18n configuration

## Authentication Flow

1. User registers or logs in
2. Backend validates credentials and sets HttpOnly cookie
3. Frontend proxy transparently forwards cookie to browser
4. Browser stores cookie automatically (secure, HttpOnly)
5. Subsequent requests include cookie automatically
6. Backend validates cookie on each request

If authentication fails (401), the app redirects to login.

## Project Standards

- **TypeScript**: No `any` types; always explicitly typed
- **Svelte 5 Runes**: Use `$state`, `$derived`, `$effect`, `$props`, `$bindable`
- **Error Handling**: All API errors caught and handled consistently
- **Components**: Props typed with `$props`, no legacy component syntax
- **Styling**: Tailwind CSS utilities, no inline styles unless necessary
- **Accessibility**: Semantic HTML, ARIA labels where needed

## Troubleshooting

### "Cannot connect to backend"
- Ensure backend is running on `http://localhost:5000`
- Check that the proxy configuration points to the correct backend URL
- Verify no firewall is blocking the connection

### "401 Unauthorized" redirects to login
- Session cookie may have expired
- Backend may require re-authentication
- Try logging in again

### Store state not updating
- Ensure you're using Svelte 5 runes syntax (`$state`, `$derived`)
- Store file must have `.svelte.ts` extension (not `.ts`)
- Components must destructure or access state directly

## Related Repositories

- **Backend API**: [rememberall-backend](../rememberall-backend) - .NET Web API with Entity Framework Core

## License

[Add license information here]

## Contributing

[Add contribution guidelines here]
