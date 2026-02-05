/**
 * List access service - manages access to shared lists
 */

import { apiGet, apiDelete } from '$lib/api/client';
import type { ListAccessDto } from '$lib/api/types';

/**
 * Get all list accesses for the current user, optionally filtered by list ID
 */
export async function getListAccess(listId?: string): Promise<ListAccessDto[]> {
  const url = listId ? `/listaccess?listId=${listId}` : '/listaccess';
  return apiGet<ListAccessDto[]>(url);
}

/**
 * Revoke access to a shared list
 */
export async function revokeAccess(accessId: string): Promise<void> {
  return apiDelete(`/listaccess/${accessId}`);
}

/**
 * Leave a shared list (revokes own access)
 */
export async function leaveList(listId: string): Promise<void> {
  const accesses = await getListAccess(listId);
  // Find the access record for the current user and delete it
  // Note: This relies on getListAccess returning the current user's access
  if (accesses.length > 0) {
    // The backend will return only the current user's access for their lists
    await revokeAccess(accesses[0].id);
  }
}
