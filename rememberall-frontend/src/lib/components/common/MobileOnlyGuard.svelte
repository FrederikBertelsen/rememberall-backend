<script lang="ts">
	import { browser } from '$app/environment';
	import type { Snippet } from 'svelte';
	import { languageTag, tSync } from '$lib/i18n/index';
	import LanguageSwitcher from './LanguageSwitcher.svelte';

	interface Props {
		children: Snippet;
	}

	let { children }: Props = $props();

	// Track device type and screen dimensions
	let isMobileDevice = $state(true); // Default to true to avoid flash
	let innerWidth = $state(browser ? window.innerWidth : 800);

	// Check if device is mobile/touch-based using modern media queries
	function checkIsMobile(): boolean {
		if (!browser) return true;

		// Check for touch capability and coarse pointer (finger)
		const hasCoarsePointer = window.matchMedia('(pointer: coarse)').matches;
		const hasNoHover = window.matchMedia('(hover: none)').matches;
		const isTouchDevice = 'ontouchstart' in window || navigator.maxTouchPoints > 0;

		// Check user agent as fallback for mobile keywords
		const userAgent = navigator.userAgent.toLowerCase();
		const mobileKeywords = [
			'android',
			'webos',
			'iphone',
			'ipad',
			'ipod',
			'blackberry',
			'windows phone'
		];
		const hasMobileUA = mobileKeywords.some((keyword) => userAgent.includes(keyword));

		// Device is mobile if it has touch + coarse pointer OR mobile user agent
		return (hasCoarsePointer && hasNoHover && isTouchDevice) || hasMobileUA;
	}

	// Set up device detection and resize listener
	$effect(() => {
		if (browser) {
			// Initial check
			isMobileDevice = checkIsMobile();

			const handleResize = () => {
				innerWidth = window.innerWidth;
				isMobileDevice = checkIsMobile();
			};

			window.addEventListener('resize', handleResize);

			return () => {
				window.removeEventListener('resize', handleResize);
			};
		}
	});
</script>

{#if isMobileDevice}
	{@render children()}
{:else}
	<div class="page-container">
		<div class="card text-center">
			<div class="mb-8 flex justify-center">
				<svg
					class="h-20 w-20"
					style="color: var(--color-accent)"
					fill="none"
					stroke="currentColor"
					viewBox="0 0 24 24"
				>
					<path
						stroke-linecap="round"
						stroke-linejoin="round"
						stroke-width="2"
						d="M12 18h.01M8 21h8a2 2 0 002-2V5a2 2 0 00-2-2H8a2 2 0 00-2 2v14a2 2 0 002 2z"
					/>
				</svg>
			</div>

			<div class="page-heading">
				<h1>{tSync($languageTag, 'mobileOnly.title')}</h1>
			</div>

			<div class="space-y-6">
				<p class="text-base" style="color: var(--color-text-secondary)">
					{tSync($languageTag, 'mobileOnly.description')}
				</p>

				<p class="text-sm" style="color: var(--color-text-secondary)">
					{tSync($languageTag, 'mobileOnly.instruction')}
				</p>

				<div
					class="rounded p-4 text-xs"
					style="background-color: var(--color-bg-secondary); color: var(--color-text-muted)"
				>
					{tSync($languageTag, 'mobileOnly.screenWidth', { width: innerWidth })}
				</div>
			</div>
		</div>
		<LanguageSwitcher />
	</div>
{/if}
