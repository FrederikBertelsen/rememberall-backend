/**
 * Items store - manages todo items state
 * Uses Svelte 5 runes for reactive state management
 */

import {
  type TodoItemDto,
  type CreateTodoItemDto,
  type UpdateTodoItemDto,
  type BatchUpdateTodoItemsDto,
  type BatchUpdateTodoItemsResultDto
} from '$lib/api/types';
import * as itemsService from '$lib/api/services/items';
import { ApiError, handleApiError } from '$lib/utils/errors';

class ItemsStore {
  // ============================================================================
  // STATE (Svelte 5 runes)
  // ============================================================================

  itemsByList = $state<Record<string, TodoItemDto[]>>({});
  isLoading = $state(false);
  error = $state<string | null>(null);

  // ============================================================================
  // METHODS
  // ============================================================================

  /**
   * Fetch all items for a list
   */
  async fetchItemsByList(listId: string): Promise<void> {
    this.isLoading = true;
    this.error = null;

    try {
      this.itemsByList[listId] = await itemsService.getItemsByList(listId);
    } catch (e) {
      const error = e instanceof ApiError ? e : new ApiError(500, String(e));
      this.error = handleApiError(error);
      throw error;
    } finally {
      this.isLoading = false;
    }
  }

  /**
   * Create a new item in a list
   */
  async createItem(data: CreateTodoItemDto): Promise<TodoItemDto> {
    this.isLoading = true;
    this.error = null;

    try {
      const newItem = await itemsService.createItem(data);
      if (!this.itemsByList[data.todoListId]) {
        this.itemsByList[data.todoListId] = [];
      }
      this.itemsByList[data.todoListId].push(newItem);
      return newItem;
    } catch (e) {
      const error = e instanceof ApiError ? e : new ApiError(500, String(e));
      this.error = handleApiError(error);
      throw error;
    } finally {
      this.isLoading = false;
    }
  }

  /**
   * Update an item
   */
  async updateItem(data: UpdateTodoItemDto, listId: string): Promise<TodoItemDto> {
    this.isLoading = true;
    this.error = null;

    try {
      const updated = await itemsService.updateItem(data);
      const items = this.itemsByList[listId];
      if (items) {
        const index = items.findIndex((item) => item.id === data.id);
        if (index >= 0) {
          items[index] = updated;
        }
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
   * Mark an item as complete
   */
  async completeItem(itemId: string, listId: string): Promise<TodoItemDto> {
    this.isLoading = true;
    this.error = null;

    try {
      const updated = await itemsService.completeItem(itemId);
      const items = this.itemsByList[listId];
      if (items) {
        const index = items.findIndex((item) => item.id === itemId);
        if (index >= 0) {
          items[index] = updated;
        }
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
   * Mark an item as incomplete
   */
  async incompleteItem(itemId: string, listId: string): Promise<TodoItemDto> {
    this.isLoading = true;
    this.error = null;

    try {
      const updated = await itemsService.incompleteItem(itemId);
      const items = this.itemsByList[listId];
      if (items) {
        const index = items.findIndex((item) => item.id === itemId);
        if (index >= 0) {
          items[index] = updated;
        }
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
   * Delete an item
   */
  async deleteItem(itemId: string, listId: string): Promise<void> {
    this.isLoading = true;
    this.error = null;

    try {
      await itemsService.deleteItem(itemId);
      const items = this.itemsByList[listId];
      if (items) {
        this.itemsByList[listId] = items.filter((item) => item.id !== itemId);
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
   * Batch update multiple items in a single transaction
   */
  async batchUpdateItems(data: BatchUpdateTodoItemsDto): Promise<BatchUpdateTodoItemsResultDto> {
    this.isLoading = true;
    this.error = null;

    try {
      const result = await itemsService.batchUpdateItems(data);
      const listId = data.todoListId;
      let items = this.itemsByList[listId] ?? [];

      // Remove deleted items
      if (result.deleted && result.deleted.length > 0) {
        items = items.filter((item) => !result.deleted.includes(item.id));
      }

      // Update existing items with new data
      const allUpdatedItems = [
        ...result.updated,
        ...result.completed,
        ...result.incompleted
      ];
      
      for (const updatedItem of allUpdatedItems) {
        const index = items.findIndex((item) => item.id === updatedItem.id);
        if (index >= 0) {
          items[index] = updatedItem;
        }
      }

      // Add newly created items
      if (result.created && result.created.length > 0) {
        items = [...items, ...result.created];
      }

      // Update the store
      this.itemsByList[listId] = items;

      return result;
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
   * Get items for a list from local state
   */
  getItemsForList(listId: string): TodoItemDto[] {
    return this.itemsByList[listId] ?? [];
  }

  /**
   * Sync items for a list - used for refresh polling
   * Silently updates items without setting loading state
   */
  syncItemsForList(listId: string, items: TodoItemDto[]): void {
    this.itemsByList[listId] = items;
  }
}

// Export singleton instance
export const itemsStore = new ItemsStore();
