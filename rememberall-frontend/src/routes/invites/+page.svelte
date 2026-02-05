<script lang="ts">
	import { invitesStore } from '$lib/stores/invites.svelte';
	import InviteCard from '$lib/components/lists/InviteCard.svelte';
	import type { CreateInviteDto } from '$lib/api/types';

	let sentTab = $state(true);

	async function handleSendInvite(data: CreateInviteDto): Promise<void> {
		await invitesStore.sendInvite(data);
	}

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

	// Load invites on mount
	$effect(() => {
		invitesStore.fetchSentInvites().catch(() => {});
		invitesStore.fetchReceivedInvites().catch(() => {});
	});
</script>

<div class="page-container-scroll">
	<h1 class="mb-2 text-3xl font-bold">Invitations</h1>
	<p class="mb-8 text-sm" style="color: var(--color-text-secondary);">
		Manage sent and received list sharing invitations
	</p>

	{#if invitesStore.error}
		<div class="alert mb-8">
			<p>{invitesStore.error}</p>
		</div>
	{/if}

	<div class="mb-8" style="border-bottom: 1px solid var(--color-border);">
		<div class="flex gap-4">
			<button
				onclick={() => (sentTab = true)}
				class="px-4 py-2 font-medium transition"
				style="color: {sentTab
					? 'var(--color-accent)'
					: 'var(--color-text-secondary)'}; border-bottom: {sentTab
					? '2px solid var(--color-accent)'
					: 'none'};"
			>
				Sent ({invitesStore.sentInvites.length})
			</button>
			<button
				onclick={() => (sentTab = false)}
				class="px-4 py-2 font-medium transition"
				style="color: {!sentTab
					? 'var(--color-accent)'
					: 'var(--color-text-secondary)'}; border-bottom: {!sentTab
					? '2px solid var(--color-accent)'
					: 'none'};"
			>
				Received ({invitesStore.receivedInvites.length})
			</button>
		</div>
	</div>

	{#if sentTab}
		<div class="space-y-6">
			<div class="text-sm" style="color: var(--color-text-secondary);">
				<p>Track invitations you've sent to share your lists with other users.</p>
			</div>

			{#if invitesStore.isLoading && invitesStore.sentInvites.length === 0}
				<div class="py-8 text-center" style="color: var(--color-text-secondary);">
					<p class="text-sm">Loading sent invitations...</p>
				</div>
			{:else if invitesStore.sentInvites.length === 0}
				<div class="rounded py-8 text-center" style="background-color: var(--color-bg-secondary);">
					<p style="color: var(--color-text-secondary);">No sent invitations</p>
					<p class="mt-1 text-sm" style="color: var(--color-text-muted);">
						Go to a list and share it to send invitations
					</p>
				</div>
			{:else}
				<div class="space-y-3">
					{#each invitesStore.sentInvites as invite (invite.id)}
						<div
							class="flex items-start justify-between rounded-lg border p-4"
							style="background-color: var(--color-bg-secondary); border-color: var(--color-border); border-left: 4px solid var(--color-accent);"
						>
							<div>
								<p class="text-sm" style="color: var(--color-text-secondary);">
									Invited <span class="font-semibold" style="color: var(--color-text-primary);"
										>{invite.inviteRecieverName}</span
									> to
								</p>
								<p class="mt-1 text-lg font-semibold" style="color: var(--color-text-primary);">
									{invite.listName}
								</p>
							</div>
							<button
								onclick={() => handleRevokeInvite(invite.id)}
								disabled={invitesStore.isLoading}
								class="ml-4 shrink-0 rounded px-3 py-1 text-sm disabled:cursor-not-allowed disabled:opacity-50"
								style="background-color: var(--color-error-light); color: var(--color-danger);"
							>
								Revoke
							</button>
						</div>
					{/each}
				</div>
			{/if}
		</div>
	{:else}
		<div class="space-y-6">
			<div class="text-sm" style="color: var(--color-text-secondary);">
				<p>Accept or decline invitations to collaborate on shared lists.</p>
			</div>

			{#if invitesStore.isLoading && invitesStore.receivedInvites.length === 0}
				<div class="py-8 text-center" style="color: var(--color-text-secondary);">
					<p class="text-sm">Loading received invitations...</p>
				</div>
			{:else if invitesStore.receivedInvites.length === 0}
				<div class="rounded py-8 text-center" style="background-color: var(--color-bg-secondary);">
					<p style="color: var(--color-text-secondary);">No pending invitations</p>
					<p class="mt-1 text-sm" style="color: var(--color-text-muted);">
						When someone invites you to a list, it will appear here
					</p>
				</div>
			{:else}
				<div class="space-y-3">
					{#each invitesStore.receivedInvites as invite (invite.id)}
						<InviteCard
							{invite}
							isLoading={invitesStore.isLoading}
							onAccept={handleAcceptInvite}
							onDecline={handleDeclineInvite}
						/>
					{/each}
				</div>
			{/if}
		</div>
	{/if}
</div>
