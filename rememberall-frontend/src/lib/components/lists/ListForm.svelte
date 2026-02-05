<script lang="ts">
	import { languageTag, tSync } from '$lib/i18n/index';
	import type { CreateTodoListDto } from '$lib/api/types';

	interface Props {
		isLoading?: boolean;
		onSubmit?: (data: CreateTodoListDto) => Promise<void>;
	}

	let { isLoading = false, onSubmit }: Props = $props();

	let showModal = $state(false);
	let name = $state<string>('');
	let error = $state<string | null>(null);
	let inputRef = $state<HTMLInputElement | null>(null);

	function openModal(): void {
		showModal = true;
		error = null;
	}

	function closeModal(): void {
		showModal = false;
		name = '';
		error = null;
	}

	async function handleSubmit(e: SubmitEvent): Promise<void> {
		e.preventDefault();
		error = null;

		if (!name.trim()) {
			error = tSync($languageTag, 'form.listNameRequired');
			return;
		}

		if (name.length > 40) {
			error = tSync($languageTag, 'form.listNameTooLong');
			return;
		}

		try {
			await onSubmit?.({ name: name.trim() });
			closeModal();
		} catch (err) {
			error = err instanceof Error ? err.message : tSync($languageTag, 'form.createListFailed');
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
	>{tSync($languageTag, 'lists.createNewList')}</button
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

	<!-- Modal Content - positioned at 25% from top -->
	<div class="pointer-events-none fixed top-1/4 right-0 left-0 z-50 flex justify-center px-4">
		<form
			onsubmit={handleSubmit}
			class="pointer-events-auto w-full max-w-sm space-y-4 rounded-lg border p-4"
			style="background-color: var(--color-bg-secondary); border-color: var(--color-border);"
		>
			<h2 class="text-xl font-semibold" style="color: var(--color-text-primary);">
				{tSync($languageTag, 'lists.createNewList')}
			</h2>

			{#if error}
				<div class="alert">
					<p>{error}</p>
				</div>
			{/if}

			<div class="form-group">
				<input
					bind:this={inputRef}
					type="text"
					bind:value={name}
					disabled={isLoading}
					placeholder={tSync($languageTag, 'placeholders.listName')}
					maxlength="40"
					class="form-input"
				/>
				<p class="form-hint">{name.length}/40 {tSync($languageTag, 'items.characters')}</p>
			</div>

			<div class="space-y-3">
				<button type="submit" disabled={isLoading} class="btn btn-primary">
					{isLoading
						? tSync($languageTag, 'common.creating')
						: tSync($languageTag, 'common.createButton')}
				</button>
				<button type="button" onclick={closeModal} disabled={isLoading} class="btn btn-secondary">
					{tSync($languageTag, 'common.cancel')}
				</button>
			</div>
		</form>
	</div>
{/if}
