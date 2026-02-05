<script lang="ts">
	import { languageTag, tSync } from '$lib/i18n/index';
	import { createViewportState, calculateModalPosition } from '$lib/utils/viewport.svelte';
	import AccessRow from './AccessRow.svelte';
	import type { CreateInviteDto, ListAccessDto } from '$lib/api/types';

	interface Props {
		show?: boolean;
		listId: string;
		isLoading?: boolean;
		access?: ListAccessDto[];
		onSubmit?: (data: CreateInviteDto) => Promise<void>;
		onRevoke?: (accessId: string) => void;
	}

	let {
		show = $bindable(false),
		listId,
		isLoading = false,
		access = [],
		onSubmit,
		onRevoke
	}: Props = $props();

	let email = $state<string>('');
	let error = $state<string | null>(null);
	let inputRef = $state<HTMLInputElement | null>(null);
	let lang = $derived($languageTag);
	let viewportState = createViewportState();
	let modalTopPercent = $derived(calculateModalPosition(viewportState, 400).topPercent);

	function closeModal(): void {
		show = false;
		email = '';
		error = null;
	}

	async function handleSubmit(e: SubmitEvent): Promise<void> {
		e.preventDefault();
		error = null;

		if (!email.trim()) {
			error = 'Email is required';
			return;
		}

		if (!email.includes('@')) {
			error = 'Please enter a valid email address';
			return;
		}

		try {
			await onSubmit?.({ inviteRecieverEmail: email.trim(), listId });
			email = '';
		} catch (err) {
			error = err instanceof Error ? err.message : 'Failed to send invite';
		}
	}

	// Auto-focus input when modal opens
	$effect(() => {
		if (show && inputRef) {
			inputRef.focus();
		}
	});
</script>

{#if show}
	<!-- Modal Overlay -->
	<div
		style="background-color: var(--color-overlay-dark);"
		class="fixed inset-0 z-40"
		role="button"
		tabindex="0"
		onclick={closeModal}
		onkeydown={(e) => {
			if (e.key === 'Escape') closeModal();
		}}
		aria-label="Close modal"
	></div>

	<!-- Modal Content - positioned to avoid keyboard -->
	<div
		class="pointer-events-none fixed right-0 left-0 z-50 flex justify-center px-4"
		style="top: {modalTopPercent}%"
	>
		<div
			class="pointer-events-auto max-h-[70vh] w-full max-w-sm overflow-y-auto rounded-lg border p-4"
			style="background-color: var(--color-bg-secondary); border-color: var(--color-border);"
		>
			<div class="mb-4 flex items-center justify-between">
				<h2 class="text-xl font-semibold" style="color: var(--color-text-primary);">
					{tSync(lang, 'pages.listDetail.shareList')}
				</h2>
				<button
					type="button"
					onclick={closeModal}
					class="text-2xl leading-none font-bold"
					style="color: var(--color-text-secondary);"
					aria-label="Close modal"
				>
					×
				</button>
			</div>

			{#if error}
				<div class="alert mb-4">
					<p>{error}</p>
				</div>
			{/if}

			<!-- Share Form -->
			<form onsubmit={handleSubmit} class="mb-6 space-y-4">
				<div class="form-group">
					<label for="email" class="form-label">Email Address</label>
					<input
						bind:this={inputRef}
						id="email"
						type="email"
						bind:value={email}
						disabled={isLoading}
						placeholder="user@example.com"
						class="form-input"
					/>
					<p class="form-hint">Enter the email address of the person you want to share with</p>
				</div>

				<button type="submit" disabled={isLoading} class="btn btn-primary">
					{isLoading ? 'Sending...' : 'Send Invite'}
				</button>
			</form>

			<!-- Access List -->
			{#if access.length > 0}
				<div style="border-top: 1px solid var(--color-border); padding-top: 1rem;">
					<p class="mb-3 font-semibold" style="color: var(--color-text-primary);">
						{tSync(lang, 'pages.listDetail.sharedWith_text')} ({access.length})
					</p>
					<div class="space-y-2">
						{#each access as item (item.id)}
							<AccessRow access={item} {isLoading} {onRevoke} />
						{/each}
					</div>
				</div>
			{/if}

			<button
				type="button"
				onclick={closeModal}
				disabled={isLoading}
				class="btn btn-secondary mt-4 w-full"
			>
				{tSync(lang, 'pages.listDetail.close')}
			</button>
		</div>
	</div>
{/if}
