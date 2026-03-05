# RememberAll Frontend - Source of Truth

**Target Backend:** `http://localhost:5000/api` (internal only)  
**Frontend Dev Server:** `http://localhost:5173`  
**Frontend API Proxy:** `/api` (all requests go through this)  
**Authentication:** HttpOnly Cookie-based (set via frontend proxy)  
**Communication Flow:** Browser → Frontend (`/api`) → Backend (`http://localhost:5000/api`)

---

## Architecture Overview

The frontend uses a **proxy pattern** for all backend communication:

```
┌─────────────┐
│   Browser   │
└──────┬──────┘
       │ All API calls to /api
       │ (same-origin)
┌──────▼──────────────────┐
│ SvelteKit Frontend       │
│ (src/routes/api/[...path]/+server.ts)
└──────┬──────────────────┘
       │ Forwards requests
       │ (internal network)
┌──────▼──────────────────┐
│ Backend API             │
│ (http://localhost:5000) │
└─────────────────────────┘
```

### Benefits

✅ **Security** - Backend never exposed directly to browser  
✅ **CORS** - Simplified; frontend server ↔ backend is same-network  
✅ **Cookies** - Can use HttpOnly (not accessible from JS)  
✅ **Centralized** - All backend communication goes through one place  
✅ **Flexible** - Can change backend URL without client code changes  

---

## Backend Architecture

### Core Entities & Relationships
- **Users**: Base entity with email/password, can own lists and receive invites
- **TodoLists**: Owned by users, contain items, can be shared via ListAccess
- **TodoItems**: Belong to lists, have completion status and completion count
- **ListAccess**: Permissions system enabling collaboration without transferring ownership
- **Invites**: Workflow for sharing lists (send → accept → creates ListAccess)

### Authentication
- HttpOnly cookie-based sessions issued by backend
- Frontend proxy transparently forwards cookies
- All endpoints except login, register, and password-requirements require authentication
- Backend enforces ownership/permissions server-side on every request
- 401 responses indicate missing/invalid auth (redirect to login)
- 403 responses indicate permission denied

### Important Behaviors
- Update operations use **PATCH** (only send changed fields, not entire object)
- All timestamps in ISO 8601 format
- Request validation happens at DTO level; mirror these rules in frontend
- Errors are standardized JSON from GlobalExceptionMiddleware
- Database operations are transactional
- Deleting a list cascades to delete all its items

---

## API Endpoints Reference

All endpoints are accessed through `/api/*` (frontend proxy):

### Authentication (Public)

| Method | Endpoint | Request | Response |
|--------|----------|---------|----------|
| POST | `/api/auth/register` | CreateUserDto | UserDto |
| POST | `/api/auth/login` | LoginDto | UserDto |
| GET | `/api/auth/password-requirements` | — | string |
| GET | `/api/auth/me` | — | UserDto |
| POST | `/api/auth/logout` | — | 204 No Content |
| DELETE | `/api/auth/delete-account` | — | 204 No Content |

### Todo Lists

| Method | Endpoint | Request | Response |
|--------|----------|---------|----------|
| POST | `/api/lists` | CreateTodoListDto | TodoListDto |
| GET | `/api/lists` | — | TodoListDto[] |
| GET | `/api/lists/{listId}` | — | TodoListDto |
| PATCH | `/api/lists` | UpdateTodoListDto | TodoListDto |
| DELETE | `/api/lists/{listId}` | — | 204 No Content |

### Todo Items

| Method | Endpoint | Request | Response |
|--------|----------|---------|----------|
| POST | `/api/todoitems` | CreateTodoItemDto | TodoItemDto |
| GET | `/api/todoitems/bylist/{listId}` | — | TodoItemDto[] |
| PATCH | `/api/todoitems` | UpdateTodoItemDto | TodoItemDto |
| PATCH | `/api/todoitems/{itemId}/complete` | — | TodoItemDto |
| PATCH | `/api/todoitems/{itemId}/incomplete` | — | TodoItemDto |
| DELETE | `/api/todoitems/{itemId}` | — | 204 No Content |

### List Access

| Method | Endpoint | Query | Response |
|--------|----------|-------|----------|
| GET | `/api/listaccess` | none or `?listId={id}` | ListAccessDto[] |
| DELETE | `/api/listaccess/{listAccessId}` | — | 204 No Content |

### Invites

| Method | Endpoint | Request | Response |
|--------|----------|---------|----------|
| POST | `/api/invites` | CreateInviteDto | InviteDto |
| GET | `/api/invites/sent` | — | InviteDto[] |
| GET | `/api/invites/received` | — | InviteDto[] |
| PATCH | `/api/invites/{inviteId}/accept` | — | 204 No Content |
| DELETE | `/api/invites/{inviteId}` | — | 204 No Content |

---

## DTOs

