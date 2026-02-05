<script lang="ts">
	import type { CreateInviteDto } from '$lib/api/types';

	interface Props {
		show?: boolean;
		listId: string;
		isLoading?: boolean;
		onSubmit?: (data: CreateInviteDto) => Promise<void>;
	}

	let { show = $bindable(false), listId, isLoading = false, onSubmit }: Props = $props();

	let email = $state<string>('');
	let error = $state<string | null>(null);
	let inputRef = $state<HTMLInputElement | null>(null);

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
			closeModal();
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

	<!-- Modal Content - positioned at 25% from top -->
	<div class="pointer-events-none fixed top-1/4 right-0 left-0 z-50 flex justify-center px-4">
		<form
			onsubmit={handleSubmit}
			class="pointer-events-auto w-full max-w-sm space-y-4 rounded-lg border p-4"
			style="background-color: var(--color-bg-secondary); border-color: var(--color-border);"
		>
			<h2 class="text-xl font-semibold" style="color: var(--color-text-primary);">Share List</h2>

			{#if error}
				<div class="alert">
					<p>{error}</p>
				</div>
			{/if}

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

			<div class="space-y-3">
				<button type="submit" disabled={isLoading} class="btn btn-primary">
					{isLoading ? 'Sending...' : 'Send Invite'}
				</button>
				<button type="button" onclick={closeModal} disabled={isLoading} class="btn btn-secondary">
					Cancel
				</button>
			</div>
		</form>
	</div>
{/if}
