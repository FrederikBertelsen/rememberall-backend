<script lang="ts">
	import type { ListAccessDto } from '$lib/api/types';
	import { languageTag, tSync } from '$lib/i18n/index';

	interface Props {
		access: ListAccessDto;
		isLoading?: boolean;
		onRevoke?: (accessId: string) => void;
	}

	let { access, isLoading = false, onRevoke }: Props = $props();
	let lang = $derived($languageTag);
</script>

<div
	class="flex min-h-14 items-center justify-between p-3"
	style="border-bottom: 1px solid var(--color-border);"
>
	<div>
		<p class="text-sm font-medium" style="color: var(--color-text-primary);">{access.userName}</p>
		<p class="text-xs" style="color: var(--color-text-secondary);">
			{tSync(lang, 'access.hasAccess')}
		</p>
	</div>

	{#if onRevoke}
		<button
			onclick={() => onRevoke?.(access.id)}
			disabled={isLoading}
			class="rounded border px-3 py-1 text-sm font-medium disabled:cursor-not-allowed disabled:opacity-50"
			style="background-color: var(--color-danger); color: var(--color-error-light); border-color: var(--color-error-light);"
			title={tSync(lang, 'access.revokeAccess')}
		>
			{tSync(lang, 'buttons.remove')}
		</button>
	{/if}
</div>
