const reloadTargetStorageKey = "wordoguessr:chunk-reload-target";

let pendingPreloadError: unknown;
let hasPendingPreloadError = false;

type VitePreloadErrorEvent = Event & {
  payload: unknown;
};

export function registerChunkLoadRecovery() {
  window.addEventListener("vite:preloadError", (event) => {
    pendingPreloadError = (event as VitePreloadErrorEvent).payload;
    hasPendingPreloadError = true;
  });
}

export function consumePendingPreloadError(error: unknown) {
  if (!hasPendingPreloadError || pendingPreloadError !== error) {
    return false;
  }

  pendingPreloadError = undefined;
  hasPendingPreloadError = false;
  return true;
}

export function tryReloadForChunkError(targetUrl: string) {
  try {
    if (window.sessionStorage.getItem(reloadTargetStorageKey) === targetUrl) {
      return false;
    }

    window.sessionStorage.setItem(reloadTargetStorageKey, targetUrl);
  } catch {
    return false;
  }

  window.location.assign(targetUrl);
  return true;
}

export function clearChunkReloadAttempt() {
  try {
    window.sessionStorage.removeItem(reloadTargetStorageKey);
  } catch {
    // Storage can be unavailable in restricted browser contexts.
  }
}
