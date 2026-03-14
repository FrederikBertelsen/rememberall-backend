<script lang="ts">
	import { browser } from '$app/environment';
	import { page } from '$app/stores';
	import { goto } from '$app/navigation';
	import { ArrowLeft, Plus, Trash2, Users } from 'lucide-svelte';
	import { authStore } from '$lib/stores/auth.svelte';
	import { listsStore } from '$lib/stores/lists.svelte';
	import { itemsStore } from '$lib/stores/items.svelte';
	import { accessStore } from '$lib/stores/access.svelte';
	import { invitesStore } from '$lib/stores/invites.svelte';
	import { languageTag, tSync } from '$lib/i18n/index';
	import { createViewportState, calculateModalPosition } from '$lib/utils/viewport.svelte';
	import ItemRow from '$lib/components/items/ItemRow.svelte';
	import AccessRow from '$lib/components/lists/AccessRow.svelte';
	import ItemFormModal from '$lib/components/items/ItemFormModal.svelte';
	import ShareFormModal from '$lib/components/lists/ShareFormModal.svelte';
	import type {
		CreateInviteDto,
		CreateBatchTodoItemDto,
		UpdateBatchTodoItemDto
	} from '$lib/api/types';

	const listId = $page.params.id ?? '';
	let list = $derived(listId ? listsStore.getListById(listId) : null);
	let items = $derived(listId ? itemsStore.getItemsForList(listId) : []);
	let access = $derived(listId ? accessStore.getAccessForList(listId) : []);
	let isOwner = $derived(list ? list.ownerId === authStore.currentUser?.id : false);
	let showItemForm = $state(false);
	let showShareForm = $state(false);
	let showAccessModal = $state(false);
	let showDeleteListConfirm = $state(false);
	let showLeaveListConfirm = $state(false);
	let showEditDeleteItem = $state(false);
	let itemToEdit = $state<string | null>(null);
	let editText = $state<string>('');

	// Edit mode state
	let isEditMode = $state(false);
	let isSaving = $state(false);

	// Pending new items (added in edit mode, committed on save)
	interface PendingNewItem {
		localId: string;
		text: string;
	}
	let pendingNewItems = $state<PendingNewItem[]>([]);

	// Viewport state for modal positioning
	let viewportState = createViewportState();
	let leaveModalTopPercent = $derived(calculateModalPosition(viewportState, 200).topPercent);
	let accessModalTopPercent = $derived(calculateModalPosition(viewportState, 400).topPercent);
	let deleteModalTopPercent = $derived(calculateModalPosition(viewportState, 200).topPercent);
	let editItemModalTopPercent = $derived(calculateModalPosition(viewportState, 300).topPercent);

	// Long press handling for edit mode
	let longPressTimer: ReturnType<typeof setTimeout> | null = null;
	let longPressDetected = $state(false);
	let longPressInitialPos = $state<{ x: number; y: number } | null>(null);
	let longPressThreshold = 10; // pixels - movement beyond this cancels long press

	// Track completed section position for scroll anchoring
	let completedSectionRef = $state<HTMLDivElement | null>(null);
	let uncompletedSectionRef = $state<HTMLDivElement | null>(null);
	let lastKnownPosition = $state<number | null>(null);

	function handleLongPressStart(itemId: string, event: MouseEvent | TouchEvent) {
		if (!isEditMode) return;
		longPressDetected = false;

		// Get initial position
		if (event instanceof TouchEvent && event.touches.length > 0) {
			longPressInitialPos = { x: event.touches[0].clientX, y: event.touches[0].clientY };
		} else if (event instanceof MouseEvent) {
			longPressInitialPos = { x: event.clientX, y: event.clientY };
		}

		longPressTimer = setTimeout(() => {
			longPressDetected = true;
			handleLongPressItem(itemId);
		}, 500);
	}

	function handleLongPressMove(event: MouseEvent | TouchEvent) {
		if (!longPressTimer || !longPressInitialPos) return;

		let currentX: number, currentY: number;
		if (event instanceof TouchEvent && event.touches.length > 0) {
			currentX = event.touches[0].clientX;
			currentY = event.touches[0].clientY;
		} else if (event instanceof MouseEvent) {
			currentX = event.clientX;
			currentY = event.clientY;
		} else {
			return;
		}

		// Calculate distance moved
		const deltaX = Math.abs(currentX - longPressInitialPos.x);
		const deltaY = Math.abs(currentY - longPressInitialPos.y);

		// If moved beyond threshold, cancel long press
		if (deltaX > longPressThreshold || deltaY > longPressThreshold) {
			handleLongPressEnd();
		}
	}

	function handleLongPressEnd() {
		if (longPressTimer) {
			clearTimeout(longPressTimer);
			longPressTimer = null;
		}
		longPressDetected = false;
		longPressInitialPos = null;
	}

	function handleItemClick(itemId: string) {
		// Only toggle if not in a long press
		if (!longPressDetected) {
			handleToggleInEditMode(itemId);
		}
	}

	// Enhanced local state for edit mode
	interface LocalItemState {
		isCompleted?: boolean; // undefined means no change
		markedForDeletion?: boolean;
		newText?: string; // undefined means no text change
	}
	let localItems = $state<Record<string, LocalItemState>>({});

	// Computed: which items have any changes
	let changedItems = $derived.by(() => {
		const changes: string[] = [];
		for (const [itemId, localState] of Object.entries(localItems)) {
			const item = items.find((i) => i.id === itemId);
			if (!item) continue;

			const hasCompletionChange =
				localState.isCompleted !== undefined && item.isCompleted !== localState.isCompleted;
			const hasTextChange = localState.newText !== undefined && item.text !== localState.newText;
			const hasDeletionMark = localState.markedForDeletion === true;

			if (hasCompletionChange || hasTextChange || hasDeletionMark) {
				changes.push(itemId);
			}
		}
		return changes;
	});

	// Computed: items with local edits applied for display
	let displayItems = $derived.by(() => {
		return items.map((item) => {
			if (!isEditMode) return item;

			const localState = localItems[item.id];
			if (!localState) {
				return { ...item, localIsCompleted: item.isCompleted };
			}

			return {
				...item,
				localIsCompleted:
					localState.isCompleted !== undefined ? localState.isCompleted : item.isCompleted,
				localNewText: localState.newText,
				localMarkedForDeletion: localState.markedForDeletion || false
			} as typeof item & {
				localIsCompleted: boolean;
				localNewText?: string;
				localMarkedForDeletion: boolean;
			};
		});
	});

	// Separate and sort items
	// In edit mode, items that are locally uncompleted appear in BOTH sections for better UX
	let completedItems = $derived(
		displayItems.filter((i) => i.isCompleted).sort((a, b) => b.completionCount - a.completionCount)
	);
	let uncompletedItems = $derived.by(() => {
		const filtered = displayItems.filter((i) => {
			if (!isEditMode) return !i.isCompleted;
			// In edit mode: show originally uncompleted OR completed items that are now locally uncompleted
			return !i.isCompleted || (i.isCompleted && (i as any).localIsCompleted === false);
		});

		if (!isEditMode) {
			// In view mode: sort by updatedAt (oldest first)
			return filtered.sort(
				(a, b) => new Date(a.updatedAt).getTime() - new Date(b.updatedAt).getTime()
			);
		}

		// In edit mode: originally uncompleted items first (sorted by updatedAt), then newly unchecked items at bottom
		const originallyUncompleted = filtered
			.filter((i) => !i.isCompleted)
			.sort((a, b) => new Date(a.updatedAt).getTime() - new Date(b.updatedAt).getTime());
		const newlyUnchecked = filtered
			.filter((i) => i.isCompleted && (i as any).localIsCompleted === false)
			.sort((a, b) => new Date(a.updatedAt).getTime() - new Date(b.updatedAt).getTime());

		return [...originallyUncompleted, ...newlyUnchecked];
	});

	async function handleToggleItem(itemId: string): Promise<void> {
		if (!listId) return;
		const item = items.find((i) => i.id === itemId);
		if (!item) return;

		if (item.isCompleted) {
			await itemsStore.incompleteItem(itemId, listId);
		} else {
			await itemsStore.completeItem(itemId, listId);
		}
	}

	function enterEditMode(): void {
		// Initialize local state for all current items (only completion state)
		localItems = items.reduce(
			(acc, item) => {
				acc[item.id] = { isCompleted: item.isCompleted };
				return acc;
			},
			{} as Record<string, LocalItemState>
		);

		isEditMode = true;
		// Reset position tracking when entering edit mode
		lastKnownPosition = null;
	}

	function exitEditMode(): void {
		isEditMode = false;
		localItems = {};
		pendingNewItems = [];
		completedSectionRef = null;
		uncompletedSectionRef = null;
		lastKnownPosition = null;
	}

	function handleToggleInEditMode(itemId: string): void {
		// Update only completion state, preserve other local changes
		const current = localItems[itemId] || {};
		const originalItem = items.find((i) => i.id === itemId);
		if (!originalItem) return;

		localItems[itemId] = {
			...current,
			isCompleted:
				current.isCompleted !== undefined ? !current.isCompleted : !originalItem.isCompleted
		};
	}

	async function handleSaveChanges(): Promise<void> {
		if (changedItems.length === 0 && pendingNewItems.length === 0) {
			exitEditMode();
			return;
		}

		isSaving = true;

		try {
			// Prepare batch update data
			const creates: CreateBatchTodoItemDto[] = [];
			const updates: UpdateBatchTodoItemDto[] = [];
			const completes: string[] = [];
			const incompletes: string[] = [];
			const deletes: string[] = [];

			// Add pending new items to creates
			for (const pending of pendingNewItems) {
				creates.push({ text: pending.text });
			}

			// Process changed items
			for (const itemId of changedItems) {
				const localState = localItems[itemId];
				const originalItem = items.find((i) => i.id === itemId);
				if (!originalItem || !localState) continue;

				// Handle deletions
				if (localState.markedForDeletion) {
					deletes.push(itemId);
					continue; // Skip other operations for deleted items
				}

				// Handle text edits
				if (localState.newText !== undefined && localState.newText !== originalItem.text) {
					updates.push({ id: itemId, text: localState.newText });
				}

				// Handle completion changes
				if (
					localState.isCompleted !== undefined &&
					localState.isCompleted !== originalItem.isCompleted
				) {
					if (localState.isCompleted) {
						completes.push(itemId);
					} else {
						incompletes.push(itemId);
					}
				}
			}

			// Send batch update if there are any changes
			if (
				creates.length > 0 ||
				updates.length > 0 ||
				completes.length > 0 ||
				incompletes.length > 0 ||
				deletes.length > 0
			) {
				await itemsStore.batchUpdateItems({
					todoListId: listId,
					creates: creates.length > 0 ? creates : undefined,
					updates: updates.length > 0 ? updates : undefined,
					completes: completes.length > 0 ? completes : undefined,
					incompletes: incompletes.length > 0 ? incompletes : undefined,
					deletes: deletes.length > 0 ? deletes : undefined
				});
			}

			// Exit edit mode on success
			exitEditMode();
		} catch (err) {
			// Error already handled in store
		} finally {
			isSaving = false;
		}
	}

	function handleCancelChanges(): void {
		exitEditMode();
	}

	function handleAddItemInEditMode(text: string): void {
		const trimmed = text.trim().toLowerCase();
		if (!trimmed) return;

		// If it matches a completed item, toggle it locally (reuse)
		const matchingCompleted = items.find((i) => i.isCompleted && i.text.toLowerCase() === trimmed);
		if (matchingCompleted) {
			handleToggleInEditMode(matchingCompleted.id);
			return;
		}

		// If it's already on the list as an uncompleted item, skip
		const existingUncompleted = items.find(
			(i) => !i.isCompleted && i.text.toLowerCase() === trimmed
		);
		if (existingUncompleted) return;

		// If already pending, skip
		if (pendingNewItems.some((p) => p.text.toLowerCase() === trimmed)) return;

		pendingNewItems = [...pendingNewItems, { localId: trimmed, text: trimmed }];
	}

	function handleLongPressItem(itemId: string): void {
		const item = items.find((i) => i.id === itemId);
		if (item) {
			itemToEdit = itemId;
			// In edit mode, use local text if available, otherwise use original
			const localState = localItems[itemId];
			editText = isEditMode && localState?.newText !== undefined ? localState.newText : item.text;
			showEditDeleteItem = true;
		}
	}

	async function handleUpdateItem(): Promise<void> {
		if (!itemToEdit || !editText.trim()) return;

		if (isEditMode) {
			// In edit mode, store changes locally only if text actually changed
			const originalItem = items.find((i) => i.id === itemToEdit);
			if (!originalItem) return;

			const current = localItems[itemToEdit] || {};
			const newText = editText.trim();

			if (newText !== originalItem.text) {
				// Text has changed, store it
				localItems[itemToEdit] = {
					...current,
					newText: newText
				};
			} else {
				// Text hasn't changed, remove newText if it exists
				const { newText: _, ...rest } = current;
				localItems[itemToEdit] = rest;
				// If no other changes, remove the item entirely
				if (Object.keys(rest).length === 0) {
					delete localItems[itemToEdit];
				}
			}
		} else {
			// In view mode, update immediately
			if (!listId) return;
			try {
				await itemsStore.updateItem({ id: itemToEdit, text: editText }, listId);
			} catch (err) {
				// Error already handled in store
				return;
			}
		}

		showEditDeleteItem = false;
		itemToEdit = null;
		editText = '';
	}

	async function handleDeleteItemConfirmed(): Promise<void> {
		if (!itemToEdit) return;

		if (isEditMode) {
			// In edit mode, toggle deletion mark
			const current = localItems[itemToEdit] || {};
			localItems[itemToEdit] = {
				...current,
				markedForDeletion: !current.markedForDeletion
			};
		} else {
			// In view mode, delete immediately
			if (!listId) return;
			await itemsStore.deleteItem(itemToEdit, listId);
		}

		showEditDeleteItem = false;
		itemToEdit = null;
		editText = '';
	}

	async function handleShareList(data: CreateInviteDto): Promise<void> {
		await invitesStore.sendInvite(data);
		showShareForm = false;
	}

	async function handleDeleteList(): Promise<void> {
		if (!listId) return;
		await listsStore.deleteList(listId);
		showDeleteListConfirm = false;
		await goto('/');
	}

	async function handleRevokeAccess(accessId: string): Promise<void> {
		if (!listId) return;
		await accessStore.revokeAccess(accessId, listId);
	}

	async function handleLeaveList(): Promise<void> {
		if (!listId) return;
		await accessStore.leaveList(listId);
		showLeaveListConfirm = false;
		await goto('/');
	}

	// Load list and items on mount
	$effect(() => {
		if (!browser || !listId) return;

		listsStore.fetchList(listId).catch(() => {
			// Error already handled
		});
		itemsStore.fetchItemsByList(listId).catch(() => {
			// Error already handled
		});
		accessStore.fetchListAccess(listId).catch(() => {
			// Error already handled
		});
	});

	// Poll for list updates every 5 seconds
	$effect(() => {
		if (!listId || !list) return;

		const interval = setInterval(async () => {
			await listsStore.refreshList(listId, list.updatedAt);
		}, 1000);

		return () => clearInterval(interval);
	});

	// Maintain completed section position when items move between sections
	// Capture position BEFORE DOM updates
	$effect.pre(() => {
		if (!browser || !isEditMode || !completedSectionRef) return;

		// Track item counts to detect when changes are happening
		const _ = [uncompletedItems.length, pendingNewItems.length];

		// Capture position before any DOM changes
		lastKnownPosition = completedSectionRef.getBoundingClientRect().top;
	});

	// Compare position AFTER DOM updates and compensate
	$effect(() => {
		if (!browser || !isEditMode || !completedSectionRef || lastKnownPosition === null) return;

		// Track the same changes to run after DOM updates
		const _ = [uncompletedItems.length, pendingNewItems.length];

		// After DOM updates, check if position changed
		requestAnimationFrame(() => {
			if (!completedSectionRef || lastKnownPosition === null) return;

			const currentPosition = completedSectionRef.getBoundingClientRect().top;
			const positionChange = currentPosition - lastKnownPosition;

			// If the section moved, scroll to keep it at the same visual position
			if (Math.abs(positionChange) > 0.5) {
				window.scrollBy({ top: positionChange, behavior: 'instant' });
			}
		});
	});
