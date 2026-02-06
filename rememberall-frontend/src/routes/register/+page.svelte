<script lang="ts">
	import { authStore } from '$lib/stores/auth.svelte';
	import { goto } from '$app/navigation';
	import { languageTag, tSync } from '$lib/i18n/index';
	import type { CreateUserDto } from '$lib/api/types';
	import Logo from '$lib/components/common/Logo.svelte';
	import LanguageSwitcher from '$lib/components/common/LanguageSwitcher.svelte';
	import PasswordInput from '$lib/components/common/PasswordInput.svelte';

	let name = $state('');
	let email = $state('');
	let password = $state('');
	let passwordConfirm = $state('');
	let formError = $state<string | null>(null);

	async function handleSubmit(e: SubmitEvent): Promise<void> {
		e.preventDefault();
		formError = null;

		// Validation
		if (!name.trim() || !email.trim() || !password.trim()) {
			formError = tSync($languageTag, 'form.allFieldsRequired');
			return;
		}

		if (!email.includes('@')) {
			formError = tSync($languageTag, 'form.invalidEmail');
			return;
		}

		if (password.length < 8) {
			formError = tSync($languageTag, 'form.passwordMinLength');
			return;
		}

		if (password !== passwordConfirm) {
			formError = tSync($languageTag, 'form.passwordsMismatch');
			return;
		}

		try {
			const userData: CreateUserDto = { name, email, password };
			await authStore.register(userData);
			await goto('/');
		} catch {
			formError = authStore.error || tSync($languageTag, 'form.registrationFailed');
		}
	}
</script>

<div class="flex min-h-screen flex-col items-center justify-start gap-8 px-4 pt-8">
	<div class="card">
		<div class="page-heading">
			<Logo size="md" />
			<p>{tSync($languageTag, 'common.createAccount')}</p>
		</div>

		<form onsubmit={handleSubmit} class="form mt-10">
			{#if formError}
				<div class="alert">
					<p>{formError}</p>
				</div>
			{/if}

			<div class="form-group">
				<label for="name" class="form-label">{tSync($languageTag, 'common.fullName')}</label>
				<input
					id="name"
					type="text"
					bind:value={name}
					disabled={authStore.isLoading}
					class="form-input"
					placeholder={tSync($languageTag, 'placeholders.name')}
				/>
			</div>

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
				<p class="form-hint">{tSync($languageTag, 'hints.minPassword')}</p>
			</div>

			<div class="form-group">
				<PasswordInput
					id="passwordConfirm"
					bind:value={passwordConfirm}
					disabled={authStore.isLoading}
					label={tSync($languageTag, 'common.confirmPassword')}
					placeholder={tSync($languageTag, 'placeholders.password')}
				/>
			</div>

			<button type="submit" disabled={authStore.isLoading} class="btn btn-accent">
				{authStore.isLoading
					? tSync($languageTag, 'common.signingUp')
					: tSync($languageTag, 'common.signUpButton')}
			</button>

			<p class="page-footer">
				{tSync($languageTag, 'auth.alreadyHaveAccount')}
				<a href="/login">{tSync($languageTag, 'common.signIn')}</a>
			</p>
		</form>
	</div>
	<LanguageSwitcher />
</div>
