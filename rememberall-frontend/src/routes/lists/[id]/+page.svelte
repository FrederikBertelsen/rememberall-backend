<script lang="ts">
	import { browser } from '$app/environment';
	import { page } from '$app/stores';
	import { goto } from '$app/navigation';
	import { authStore } from '$lib/stores/auth.svelte';
	import { listsStore } from '$lib/stores/lists.svelte';
	import { itemsStore } from '$lib/stores/items.svelte';
	import { accessStore } from '$lib/stores/access.svelte';
	import { invitesStore } from '$lib/stores/invites.svelte';
	import { languageTag, tSync } from '$lib/i18n/index';
	import ItemRow from '$lib/components/items/ItemRow.svelte';
	import AccessRow from '$lib/components/lists/AccessRow.svelte';
	import ItemFormModal from '$lib/components/items/ItemFormModal.svelte';
	import ShareFormModal from '$lib/components/lists/ShareFormModal.svelte';
	import type { CreateInviteDto } from '$lib/api/types';

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

	// Separate and sort items
	let completedItems = $derived(
		items.filter((i) => i.isCompleted).sort((a, b) => b.completionCount - a.completionCount)
	);
	let uncompletedItems = $derived(
		items.filter((i) => !i.isCompleted).sort((a, b) => b.completionCount - a.completionCount)
	);

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

	function handleLongPressItem(itemId: string): void {
		const item = items.find((i) => i.id === itemId);
		if (item) {
			itemToEdit = itemId;
			editText = item.text;
			showEditDeleteItem = true;
		}
	}

	async function handleUpdateItem(): Promise<void> {
		if (!listId || !itemToEdit || !editText.trim()) return;
		try {
			await itemsStore.updateItem({ id: itemToEdit, text: editText }, listId);
			showEditDeleteItem = false;
			itemToEdit = null;
			editText = '';
		} catch (err) {
			// Error already handled in store
		}
	}

	async function handleDeleteItemConfirmed(): Promise<void> {
		if (!listId || !itemToEdit) return;
		await itemsStore.deleteItem(itemToEdit, listId);
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
				<a href="/" class="text-sm font-medium" style="color: var(--color-accent);"
					>← {tSync($languageTag, 'pages.listDetail.back')}</a
				>
				<div class="flex shrink-0 gap-2">
					{#if isOwner}
						<button
							onclick={() => (showShareForm = true)}
							class="rounded px-2 py-1 text-sm disabled:cursor-not-allowed disabled:opacity-50"
							style="background-color: rgba(20, 184, 166, 0.2); color: var(--color-accent);"
							title={tSync($languageTag, 'pages.listDetail.shareList')}
						>
							+👥
						</button>
						<button
							onclick={() => (showDeleteListConfirm = true)}
							class="rounded px-2 py-1 text-sm disabled:cursor-not-allowed disabled:opacity-50"
							style="background-color: var(--color-error-light); color: var(--color-danger);"
							title={tSync($languageTag, 'pages.listDetail.deleteList')}
						>
							🗑️
						</button>
					{:else}
						<button
							onclick={() => (showLeaveListConfirm = true)}
							class="rounded px-3 py-1 text-xs font-medium disabled:cursor-not-allowed disabled:opacity-50"
							style="background-color: var(--color-error-light); color: var(--color-danger);"
							title={tSync($languageTag, 'pages.listDetail.leaveList')}
						>
							{tSync($languageTag, 'pages.listDetail.leave')}
						</button>
					{/if}
				</div>
			</div>

			<h1 class="mb-1 text-3xl font-bold">{list.name}</h1>
			<div class="flex flex-wrap items-center gap-2">
				{#if isOwner && access.length > 0}
					<button
						onclick={() => (showAccessModal = true)}
						class="rounded px-2 py-1 text-xs"
						style="background-color: rgba(20, 184, 166, 0.15); color: var(--color-accent);"
						title={tSync($languageTag, 'pages.listDetail.viewWhoHasAccess')}
					>
						👥 {tSync($languageTag, 'pages.listDetail.sharedWith')}
						{access.length}
					</button>
				{/if}
			</div>
		</div>

		{#if showLeaveListConfirm}
			<div
				role="dialog"
				tabindex="-1"
				class="fixed inset-0 z-50 flex items-center justify-center px-4"
				style="background-color: var(--color-overlay-dark);"
				onmousedown={(e) => e.currentTarget === e.target && (showLeaveListConfirm = false)}
				ontouchstart={(e) => e.currentTarget === e.target && (showLeaveListConfirm = false)}
			>
				<div
					class="max-w-xs rounded-lg border p-4"
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
			<div
				role="dialog"
				tabindex="-1"
				class="fixed inset-0 z-50 flex items-center justify-center px-4"
				style="background-color: var(--color-overlay-dark);"
				onmousedown={(e) => e.currentTarget === e.target && (showAccessModal = false)}
				ontouchstart={(e) => e.currentTarget === e.target && (showAccessModal = false)}
			>
				<div
					class="max-h-[60vh] w-full overflow-y-auto rounded-lg border p-4"
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
			<div
				role="dialog"
				tabindex="-1"
				class="fixed inset-0 z-50 flex items-center justify-center px-4"
				style="background-color: var(--color-overlay-dark);"
				onmousedown={(e) => e.currentTarget === e.target && (showDeleteListConfirm = false)}
				ontouchstart={(e) => e.currentTarget === e.target && (showDeleteListConfirm = false)}
			>
				<div
					class="max-w-xs rounded-lg border p-4"
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
			<div
				role="dialog"
				tabindex="-1"
				class="fixed inset-0 z-50 flex items-center justify-center px-4"
				style="background-color: var(--color-overlay-dark);"
				onmousedown={(e) => e.currentTarget === e.target && (showEditDeleteItem = false)}
				ontouchstart={(e) => e.currentTarget === e.target && (showEditDeleteItem = false)}
			>
				<div
					class="max-w-xs rounded-lg border p-4"
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
							disabled={itemsStore.isLoading}
						>
							{tSync($languageTag, 'modals.cancel')}
						</button>
						<button
							onclick={() => handleUpdateItem()}
							disabled={itemsStore.isLoading}
							class="btn flex-1 py-2 text-sm"
							style="background-color: var(--color-accent); color: var(--color-text-primary);"
						>
							{tSync($languageTag, 'pages.listDetail.save')}
						</button>
						<button
							onclick={() => handleDeleteItemConfirmed()}
							disabled={itemsStore.isLoading}
							class="btn flex-1 py-2 text-sm"
							style="background-color: var(--color-danger); color: var(--color-text-primary);"
						>
							{tSync($languageTag, 'pages.listDetail.deleteItem')}
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

		<ShareFormModal bind:show={showShareForm} {listId} onSubmit={handleShareList} />

		<div class="space-y-8">
			{#if uncompletedItems.length > 0}
				<div class="space-y-0">
					{#each uncompletedItems as item, index (item.id)}
						<ItemRow
							{item}
							isLoading={itemsStore.isLoading}
							isLast={index === uncompletedItems.length - 1}
							onToggle={handleToggleItem}
							onLongPress={handleLongPressItem}
						/>
					{/each}
				</div>
			{/if}

			{#if completedItems.length > 0}
				<div
					class="space-y-0"
					style="margin-top: 2rem; padding-top: 2rem; border-top: 2px solid var(--color-text-primary);"
				>
					{#each completedItems as item, index (item.id)}
						<ItemRow
							{item}
							isLoading={itemsStore.isLoading}
							isLast={index === completedItems.length - 1}
							onToggle={handleToggleItem}
							onLongPress={handleLongPressItem}
						/>
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

		<!-- Floating Add Button -->
		<button
			onclick={() => (showItemForm = true)}
			class="fixed right-4 bottom-8 z-40 flex h-12 w-12 items-center justify-center rounded-lg text-xl font-bold disabled:cursor-not-allowed disabled:opacity-50"
			style="background-color: var(--color-accent); color: var(--color-text-primary);"
			title="Add item"
			disabled={itemsStore.isLoading}
		>
			+
		</button>

		<!-- Item Form Modal -->
		<ItemFormModal bind:show={showItemForm} {listId} />
	</div>
{/if}
