<script lang="ts">
	import type { TodoListDto } from '$lib/api/types';

	interface Props {
		list: TodoListDto;
	}

	let { list }: Props = $props();
</script>

<a
	href="/lists/{list.id}"
	class="block space-y-4 rounded-lg border p-4"
	style="background-color: var(--color-bg-secondary); border-color: var(--color-border); border-left: 4px solid var(--color-accent);"
>
	<div class="flex items-start justify-between">
		<div class="flex-1">
			<div class="text-lg font-semibold" style="color: var(--color-accent);">
				{list.name}
			</div>
		</div>
	</div>
</a>
