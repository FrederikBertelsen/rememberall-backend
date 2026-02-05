<script lang="ts">
  interface Props {
    size?: 'sm' | 'md' | 'lg';
    outline?: boolean;
  }

  let { size = 'md', outline = true }: Props = $props();

  const textClasses = {
    sm: 'text-lg',
    md: 'text-4xl',
    lg: 'text-5xl'
  };
</script>

<div class="inline-block font-bold tracking-tight leading-tight" style="font-family: 'Inter', sans-serif; color: var(--color-text-primary);">
  {#if outline}
    <!-- With outline for auth pages -->
    <div
      class="px-6 py-4 rounded-lg border-2 {textClasses[size]}"
      style="border-color: var(--color-accent); background-color: var(--color-bg-secondary);"
    >
      Remember<span style="color: var(--color-accent);">All</span>
    </div>
  {:else}
    <!-- No outline for navbar -->
    <span class="{textClasses[size]}">
      Remember<span style="color: var(--color-accent);">All</span>
    </span>
  {/if}
</div>
