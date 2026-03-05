/**
 * Items service - handles todo item CRUD operations
 */

import { apiGet, apiPost, apiPatch, apiDelete } from '$lib/api/client';
import type {
  TodoItemDto,
  CreateTodoItemDto,
  UpdateTodoItemDto,
  BatchUpdateTodoItemsDto,
  BatchUpdateTodoItemsResultDto
} from '$lib/api/types';

/**
 * Create a new todo item in a list
 */
export async function createItem(data: CreateTodoItemDto): Promise<TodoItemDto> {
  return apiPost<TodoItemDto>('/todoitems', data);
}

/**
 * Get all todo items in a list
 */
export async function getItemsByList(listId: string): Promise<TodoItemDto[]> {
  return apiGet<TodoItemDto[]>(`/todoitems/bylist/${listId}`);
}

/**
 * Update a todo item (text only via PATCH)
 */
export async function updateItem(data: UpdateTodoItemDto): Promise<TodoItemDto> {
  return apiPatch<TodoItemDto>('/todoitems', data);
}

/**
 * Mark a todo item as complete
 */
export async function completeItem(itemId: string): Promise<TodoItemDto> {
  return apiPatch<TodoItemDto>(`/todoitems/${itemId}/complete`, {});
}

/**
 * Mark a todo item as incomplete
 */
export async function incompleteItem(itemId: string): Promise<TodoItemDto> {
  return apiPatch<TodoItemDto>(`/todoitems/${itemId}/incomplete`, {});
}

/**
 * Delete a todo item
 */
export async function deleteItem(itemId: string): Promise<void> {
  return apiDelete(`/todoitems/${itemId}`);
}

/**
 * Batch update multiple todo items in a single transaction
 */
export async function batchUpdateItems(
  data: BatchUpdateTodoItemsDto
): Promise<BatchUpdateTodoItemsResultDto> {
  return apiPost<BatchUpdateTodoItemsResultDto>('/todoitems/batch', data);
}
