<script lang="ts">
	import { authStore } from '$lib/stores/auth.svelte';
	import { goto } from '$app/navigation';
	import { languageTag, tSync } from '$lib/i18n/index';
	import Logo from './Logo.svelte';
	import LanguageSwitcher from './LanguageSwitcher.svelte';

	interface Props {
		isAuthenticated?: boolean;
	}

	let { isAuthenticated = false }: Props = $props();
	let menuOpen = $state(false);

	async function handleLogout(): Promise<void> {
		try {
			await authStore.logout();
			await goto('/login');
		} catch {
			// Error already handled in store
		}
	}

	function toggleMenu(): void {
		menuOpen = !menuOpen;
	}

	function closeMenu(): void {
		menuOpen = false;
	}
</script>

<nav
	style="background-color: var(--color-bg-secondary); border-bottom: 1px solid var(--color-border);"
	class="fixed top-0 right-0 left-0 z-50 flex h-16 items-center justify-between px-4"
>
	<a href="/" class="inline-block"><Logo size="md" outline={false} /></a>

	{#if isAuthenticated}
		<button
			onclick={toggleMenu}
			class="p-2 text-2xl leading-none"
			style="color: var(--color-text-primary);"
			aria-label={tSync($languageTag, 'common.menu')}
		>
			☰
		</button>

		{#if menuOpen}
			<!-- Menu Overlay -->
			<div
				style="background-color: var(--color-overlay-dark);"
				class="fixed inset-0 z-40"
				role="button"
				tabindex="0"
				onclick={closeMenu}
				onkeydown={(e) => {
					if (e.key === 'Escape') closeMenu();
				}}
				aria-label="Close menu"
			></div>

			<!-- Menu Dropdown -->
			<div
				style="background-color: var(--color-bg-secondary); border-color: var(--color-border);"
				class="absolute top-16 right-0 z-40 w-48 border border-t-0"
			>
				{#if authStore.currentUser}
					<div
						style="color: var(--color-text-secondary);"
						class="border-b px-4 py-3 text-sm"
						style:border-bottom-color="var(--color-border)"
					>
						{authStore.currentUser.name}
					</div>
				{/if}

				<div class="border-b px-4 py-3" style:border-bottom-color="var(--color-border)">
					<LanguageSwitcher variant="vertical" />
				</div>

				<button
					onclick={() => {
						handleLogout();
						closeMenu();
					}}
					disabled={authStore.isLoading}
					class="w-full border-t px-4 py-3 text-left text-sm font-medium disabled:cursor-not-allowed disabled:opacity-50"
					style="color: var(--color-danger); border-top-color: var(--color-border);"
				>
					{authStore.isLoading
						? tSync($languageTag, 'common.loggingOut')
						: tSync($languageTag, 'common.logout')}
				</button>
			</div>
		{/if}
	{/if}
</nav>

<!-- Spacer to prevent content from hiding under fixed navbar -->
<div class="h-16"></div>
