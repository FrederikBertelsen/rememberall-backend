<script lang="ts">
	import { browser } from '$app/environment';
	import { authStore } from '$lib/stores/auth.svelte';
	import { listsStore } from '$lib/stores/lists.svelte';
	import { invitesStore } from '$lib/stores/invites.svelte';
	import { languageTag, tSync } from '$lib/i18n/index';
	import ListCard from '$lib/components/lists/ListCard.svelte';
	import ListForm from '$lib/components/lists/ListForm.svelte';
	import type { CreateTodoListDto } from '$lib/api/types';

	async function handleCreateList(data: CreateTodoListDto): Promise<void> {
		await listsStore.createList(data);
	}

	// Derived state for owned and shared lists
	const ownedLists = $derived(
		listsStore.lists.filter((list) => list.ownerId === authStore.currentUser?.id)
	);

	const sharedLists = $derived(
		listsStore.lists.filter((list) => list.ownerId !== authStore.currentUser?.id)
	);

	// Load lists and invites on mount
	$effect(() => {
		if (!browser) return;

		listsStore.fetchLists().catch(() => {
			// Error already handled in store
		});
		invitesStore.fetchSentInvites().catch(() => {});
		invitesStore.fetchReceivedInvites().catch(() => {});
	});

	// Determine if there are any active invites
	const hasInvites = $derived(
		invitesStore.sentInvites.length > 0 || invitesStore.receivedInvites.length > 0
	);

	// Handle accept/decline/revoke
	async function handleAcceptInvite(inviteId: string): Promise<void> {
		try {
			await invitesStore.acceptInvite(inviteId);
			// Refresh page to show newly added shared list
			window.location.href = window.location.href;
		} catch (error) {
			console.error('Error accepting invite:', error);
		}
	}

	async function handleDeclineInvite(inviteId: string): Promise<void> {
		await invitesStore.declineInvite(inviteId);
	}

	async function handleRevokeInvite(inviteId: string): Promise<void> {
		await invitesStore.revokeInvite(inviteId);
	}

	let lang = $derived($languageTag);
</script>

<div
	class="flex min-h-[calc(100vh-4rem)] flex-col overflow-hidden"
	style="background-color: var(--color-bg-primary);"
>
	<!-- Scrollable lists area -->
	<div class="flex-1 overflow-y-auto px-4">
		<div class="space-y-6 py-4">
			{#if hasInvites}
				<div class="card-section">
					<h2 class="mb-4 text-xl font-semibold" style="color: var(--color-text-primary);">
						{tSync(lang, 'dashboard.invitations')}
					</h2>
					<div class="space-y-2">
						{#each invitesStore.receivedInvites as invite (invite.id)}
							<div
								class="flex items-center justify-between rounded p-3"
								style="background-color: var(--color-bg-secondary);"
							>
								<div class="min-w-0 flex-1">
									<p class="truncate text-sm" style="color: var(--color-text-secondary);">
										<span style="color: var(--color-text-primary);">{invite.inviteSenderName}</span>
										{tSync(lang, 'invites.invitedYouTo')}
										<span style="color: var(--color-text-primary);">{invite.listName}</span>
									</p>
								</div>
								<div class="ml-2 flex shrink-0 gap-2">
									<button
										onclick={() => handleAcceptInvite(invite.id)}
										disabled={invitesStore.isLoading}
										class="rounded px-2 py-1 text-xs font-medium disabled:cursor-not-allowed disabled:opacity-50"
										style="background-color: var(--color-accent); color: var(--color-text-primary);"
									>
										{tSync(lang, 'buttons.accept')}
									</button>
									<button
										onclick={() => handleDeclineInvite(invite.id)}
										disabled={invitesStore.isLoading}
										class="rounded px-2 py-1 text-xs disabled:cursor-not-allowed disabled:opacity-50"
										style="background-color: var(--color-bg-primary); border: 1px solid var(--color-border); color: var(--color-text-muted);"
									>
										{tSync(lang, 'buttons.decline')}
									</button>
								</div>
							</div>
						{/each}
						{#each invitesStore.sentInvites as invite (invite.id)}
							<div
								class="flex items-center justify-between rounded p-3"
								style="background-color: var(--color-bg-secondary);"
							>
								<div class="min-w-0 flex-1">
									<p class="truncate text-sm" style="color: var(--color-text-secondary);">
										{tSync(lang, 'invites.youInvited')}
										<span style="color: var(--color-text-primary);"
											>{invite.inviteRecieverName}</span
										>
										{tSync(lang, 'invites.to')}
										<span style="color: var(--color-text-primary);">{invite.listName}</span>
									</p>
								</div>
								<button
									onclick={() => handleRevokeInvite(invite.id)}
									disabled={invitesStore.isLoading}
									class="ml-2 shrink-0 rounded px-2 py-1 text-xs disabled:cursor-not-allowed disabled:opacity-50"
									style="background-color: var(--color-bg-primary); border: 1px solid var(--color-border); color: var(--color-text-muted);"
								>
									{tSync(lang, 'buttons.revoke')}
								</button>
							</div>
						{/each}
					</div>
				</div>
			{/if}

			{#if listsStore.isLoading && listsStore.lists.length === 0}
				<div class="py-8 text-center" style="color: var(--color-text-secondary);">
					<p class="text-sm">{tSync(lang, 'pages.dashboard.loadingLists')}</p>
				</div>
			{/if}

			<div class="card-section">
				<h2 class="mb-4 text-xl font-semibold" style="color: var(--color-text-primary);">
					{tSync(lang, 'pages.dashboard.yourLists')}
				</h2>
				<div class="mb-4">
					<ListForm isLoading={listsStore.isLoading} onSubmit={handleCreateList} />
				</div>
				{#if ownedLists.length > 0}
					<div class="space-y-3">
						{#each ownedLists as list (list.id)}
							<ListCard {list} />
						{/each}
					</div>
				{:else}
					<div
						class="rounded py-6 text-center"
						style="background-color: var(--color-bg-secondary);"
					>
						<p class="text-sm" style="color: var(--color-text-secondary);">
							{tSync(lang, 'pages.dashboard.noListsYet')}
						</p>
						<p class="mt-1 text-xs" style="color: var(--color-text-muted);">
							{tSync(lang, 'pages.dashboard.createFirstList')}
						</p>
					</div>
				{/if}
			</div>

			<div class="card-section">
				<h2 class="mb-4 text-xl font-semibold" style="color: var(--color-text-primary);">
					{tSync(lang, 'pages.dashboard.sharedWithYou')}
				</h2>
				{#if sharedLists.length > 0}
					<div class="space-y-3">
						{#each sharedLists as list (list.id)}
							<ListCard {list} />
						{/each}
					</div>
				{:else}
					<div
						class="rounded py-6 text-center"
						style="background-color: var(--color-bg-secondary);"
					>
						<p class="text-sm" style="color: var(--color-text-secondary);">
							{tSync(lang, 'pages.dashboard.noSharedLists')}
						</p>
						<p class="mt-1 text-xs" style="color: var(--color-text-muted);">
							{tSync(lang, 'pages.dashboard.acceptInvite')}
						</p>
					</div>
				{/if}
			</div>
		</div>
	</div>
</div>
