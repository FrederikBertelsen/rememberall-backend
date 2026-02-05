<script lang="ts">
	import type { InviteDto } from '$lib/api/types';
	import { languageTag, tSync } from '$lib/i18n/index';

	interface Props {
		invite: InviteDto;
		isLoading?: boolean;
		onAccept?: (inviteId: string) => void;
		onDecline?: (inviteId: string) => void;
	}

	let { invite, isLoading = false, onAccept, onDecline }: Props = $props();
	let lang = $derived($languageTag);
</script>

<div
	class="space-y-4 rounded-lg border p-4"
	style="background-color: var(--color-bg-secondary); border-color: var(--color-border); border-left: 4px solid var(--color-accent);"
>
	<div>
		<p class="text-sm" style="color: var(--color-text-secondary);">
			<span class="font-semibold" style="color: var(--color-text-primary);"
				>{invite.inviteSenderName}</span
			>
			{tSync(lang, 'invites.invitedYouTo')}
			<span class="font-semibold" style="color: var(--color-text-primary);">{invite.listName}</span>
		</p>
	</div>

	<div class="flex gap-2">
		{#if onAccept}
			<button
				onclick={() => onAccept?.(invite.id)}
				disabled={isLoading}
				class="btn btn-primary flex-1 py-2 text-sm"
			>
				{isLoading ? tSync(lang, 'buttons.accepting') : tSync(lang, 'buttons.accept')}
			</button>
		{/if}

		{#if onDecline}
			<button
				onclick={() => onDecline?.(invite.id)}
				disabled={isLoading}
				class="btn btn-secondary flex-1 py-2 text-sm"
			>
				{isLoading ? tSync(lang, 'buttons.declining') : tSync(lang, 'buttons.decline')}
			</button>
		{/if}
	</div>
</div>
