<script lang="ts">
	import { authStore } from '$lib/stores/auth.svelte';
	import { goto } from '$app/navigation';
	import { languageTag, tSync } from '$lib/i18n/index';
	import type { CreateUserDto } from '$lib/api/types';
	import Logo from '$lib/components/common/Logo.svelte';
	import LanguageSwitcher from '$lib/components/common/LanguageSwitcher.svelte';

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

<div class="page-container">
	<div class="card">
		<div class="page-heading">
			<Logo size="md" />
			<p>{tSync($languageTag, 'common.createAccount')}</p>
		</div>

		<form onsubmit={handleSubmit} class="form">
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
				<label for="password" class="form-label">{tSync($languageTag, 'common.password')}</label>
				<input
					id="password"
					type="password"
					bind:value={password}
					disabled={authStore.isLoading}
					class="form-input"
					placeholder={tSync($languageTag, 'placeholders.password')}
				/>
				<p class="form-hint">{tSync($languageTag, 'hints.minPassword')}</p>
			</div>

			<div class="form-group">
				<label for="passwordConfirm" class="form-label"
					>{tSync($languageTag, 'common.confirmPassword')}</label
				>
				<input
					id="passwordConfirm"
					type="password"
					bind:value={passwordConfirm}
					disabled={authStore.isLoading}
					class="form-input"
					placeholder={tSync($languageTag, 'placeholders.password')}
				/>
			</div>

			<button type="submit" disabled={authStore.isLoading} class="btn btn-primary">
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
