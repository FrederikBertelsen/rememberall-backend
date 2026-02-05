<script lang="ts">
	import type { TodoItemDto } from '$lib/api/types';

	interface Props {
		item: TodoItemDto;
		isLoading?: boolean;
		isLast?: boolean;
		onToggle?: (itemId: string) => void;
		onLongPress?: (itemId: string) => void;
	}

	let { item, isLoading = false, isLast = false, onToggle, onLongPress }: Props = $props();
	let longPressTimer: ReturnType<typeof setTimeout> | null = null;
	let longPressDetected = $state(false);
	let touchStartY = $state(0);
	let touchStartX = $state(0);

	function handleMouseDown() {
		longPressTimer = setTimeout(() => {
			longPressDetected = true;
			onLongPress?.(item.id);
		}, 500);
	}

	function handleMouseUp() {
		if (longPressTimer) {
			clearTimeout(longPressTimer);
			longPressTimer = null;
		}
		longPressDetected = false;
	}

	function handleTouchStart(e: TouchEvent) {
		e.preventDefault();
		touchStartY = e.touches[0].clientY;
		touchStartX = e.touches[0].clientX;
		longPressDetected = false;
		longPressTimer = setTimeout(() => {
			longPressDetected = true;
			onLongPress?.(item.id);
		}, 500);
	}

	function handleTouchMove(e: TouchEvent) {
		// Cancel long press if user moves their finger significantly
		const currentY = e.touches[0].clientY;
		const currentX = e.touches[0].clientX;
		const moveY = Math.abs(currentY - touchStartY);
		const moveX = Math.abs(currentX - touchStartX);

		if ((moveY > 10 || moveX > 10) && longPressTimer) {
			clearTimeout(longPressTimer);
			longPressTimer = null;
			longPressDetected = false;
		}
	}

	function handleTouchEnd(e: TouchEvent) {
		if (longPressTimer) {
			clearTimeout(longPressTimer);
			longPressTimer = null;
		}

		// Check if touch moved significantly (scrolling)
		const touchEndY = e.changedTouches[0].clientY;
		const touchEndX = e.changedTouches[0].clientX;
		const moveY = Math.abs(touchEndY - touchStartY);
		const moveX = Math.abs(touchEndX - touchStartX);

		// Only trigger toggle if movement is less than 10px (tap, not scroll) and no long-press was detected
		if (moveY < 10 && moveX < 10 && !longPressDetected) {
			onToggle?.(item.id);
		}

		longPressDetected = false;
	}
</script>

<button
	onmousedown={handleMouseDown}
	onmouseup={() => {
		handleMouseUp();
		onToggle?.(item.id);
	}}
	onmouseleave={handleMouseUp}
	ontouchstart={handleTouchStart}
	ontouchmove={handleTouchMove}
	ontouchend={handleTouchEnd}
	disabled={isLoading}
	class="flex min-h-14 w-full items-center gap-3 p-3 text-left disabled:opacity-50"
	style="border-bottom: {isLast ? 'none' : '1px solid var(--color-border)'};"
	title="Tap to toggle, hold to delete"
>
	<p
		class="min-w-0 flex-1 text-lg"
		style="color: {item.isCompleted
			? 'var(--color-text-muted)'
			: 'var(--color-text-primary)'}; text-decoration: {item.isCompleted
			? 'line-through'
			: 'none'};"
	>
		{item.text}
	</p>
</button>
