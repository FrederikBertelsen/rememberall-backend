/**
 * Error handling utilities for API requests
 */

export class ApiError extends Error {
  constructor(
    public status: number,
    message: string
  ) {
    super(message);
    this.name = 'ApiError';
  }
}

/**
 * Parse error response from API
 * Backend returns standardized error JSON from GlobalExceptionMiddleware
 */
export async function parseApiError(response: Response): Promise<ApiError> {
  try {
    const data = await response.json();
    // Assume backend returns { message: string }
    const message = typeof data.message === 'string' ? data.message : response.statusText;
    return new ApiError(response.status, message);
  } catch {
    // Fallback if response is not JSON
    return new ApiError(response.status, response.statusText);
  }
}

/**
 * Convert API error to user-friendly message
 */
export function handleApiError(error: ApiError): string {
  switch (error.status) {
    case 400:
      return error.message || 'Invalid input. Please check your data.';
    case 401:
      return 'Authentication failed. Please log in again.';
    case 403:
      return 'You do not have permission to perform this action.';
    case 404:
      return 'The requested resource was not found.';
    case 409:
      return 'This action conflicts with existing data.';
    case 500:
    case 502:
    case 503:
      return 'Server error. Please try again later.';
    default:
      return error.message || 'An unexpected error occurred.';
  }
}
