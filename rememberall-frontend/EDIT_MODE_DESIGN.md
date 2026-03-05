# Edit Mode Design for Todo List - Design & Implementation Guide

## Problem Statement

**Current Issues:**
1. **Accidental clicks**: Users can accidentally toggle items with a single tap, immediately syncing to server
2. **Hard to notice changes**: If the completed items section is long, the moved item disappears off-screen
3. **No preview**: Users can't see what they've changed before it's persisted
4. **No recovery**: Once tapped, the change is instantly reflected and needs another tap to undo

---

## Proposed Solution: Edit Mode with Local Preview

### Design Philosophy

- **Clear intent**: Make it obvious when editing vs. viewing
- **Safe operations**: All changes are previewed locally before saving
- **Minimal complexity**: Use simple state management without extra complexity
- **Visual feedback**: Show exactly what has changed before saving

---

## User Experience Flow

### Default State: View Mode
- **UI**: Clean, read-only list of items
- **Interactions**: 
  - Single tap on item: **Enter edit mode** (not toggle)
  - Long press: Show edit/delete options (existing behavior, kept as-is)
- **Visual**: Normal list appearance

### Edit Mode: Editing State
- **Trigger**: User taps "Edit" button (top of list) or single-tap on item
- **UI Changes**:
  - **Toggle controls** appear next to each item (checkbox or simple button)
  - Tapping toggle switches item state locally (not saved)
  - **Save button** appears at bottom (sticky, always visible)
  - **Cancel button** discards all local changes
  - **Visual separator** between uncompleted/completed stays present
  - Items can still show completion state visually

- **Interaction Model**:
  - Tap toggle icon to flip completion state (local only)
  - State updates immediately for visual feedback
  - No server calls until "Save" is clicked
  - User can see all their planned changes before committing

- **Visual Feedback**:
  - Toggled items visually move to their new section
  - Completed section stays visible below uncompleted
  - User can scroll to see all changes
  - Minimum height on uncompleted section ensures visibility

### Save Action
- **Trigger**: User clicks "Save" button
- **Behavior**:
  - Send all changed items to server in batches (changed items only)
  - Show loading state while syncing
  - Exit edit mode on success
  - Return to View Mode

### Cancel Action
- **Trigger**: User clicks "Cancel" button
- **Behavior**:
  - Discard all local changes
  - Reload from server state (or reset from store)
  - Exit edit mode back to View Mode

---

## Implementation Strategy

### State Management (Svelte 5 Runes)

#### In the Page Component
```typescript
// View/Edit mode state
let isEditMode = $state(false);

// Local changes tracking (independent from server state)
let localItems = $state<Record<string, boolean>>({}); // itemId -> isCompleted

// Computed: which items have changed
let changedItems = $derived.by(() => {
  return Object.entries(localItems)
    .filter(([itemId, isCompleted]) => {
      const item = items.find(i => i.id === itemId);
      return item && item.isCompleted !== isCompleted;
    })
    .map(([itemId]) => itemId);
});

// Computed: items with potential local edits applied
let displayItems = $derived.by(() => {
  if (!isEditMode) return items;
  
  return items.map(item => {
    const localState = localItems[item.id];
    return localState !== undefined 
      ? { ...item, isCompleted: localState }
      : item;
  });
});
```

#### Initialization
```typescript
function enterEditMode() {
  // Snapshot current state as baseline for local changes
  localItems = items.reduce((acc, item) => {
    acc[item.id] = item.isCompleted;
    return acc;
  }, {} as Record<string, boolean>);
  
  isEditMode = true;
}

function exitEditMode() {
  isEditMode = false;
  localItems = {};
}
```

#### Toggle Handler
```typescript
async function handleToggleInEditMode(itemId: string) {
  // Only update local state, no server call
  localItems[itemId] = !localItems[itemId];
}
```

#### Save Handler
```typescript
async function handleSaveChanges() {
  if (changedItems.length === 0) {
    exitEditMode();
    return;
  }

  try {
    // Send only changed items to server
    for (const itemId of changedItems) {
      const newState = localItems[itemId];
      if (newState) {
        await itemsStore.completeItem(itemId, listId);
      } else {
        await itemsStore.incompleteItem(itemId, listId);
      }
    }
    
    // Store will refresh, state will sync
    exitEditMode();
  } catch (err) {
    // Error handling
  }
}
```

