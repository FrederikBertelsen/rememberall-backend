# Edit Mode - Visual Guide & User Instructions

## How to Use Edit Mode

### Step 1: Enter Edit Mode
Click the **"Edit"** button in the top-right corner of the list.

```
┌─────────────────────────────────────────┐
│ ← Back              [Edit] ← Click here  │
│ My Shopping List                        │
├─────────────────────────────────────────┤
│ ☐ Milk                                  │
│ ☐ Bread                                 │
│ ☐ Eggs                                  │
├─────────────────────────────────────────┤
│ Completed                               │
│ ☑ Butter (from yesterday)               │
└─────────────────────────────────────────┘
```

---

### Step 2: Toggle Items with Checkboxes
Once in edit mode, tap the **checkbox** next to any item to mark it complete or incomplete.

```
Edit Mode Active:
┌─────────────────────────────────────────┐
│ ← Back      [Cancel] [Save (0)]         │ ← Changed count
│ My Shopping List                        │
├─────────────────────────────────────────┤
│ ☐ Milk                                  │
│ ☑ Bread                  ← Just toggled │
│ ☐ Eggs                                  │
├─────────────────────────────────────────┤
│ Completed                               │
│ ☑ Butter                               │
│ ☑ Bread  ← Moved here (strikethrough)   │
└─────────────────────────────────────────┘
```

---

### Step 3: Preview Your Changes
Items move to their new section immediately. You can see:
- Which items you've changed
- Where they'll end up after saving
- The complete new state of your list

```
After toggling Bread and Eggs:
┌─────────────────────────────────────────┐
│ ← Back      [Cancel] [Save (2)]         │ ← "2" changes pending
│ My Shopping List                        │
├─────────────────────────────────────────┤
│ ☐ Milk                                  │
│ ☐ Eggs                                  │
├─────────────────────────────────────────┤
│ Completed                               │
│ ☑ Butter                               │
│ ☑ Bread                                │
│ ☑ Eggs                                 │
└─────────────────────────────────────────┘
```

---

### Step 4: Save or Cancel

#### Option A: Save Changes
Click **"Save (X)"** to send your changes to the server.
- Shows "Saving..." while syncing
- Returns to view mode on success
- Stays in edit mode if error (allows retry)

```
While saving:
[Cancel] [Saving...]  ← Shows loading state

After success:
Back to View Mode:
├─────────────────────────────────────────┤
│ ← Back              [Edit]               │ ← Back to view mode
│ My Shopping List                        │
├─────────────────────────────────────────┤
│ ☐ Milk                                  │
├─────────────────────────────────────────┤
│ Completed                               │
│ ☑ Bread                                │
│ ☑ Butter                               │
│ ☑ Eggs                                 │
└─────────────────────────────────────────┘
```

#### Option B: Cancel Changes
Click **"Cancel"** to discard all changes and return to view mode.

```
Clicking Cancel:
[Cancel] [Save (2)]  ← Click Cancel

Result:
┌─────────────────────────────────────────┐
│ ← Back              [Edit]               │ ← Back to view mode
│ My Shopping List                        │
├─────────────────────────────────────────┤
│ ☐ Milk              ← Original state    │
│ ☐ Bread   ← Restored                    │
│ ☐ Eggs    ← Restored                    │
├─────────────────────────────────────────┤
│ Completed                               │
│ ☑ Butter            ← No changes made   │
└─────────────────────────────────────────┘
```

---

## Key Features

### ✅ Safe Workflow
- Preview changes before saving
- Cancel anytime to discard changes
- Changes only persist when you click "Save"

### ✅ Clear Visual Feedback
- Checkboxes show item state
- Save button shows count of changes: `Save (3)`
- Items move sections visually as you toggle
- Strikethrough text for completed items

### ✅ Prevents Accidental Clicks
- Must intentionally click "Edit" to change items
- No instant server sync on single tap
- Easy to notice what's changed before committing

### ✅ Minimum Height
- Uncompleted items section has minimum height
- Ensures completed items are visible
- Forces scroll to see all items (mental model: scrolling to see accomplishments)

### ✅ Edit Text Still Works
- Long-press on any item (in both view and edit modes)
- Opens modal to edit item text or delete
- Separate from the completion toggle

---

## Common Scenarios

### Scenario 1: "Oops, I Toggled the Wrong Item"
1. You're in edit mode
2. You tap the checkbox for the wrong item
3. It moves to completed section
4. **No problem!** Click it again to toggle back, or click Cancel to discard all changes
5. Click Save only when ready

### Scenario 2: "I Want to Change Multiple Items"
1. Click "Edit"
2. Tap checkboxes for all items you want to change
3. Review the final state (items are in correct sections)
4. Click "Save (5)" to save all 5 changes at once
5. Much faster and safer than clicking each individually

### Scenario 3: "I Started Editing but Changed My Mind"
1. You're in edit mode, toggled a few items
2. You realize you made a mistake
3. Click "Cancel" - all changes discarded
4. Back to original state instantly

### Scenario 4: "Network Error While Saving"
1. Click "Save"
2. Network fails midway
3. Error message shows
4. You're still in edit mode
5. Fix your network or review changes
6. Click "Save" again to retry

---

## Keyboard & Accessibility

### Touch Targets
- Checkbox: 24px (with 8px padding = 40px touch target) ✅
- Buttons: 40px minimum height ✅
- All targets easily tappable on mobile

### Visual Indicators
- Checkbox border highlights in teal when checked
- Strikethrough text shows completion
- Button text "Save (X)" shows change count
- "Saving..." text shows loading state

### Screen Reader Support
- Semantic HTML structure
- Form inputs properly labeled
- Buttons have clear labels

---

## Tips for Best Results

1. **Batch your changes**
   - Toggle all items you want to change
   - Save once instead of toggling individual items
   - Faster and safer

2. **Use long-press for editing text**
   - Edit mode is for completion status only
   - Long-press on item opens text editor
   - Both work in view and edit modes

3. **Review before saving**
   - Take a moment to review items
   - Verify they're in the right sections
   - Check the "Save (X)" count

4. **Use cancel liberally**
   - Made a mistake? Cancel and start fresh
   - No harm in discarding changes
   - Especially useful if you're second-guessing

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| "Save button is disabled" | No items have changed. Toggle something first. |
| "Save button shows 'Saving...'" | Network request in progress. Wait for completion. |
| "Items didn't save" | Check network. Error message should show above list. |
| "I can't find an item" | Check the completed section. It moved when you toggled. Scroll down to see it. |
| "Long-press didn't work" | Make sure you're not in edit mode. Edit mode uses toggles, not long-press. (Actually long-press still works!) |

---

## Summary

Edit mode makes managing your lists safer and easier:

1. **Click Edit** to enter edit mode
2. **Tap checkboxes** to toggle items (local preview)
3. **Review** your changes and the new list state
4. **Save** to persist, or **Cancel** to discard
5. **Done!** Back in view mode with your updates saved

No more accidental toggles. No more losing items in long lists. Just safe, intentional list management.
