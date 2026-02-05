<script lang="ts">
	import { languageTag, tSync } from '$lib/i18n/index';
	import { createViewportState, calculateModalPosition } from '$lib/utils/viewport.svelte';
	import type { CreateInviteDto } from '$lib/api/types';

	interface Props {
		listId: string;
		isLoading?: boolean;
		onSubmit?: (data: CreateInviteDto) => Promise<void>;
	}

	let { listId, isLoading = false, onSubmit }: Props = $props();

	let showModal = $state(false);
	let email = $state<string>('');
	let error = $state<string | null>(null);
	let inputRef = $state<HTMLInputElement | null>(null);
	let viewportState = createViewportState();
	let modalTopPercent = $derived(calculateModalPosition(viewportState, 280).topPercent);

	function openModal(): void {
		showModal = true;
		error = null;
	}

	function closeModal(): void {
		showModal = false;
		email = '';
		error = null;
	}

	async function handleSubmit(e: SubmitEvent): Promise<void> {
		e.preventDefault();
		error = null;

		if (!email.trim()) {
			error = tSync($languageTag, 'form.validEmailRequired');
			return;
		}

		if (!email.includes('@')) {
			error = tSync($languageTag, 'form.validEmailRequired');
			return;
		}

		try {
			await onSubmit?.({ inviteRecieverEmail: email.trim(), listId });
			closeModal();
		} catch (err) {
			error = err instanceof Error ? err.message : tSync($languageTag, 'form.sendInviteFailed');
		}
	}

	// Auto-focus input when modal opens
	$effect(() => {
		if (showModal && inputRef) {
			inputRef.focus();
		}
	});
</script>

<button onclick={openModal} class="btn btn-primary"
	>{tSync($languageTag, 'lists.shareThisList')}</button
>

{#if showModal}
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
		<form
			onsubmit={handleSubmit}
			class="pointer-events-auto w-full max-w-sm space-y-4 rounded-lg border p-4"
			style="background-color: var(--color-bg-secondary); border-color: var(--color-border);"
		>
			<div class="flex items-center justify-between">
				<h2 class="text-xl font-semibold" style="color: var(--color-text-primary);">
					{tSync($languageTag, 'lists.shareThisList')}
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
				<div class="alert">
					<p>{error}</p>
				</div>
			{/if}

			<div class="form-group">
				<input
					bind:this={inputRef}
					id="email"
					type="email"
					bind:value={email}
					disabled={isLoading}
					placeholder={tSync($languageTag, 'placeholders.userEmail')}
					class="form-input"
				/>
			</div>

			<div class="space-y-3">
				<button type="submit" disabled={isLoading} class="btn btn-primary">
					{isLoading
						? tSync($languageTag, 'buttons.sending')
						: tSync($languageTag, 'buttons.sendInvite')}
				</button>
				<button type="button" onclick={closeModal} disabled={isLoading} class="btn btn-secondary">
					{tSync($languageTag, 'common.cancel')}
				</button>
			</div>
		</form>
	</div>
{/if}
