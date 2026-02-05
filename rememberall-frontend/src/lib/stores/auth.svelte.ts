/**
 * Authentication store - manages current user and auth state
 * Uses Svelte 5 runes for reactive state management
 */

import { type UserDto, type LoginDto, type CreateUserDto } from '$lib/api/types';
import * as authService from '$lib/api/services/auth';
import { ApiError, handleApiError } from '$lib/utils/errors';

class AuthStore {
  // ============================================================================
  // STATE (Svelte 5 runes)
  // ============================================================================

  currentUser = $state<UserDto | null>(null);
  isLoading = $state(false);
  error = $state<string | null>(null);
  isAuthenticated = $derived(!!this.currentUser);

  // ============================================================================
  // METHODS
  // ============================================================================

  /**
   * Initialize store by checking if user is already authenticated
   * Call this in root +layout.svelte
   */
  async checkAuth(): Promise<void> {
    this.isLoading = true;
    this.error = null;

    try {
      const user = await authService.getCurrentUser();
      this.currentUser = user;
    } catch (e) {
      // Not authenticated or network error
      // Don't treat 401 as an error; user just needs to login
      if (e instanceof ApiError && e.status === 401) {
        this.currentUser = null;
      } else {
        this.error = e instanceof Error ? e.message : 'Unknown error';
      }
    } finally {
      this.isLoading = false;
    }
  }

  /**
   * Register a new user and log them in
   */
  async register(data: CreateUserDto): Promise<void> {
    this.isLoading = true;
    this.error = null;

    try {
      const user = await authService.register(data);
      this.currentUser = user;
    } catch (e) {
      const error = e instanceof ApiError ? e : new ApiError(500, String(e));
      this.error = handleApiError(error);
      throw error;
    } finally {
      this.isLoading = false;
    }
  }

  /**
   * Login with email and password
   */
  async login(data: LoginDto): Promise<void> {
    this.isLoading = true;
    this.error = null;

    try {
      const user = await authService.login(data);
      this.currentUser = user;
    } catch (e) {
      const error = e instanceof ApiError ? e : new ApiError(500, String(e));
      this.error = handleApiError(error);
      throw error;
    } finally {
      this.isLoading = false;
    }
  }

  /**
   * Logout current user
   */
  async logout(): Promise<void> {
    this.isLoading = true;
    this.error = null;

    try {
      await authService.logout();
      this.currentUser = null;
    } catch (e) {
      const error = e instanceof ApiError ? e : new ApiError(500, String(e));
      this.error = handleApiError(error);
      throw error;
    } finally {
      this.isLoading = false;
    }
  }

  /**
   * Delete current user account
   */
  async deleteAccount(): Promise<void> {
    this.isLoading = true;
    this.error = null;

    try {
      await authService.deleteAccount();
      this.currentUser = null;
    } catch (e) {
      const error = e instanceof ApiError ? e : new ApiError(500, String(e));
      this.error = handleApiError(error);
      throw error;
    } finally {
      this.isLoading = false;
    }
  }

  /**
   * Clear any error message
   */
  clearError(): void {
    this.error = null;
  }
}

// Export singleton instance
export const authStore = new AuthStore();
