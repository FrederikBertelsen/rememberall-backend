/**
 * Lists service - handles todo list CRUD operations
 */

import { apiGet, apiPost, apiPatch, apiDelete } from '$lib/api/client';
import type {
  TodoListDto,
  CreateTodoListDto,
  UpdateTodoListDto
} from '$lib/api/types';

/**
 * Create a new todo list
 */
export async function createList(data: CreateTodoListDto): Promise<TodoListDto> {
  return apiPost<TodoListDto>('/lists', data);
}

/**
 * Get all todo lists for the authenticated user
 */
export async function getLists(): Promise<TodoListDto[]> {
  return apiGet<TodoListDto[]>('/lists');
}

/**
 * Get a single todo list by ID
 */
export async function getList(listId: string): Promise<TodoListDto> {
  return apiGet<TodoListDto>(`/lists/${listId}`);
}

/**
 * Update a todo list (name only via PATCH)
 */
export async function updateList(data: UpdateTodoListDto): Promise<TodoListDto> {
  return apiPatch<TodoListDto>('/lists', data);
}

/**
 * Delete a todo list (cascades to delete all items)
 */
export async function deleteList(listId: string): Promise<void> {
  return apiDelete(`/lists/${listId}`);
}

/**
 * Refresh a todo list - check if it has been updated since the given timestamp
 * Returns the updated list if changes exist, null if no updates
 */
export async function refreshList(
  listId: string,
  currentUpdatedAt: string
): Promise<TodoListDto | null> {
  const encodedTimestamp = encodeURIComponent(currentUpdatedAt);
  return apiGet<TodoListDto | null>(
    `/lists/${listId}/refresh?currentUpdatedAt=${encodedTimestamp}`
  );
}
