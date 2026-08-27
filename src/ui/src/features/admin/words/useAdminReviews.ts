import { toApiError } from "../../../shared/api/apiError";
import { showToast } from "../../../shared/notifications/toastStore";
import { usePagedList } from "../../../shared/composables/usePagedList";
import { getDailySourceReviews, type GetReviewsParams } from "../api/adminApi";
import type { DailyGameSourceReview } from "../model/admin";

export function useAdminReviews(getParams: () => Omit<GetReviewsParams, "skip" | "take">) {
  const pagedList = usePagedList<DailyGameSourceReview>({
    pageSize: 30,
    rootMargin: "240px",
    loadPage: async (skip, take, signal) => {
      const result = await getDailySourceReviews(
        { ...getParams(), skip, take },
        { signal },
      );
      return { items: result.reviews, hasMore: result.hasMore };
    },
    getErrorMessage: (error) => toApiError(error).code === "InvalidWord"
        ? "Введите одно корректное слово целиком"
        : "Не удалось загрузить список слов",
    onError: (message) => showToast({ status: "error", title: "Каталог решений", message }),
  });

  return {
    reviews: pagedList.items,
    ...pagedList,
    retryLoadMore: pagedList.retry,
  };
}
