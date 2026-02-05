<script lang="ts">
	import { Eye, EyeOff } from 'lucide-svelte';

	interface Props {
		value?: string;
		placeholder?: string;
		disabled?: boolean;
		label?: string;
		id?: string;
	}

	let { value = $bindable(''), placeholder, disabled = false, label, id }: Props = $props();
	let showPassword = $state(false);

	function togglePasswordVisibility(e: MouseEvent) {
		e.preventDefault();
		showPassword = !showPassword;
	}
</script>

{#if label}
	<label for={id} class="form-label">{label}</label>
{/if}

<div class="relative">
	<input
		{id}
		type={showPassword ? 'text' : 'password'}
		bind:value
		{placeholder}
		{disabled}
		class="form-input pr-10"
	/>
	<button
		type="button"
		onmousedown={togglePasswordVisibility}
		{disabled}
		class="absolute top-1/2 right-3 -translate-y-1/2 disabled:cursor-not-allowed disabled:opacity-50"
		aria-label={showPassword ? 'Hide password' : 'Show password'}
	>
		{#if showPassword}
			<EyeOff size={20} style="color: var(--color-text-muted);" />
		{:else}
			<Eye size={20} style="color: var(--color-text-muted);" />
		{/if}
	</button>
</div>