#### Cancel Handler
```typescript
function handleCancelChanges() {
  exitEditMode();
}
```

---

## Component Architecture

### Page Component Structure
```
+page.svelte
├── Header with "Edit" button
├── Error alerts
├── List rendering logic
│   ├── Edit mode: Show toggles, render from displayItems
│   └── View mode: Read-only, normal click behavior
├── Item rows (existing ItemRow component)
├── Modals (edit/delete, share, etc. - unchanged)
└── Action buttons
    ├── Edit button (View mode only)
    └── Save/Cancel buttons (Edit mode only)
```

### UI Layout Changes

#### View Mode
```
┌─────────────────────────────┐
│ ← Back         Share | Delete│
│ List Name                    │
├─────────────────────────────┤
│ [Edit] button (top right)    │
├─────────────────────────────┤
│ Item 1                       │
│ Item 2                       │
├─────────────────────────────┤
│ Completed                    │
│ Item 3 (strikethrough)       │
│ Item 4 (strikethrough)       │
├─────────────────────────────┤
│ [+] floating button          │
└─────────────────────────────┘
```

#### Edit Mode
```
┌─────────────────────────────┐
│ ← Back         Share | Delete│
│ List Name                    │
├─────────────────────────────┤
│ [Cancel] ... [Save] buttons  │
├─────────────────────────────┤
│ ☐ Item 1                    │
│ ☑ Item 2                    │
├─────────────────────────────┤
│ Completed (highlight)        │
│ ☑ Item 3                    │
│ ☑ Item 4                    │
├─────────────────────────────┤
│ [Cancel] ... [Save] buttons  │
└─────────────────────────────┘
```

---

## Minimum Height for Uncompleted Section

To ensure users always see at least some completed items and understand the feature, add a CSS rule:

```css
.uncompleted-items-section {
  min-height: 400px; /* Adjust based on design */
}
```

Or use Tailwind utility:
```svelte
<div class="space-y-0 min-h-50">
  <!-- uncompleted items -->
</div>
```

This ensures:
- Single item doesn't take entire screen
- User must scroll to see completed items (mental model: "I'm scrolling down to see what I've accomplished")
- Completed items are always partially visible as a "sink"

---

## Implementation Checklist

### Phase 1: State & Logic
- [ ] Add `isEditMode`, `localItems`, `changedItems`, `displayItems` state to page
- [ ] Implement `enterEditMode()`, `exitEditMode()` functions
- [ ] Implement `handleToggleInEditMode()` - updates local state only
- [ ] Implement `handleSaveChanges()` - batch send to server
- [ ] Implement `handleCancelChanges()` - discard local changes

### Phase 2: UI Changes
- [ ] Add "Edit" button (View mode only) - top right near existing controls
- [ ] Add "Cancel" and "Save" buttons (Edit mode only) - sticky at bottom or top
- [ ] Update item rendering: Show toggles in edit mode, normal in view mode
- [ ] Apply minimum height to uncompleted items section
- [ ] Add visual feedback for changed items (optional: highlight changed items)

### Phase 3: UX Polish
- [ ] Disable Save button if no items changed (`changedItems.length === 0`)
- [ ] Show loading state on Save button during sync
- [ ] Sync loading state from `itemsStore.isLoading`
- [ ] Add confirmation if user tries to leave edit mode with unsaved changes (optional)
- [ ] Ensure toggles are large enough for mobile (44px minimum)

### Phase 4: Testing
- [ ] Test entering/exiting edit mode
- [ ] Test toggling items locally (no server calls)
- [ ] Test saving changes (batch send)
- [ ] Test canceling (discards changes)
- [ ] Test with network failures
- [ ] Test on mobile devices

---

## Code Example: Edit Mode Toggle Button

