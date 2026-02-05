<script lang="ts">
	import './layout.css';
	import { authStore } from '$lib/stores/auth.svelte';
	import { goto } from '$app/navigation';
	import { page } from '$app/stores';
	import type { Snippet } from 'svelte';
	import Navbar from '$lib/components/common/Navbar.svelte';
	import MobileOnlyGuard from '$lib/components/common/MobileOnlyGuard.svelte';
	import { browser } from '$app/environment';

	interface Props {
		children: Snippet;
	}

	let { children }: Props = $props();

	// Routes that don't require authentication
	const publicRoutes = new Set(['/login', '/register']);

	let isInitialized = $state<boolean>(false);

	// Register service worker for PWA functionality
	$effect(() => {
		if (browser && 'serviceWorker' in navigator) {
			navigator.serviceWorker.register('/sw.js').catch((err) => {
				console.error('Service Worker registration failed:', err);
			});
		}
	});

	// Initialize auth on mount (runs once)
	$effect.pre(() => {
		if (browser && !isInitialized) {
			initializeAuth();
		}
	});

	async function initializeAuth(): Promise<void> {
		try {
			await authStore.checkAuth();
		} catch {
			// Error handled in store
		} finally {
			isInitialized = true;
		}
	}

	// Handle redirects based on auth state
	$effect(() => {
		if (!isInitialized) return;

		const currentPath = $page.url.pathname;
		const isPublicRoute = publicRoutes.has(currentPath);
		const isAuthenticated = authStore.isAuthenticated;

		// Redirect to login if accessing protected route without auth
		if (!isPublicRoute && !isAuthenticated) {
			goto('/login');
		}

		// Redirect to dashboard if accessing login/register while authenticated
		if (isPublicRoute && isAuthenticated) {
			goto('/');
		}
	});
</script>

<MobileOnlyGuard>
	<div class="min-h-screen">
		{#if authStore.isAuthenticated}
			<Navbar isAuthenticated={authStore.isAuthenticated} />
		{/if}

		<main>
			{@render children()}
		</main>
	</div>
</MobileOnlyGuard>
