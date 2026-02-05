/**
 * HTTP client wrapper for API requests
 * All requests go through the frontend server proxy at /api
 * This ensures backend is never exposed directly to the client
 */

import { parseApiError } from '$lib/utils/errors';

const API_BASE_URL = '/api';

interface FetchOptions extends RequestInit {
  method?: 'GET' | 'POST' | 'PATCH' | 'DELETE' | 'PUT';
}

/**
 * Make a typed API request through the frontend proxy
 * @param endpoint - API endpoint (e.g., '/auth/login')
 * @param options - Fetch options
 * @returns Parsed response JSON
 * @throws ApiError on non-2xx responses
 */
export async function apiFetch<T>(
  endpoint: string,
  options: FetchOptions = {}
): Promise<T> {
  const url = `${API_BASE_URL}${endpoint}`;

  const response = await fetch(url, {
    ...options,
    // No need for 'include' since frontend proxy uses same-origin
    // Backend can use HttpOnly cookies
    headers: {
      'Content-Type': 'application/json',
      ...(options.headers || {})
    }
  });

  // Handle non-2xx responses
  if (!response.ok) {
    const error = await parseApiError(response);
    throw error;
  }

  // 204 No Content response has no body - return null
  if (response.status === 204) {
    return null as T;
  }

  return response.json() as Promise<T>;
}

/**
 * Convenience wrappers for common HTTP methods
 */

export function apiGet<T>(endpoint: string): Promise<T> {
  return apiFetch<T>(endpoint, { method: 'GET' });
}

export function apiPost<T>(endpoint: string, data: unknown): Promise<T> {
  return apiFetch<T>(endpoint, {
    method: 'POST',
    body: JSON.stringify(data)
  });
}

export function apiPatch<T>(endpoint: string, data: unknown): Promise<T> {
  return apiFetch<T>(endpoint, {
    method: 'PATCH',
    body: JSON.stringify(data)
  });
}

export function apiDelete(endpoint: string): Promise<void> {
  return apiFetch<void>(endpoint, { method: 'DELETE' });
}
