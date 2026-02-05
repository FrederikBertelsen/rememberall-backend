/**
 * API Proxy Server Route
 * Forwards all requests from frontend to backend
 * This ensures all backend communication goes through the frontend server
 * for better security, simplified CORS handling, and centralized request management
 */

import type { RequestHandler } from '@sveltejs/kit';

const BACKEND_URL = process.env.BACKEND_URL || 'http://localhost:5000/api';

/**
 * Forward GET requests to backend
 */
export const GET: RequestHandler = async ({ params, request, url }) => {
  const path = params.path;
  const queryString = url.search;
  const backendUrl = `${BACKEND_URL}/${path}${queryString}`;

  try {
    const response = await fetch(backendUrl, {
      method: 'GET',
      headers: getForwardedHeaders(request),
      credentials: 'include'
    });

    return await forwardResponse(response);
  } catch (error) {
    return errorResponse(error);
  }
};

/**
 * Forward POST requests to backend
 */
export const POST: RequestHandler = async ({ params, request }) => {
  const path = params.path;
  const backendUrl = `${BACKEND_URL}/${path}`;
  const body = await request.text();

  try {
    const response = await fetch(backendUrl, {
      method: 'POST',
      headers: getForwardedHeaders(request),
      body: body || undefined,
      credentials: 'include'
    });

    return await forwardResponse(response);
  } catch (error) {
    return errorResponse(error);
  }
};

/**
 * Forward PATCH requests to backend
 */
export const PATCH: RequestHandler = async ({ params, request }) => {
  const path = params.path;
  const backendUrl = `${BACKEND_URL}/${path}`;
  const body = await request.text();

  try {
    const response = await fetch(backendUrl, {
      method: 'PATCH',
      headers: getForwardedHeaders(request),
      body: body || undefined,
      credentials: 'include'
    });

    return await forwardResponse(response);
  } catch (error) {
    return errorResponse(error);
  }
};

/**
 * Forward DELETE requests to backend
 */
export const DELETE: RequestHandler = async ({ params, request }) => {
  const path = params.path;
  const backendUrl = `${BACKEND_URL}/${path}`;

  try {
    const response = await fetch(backendUrl, {
      method: 'DELETE',
      headers: getForwardedHeaders(request),
      credentials: 'include'
    });

    return await forwardResponse(response);
  } catch (error) {
    return errorResponse(error);
  }
};

/**
 * Get headers to forward to backend
 * Removes Host and other hop-by-hop headers
 * IMPORTANT: Must include Cookie header so backend receives auth cookies
 */
function getForwardedHeaders(request: Request): Record<string, string> {
  const headers: Record<string, string> = {};

  // Copy relevant headers from original request
  // CRITICAL: Must include 'cookie' so backend receives authentication cookies
  const headersToForward = [
    'content-type',
    'accept',
    'accept-language',
    'user-agent',
    'cookie' // MUST include this!
  ];

  headersToForward.forEach((header) => {
    const value = request.headers.get(header);
    if (value) {
      headers[header] = value;
    }
  });

  // Ensure Content-Type is set for JSON requests
  if (!headers['content-type']) {
    headers['content-type'] = 'application/json';
  }

  return headers;
}

/**
 * Forward response from backend to client
 * Ensures all headers (including Set-Cookie) are properly forwarded
 */
async function forwardResponse(response: Response): Promise<Response> {
  // Handle 204 No Content specially (no body)
  if (response.status === 204) {
    return new Response(null, {
      status: response.status,
      statusText: response.statusText,
      headers: createHeadersObject(response.headers)
    });
  }

  // For all other responses, properly forward the body and headers
  // Read the body to avoid stream issues
  const body = await response.arrayBuffer();

  return new Response(body, {
    status: response.status,
    statusText: response.statusText,
    headers: createHeadersObject(response.headers)
  });
}

/**
 * Create a proper Headers object from fetch response headers
 * Ensures Set-Cookie and other headers are correctly transferred
 */
function createHeadersObject(sourceHeaders: Headers): Record<string, string> {
  const headers: Record<string, string> = {};

  // Copy all headers from the source response
  sourceHeaders.forEach((value: string, key: string) => {
    // Special handling for Set-Cookie (can have multiple values)
    if (key.toLowerCase() === 'set-cookie') {
      // Preserve Set-Cookie headers
      headers[key] = value;
    } else {
      headers[key] = value;
    }
  });

  return headers;
}

/**
 * Handle errors from backend requests
 */
function errorResponse(error: unknown): Response {
  const message = error instanceof Error ? error.message : 'Unknown error';

  console.error('API Proxy Error:', message);

  return new Response(
    JSON.stringify({
      status: 500,
      title: 'Internal Server Error',
      detail: 'Failed to communicate with backend',
      instance: '/api'
    }),
    {
      status: 500,
      statusText: 'Internal Server Error',
      headers: {
        'Content-Type': 'application/problem+json'
      }
    }
  );
}