### UserDto
```typescript
{
  id: string (uuid);
  name: string;
  email: string;
}
```

### CreateUserDto
```typescript
{
  name: string;
  email: string;
  password: string;
}
```

### LoginDto
```typescript
{
  email: string;
  password: string;
}
```

### TodoListDto
```typescript
{
  id: string (uuid);
  ownerId: string (uuid);
  name: string;
  items: TodoItemDto[];
}
```

### CreateTodoListDto
```typescript
{
  name: string;
}
```

### UpdateTodoListDto
```typescript
{
  id: string (uuid);
  name: string;
}
```

### TodoItemDto
```typescript
{
  id: string (uuid);
  text: string;
  isCompleted: boolean;
  completionCount: number;
}
```

### CreateTodoItemDto
```typescript
{
  todoListId: string (uuid);
  text: string;
}
```

### UpdateTodoItemDto
```typescript
{
  id: string (uuid);
  text: string;
}
```

### ListAccessDto
```typescript
{
  id: string (uuid);
  userId: string (uuid);
  userName: string;
  listId: string (uuid);
  listName: string;
}
```

### InviteDto
```typescript
{
  id: string (uuid);
  inviteSenderId: string (uuid);
  inviteSenderName: string;
  inviteRecieverId: string (uuid);
  inviteRecieverName: string;
  listId: string (uuid);
  listName: string;
}
```

### CreateInviteDto
```typescript
{
  inviteRecieverId: string (uuid);
  listId: string (uuid);
}
```

---

## HTTP Status Codes

| Code | Meaning |
|------|---------|
| 200 | OK - Request successful |
| 201 | Created - Resource created |
| 204 | No Content - Successful, no body |
| 400 | Bad Request - Validation failed |
| 401 | Unauthorized - Missing/invalid auth |
| 403 | Forbidden - No permission |
| 404 | Not Found - Resource not found |
| 409 | Conflict - Business logic violation |
| 500 | Internal Server Error |

---

## Recommended Frontend Architecture

### Folder Structure
```
src/
├── lib/
│   ├── api/
│   │   ├── client.ts                # HTTP client wrapper (fetch + credentials)
│   │   ├── types.ts                 # All DTOs as TypeScript interfaces
│   │   └── services/
│   │       ├── auth.ts              # login, register, logout, getCurrentUser, etc.
│   │       ├── lists.ts             # CRUD for lists
│   │       ├── items.ts             # CRUD for items
│   │       ├── invites.ts           # Send, accept, decline invites
│   │       └── access.ts            # List access management
│   ├── stores/
│   │   ├── auth.svelte.ts           # Current user & auth state (Svelte 5 rune-based)
│   │   ├── lists.svelte.ts          # Todo lists state (Svelte 5 rune-based)
│   │   └── invites.svelte.ts        # Invites state (Svelte 5 rune-based)
│   ├── components/
│   │   ├── common/                  # Generic UI components
│   │   ├── auth/                    # Auth-specific components
│   │   ├── lists/                   # List-specific components
│   │   └── items/                   # Item-specific components
│   └── utils/
│       ├── errors.ts                # ApiError, handleApiError()
│       ├── validators.ts            # Email, password, required fields
│       └── format.ts                # Date/string formatting
├── routes/
│   ├── +layout.svelte               # Root layout (auth check, navbar)
│   ├── +page.svelte                 # Dashboard/lists
│   ├── login/
│   │   └── +page.svelte
│   ├── register/
│   │   └── +page.svelte
│   ├── lists/
│   │   ├── [id]/
│   │   │   └── +page.svelte         # Single list view
│   │   └── +page.svelte
│   └── invites/
│       └── +page.svelte
├── app.html
├── app.d.ts
└── app.css
```

### Implementation Priority

**Phase 1 - Core Infrastructure:**
1. HTTP client wrapper (`lib/api/client.ts`)
   - Fetch wrapper with credentials: 'include'
   - Base URL configuration
   - Error handling for 401/403
   - Typed responses
   
2. DTOs (`lib/api/types.ts`)
   - All interfaces matching backend exactly
   
3. Auth service (`lib/api/services/auth.ts`)
   - login(), register(), logout(), getCurrentUser()
   - getPasswordRequirements()
   
4. Auth store (`lib/stores/auth.ts`)
   - currentUser state
   - isAuthenticated derived state
   - dispatch methods for login/logout

**Phase 2 - Routes & Guards:**
1. Root layout (`routes/+layout.svelte`)
   - Check auth on mount
   - Redirect to login if needed
   - Show navbar if authenticated
   
2. Login/register pages
3. Dashboard (lists overview)

**Phase 3 - Core Features:**
1. Lists service & store
2. Items service & store
3. List detail page & components

**Phase 4 - Sharing:**
1. Invites service & store
2. List access display & management
3. Invite acceptance flow

