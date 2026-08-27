import { computed } from "vue";
import { showToast } from "../../../shared/notifications/toastStore";
import { usePagedList } from "../../../shared/composables/usePagedList";

type HistoryPage<T> = { games: T[]; hasMore: boolean };

export function usePagedGameHistory<T>(
  loader: (skip: number, take: number, signal: AbortSignal) => Promise<HistoryPage<T>>,
  title: string,
  errorMessage: string,
) {
  const pagedList = usePagedList<T>({
    pageSize: 20,
    loadPage: async (skip, take, signal) => {
      const page = await loader(skip, take, signal);
      return { items: page.games, hasMore: page.hasMore };
    },
    getErrorMessage: () => errorMessage,
    onError: (message) => {
      showToast({ status: "error", title, message });
    },
  });

  return {
    games: pagedList.items,
    hasMore: pagedList.hasMore,
    loading: pagedList.loading,
    loadedOnce: pagedList.loadedOnce,
    loadFailed: computed(() => pagedList.loadError.value !== null),
    setLoadAnchor: (element: HTMLElement | null) => {
      pagedList.sentinel.value = element;
    },
    loadMore: pagedList.load,
    retryLoad: pagedList.retry,
  };
}
