import { redirect } from '@sveltejs/kit';
import type { LayoutLoad } from './$types';

export const load: LayoutLoad = async ({ url, fetch, depends }) => {
	depends('auth:check');

	const pathname = url.pathname;
	const publicRoutes = new Set(['/login', '/register']);
	const isPublicRoute = publicRoutes.has(pathname);

	try {
		// Check authentication on server before rendering
		const response = await fetch('/api/auth/me');
		const isAuthenticated = response.ok;

		// Redirect unauthenticated users away from protected routes
		if (!isPublicRoute && !isAuthenticated) {
			redirect(303, '/login');
		}

		// Redirect authenticated users away from auth pages
		if (isPublicRoute && isAuthenticated) {
			redirect(303, '/');
		}
	} catch {
		// Network error - treat as unauthenticated
		if (!isPublicRoute) {
			redirect(303, '/login');
		}
	}
};
