import {
  computed,
  nextTick,
  onBeforeUnmount,
  ref,
  shallowRef,
  watch,
  type Ref,
} from "vue";
import { isApiRequestCanceled } from "../api/apiError";

export type PagedListPage<T> = {
  items: T[];
  hasMore: boolean;
};

type PagedListOptions<T> = {
  loadPage: (skip: number, take: number, signal: AbortSignal) => Promise<PagedListPage<T>>;
  pageSize: number;
  enabled?: Ref<boolean>;
  rootMargin?: string;
  preserveItemsOnReset?: boolean;
  fillVisibleSentinel?: boolean;
  getErrorMessage: (error: unknown) => string;
  onError?: (message: string) => void;
};

export function usePagedList<T>(options: PagedListOptions<T>) {
  const items = shallowRef<T[]>([]);
  const loading = ref(false);
  const initialLoading = ref(true);
  const loadedOnce = ref(false);
  const hasMore = ref(true);
  const loadError = ref<string | null>(null);
  const sentinel = ref<HTMLElement | null>(null);
  let requestId = 0;
  let controller: AbortController | null = null;
  let observer: IntersectionObserver | null = null;
  let sentinelIntersecting = false;

  const enabled = options.enabled ?? ref(true);
  const canLoadMore = computed(() =>
    enabled.value && hasMore.value && !loading.value && loadError.value === null,
  );

  watch(sentinel, observeSentinel);
  watch(enabled, (value) => {
    if (value && !loadedOnce.value) void load();
  }, { immediate: true });

  async function load(reset = false) {
    if (!enabled.value || (!reset && !canLoadMore.value)) return;

    if (reset) controller?.abort();
    const currentRequest = ++requestId;
    const abortController = new AbortController();
    controller = abortController;
    const skip = reset ? 0 : items.value.length;

    loading.value = true;
    loadError.value = null;
    if (reset && !options.preserveItemsOnReset) {
      items.value = [];
      hasMore.value = true;
    }

    try {
      const page = await options.loadPage(skip, options.pageSize, abortController.signal);
      if (currentRequest !== requestId) return;

      items.value = reset ? page.items : [...items.value, ...page.items];
      hasMore.value = page.hasMore && page.items.length > 0;
      loadedOnce.value = true;
    } catch (error) {
      if (isApiRequestCanceled(error) || currentRequest !== requestId) return;

      loadError.value = options.getErrorMessage(error);
      loadedOnce.value = true;
      options.onError?.(loadError.value);
    } finally {
      if (currentRequest === requestId) {
        loading.value = false;
        initialLoading.value = false;
        if (options.fillVisibleSentinel && sentinelIntersecting) {
          await nextTick();
          if (currentRequest === requestId && sentinelIntersecting) void load();
        }
      }
      if (controller === abortController) controller = null;
    }
  }

  function retry() {
    loadError.value = null;
    return load();
  }

  function observeSentinel(anchor: HTMLElement | null) {
    observer?.disconnect();
    sentinelIntersecting = false;
    if (!anchor) return;

    observer = new IntersectionObserver((entries) => {
      sentinelIntersecting = entries.some((entry) => entry.isIntersecting);
      if (sentinelIntersecting) void load();
    }, { rootMargin: options.rootMargin });
    observer.observe(anchor);
  }

  onBeforeUnmount(() => {
    requestId++;
    controller?.abort();
    observer?.disconnect();
    sentinelIntersecting = false;
  });

  return {
    items,
    loading,
    initialLoading,
    loadedOnce,
    hasMore,
    loadError,
    sentinel,
    load,
    retry,
  };
}
