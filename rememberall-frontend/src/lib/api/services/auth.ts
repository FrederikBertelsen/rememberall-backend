/**
 * Authentication service - handles login, register, logout, etc.
 * Simple async functions using the API client
 */

import { apiGet, apiPost, apiDelete } from '$lib/api/client';
import type { UserDto, CreateUserDto, LoginDto } from '$lib/api/types';

/**
 * Register a new user
 * @param data - User name, email, and password
 * @returns Newly created user with ID
 */
export async function register(data: CreateUserDto): Promise<UserDto> {
  return apiPost<UserDto>('/auth/register', data);
}

/**
 * Login with email and password
 * Backend will issue an HttpOnly, secure cookie
 * @param data - Email and password
 * @returns Authenticated user
 */
export async function login(data: LoginDto): Promise<UserDto> {
  return apiPost<UserDto>('/auth/login', data);
}

/**
 * Get current authenticated user
 * @returns Current user or throws 401 if not authenticated
 */
export async function getCurrentUser(): Promise<UserDto> {
  return apiGet<UserDto>('/auth/me');
}

/**
 * Get password requirements from backend
 * @returns Password requirements string (e.g., "Minimum 8 characters")
 */
export async function getPasswordRequirements(): Promise<string> {
  return apiGet<string>('/auth/password-requirements');
}

/**
 * Logout current user
 * Backend will clear the session cookie
 */
export async function logout(): Promise<void> {
  return apiPost<void>('/auth/logout', {});
}

/**
 * Delete current user account
 */
export async function deleteAccount(): Promise<void> {
  return apiDelete('/auth/delete-account');
}
