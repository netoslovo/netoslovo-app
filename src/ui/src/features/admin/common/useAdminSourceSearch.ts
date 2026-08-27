import { onBeforeUnmount, ref } from "vue";
import { isApiRequestCanceled, toApiError } from "../../../shared/api/apiError";
import { showToast } from "../../../shared/notifications/toastStore";

type AdminSourceSearchOptions<T> = {
  emptyMessage: string;
  notFoundMessage: string;
  errorMessage: string;
  load: (word: string, signal: AbortSignal) => Promise<T>;
  onFound: (result: T) => void;
  onBeforeSearch?: () => void;
};

export function useAdminSourceSearch<T>(options: AdminSourceSearchOptions<T>) {
  const searchWord = ref("");
  const searchError = ref<string | null>(null);
  const searching = ref(false);
  let searchRequestId = 0;
  let searchAbortController: AbortController | null = null;

  async function search() {
    const word = searchWord.value.trim();
    cancel();
    options.onBeforeSearch?.();
    searchError.value = null;
    if (!word) {
      searchError.value = options.emptyMessage;
      return;
    }

    const requestId = searchRequestId + 1;
    searchRequestId = requestId;
    const abortController = new AbortController();
    searchAbortController = abortController;
    searching.value = true;
    try {
      const result = await options.load(word, abortController.signal);
      if (requestId === searchRequestId) {
        options.onFound(result);
      }
    } catch (caught) {
      if (isApiRequestCanceled(caught) || requestId !== searchRequestId) {
        return;
      }

      const apiError = toApiError(caught);
      searchError.value = apiError.status === 404
        ? options.notFoundMessage
        : options.errorMessage;
      if (apiError.status !== 404) {
        showToast({ status: "error", title: "Поиск слова", message: searchError.value });
      }
    } finally {
      if (requestId === searchRequestId) {
        searching.value = false;
        searchAbortController = null;
      }
    }
  }

  function cancel() {
    searchRequestId++;
    searchAbortController?.abort();
    searchAbortController = null;
    searching.value = false;
  }

  onBeforeUnmount(cancel);

  return {
    searchWord,
    searchError,
    searching,
    search,
    cancel,
  };
}
