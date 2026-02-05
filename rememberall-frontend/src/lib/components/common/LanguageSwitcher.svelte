<script lang="ts">
	import { languageTag, setLanguage, tSync } from '$lib/i18n/index';
	import type { Language } from '$lib/i18n/index';

	interface Props {
		variant?: 'horizontal' | 'vertical';
	}

	let { variant = 'horizontal' }: Props = $props();

	const languages: { code: Language; label: string }[] = [
		{ code: 'en', label: 'English' },
		{ code: 'da', label: 'Dansk' }
	];
</script>

<div
	class="flex items-center"
	class:gap-2={variant === 'horizontal'}
	class:flex-col={variant === 'vertical'}
	class:gap-1={variant === 'vertical'}
>
	<div
		class="flex rounded-lg p-1"
		class:gap-1={variant === 'horizontal'}
		class:flex-col={variant === 'vertical'}
		class:w-full={variant === 'vertical'}
		style="background-color: var(--color-bg-secondary); border: 1px solid var(--color-border);"
	>
		{#each languages as lang (lang.code)}
			<button
				onclick={() => setLanguage(lang.code)}
				class="rounded px-3 py-1 text-sm font-medium transition-colors"
				class:w-full={variant === 'vertical'}
				class:active={$languageTag === lang.code}
				style="background-color: {$languageTag === lang.code
					? 'var(--color-accent)'
					: 'transparent'}; color: {$languageTag === lang.code
					? 'white'
					: 'var(--color-text-primary)'}"
			>
				{lang.label}
			</button>
		{/each}
	</div>
</div>

<style>
	:global(.active) {
		font-weight: 700;
	}
</style>
