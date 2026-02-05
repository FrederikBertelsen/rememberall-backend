<script lang="ts">
	import { onMount } from 'svelte';
	import { languageTag, tSync } from '$lib/i18n/index';
	import { Smartphone, Hourglass, SquareCheckBig } from 'lucide-svelte';

	interface Props {
		text?: string;
		hideWhenInstalled?: boolean;
	}

	const { text = 'buttons.installApp', hideWhenInstalled = true }: Props = $props();

	let deferredPrompt = $state<any>(null);
	let showInstallButton = $state(true);
	let isInstalling = $state(false);
	let isAlreadyInstalled = $state(false);
	let isPWASupported = $state(false);
	let lang = $derived($languageTag);

	onMount(() => {
		// Check if already installed
		if (window.matchMedia('(display-mode: standalone)').matches) {
			isAlreadyInstalled = true;
			if (hideWhenInstalled) {
				showInstallButton = false;
				return;
			}
		}

		// Use the global deferred prompt captured in app.html
		if ((window as any).deferredPrompt) {
			deferredPrompt = (window as any).deferredPrompt;
			isPWASupported = (window as any).isPWAInstallAvailable;
		}

		// Listen for app installed event
		window.addEventListener('appinstalled', () => {
			isAlreadyInstalled = true;
			if (hideWhenInstalled) {
				showInstallButton = false;
			}
			deferredPrompt = null;
		});

		return () => {
			// Cleanup if needed
		};
	});

	async function handleInstallClick() {
		if (!deferredPrompt) {
			console.warn('PWA install not available - beforeinstallprompt not triggered');
			return;
		}

		isInstalling = true;

		try {
			// Show the install prompt
			deferredPrompt.prompt();

			// Wait for the user to respond to the prompt
			const { outcome } = await deferredPrompt.userChoice;

			if (outcome === 'accepted') {
				isAlreadyInstalled = true;
				if (hideWhenInstalled) {
					showInstallButton = false;
				}
			}
		} catch (error) {
			console.error('Error during installation:', error);
		} finally {
			isInstalling = false;
			deferredPrompt = null;
		}
	}

	const isDisabled = $derived(isInstalling || isAlreadyInstalled || !isPWASupported);
</script>

{#if showInstallButton}
	<button
		onclick={handleInstallClick}
		disabled={isDisabled}
		class="btn btn-primary flex items-center justify-center gap-2"
		style={isDisabled && !isAlreadyInstalled
			? `background-color: var(--color-text-muted); cursor: not-allowed;`
			: ''}
	>
		{#if isAlreadyInstalled && !hideWhenInstalled}
			<SquareCheckBig size={20} style="color: var(--color-text-primary);" />
			<span>{tSync(lang, 'buttons.appInstalled')}</span>
		{:else if isInstalling}
			<Hourglass size={20} style="color: var(--color-text-primary);" />
			<span>{tSync(lang, 'buttons.installing')}</span>
		{:else if !isPWASupported}
			<Smartphone size={20} style="color: var(--color-text-primary);" />
			<span>{tSync(lang, 'buttons.installUnavailable')}</span>
		{:else}
			<Smartphone size={20} style="color: var(--color-text-primary);" />
			<span>{tSync(lang, text)}</span>
		{/if}
	</button>
{/if}
