import tailwindcss from '@tailwindcss/vite';
import { sveltekit } from '@sveltejs/kit/vite';
import { defineConfig } from 'vite';

export default defineConfig({
	plugins: [tailwindcss(), sveltekit()],
	optimizeDeps: {
		esbuildOptions: {
			// Suppress the warning about missing .svelte-kit/tsconfig.json
			// This file is generated during the build process
			logOverride: {
				'tsconfig.json': 'silent'
			}
		}
	}
});