```svelte
<script lang="ts">
  let isEditMode = $state(false);
  let changedItems = $derived([/* calculated */]);
  
  function enterEditMode() {
    // ... snapshot state
    isEditMode = true;
  }
</script>

<!-- View Mode: Show Edit Button -->
{#if !isEditMode}
  <button
    onclick={enterEditMode}
    class="px-3 py-1 text-sm font-medium rounded"
    style="background-color: var(--color-accent-light); color: var(--color-accent);"
  >
    Edit
  </button>
{:else}
  <!-- Edit Mode: Show Save/Cancel -->
  <div class="flex gap-2">
    <button
      onclick={handleCancelChanges}
      class="btn btn-secondary flex-1 text-sm"
    >
      Cancel
    </button>
    <button
      onclick={handleSaveChanges}
      disabled={changedItems.length === 0 || isLoading}
      class="btn flex-1 text-sm"
      style="background-color: var(--color-accent);"
    >
      {isLoading ? 'Saving...' : `Save (${changedItems.length})`}
    </button>
  </div>
{/if}
```

---

## Edge Cases & Considerations

### 1. Network Errors During Save
- **Problem**: User clicks Save, but server fails midway
- **Solution**: Show error alert, keep edit mode active, allow retry or cancel
- **Code**: Wrap `handleSaveChanges()` in try/catch, show error in UI

### 2. Server State Changes During Edit
- **Problem**: Another user modifies the list while user is editing
- **Solution**: Periodically poll server; if state changes, show warning and reload
- **Code**: Use existing `refreshList` mechanism, handle conflict gracefully

### 3. Partial Save Failure
- **Problem**: Some items save, others fail
- **Solution**: Re-fetch list from server to sync state, show error
- **Code**: After save attempt, call `listsStore.fetchList(listId)` to resync

### 4. User Navigates Away
- **Problem**: User closes tab or navigates while in edit mode
- **Solution**: Browser's beforeunload event can warn user
- **Code**: Optional - add `beforeunload` handler if needed

### 5. Long Lists (Performance)
- **Problem**: Rendering 100+ items might be slow in edit mode
- **Solution**: Current approach is fine (no extra DOM elements), but monitor performance
- **Code**: Profile with DevTools if needed

---

## Alternative Approaches (Not Recommended)

### Option A: Per-Item Inline Save Button
- **Pros**: More granular control
- **Cons**: More server calls, more UI clutter, less safe
- **Decision**: Rejected - doesn't solve the "minimize errors" goal

### Option B: Undo/Redo Stack
- **Pros**: Power user feature
- **Cons**: Over-engineered, adds complexity
- **Decision**: Rejected - out of scope

### Option C: Drag-to-reorder with Save
- **Pros**: Combines reordering + completion
- **Cons**: Complex UX, harder on mobile
- **Decision**: Rejected - focus on toggle completion first

---

## Design Notes

### Why This Approach Works
1. **Simple**: Only adds one mode toggle, minimal state changes
2. **Safe**: Preview before persisting prevents accidents
3. **Clear**: Edit vs. View mode is visually distinct
4. **Mobile-friendly**: No hover states, tap-based interactions
5. **Scalable**: Works with lists of any size
6. **Non-intrusive**: Doesn't disrupt existing workflows (long-press still available)

### Why NOT Drag-to-Complete
- Harder to implement on mobile
- More error-prone (accidental drags)
- Less discoverable (no visual cue)
- Slower than toggling

### Why Sticky Save Button
- Always visible, no scrolling required
- Clear call-to-action
- Matches mobile app patterns
- Prevents accidental navigation away

---

## Future Enhancements (Out of Scope)

1. Swipe gesture to enter edit mode
2. Bulk operations (select multiple, complete all)
3. Item reordering in edit mode
4. Keyboard shortcuts (Enter to save, Esc to cancel)
5. Animation when items move between sections
6. "What changed" summary before saving

---

## Summary

**Key Changes:**
1. Add edit mode toggle with local state preview
2. Save button commits all changes to server
3. Minimum height on uncompleted section ensures visibility
4. Long-press functionality unchanged (still available in both modes)

**Benefits:**
- Prevents accidental clicks
- Users see all changes before committing
- Easier to notice what moved and where
- Still simple, not overengineered

**Implementation Time:** 1-2 hours (straightforward Svelte 5 state management)