</script>

{#if !list}
	<div class="page-container-scroll">
		<div class="py-12 text-center" style="color: var(--color-text-secondary);">
			<p class="text-sm">{tSync($languageTag, 'pages.listDetail.loadingList')}</p>
		</div>
	</div>
{:else}
	<div class="page-container-scroll">
		<div class="mb-8">
			<div class="mb-4 flex items-center justify-between gap-4">
				{#if isEditMode}
					<div
						aria-hidden="true"
						class="text-md inline-flex items-center gap-2 font-medium"
						style="visibility:hidden;"
					>
						<ArrowLeft size={20} />
						<span>{tSync($languageTag, 'pages.listDetail.back')}</span>
					</div>
				{:else}
					<a
						href="/"
						class="text-md inline-flex items-center gap-2 font-medium"
						style="color: var(--color-accent);"
					>
						<ArrowLeft size={20} />
						<span>{tSync($languageTag, 'pages.listDetail.back')}</span>
					</a>
				{/if}
				{#if !isEditMode}
					<div class="flex shrink-0 gap-4">
						{#if isOwner}
							<button
								onclick={() => (showShareForm = true)}
								class="flex items-center justify-center rounded border px-2 py-1 text-sm disabled:cursor-not-allowed disabled:opacity-50"
								style="background-color: var(--color-accent-light); color: var(--color-accent); border-color: var(--color-accent);"
								title={tSync($languageTag, 'pages.listDetail.shareList')}
							>
								<Plus size={24} />
								<Users size={24} style="margin-left: -4px;" />
							</button>
							<button
								onclick={() => (showDeleteListConfirm = true)}
								class="flex items-center justify-center rounded border px-2 py-1 text-sm disabled:cursor-not-allowed disabled:opacity-50"
								style="background-color: var(--color-danger); color: var(--color-error-light); border-color: var(--color-error-light);"
								title={tSync($languageTag, 'pages.listDetail.deleteList')}
							>
								<Trash2 size={24} />
							</button>
						{:else}
							<button
								onclick={() => (showLeaveListConfirm = true)}
								class="rounded border px-3 py-1 text-xs font-medium disabled:cursor-not-allowed disabled:opacity-50"
								style="background-color: var(--color-danger); color: var(--color-error-light); border-color: var(--color-error-light);"
								title={tSync($languageTag, 'pages.listDetail.leaveList')}
							>
								{tSync($languageTag, 'pages.listDetail.leave')}
							</button>
						{/if}
					</div>
				{:else}
					<div class="flex shrink-0 gap-6">
						<button
							onclick={handleCancelChanges}
							class="btn btn-secondary min-h-12 px-4 py-3 text-sm font-medium disabled:cursor-not-allowed disabled:opacity-50"
							disabled={isSaving}
						>
							{tSync($languageTag, 'modals.cancel')}
						</button>
						<button
							onclick={handleSaveChanges}
							disabled={(changedItems.length === 0 && pendingNewItems.length === 0) || isSaving}
							class="text-md min-h-12 rounded px-4 py-3 font-medium whitespace-nowrap disabled:cursor-not-allowed disabled:opacity-50"
							style="background-color: var(--color-accent); color: var(--color-text-primary);"
						>
							{isSaving
								? tSync($languageTag, 'pages.listDetail.saving')
								: `${tSync($languageTag, 'pages.listDetail.save')} (${changedItems.length + pendingNewItems.length})`}
						</button>
					</div>
				{/if}
			</div>

			<div class="flex items-center justify-between gap-4">
				<h1 class="text-3xl font-bold">{list.name}</h1>
				{#if !isEditMode}
					<button
						onclick={enterEditMode}
						class="flex items-center justify-center rounded border px-4 py-3 text-xl font-medium disabled:cursor-not-allowed disabled:opacity-50"
						style="background-color: var(--color-accent-light); color: var(--color-accent); border-color: var(--color-accent); min-width: 120px; min-height: 48px;"
						title="Enter edit mode"
					>
						{tSync($languageTag, 'buttons.edit')}
					</button>
				{/if}
			</div>
		</div>

		{#if showLeaveListConfirm}
			<!-- Modal Overlay -->
			<div
				style="background-color: var(--color-overlay-dark);"
				class="fixed inset-0 z-40"
				role="button"
				tabindex="0"
				onclick={() => (showLeaveListConfirm = false)}
				onkeydown={(e) => {
					if (e.key === 'Escape') showLeaveListConfirm = false;
				}}
				aria-label="Close modal"
			></div>

			<!-- Modal Content - positioned to avoid keyboard -->
			<div
				class="pointer-events-none fixed right-0 left-0 z-50 flex justify-center px-4"
				style="top: {leaveModalTopPercent}%"
			>
				<div
					class="pointer-events-auto max-w-xs rounded-lg border p-4"
					style="background-color: var(--color-bg-secondary); border-color: var(--color-border);"
				>
					<p class="mb-2 font-semibold" style="color: var(--color-text-primary);">
						{tSync($languageTag, 'pages.listDetail.leaveConfirmTitle')} "{list.name}"?
					</p>
					<p class="mb-4 text-sm" style="color: var(--color-text-secondary);">
						{tSync($languageTag, 'pages.listDetail.leaveConfirmMessage')}
					</p>
					<div class="flex gap-2">
						<button
							onclick={() => (showLeaveListConfirm = false)}
							class="btn btn-secondary flex-1 py-2 text-sm"
						>
							{tSync($languageTag, 'modals.cancel')}
						</button>
						<button
							onclick={() => handleLeaveList()}
							disabled={accessStore.isLoading}
							class="btn flex-1 py-2 text-sm"
							style="background-color: var(--color-danger); color: var(--color-text-primary);"
						>
							{tSync($languageTag, 'pages.listDetail.leave')}
						</button>
					</div>
				</div>
			</div>
		{/if}

		{#if showAccessModal && isOwner}
			<!-- Modal Overlay -->
			<div
				style="background-color: var(--color-overlay-dark);"
				class="fixed inset-0 z-40"
				role="button"
				tabindex="0"
				onclick={() => (showAccessModal = false)}
				onkeydown={(e) => {
					if (e.key === 'Escape') showAccessModal = false;
				}}
				aria-label="Close modal"
			></div>

			<!-- Modal Content - positioned to avoid keyboard -->
			<div
				class="pointer-events-none fixed right-0 left-0 z-50 flex justify-center px-4"
				style="top: {accessModalTopPercent}%"
			>
				<div
					class="pointer-events-auto max-h-[60vh] w-full overflow-y-auto rounded-lg border p-4"
					style="background-color: var(--color-bg-secondary); border-color: var(--color-border);"
				>
					<p class="mb-4 font-semibold" style="color: var(--color-text-primary);">
						{tSync($languageTag, 'pages.listDetail.sharedWith_text')} ({access.length})
					</p>

					{#if access.length === 0}
						<p class="text-sm" style="color: var(--color-text-secondary);">
							{tSync($languageTag, 'pages.listDetail.notSharedYet')}
						</p>
					{:else}
						<div class="space-y-2">
							{#each access as item (item.id)}
								<AccessRow
									access={item}
									isLoading={accessStore.isLoading}
									onRevoke={handleRevokeAccess}
								/>
							{/each}
						</div>
					{/if}

					<button
						onclick={() => (showAccessModal = false)}
						class="btn btn-secondary mt-4 w-full py-2 text-sm"
					>
						{tSync($languageTag, 'pages.listDetail.close')}
					</button>
				</div>
			</div>
		{/if}

		{#if showDeleteListConfirm}
			<!-- Modal Overlay -->
			<div
				style="background-color: var(--color-overlay-dark);"
				class="fixed inset-0 z-40"
				role="button"
				tabindex="0"
				onclick={() => (showDeleteListConfirm = false)}
				onkeydown={(e) => {
					if (e.key === 'Escape') showDeleteListConfirm = false;
				}}
				aria-label="Close modal"
			></div>

			<!-- Modal Content - positioned to avoid keyboard -->
			<div
				class="pointer-events-none fixed right-0 left-0 z-50 flex justify-center px-4"
				style="top: {deleteModalTopPercent}%"
			>
				<div
					class="pointer-events-auto max-w-xs rounded-lg border p-4"
					style="background-color: var(--color-bg-secondary); border-color: var(--color-border);"
				>
					<p class="mb-2 font-semibold" style="color: var(--color-text-primary);">
						{tSync($languageTag, 'pages.listDetail.deleteConfirmTitle')} "{list.name}"?
					</p>
					<p class="mb-4 text-sm" style="color: var(--color-text-secondary);">
						{tSync($languageTag, 'pages.listDetail.deleteConfirmMessage')}
					</p>
					<div class="flex gap-2">
						<button
							onclick={() => (showDeleteListConfirm = false)}
							class="btn btn-secondary flex-1 py-2 text-sm"
						>
							{tSync($languageTag, 'modals.cancel')}
						</button>
						<button
							onclick={() => handleDeleteList()}
							disabled={listsStore.isLoading}
							class="btn flex-1 py-2 text-sm"
							style="background-color: var(--color-danger); color: var(--color-text-primary);"
						>
							{tSync($languageTag, 'modals.delete')}
						</button>
					</div>
				</div>
			</div>
		{/if}

		{#if showEditDeleteItem && itemToEdit}
			<!-- Modal Overlay -->
			<div
				style="background-color: var(--color-overlay-dark);"
				class="fixed inset-0 z-40"
				role="button"
				tabindex="0"
				onclick={() => (showEditDeleteItem = false)}
				onkeydown={(e) => {
					if (e.key === 'Escape') showEditDeleteItem = false;
				}}
				aria-label="Close modal"
			></div>

			<!-- Modal Content - positioned to avoid keyboard -->
			<div
				class="pointer-events-none fixed right-0 left-0 z-50 flex justify-center px-4"
				style="top: {editItemModalTopPercent}%"
			>
				<div
					class="pointer-events-auto max-w-xs rounded-lg border p-4"
					style="background-color: var(--color-bg-secondary); border-color: var(--color-border);"
				>
					<p class="mb-4 font-semibold" style="color: var(--color-text-primary);">
						{tSync($languageTag, 'pages.listDetail.editItem')}
					</p>

					<div class="form-group mb-4">
						<input
							type="text"
							bind:value={editText}
							maxlength="40"
							class="form-input"
							placeholder={tSync($languageTag, 'pages.listDetail.itemText')}
							disabled={itemsStore.isLoading}
						/>
						<p class="form-hint">
							{editText.length}/40 {tSync($languageTag, 'pages.listDetail.characterCount')}
						</p>
					</div>

					<div class="flex gap-2">
						<button
							onclick={() => (showEditDeleteItem = false)}
							class="btn btn-secondary flex-1 py-2 text-sm"
							disabled={itemsStore.isLoading || (isEditMode && isSaving)}
						>
							{tSync($languageTag, 'modals.cancel')}
						</button>
						<button
							onclick={() => handleUpdateItem()}
							disabled={itemsStore.isLoading || (isEditMode && isSaving)}
							class="btn flex-1 py-2 text-sm"
							style="background-color: var(--color-accent); color: var(--color-text-primary);"
						>
							{tSync($languageTag, 'pages.listDetail.save')}
						</button>
						<button
							onclick={() => handleDeleteItemConfirmed()}
							disabled={itemsStore.isLoading || (isEditMode && isSaving)}
							class="btn flex-1 py-2 text-sm"
							style="background-color: {isEditMode && localItems[itemToEdit]?.markedForDeletion
								? 'var(--color-accent)'
								: 'var(--color-danger)'}; color: var(--color-text-primary);"
						>
							{#if isEditMode}
								{localItems[itemToEdit]?.markedForDeletion
									? tSync($languageTag, 'pages.listDetail.unmarkDelete')
									: tSync($languageTag, 'pages.listDetail.markDelete')}
							{:else}
								{tSync($languageTag, 'pages.listDetail.deleteItem')}
							{/if}
						</button>
					</div>
				</div>
			</div>
		{/if}

		{#if itemsStore.error || listsStore.error || accessStore.error || invitesStore.error}
			<div class="alert mb-8">
				<p>{itemsStore.error || listsStore.error || accessStore.error || invitesStore.error}</p>
			</div>
		{/if}

		<ShareFormModal
			bind:show={showShareForm}
			{listId}
			{access}
			isLoading={invitesStore.isLoading || accessStore.isLoading}
			onSubmit={handleShareList}
			onRevoke={handleRevokeAccess}
		/>

		<ItemFormModal bind:show={showItemForm} {listId} onAdd={handleAddItemInEditMode} />

		<div class="space-y-8">
			{#if uncompletedItems.length > 0 || (isEditMode && pendingNewItems.length > 0)}
				<div bind:this={uncompletedSectionRef} class="min-h-50 space-y-0">
					{#each uncompletedItems as item, index (item.id)}
						{#if isEditMode}
							<div
								role="button"
								tabindex="0"
								class="flex min-h-14 cursor-pointer items-center gap-3 border-b px-3 py-2"
								style="border-color: var(--color-border); background-color: {(item as any)
									.localMarkedForDeletion
									? 'var(--color-danger-light)'
									: (item as any).localNewText !== undefined
										? 'var(--color-warning-light)'
										: (item as any).localIsCompleted !== item.isCompleted
											? 'var(--color-accent-light)'
											: 'transparent'};"
								onclick={() => handleItemClick(item.id)}
								onmousedown={(e) => handleLongPressStart(item.id, e)}
								onmousemove={(e) => handleLongPressMove(e)}
								onmouseup={() => handleLongPressEnd()}
								onmouseleave={() => handleLongPressEnd()}
								ontouchstart={(e) => handleLongPressStart(item.id, e)}
								ontouchmove={(e) => handleLongPressMove(e)}
								ontouchend={() => handleLongPressEnd()}
								ontouchcancel={() => handleLongPressEnd()}
								onkeydown={(e) => {
									if (e.key === 'Enter' || e.key === ' ') {
										e.preventDefault();
										handleToggleInEditMode(item.id);
									}
								}}
							>
								<div class="flex-1">
									{#if (item as any).localNewText !== undefined}
										<!-- Show old and new text when edited -->
										<div class="text-lg">
											<div class="line-through opacity-60" style="color: var(--color-text-muted);">
												{item.text}
											</div>
											<div
												style="color: {(item as any).localMarkedForDeletion
													? 'var(--color-error)'
													: (item as any).localIsCompleted
														? 'var(--color-text-muted)'
														: 'var(--color-text-primary)'}; text-decoration: {(item as any)
													.localIsCompleted
													? 'line-through'
													: 'none'};"
											>
												{(item as any).localNewText}
											</div>
										</div>
									{:else}
										<!-- Show current text -->
										<span
											class="text-lg"
											style="color: {(item as any).localMarkedForDeletion
												? 'var(--color-error)'
												: (item as any).localIsCompleted
													? 'var(--color-text-muted)'
													: 'var(--color-text-primary)'}; text-decoration: {(item as any)
												.localMarkedForDeletion || (item as any).localIsCompleted
												? 'line-through'
												: 'none'};"
										>
											{item.text}
										</span>
									{/if}
								</div>
								<button
									type="button"
									onclick={(e) => {
										e.stopPropagation();
										handleToggleInEditMode(item.id);
									}}
									class="flex h-8 w-8 shrink-0 items-center justify-center rounded border-2"
									style="border-color: var(--color-accent); background-color: {(item as any)
										.localIsCompleted
										? 'var(--color-accent)'
										: 'transparent'};"
									disabled={(item as any).localMarkedForDeletion}
								>
									{#if (item as any).localIsCompleted}
										<svg
											class="h-5 w-5"
											fill="none"
											stroke="currentColor"
											viewBox="0 0 24 24"
											style="color: var(--color-text-primary);"
										>
											<path
												stroke-linecap="round"
												stroke-linejoin="round"
												stroke-width="2"
												d="M5 13l4 4L19 7"
											/>
										</svg>
									{/if}
								</button>
							</div>
						{:else}
							<ItemRow
								{item}
								isLoading={itemsStore.isLoading}
								isLast={index === uncompletedItems.length - 1 && pendingNewItems.length === 0}
							/>
						{/if}
					{/each}
					{#if isEditMode}
						{#each pendingNewItems as pending (pending.localId)}
							<div
								class="flex min-h-14 items-center gap-3 border-b px-3 py-2"
								style="border-color: var(--color-border); background-color: var(--color-success-light);"
							>
								<div class="flex-1">
									<span class="text-lg" style="color: var(--color-success);">{pending.text}</span>
								</div>
								<button
									type="button"
									onclick={() =>
										(pendingNewItems = pendingNewItems.filter(
											(p) => p.localId !== pending.localId
										))}
									class="flex h-8 w-8 shrink-0 items-center justify-center"
									style="color: var(--color-danger);"
									aria-label="Remove pending item"><Trash2 size={24} /></button
								>
							</div>
						{/each}
					{/if}
				</div>
			{/if}

			{#if completedItems.length > 0}
				<div
					bind:this={completedSectionRef}
					class="space-y-0"
					style="margin-top: 2rem; padding-top: 2rem; border-top: 2px solid var(--color-text-primary);"
				>
					{#each completedItems as item, index (item.id)}
						{#if isEditMode}
							<div
								role="button"
								tabindex="0"
								class="flex min-h-14 cursor-pointer items-center gap-3 border-b px-3 py-2"
								style="border-color: var(--color-border); background-color: {(item as any)
									.localMarkedForDeletion
									? 'var(--color-danger-light)'
									: (item as any).localNewText !== undefined
										? 'var(--color-warning-light)'
										: 'transparent'};"
								onclick={() => handleItemClick(item.id)}
								onmousedown={(e) => handleLongPressStart(item.id, e)}
								onmousemove={(e) => handleLongPressMove(e)}
								onmouseup={() => handleLongPressEnd()}
								onmouseleave={() => handleLongPressEnd()}
								ontouchstart={(e) => handleLongPressStart(item.id, e)}
								ontouchmove={(e) => handleLongPressMove(e)}
								ontouchend={() => handleLongPressEnd()}
								ontouchcancel={() => handleLongPressEnd()}
								onkeydown={(e) => {
									if (e.key === 'Enter' || e.key === ' ') {
										e.preventDefault();
										handleToggleInEditMode(item.id);
									}
								}}
							>
								<div class="flex-1">
									{#if (item as any).localNewText !== undefined}
										<!-- Show old and new text when edited -->
										<div class="text-lg">
											<div class="line-through opacity-60" style="color: var(--color-text-muted);">
												{item.text}
											</div>
											<div
												style="color: {(item as any).localMarkedForDeletion
													? 'var(--color-error)'
													: (item as any).localIsCompleted === false
														? 'var(--color-text-primary)'
														: (item as any).localIsCompleted
															? 'var(--color-text-muted)'
															: 'var(--color-text-primary)'}; text-decoration: {(item as any)
													.localIsCompleted
													? 'line-through'
													: 'none'};"
											>
												{(item as any).localNewText}
											</div>
										</div>
									{:else}
										<!-- Show current text -->
										<span
											class="text-lg"
											style="color: {(item as any).localMarkedForDeletion
												? 'var(--color-error)'
												: (item as any).localIsCompleted === false
													? 'var(--color-text-primary)'
													: (item as any).localIsCompleted
														? 'var(--color-text-muted)'
														: 'var(--color-text-primary)'}; text-decoration: {(item as any)
												.localMarkedForDeletion || (item as any).localIsCompleted
												? 'line-through'
												: 'none'};"
										>
											{item.text}
										</span>
									{/if}
								</div>
								<button
									type="button"
									onclick={(e) => {
										e.stopPropagation();
										handleToggleInEditMode(item.id);
									}}
									class="flex h-8 w-8 shrink-0 items-center justify-center rounded border-2"
									style="border-color: var(--color-accent); background-color: {(item as any)
										.localIsCompleted
										? 'var(--color-accent)'
										: 'transparent'};"
									disabled={(item as any).localMarkedForDeletion}
								>
									{#if (item as any).localIsCompleted}
										<svg
											class="h-5 w-5"
											fill="none"
											stroke="currentColor"
											viewBox="0 0 24 24"
											style="color: var(--color-text-primary);"
										>
											<path
												stroke-linecap="round"
												stroke-linejoin="round"
												stroke-width="2"
												d="M5 13l4 4L19 7"
											/>
										</svg>
									{/if}
								</button>
							</div>
						{:else}
							<ItemRow
								{item}
								isLoading={itemsStore.isLoading}
								isLast={index === completedItems.length - 1}
							/>
						{/if}
					{/each}
				</div>
			{/if}

			{#if items.length === 0}
				<div class="rounded py-8 text-center" style="background-color: var(--color-bg-secondary);">
					<p style="color: var(--color-text-secondary);">
						{tSync($languageTag, 'pages.listDetail.noItemsYet')}
					</p>
					<p class="mt-1 text-sm" style="color: var(--color-text-muted);">
						{tSync($languageTag, 'pages.listDetail.addItemUsingButton')}
					</p>
				</div>
			{/if}
		</div>

		<!-- Floating Add Button (edit mode only) -->
		{#if isEditMode}
			<button
				onclick={() => (showItemForm = true)}
				class="fixed right-4 bottom-8 z-40 flex h-14 w-14 items-center justify-center rounded-lg border disabled:cursor-not-allowed disabled:opacity-50"
				style="background-color: var(--color-accent-light); color: var(--color-accent); border-color: var(--color-accent);"
				title="Add item"
				disabled={itemsStore.isLoading}
			>
				<Plus size={36} />
			</button>
		{/if}
	</div>
{/if}