### Key Patterns

**HTTP Client (Simplified):**
```typescript
// Uses frontend proxy at /api (same-origin)
// No need for credentials: 'include' (same-origin by default)
// No CORS issues (frontend server handles that)
const response = await fetch('/api/auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify(data)
});

// Cookies are automatically sent/received (same-origin)
// HttpOnly cookies can be used (not accessible from JS)
```

**Service Methods:**
```typescript
// Type-safe, error-aware
// All calls go through frontend proxy
export async function createList(data: CreateTodoListDto): Promise<TodoListDto> {
  const response = await fetch('/api/lists', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(data)
  });
  if (!response.ok) throw new Error(`API error: ${response.status}`);
  return response.json() as Promise<TodoListDto>;
}
```

**Svelte Stores (in `.svelte.ts` file):**
```typescript
// Use $state, $derived, $effect (Svelte 5 runes)
import { type UserDto } from './api/types';

class AuthStore {
  currentUser = $state<UserDto | null>(null);
  isLoading = $state(false);
  error = $state<string | null>(null);

  isAuthenticated = $derived(!!this.currentUser);

  async login(credentials: LoginDto) {
    this.isLoading = true;
    try {
      // Calls frontend proxy at /api
      const response = await fetch('/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(credentials)
      });
      if (!response.ok) throw new Error('Login failed');
      // HttpOnly cookie set automatically by frontend proxy
      this.currentUser = await response.json();
    } catch (e) {
      this.error = e instanceof Error ? e.message : 'Unknown error';
    } finally {
      this.isLoading = false;
    }
  }
}

export const authStore = new AuthStore();
```

**Component Integration:**
```svelte
<script lang="ts">
  import { authStore } from '$lib/stores/auth.svelte';
  
  // Direct access to reactive store state
  const { currentUser, isAuthenticated, isLoading } = authStore;
</script>

{#if isAuthenticated}
  <p>Hello {currentUser?.name}</p>
{:else}
  <p>Please log in</p>
{/if}
```

**Alternative: Using `$effect` for side effects:**
```svelte
<script lang="ts">
  import { authStore } from '$lib/stores/auth.svelte';
  
  let displayName = $state('');
  
  $effect(() => {
    displayName = authStore.currentUser?.name ?? 'Guest';
  });
</script>

<p>Hello {displayName}</p>
```

### Validation Rules

Mirror backend validation in frontend for better UX:
- Fetch password requirements from `/api/auth/password-requirements`
- Validate email format before submission
- Check required fields
- Validate list/item name lengths

### Error Handling Strategy

1. All errors wrapped in ApiError class with status + message
2. Use handleApiError(error) utility for user-friendly messages
3. 401 → Redirect to login
4. 403 → Show "Permission denied"
5. 400 → Show validation error details
6. 5xx → Show generic error message

### Session & Cookies

- **Proxy Pattern**: All backend calls go through `/api` (frontend proxy)
- **Same-Origin**: Frontend proxy to client is same-origin (no CORS issues)
- **Cookies**: Backend can use HttpOnly, Secure, SameSite cookies
- **Security**: Backend URL hidden from browser; only frontend knows backend location
- **Cookie Flow**: Backend sets cookie in response → Frontend proxy forwards to client → Browser stores automatically
- **Proxy Location**: `src/routes/api/[...path]/+server.ts`
- **Backend URL**: Configured at top of proxy file (can change without client code changes)

---

## Development Checklist

- [ ] HTTP client with typed responses
- [ ] All DTOs defined in types.ts
- [ ] Auth service (login, register, getCurrentUser)
- [ ] Auth store with currentUser state
- [ ] Root layout with auth guard
- [ ] Login/register pages
- [ ] Error handling utilities
- [ ] Lists service & store
- [ ] Items service & store
- [ ] List management UI
- [ ] Invites service & store
- [ ] Share/invite UI

---

## Important Notes for LLM Building This

1. **Always use TypeScript** - No `any` types
2. **Use Svelte 5 runes** - `$state`, `$derived`, `$effect`, `$props`, `$bindable` (no legacy syntax)
3. **Validate with svelte-autofixer** - Check each component before finalizing
4. **Style with Tailwind** - Utility-first, no inline styles unless complex
5. **HTTP client uses native fetch** - No external HTTP library
6. **Services are simple async functions** - No class wrappers needed
7. **Stores use class + runes pattern** - Store classes with `$state` fields in `.svelte.ts` files
8. **Component props typed with `$props`** - Always type-safe
9. **Error handling is critical** - All API errors handled consistently
10. **Authentication first** - Verify current user before showing protected content
11. **Store files use `.svelte.ts` extension** - Required for runes to work in stores
12. **Direct state access in components** - Destructure or access via dot notation, no separate subscription pattern
