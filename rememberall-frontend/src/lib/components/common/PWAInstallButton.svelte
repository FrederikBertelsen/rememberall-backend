<script lang="ts">
	import { onMount } from 'svelte';
	import { languageTag, tSync } from '$lib/i18n/index';

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

		// Listen for the beforeinstallprompt event
		const handleBeforeInstallPrompt = (e: Event) => {
			// Prevent Chrome 67 and earlier from automatically showing the prompt
			e.preventDefault();
			// Stash the event so it can be triggered later
			deferredPrompt = e;
			isPWASupported = true;
			showInstallButton = true;
		};

		window.addEventListener('beforeinstallprompt', handleBeforeInstallPrompt);

		// Listen for app installed event
		window.addEventListener('appinstalled', () => {
			isAlreadyInstalled = true;
			if (hideWhenInstalled) {
				showInstallButton = false;
			}
			deferredPrompt = null;
		});

		return () => {
			window.removeEventListener('beforeinstallprompt', handleBeforeInstallPrompt);
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
		class="btn btn-primary"
		style={isDisabled && !isAlreadyInstalled
			? `background-color: var(--color-text-muted); cursor: not-allowed;`
			: ''}
	>
		{#if isAlreadyInstalled && !hideWhenInstalled}
			✅ {tSync(lang, 'buttons.appInstalled')}
		{:else if isInstalling}
			⏳ {tSync(lang, 'buttons.installing')}
		{:else if !isPWASupported}
			📱 {tSync(lang, 'buttons.installUnavailable')}
		{:else}
			📱 {tSync(lang, text)}
		{/if}
	</button>
{/if}
