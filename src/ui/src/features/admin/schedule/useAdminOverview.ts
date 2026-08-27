import { computed, onMounted, ref } from "vue";
import { showToast } from "../../../shared/notifications/toastStore";
import { getDailySchedule } from "../api/adminApi";
import { addDays, formatLocalDate } from "../lib/date";
import type { DailyGameSchedule } from "../model/admin";

export function useAdminOverview() {
  const today = formatLocalDate(new Date());
  const periodEnd = addDays(today, 13);
  const schedules = ref<DailyGameSchedule[]>([]);
  const loading = ref(true);
  const loadError = ref<string | null>(null);
  const assignedCount = computed(() => schedules.value.filter((item) => item.gameSource !== null).length);
  const emptySchedules = computed(() => schedules.value.filter((item) => item.gameSource === null));
  const nextEmptyDay = computed(() => emptySchedules.value[0]?.day ?? null);

  async function loadSchedule() {
    loading.value = true;
    loadError.value = null;
    try {
      schedules.value = (await getDailySchedule({
        from: today,
        to: periodEnd,
        skip: 0,
        take: 20,
        sortDirection: "Asc",
      })).schedules;
    } catch {
      loadError.value = "Не удалось загрузить ближайшее расписание";
      showToast({ status: "error", title: "Расписание", message: "Не удалось загрузить ближайшие даты." });
    } finally {
      loading.value = false;
    }
  }

  onMounted(loadSchedule);
  return { today, periodEnd, schedules, loading, loadError, assignedCount, emptySchedules, nextEmptyDay, loadSchedule };
}
