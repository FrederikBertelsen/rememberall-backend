<script lang="ts">
	import { authStore } from '$lib/stores/auth.svelte';
	import { goto } from '$app/navigation';
	import { languageTag, tSync } from '$lib/i18n/index';
	import type { LoginDto } from '$lib/api/types';
	import Logo from '$lib/components/common/Logo.svelte';
	import LanguageSwitcher from '$lib/components/common/LanguageSwitcher.svelte';
	import PasswordInput from '$lib/components/common/PasswordInput.svelte';
	import PWAInstallButton from '$lib/components/common/PWAInstallButton.svelte';

	let email = $state('');
	let password = $state('');
	let formError = $state<string | null>(null);

	async function handleSubmit(e: SubmitEvent): Promise<void> {
		e.preventDefault();
		formError = null;

		// Basic validation
		if (!email.trim() || !password.trim()) {
			formError = tSync($languageTag, 'form.emailRequired');
			return;
		}

		if (!email.includes('@')) {
			formError = tSync($languageTag, 'form.invalidEmail');
			return;
		}

		try {
			const credentials: LoginDto = { email, password };
			await authStore.login(credentials);
			await goto('/');
		} catch {
			formError = authStore.error || tSync($languageTag, 'form.loginFailed');
		}
	}
</script>

<div class="flex min-h-screen flex-col items-center justify-start gap-8 px-4 pt-8">
	<div class="card space-y-0">
		<div class="page-heading">
			<Logo size="lg" />
			<p>{tSync($languageTag, 'common.signInToAccount')}</p>
		</div>

		<form onsubmit={handleSubmit} class="form mt-10">
			{#if formError}
				<div class="alert">
					<p>{formError}</p>
				</div>
			{/if}

			<div class="form-group">
				<label for="email" class="form-label">{tSync($languageTag, 'common.email')}</label>
				<input
					id="email"
					type="email"
					bind:value={email}
					disabled={authStore.isLoading}
					class="form-input"
					placeholder={tSync($languageTag, 'placeholders.email')}
				/>
			</div>

			<div class="form-group">
				<PasswordInput
					id="password"
					bind:value={password}
					disabled={authStore.isLoading}
					label={tSync($languageTag, 'common.password')}
					placeholder={tSync($languageTag, 'placeholders.password')}
				/>
			</div>

			<button type="submit" disabled={authStore.isLoading} class="btn btn-accent">
				{authStore.isLoading
					? tSync($languageTag, 'common.signingIn')
					: tSync($languageTag, 'common.signIn')}
			</button>

			<p class="page-footer">
				{tSync($languageTag, 'auth.dontHaveAccount')}
				<a href="/register">{tSync($languageTag, 'common.signUp')}</a>
			</p>
		</form>
	</div>
	<div class="flex flex-col items-center gap-6">
		<LanguageSwitcher />
		<PWAInstallButton hideWhenInstalled={true} hideWhenUnavailable={true} />
	</div>
</div>
