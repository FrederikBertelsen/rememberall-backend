import { writable } from 'svelte/store';

export type Language = 'en' | 'da';

interface Translations {
	[key: string]: Translations | string;
}

// Import translations
import en from '../messages/en.json';
import da from '../messages/da.json';

const translations: Record<Language, Translations> = {
	en,
	da
};

// Create a writable store for the current language
const storedLang =
	typeof window !== 'undefined' ? localStorage.getItem('lang') : null;
const initialLang: Language = (storedLang as Language) || 'en';

export const languageTag = writable<Language>(initialLang);

// Subscribe to changes and persist
if (typeof window !== 'undefined') {
	languageTag.subscribe((lang) => {
		localStorage.setItem('lang', lang);
		document.documentElement.lang = lang;
	});
}

export function setLanguage(lang: Language): void {
	languageTag.set(lang);
}

// Get translation by key with dot notation
export function getTranslation(
	lang: Language,
	key: string,
	params?: Record<string, string | number>
): string {
	const keys = key.split('.');
	let value: Translations | string | undefined = translations[lang];

	for (const k of keys) {
		if (value && typeof value === 'object' && k in value) {
			value = value[k];
		} else {
			return key; // Return key if not found
		}
	}

	if (typeof value !== 'string') {
		return key;
	}

	// Simple parameter replacement
	if (params) {
		let result = value;
		for (const [paramKey, paramValue] of Object.entries(params)) {
			result = result.replace(`{${paramKey}}`, String(paramValue));
		}
		return result;
	}

	return value;
}

// Helper to get translations synchronously
export function tSync(lang: Language, key: string, params?: Record<string, string | number>): string {
	return getTranslation(lang, key, params);
}
