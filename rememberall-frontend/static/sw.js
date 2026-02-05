/// <reference lib="webworker" />

/**
 * @type {string}
 */
const CACHE_NAME = 'rememberall-v1';

/**
 * @type {string[]}
 */
const ASSETS_TO_CACHE = [
  '/',
  '/manifest.json'
];

// Install event: cache assets
/**
 * @param {ExtendableEvent} event
 */
self.addEventListener('install', (event) => {
  event.waitUntil(
    caches.open(CACHE_NAME).then((cache) => {
      return cache.addAll(ASSETS_TO_CACHE).catch(() => {
        // Ignore cache errors during installation
        return Promise.resolve();
      });
    })
  );
  self.skipWaiting();
});

// Activate event: clean up old caches
/**
 * @param {ExtendableEvent} event
 */
self.addEventListener('activate', (event) => {
  event.waitUntil(
    caches.keys().then((cacheNames) => {
      return Promise.all(
        cacheNames.map((cacheName) => {
          if (cacheName !== CACHE_NAME) {
            return caches.delete(cacheName);
          }
        })
      );
    })
  );
  self.clients.claim();
});

// Fetch event: serve from cache, fallback to network
/**
 * @param {FetchEvent} event
 */
self.addEventListener('fetch', (event) => {
  // Skip non-GET requests
  if (event.request.method !== 'GET') {
    return;
  }

  // Skip navigation requests (HTML page loads) - let them bypass the service worker
  // This prevents issues with auth redirects
  if (event.request.mode === 'navigate') {
    return;
  }

  // Skip requests to API routes - always go to network with redirect handling
  if (event.request.url.includes('/api/')) {
    event.respondWith(
      fetch(event.request, { redirect: 'follow' }).catch(() => {
        return new Response('Network error', { status: 408 });
      })
    );
    return;
  }

  event.respondWith(
    caches.match(event.request).then((response) => {
      if (response) {
        return response;
      }

      return fetch(event.request, { redirect: 'follow' }).then((response) => {
        // Don't cache non-successful responses or redirects
        if (!response || response.status !== 200) {
          return response;
        }

        // Clone the response
        const responseToCache = response.clone();
        caches.open(CACHE_NAME).then((cache) => {
          cache.put(event.request, responseToCache);
        });

        return response;
      }).catch(() => {
        // Network error - return cached or error response
        return caches.match(event.request) || new Response('Network error', { status: 408 });
      });
    })
  );
});
