/**
 * Lists store - manages todo lists state
 * Uses Svelte 5 runes for reactive state management
 */

import { type TodoListDto, type CreateTodoListDto, type UpdateTodoListDto } from '$lib/api/types';
import * as listsService from '$lib/api/services/lists';
import { ApiError, handleApiError } from '$lib/utils/errors';
import { itemsStore } from './items.svelte';
import { accessStore } from './access.svelte';
import { invitesStore } from './invites.svelte';

class ListsStore {
  // ============================================================================
  // STATE (Svelte 5 runes)
  // ============================================================================

  lists = $state<TodoListDto[]>([]);
  isLoading = $state(false);
  error = $state<string | null>(null);

  // ============================================================================
  // METHODS
  // ============================================================================

  /**
   * Fetch all lists for the current user
   */
  async fetchLists(): Promise<void> {
    this.isLoading = true;
    this.error = null;

    try {
      this.lists = await listsService.getLists();
    } catch (e) {
      const error = e instanceof ApiError ? e : new ApiError(500, String(e));
      this.error = handleApiError(error);
      throw error;
    } finally {
      this.isLoading = false;
    }
  }

  /**
   * Fetch a single list by ID
   */
  async fetchList(listId: string): Promise<TodoListDto> {
    this.isLoading = true;
    this.error = null;

    try {
      const list = await listsService.getList(listId);
      // Update in lists array if it exists, otherwise add it
      const index = this.lists.findIndex((l) => l.id === listId);
      if (index >= 0) {
        this.lists[index] = list;
      } else {
        this.lists.push(list);
      }
      return list;
    } catch (e) {
      const error = e instanceof ApiError ? e : new ApiError(500, String(e));
      this.error = handleApiError(error);
      throw error;
    } finally {
      this.isLoading = false;
    }
  }

  /**
   * Create a new list
   */
  async createList(data: CreateTodoListDto): Promise<TodoListDto> {
    this.isLoading = true;
    this.error = null;

    try {
      const newList = await listsService.createList(data);
      this.lists.push(newList);
      return newList;
    } catch (e) {
      const error = e instanceof ApiError ? e : new ApiError(500, String(e));
      this.error = handleApiError(error);
      throw error;
    } finally {
      this.isLoading = false;
    }
  }

  /**
   * Update a list
   */
  async updateList(data: UpdateTodoListDto): Promise<TodoListDto> {
    this.isLoading = true;
    this.error = null;

    try {
      const updated = await listsService.updateList(data);
      const index = this.lists.findIndex((l) => l.id === data.id);
      if (index >= 0) {
        this.lists[index] = updated;
      }
      return updated;
    } catch (e) {
      const error = e instanceof ApiError ? e : new ApiError(500, String(e));
      this.error = handleApiError(error);
      throw error;
    } finally {
      this.isLoading = false;
    }
  }

  /**
   * Delete a list
   */
  async deleteList(listId: string): Promise<void> {
    this.isLoading = true;
    this.error = null;

    try {
      await listsService.deleteList(listId);
      this.lists = this.lists.filter((l) => l.id !== listId);
    } catch (e) {
      const error = e instanceof ApiError ? e : new ApiError(500, String(e));
      this.error = handleApiError(error);
      throw error;
    } finally {
      this.isLoading = false;
    }
  }

  /**
   * Clear error message
   */
  clearError(): void {
    this.error = null;
  }

  /**
   * Refresh a list - check if it has been updated since the given timestamp
   * Returns the updated list if changes exist, null if no updates
   * This is a silent operation that doesn't set loading state
   */
  async refreshList(listId: string, currentUpdatedAt: string): Promise<TodoListDto | null> {
    try {
      const updated = await listsService.refreshList(listId, currentUpdatedAt);
      
      // If there are updates, sync them to all relevant stores
      if (updated) {
        // Update lists store
        const index = this.lists.findIndex((l) => l.id === listId);
        if (index >= 0) {
          this.lists[index] = updated;
        }
        
        // Sync items if they exist in the response
        if (updated.items && updated.items.length > 0) {
          itemsStore.syncItemsForList(listId, updated.items);
        }
        
        // Sync access if it exists in the response (from accessors field)
        if (updated.accessors) {
          accessStore.syncAccessForList(listId, updated.accessors);
        }
        
        // Sync invites if they exist in the response
        if (updated.invites) {
          invitesStore.syncInvitesForList(listId, updated.invites);
        }
      }
      
      return updated;
    } catch (e) {
      // Log the error but don't set error state (this is background polling)
      console.error('List refresh failed:', e);
      return null;
    }
  }

  /**
   * Get a list by ID from local state
   */
  getListById(listId: string): TodoListDto | undefined {
    return this.lists.find((l) => l.id === listId);
  }
}

// Export singleton instance
export const listsStore = new ListsStore();
