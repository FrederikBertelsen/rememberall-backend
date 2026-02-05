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
  const url = new URL(event.request.url);

  // Skip non-GET requests
  if (event.request.method !== 'GET') {
    return;
  }

  // Skip all navigation requests - let browser handle them directly
  if (event.request.mode === 'navigate') {
    return;
  }

  // Skip root path and HTML files - let browser handle navigation
  if (url.pathname === '/' || url.pathname.endsWith('.html')) {
    return;
  }

  // Skip requests to API routes - always go to network with redirect handling
  if (url.pathname.includes('/api/')) {
    event.respondWith(
      fetch(event.request, { redirect: 'follow' }).catch(() => {
        return new Response('Network error', { status: 408 });
      })
    );
    return;
  }

  // Cache static assets (JS, CSS, fonts, images)
  event.respondWith(
    caches.match(event.request).then((response) => {
      if (response) {
        return response;
      }

      return fetch(event.request, { redirect: 'follow' }).then((response) => {
        // Don't cache non-successful responses
        if (!response || response.status !== 200) {
          return response;
        }

        // Clone and cache the response
        const responseToCache = response.clone();
        caches.open(CACHE_NAME).then((cache) => {
          cache.put(event.request, responseToCache);
        });

        return response;
      }).catch(() => {
        // Network error - return cached if available
        return caches.match(event.request);
      });
    })
  );
});
