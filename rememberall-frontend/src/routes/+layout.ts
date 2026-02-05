import type { LayoutLoad } from './$types';

export const load: LayoutLoad = async () => {
	// Auth is handled entirely by +layout.svelte using authStore
	// This prevents redundant API calls and redirect loops
};
