<script lang="ts">
	import { goto } from '$app/navigation';
	import { ArrowLeft } from 'lucide-svelte';
	import { authStore } from '$lib/stores/auth.svelte';
	import { languageTag, tSync } from '$lib/i18n/index';
	import LanguageSwitcher from '$lib/components/common/LanguageSwitcher.svelte';
	import PWAInstallButton from '$lib/components/common/PWAInstallButton.svelte';

	let showLogoutConfirm = $state(false);

	async function handleLogout(): Promise<void> {
		try {
			await authStore.logout();
			await goto('/login');
		} catch {
			// Error already handled in store
		}
	}

	let lang = $derived($languageTag);
</script>

<div class="page-container-scroll flex flex-col">
	<!-- Back Button -->
	<a
		href="/"
		class="text-md mb-4 inline-flex items-center gap-2 font-medium"
		style="color: var(--color-accent);"
	>
		<ArrowLeft size={20} />
		<span>{tSync(lang, 'pages.settings.back')}</span>
	</a>

	<!-- Page Title -->
	<h1 class="mb-6 text-3xl font-bold">{tSync(lang, 'pages.settings.title')}</h1>

	<div class="space-y-4">
		<!-- Profile Section -->
		{#if authStore.currentUser}
			<div
				class="space-y-2 rounded-lg border p-4"
				style="background-color: var(--color-bg-secondary); border-color: var(--color-border);"
			>
				<div>
					<p class="text-xs font-medium" style="color: var(--color-text-muted);">
						{tSync(lang, 'pages.settings.userName')}
					</p>
					<p class="mt-1" style="color: var(--color-text-primary);">
						{authStore.currentUser.name}
					</p>
				</div>

				<div style="border-top: 1px solid var(--color-border);" class="pt-2">
					<p class="text-xs font-medium" style="color: var(--color-text-muted);">
						{tSync(lang, 'pages.settings.userEmail')}
					</p>
					<p class="mt-1 text-sm" style="color: var(--color-text-primary);">
						{authStore.currentUser.email}
					</p>
				</div>
			</div>
		{/if}

		<!-- Language Section -->
		<div
			class="flex items-center justify-between rounded-lg border p-4"
			style="background-color: var(--color-bg-secondary); border-color: var(--color-border);"
		>
			<p class="text-sm font-medium" style="color: var(--color-text-muted);">
				{tSync(lang, 'pages.settings.language')}
			</p>
			<LanguageSwitcher variant="horizontal" />
		</div>

		<!-- Install App Section -->
		<div
			class="rounded-lg border p-4"
			style="background-color: var(--color-bg-secondary); border-color: var(--color-border);"
		>
			<PWAInstallButton hideWhenInstalled={false} />
		</div>
	</div>
</div>

<!-- Logout Button - Fixed to bottom -->
<button
	onclick={() => (showLogoutConfirm = true)}
	disabled={authStore.isLoading}
	class="fixed right-4 bottom-4 left-4 z-40 rounded-lg border px-4 py-3 font-medium disabled:cursor-not-allowed disabled:opacity-50"
	style="background-color: var(--color-danger); color: var(--color-text-primary); border-color: var(--color-error-light);"
>
	{authStore.isLoading ? tSync(lang, 'common.loggingOut') : tSync(lang, 'pages.settings.logout')}
</button>

<!-- Logout Confirmation Modal -->
{#if showLogoutConfirm}
	<div
		role="dialog"
		tabindex="-1"
		class="fixed inset-0 z-50 flex items-center justify-center px-4"
		style="background-color: var(--color-overlay-dark);"
		onmousedown={(e) => e.currentTarget === e.target && (showLogoutConfirm = false)}
		ontouchstart={(e) => e.currentTarget === e.target && (showLogoutConfirm = false)}
	>
		<div
			class="max-w-xs rounded-lg border p-4"
			style="background-color: var(--color-bg-secondary); border-color: var(--color-border);"
		>
			<p class="mb-2 font-semibold" style="color: var(--color-text-primary);">
				{tSync(lang, 'pages.settings.logout')}
			</p>
			<p class="mb-4 text-sm" style="color: var(--color-text-secondary);">
				{tSync(lang, 'pages.settings.logoutConfirm')}
			</p>
			<div class="flex gap-2">
				<button
					onclick={() => (showLogoutConfirm = false)}
					disabled={authStore.isLoading}
					class="flex-1 rounded px-3 py-2 text-sm font-medium disabled:cursor-not-allowed disabled:opacity-50"
					style="background-color: var(--color-border); color: var(--color-text-primary);"
				>
					{tSync(lang, 'common.cancel')}
				</button>
				<button
					onclick={() => handleLogout()}
					disabled={authStore.isLoading}
					class="flex-1 rounded px-3 py-2 text-sm font-medium disabled:cursor-not-allowed disabled:opacity-50"
					style="background-color: var(--color-danger); color: var(--color-text-primary);"
				>
					{authStore.isLoading
						? tSync(lang, 'common.loggingOut')
						: tSync(lang, 'pages.settings.logout')}
				</button>
			</div>
		</div>
	</div>
{/if}
