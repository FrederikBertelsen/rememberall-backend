<script lang="ts">
	import { goto } from '$app/navigation';
	import { page } from '$app/stores';
	import { languageTag, tSync } from '$lib/i18n/index';
	import Logo from './Logo.svelte';
	import { Settings } from 'lucide-svelte';

	interface Props {
		isAuthenticated?: boolean;
	}

	let { isAuthenticated = false }: Props = $props();

	async function goToSettings(): Promise<void> {
		await goto('/settings');
	}

	const isHomePage = $derived($page.url.pathname === '/');
	let lang = $derived($languageTag);
</script>

<nav
	style="background-color: var(--color-bg-secondary); border-bottom: 1px solid var(--color-border);"
	class="fixed top-0 right-0 left-0 z-50 flex h-16 items-center justify-between px-4"
>
	<a href="/" class="inline-block"><Logo size="md" outline={false} /></a>

	{#if isAuthenticated && isHomePage}
		<button
			onclick={goToSettings}
			class="flex h-10 w-10 items-center justify-center rounded-lg transition-colors"
			style="color: var(--color-text-primary);"
			aria-label={tSync(lang, 'pages.settings.title')}
			title={tSync(lang, 'pages.settings.title')}
		>
			<Settings size={24} />
		</button>
	{/if}
</nav>

<!-- Spacer to prevent content from hiding under fixed navbar -->
<div class="h-16"></div>
