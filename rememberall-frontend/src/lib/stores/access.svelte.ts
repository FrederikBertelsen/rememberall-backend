/**
 * List access store - manages access to shared lists
 * Uses Svelte 5 runes for reactive state management
 */

import { type ListAccessDto } from '$lib/api/types';
import * as accessService from '$lib/api/services/access';
import { ApiError, handleApiError } from '$lib/utils/errors';

class AccessStore {
  // ============================================================================
  // STATE (Svelte 5 runes)
  // ============================================================================

  accessByList = $state<Record<string, ListAccessDto[]>>({});
  isLoading = $state(false);
  error = $state<string | null>(null);

  // ============================================================================
  // METHODS
  // ============================================================================

  /**
   * Fetch all users who have access to a specific list
   */
  async fetchListAccess(listId: string): Promise<void> {
    this.isLoading = true;
    this.error = null;

    try {
      this.accessByList[listId] = await accessService.getListAccess(listId);
    } catch (e) {
      const error = e instanceof ApiError ? e : new ApiError(500, String(e));
      this.error = handleApiError(error);
      throw error;
    } finally {
      this.isLoading = false;
    }
  }

  /**
   * Revoke access to a list for a specific user
   */
  async revokeAccess(accessId: string, listId: string): Promise<void> {
    this.isLoading = true;
    this.error = null;

    try {
      await accessService.revokeAccess(accessId);
      const access = this.accessByList[listId];
      if (access) {
        this.accessByList[listId] = access.filter((a) => a.id !== accessId);
      }
    } catch (e) {
      const error = e instanceof ApiError ? e : new ApiError(500, String(e));
      this.error = handleApiError(error);
      throw error;
    } finally {
      this.isLoading = false;
    }
  }

  /**
   * Get access list for a specific list from local state
   */
  getAccessForList(listId: string): ListAccessDto[] {
    return this.accessByList[listId] ?? [];
  }

  /**
   * Leave a shared list (revoke own access)
   */
  async leaveList(listId: string): Promise<void> {
    this.isLoading = true;
    this.error = null;

    try {
      await accessService.leaveList(listId);
      // Clear the access list for this list since user no longer has access
      this.accessByList[listId] = [];
    } catch (e) {
      const error = e instanceof ApiError ? e : new ApiError(500, String(e));
      this.error = handleApiError(error);
      throw error;
    } finally {
      this.isLoading = false;
    }
  }

  /**
   * Sync access for a list - used for refresh polling
   * Silently updates access without setting loading state
   */
  syncAccessForList(listId: string, access: ListAccessDto[]): void {
    this.accessByList[listId] = access;
  }
}

// Export singleton instance
export const accessStore = new AccessStore();
