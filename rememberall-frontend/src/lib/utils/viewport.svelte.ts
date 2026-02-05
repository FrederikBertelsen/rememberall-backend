interface ViewportState {
	readonly visualHeight: number;
	readonly screenHeight: number;
	readonly keyboardVisible: boolean;
	readonly keyboardHeight: number;
}

/**
 * Create a reactive viewport state that detects keyboard visibility
 * Useful for adjusting modal positions to avoid keyboard overlap
 */
export function createViewportState() {
	let visualHeight = $state(
		typeof window !== 'undefined' ? window.visualViewport?.height || window.innerHeight : 0
	);
	let screenHeight = $state(typeof window !== 'undefined' ? window.innerHeight : 0);
	let keyboardVisible = $state(false);
	let keyboardHeight = $state(0);

	$effect(() => {
		if (typeof window === 'undefined') return;

		const updateViewport = () => {
			const newVisualHeight = window.visualViewport?.height || window.innerHeight;
			const newScreenHeight = window.innerHeight;
			const newKeyboardHeight = Math.max(0, newScreenHeight - newVisualHeight);
			const isKeyboardVisible = newKeyboardHeight > 50; // Threshold to avoid false positives

			visualHeight = newVisualHeight;
			screenHeight = newScreenHeight;
			keyboardHeight = newKeyboardHeight;
			keyboardVisible = isKeyboardVisible;
		};

		// Listen to visual viewport changes (keyboard appearance/disappearance)
		window.visualViewport?.addEventListener('resize', updateViewport);
		window.addEventListener('resize', updateViewport);
		window.addEventListener('orientationchange', updateViewport);

		// Initial state
		updateViewport();

		return () => {
			window.visualViewport?.removeEventListener('resize', updateViewport);
			window.removeEventListener('resize', updateViewport);
			window.removeEventListener('orientationchange', updateViewport);
		};
	});

	return {
		get visualHeight() {
			return visualHeight;
		},
		get screenHeight() {
			return screenHeight;
		},
		get keyboardVisible() {
			return keyboardVisible;
		},
		get keyboardHeight() {
			return keyboardHeight;
		}
	};
}

/**
 * Calculate the optimal modal position to avoid keyboard overlap
 * @param modalHeight - The height of the modal in pixels
 * @param bottomMargin - Minimum margin from bottom of screen (in pixels, default 16)
 * @param maxBottomPercent - Maximum percent from top where bottom can be (0-100, default 90)
 * @returns Object with top position (in pixels or percent) and whether keyboard is visible
 */
export function calculateModalPosition(
	viewportState: ViewportState,
	modalHeight: number,
	bottomMargin: number = 16,
	maxBottomPercent: number = 90
) {
	const availableHeight = viewportState.visualHeight; // Height not covered by keyboard
	const safeBottomPixels = (maxBottomPercent / 100) * viewportState.screenHeight;

	// Center the modal by default
	let topPixels = Math.max(0, (availableHeight - modalHeight) / 2);

	// If modal would extend below the safe zone, move it up
	if (topPixels + modalHeight > safeBottomPixels) {
		topPixels = Math.max(0, safeBottomPixels - modalHeight - bottomMargin);
	}

	return {
		topPixels,
		topPercent: (topPixels / viewportState.screenHeight) * 100,
		keyboardVisible: viewportState.keyboardVisible,
		keyboardHeight: viewportState.keyboardHeight
	};
}
