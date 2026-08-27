import { showToast } from "../../../shared/notifications/toastStore";
import { usePagedList } from "../../../shared/composables/usePagedList";
import { getDailySchedule, type GetScheduleParams } from "../api/adminApi";
import type { DailyGameSchedule } from "../model/admin";

export function useAdminSchedule(getParams: () => Omit<GetScheduleParams, "skip" | "take">) {
  const pagedList = usePagedList<DailyGameSchedule>({
    pageSize: 40,
    rootMargin: "260px",
    loadPage: async (skip, take, signal) => {
      const result = await getDailySchedule(
        { ...getParams(), skip, take },
        { signal },
      );
      return { items: result.schedules, hasMore: result.hasMore };
    },
    getErrorMessage: () => "Не удалось загрузить расписание",
    onError: (message) => showToast({ status: "error", title: "Расписание", message }),
  });

  return {
    schedules: pagedList.items,
    ...pagedList,
    retryLoadMore: pagedList.retry,
  };
}
