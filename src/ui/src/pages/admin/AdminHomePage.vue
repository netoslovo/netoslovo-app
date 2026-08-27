<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from "vue";
import { RouterLink } from "vue-router";
import { getDailySchedule } from "../../features/admin/api/adminApi";
import { getCurrentUserNameFilterVersion } from "../../features/admin/api/authAdminApi";
import { getWordsVersionUploadHistory } from "../../features/admin/api/sagasApi";
import AdminDashboardAlert from "../../features/admin/dashboard/AdminDashboardAlert.vue";
import { addDays, formatAdminDate, formatLocalDate } from "../../features/admin/lib/date";
import { formatUploadDateTime, uploadStateLabels, uploadStepLabels } from "../../features/admin/lib/wordUploads";
import type { DailyGameSchedule } from "../../features/admin/model/admin";
import type { WordsVersionUpload } from "../../features/admin/model/wordUploads";
import { adminDestinations } from "../../features/admin/navigation/adminNavigation";
import { authState } from "../../features/auth/model/authSession";
import { adminPermissions, hasPermission } from "../../features/auth/model/permissions";
import { isApiRequestCanceled } from "../../shared/api/apiError";
import UiSkeleton from "../../shared/ui/UiSkeleton.vue";

const canViewDaily = computed(() => hasPermission(authState.value, adminPermissions.viewSources));
const canViewUploads = computed(() => hasPermission(authState.value, adminPermissions.viewWordsVersionUpload));
const canManageUserNameFilter = computed(() => hasPermission(authState.value, adminPermissions.manageUserNameFilter));
const hasDashboardCards = computed(() => canViewDaily.value || canViewUploads.value || canManageUserNameFilter.value);

const controller = new AbortController();
const today = formatLocalDate(new Date());
const periodEnd = addDays(today, 13);

const schedules = ref<DailyGameSchedule[]>([]);
const scheduleLoading = ref(false);
const scheduleError = ref<string | null>(null);
const assignedCount = computed(() => schedules.value.filter((item) => item.gameSource !== null).length);
const emptySchedules = computed(() => schedules.value.filter((item) => item.gameSource === null));
const nearestEmptyDay = computed(() => emptySchedules.value[0]?.day ?? null);
const scheduleAlertMessage = computed(() => {
  if (scheduleError.value) return scheduleError.value;
  if (!nearestEmptyDay.value) return null;
  return `Ближайшая дата без назначенного слова: ${formatAdminDate(nearestEmptyDay.value, true)}.`;
});

const uploads = ref<WordsVersionUpload[]>([]);
const uploadsLoading = ref(false);
const uploadsError = ref<string | null>(null);
const latestUpload = computed(() => uploads.value[0] ?? null);
const latestUploadFailed = computed(() => latestUpload.value?.state === "Error" || latestUpload.value?.state === "Timeout");
const uploadAlertMessage = computed(() => {
  if (uploadsError.value) return uploadsError.value;
  if (!latestUpload.value || !latestUploadFailed.value) return null;
  return `Версия ${latestUpload.value.wordsVersion}: ${uploadStateLabels[latestUpload.value.state].toLowerCase()}, текущий этап — ${uploadStepLabels[latestUpload.value.currentStep].toLowerCase()}.`;
});

const userNameFilterVersion = ref<number | null>(null);
const userNameFilterLoading = ref(false);
const userNameFilterError = ref<string | null>(null);

async function loadSchedule() {
  scheduleLoading.value = true;
  scheduleError.value = null;
  try {
    schedules.value = (await getDailySchedule({
      from: today,
      to: periodEnd,
      skip: 0,
      take: 14,
      sortDirection: "Asc",
    }, { signal: controller.signal })).schedules;
  } catch (error) {
    if (!isApiRequestCanceled(error)) scheduleError.value = "Не удалось загрузить расписание на ближайшие 14 дней.";
  } finally {
    scheduleLoading.value = false;
  }
}

async function loadUploads() {
  uploadsLoading.value = true;
  uploadsError.value = null;
  try {
    uploads.value = (await getWordsVersionUploadHistory({
      skip: 0,
      take: 5,
      sortBy: "CreatedAt",
      sortDirection: "Desc",
    }, { signal: controller.signal })).uploads;
  } catch (error) {
    if (!isApiRequestCanceled(error)) uploadsError.value = "Не удалось загрузить историю версий словаря.";
  } finally {
    uploadsLoading.value = false;
  }
}

async function loadUserNameFilterVersion() {
  userNameFilterLoading.value = true;
  userNameFilterError.value = null;
  try {
    userNameFilterVersion.value = await getCurrentUserNameFilterVersion({ signal: controller.signal });
  } catch (error) {
    if (!isApiRequestCanceled(error)) userNameFilterError.value = "Не удалось загрузить текущую версию фильтра UserName.";
  } finally {
    userNameFilterLoading.value = false;
  }
}

onMounted(() => {
  if (canViewDaily.value) void loadSchedule();
  if (canViewUploads.value) void loadUploads();
  if (canManageUserNameFilter.value) void loadUserNameFilterVersion();
});

onBeforeUnmount(() => controller.abort());
</script>

<template>
  <div class="admin-page admin-home">
    <header class="admin-page__header">
      <div>
        <p class="admin-page__eyebrow">Управление</p>
        <h1>Администрирование</h1>
        <p class="admin-page__description">Краткое состояние административных разделов.</p>
      </div>
    </header>

    <section v-if="hasDashboardCards" class="admin-dashboard" aria-label="Состояние разделов">
      <article v-if="canViewDaily" class="admin-dashboard-card" :class="{
        'admin-dashboard-card--primary': !scheduleLoading && !scheduleError && emptySchedules.length === 0,
        'admin-dashboard-card--danger': !scheduleLoading && (scheduleError || emptySchedules.length > 0),
      }">
        <div class="admin-dashboard-card__header">
          <RouterLink :to="{ name: adminDestinations.dailyOverview.name }" class="admin-dashboard-card__link admin-native-action">
            <span>Обзор слов дня</span><i class="pi pi-arrow-right" aria-hidden="true"></i>
          </RouterLink>
          <AdminDashboardAlert v-if="scheduleAlertMessage" label="Пояснение к состоянию расписания" :message="scheduleAlertMessage" />
        </div>
        <div v-if="scheduleLoading" class="admin-dashboard-card__skeleton" aria-hidden="true">
          <UiSkeleton width="84px" height="30px" /><UiSkeleton width="150px" height="14px" />
        </div>
        <div v-else-if="scheduleError" class="admin-dashboard-card__unavailable">Данные временно недоступны</div>
        <div v-else class="admin-dashboard-card__summary">
          <strong>{{ assignedCount }} из 14</strong><span>дней со словом</span>
          <small v-if="nearestEmptyDay">Ближайший пропуск: {{ formatAdminDate(nearestEmptyDay) }}</small>
          <small v-else>Расписание заполнено</small>
        </div>
      </article>

      <article v-if="canViewUploads" class="admin-dashboard-card admin-dashboard-card--uploads" :class="{
        'admin-dashboard-card--primary': !uploadsLoading && !uploadsError && latestUpload?.state === 'Completed',
        'admin-dashboard-card--danger': !uploadsLoading && (uploadsError || latestUploadFailed),
      }">
        <div class="admin-dashboard-card__header">
          <RouterLink :to="{ name: adminDestinations.dictionaryUploads.name }" class="admin-dashboard-card__link admin-native-action">
            <span>Загрузка слов</span><i class="pi pi-arrow-right" aria-hidden="true"></i>
          </RouterLink>
          <AdminDashboardAlert v-if="uploadAlertMessage" label="Пояснение к состоянию загрузки слов" :message="uploadAlertMessage" />
        </div>
        <div v-if="uploadsLoading" class="admin-dashboard-card__skeleton" aria-hidden="true">
          <UiSkeleton v-for="index in 3" :key="index" height="24px" border-radius="6px" />
        </div>
        <div v-else-if="uploadsError" class="admin-dashboard-card__unavailable">Данные временно недоступны</div>
        <div v-else-if="uploads.length === 0" class="admin-dashboard-card__unavailable">Завершённых загрузок пока нет</div>
        <div v-else class="admin-dashboard-card__uploads">
          <p class="admin-dashboard-card__latest">Последняя: <strong>{{ uploadStateLabels[uploads[0].state] }}</strong></p>
          <table>
            <thead><tr><th>Версия</th><th>Статус</th><th>Завершена</th></tr></thead>
            <tbody>
              <tr v-for="upload in uploads" :key="upload.sagaId">
                <td>{{ upload.wordsVersion }}</td><td>{{ uploadStateLabels[upload.state] }}</td><td>{{ formatUploadDateTime(upload.completedAt) }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </article>

      <article v-if="canManageUserNameFilter" class="admin-dashboard-card admin-dashboard-card--primary">
        <div class="admin-dashboard-card__header">
          <RouterLink :to="{ name: adminDestinations.usersNameFilter.name }" class="admin-dashboard-card__link admin-native-action">
            <span>Фильтр UserName</span><i class="pi pi-arrow-right" aria-hidden="true"></i>
          </RouterLink>
          <AdminDashboardAlert v-if="userNameFilterError" label="Пояснение к состоянию фильтра UserName" :message="userNameFilterError" />
        </div>
        <div v-if="userNameFilterLoading" class="admin-dashboard-card__skeleton" aria-hidden="true">
          <UiSkeleton width="100px" height="30px" /><UiSkeleton width="140px" height="14px" />
        </div>
        <div v-else-if="userNameFilterError" class="admin-dashboard-card__unavailable">Версия временно недоступна</div>
        <div v-else class="admin-dashboard-card__summary"><strong>{{ userNameFilterVersion }}</strong><span>текущая версия</span></div>
      </article>
    </section>

    <p v-else class="admin-panel admin-home__empty">Для вашей учётной записи пока нет доступных разделов администрирования.</p>
  </div>
</template>

<style scoped>
.admin-home__empty { margin: 0; color: var(--color-gray-600); }
.admin-dashboard { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 12px; }
.admin-dashboard-card { min-width: 0; border: 1px solid var(--color-gray-200); border-radius: 8px; padding: 12px; display: flex; flex-direction: column; gap: 12px; background: white; }
.admin-dashboard-card--primary { border-color: var(--color-primary-500); }
.admin-dashboard-card--danger { border-color: var(--color-red-600); }
.admin-dashboard-card--uploads { grid-row: span 2; }
.admin-dashboard-card__header { min-width: 0; display: flex; align-items: center; justify-content: space-between; gap: 8px; }
.admin-dashboard-card__link { min-width: 0; min-height: 40px; border-radius: 8px; padding: 0 8px; display: inline-flex; align-items: center; gap: 8px; color: var(--color-gray-800); font-weight: 600; text-decoration: none; }
.admin-dashboard-card__link i { color: var(--color-primary-600); font-size: 13px; }
.admin-dashboard-card__skeleton, .admin-dashboard-card__summary { min-height: 68px; display: flex; flex-direction: column; justify-content: center; gap: 5px; }
.admin-dashboard-card__summary strong { color: var(--color-gray-900); font-size: 28px; line-height: 1; }
.admin-dashboard-card__summary span, .admin-dashboard-card__summary small, .admin-dashboard-card__unavailable { color: var(--color-gray-600); }
.admin-dashboard-card__unavailable { min-height: 68px; display: flex; align-items: center; }
.admin-dashboard-card__uploads { min-width: 0; overflow-x: auto; }
.admin-dashboard-card__latest { margin: 0 0 8px; color: var(--color-gray-600); font-size: 13px; }
.admin-dashboard-card__uploads table { width: 100%; border-collapse: collapse; font-size: 12px; }
.admin-dashboard-card__uploads th, .admin-dashboard-card__uploads td { border-top: 1px solid var(--color-gray-200); padding: 7px 5px; text-align: left; white-space: nowrap; }
.admin-dashboard-card__uploads th { color: var(--color-gray-600); font-weight: 500; }
@container admin-content (max-width: 720px) { .admin-dashboard { grid-template-columns: 1fr; } .admin-dashboard-card--uploads { grid-row: auto; } }
@container admin-content (max-width: 380px) { .admin-dashboard-card { padding: 10px; } .admin-dashboard-card__uploads table { font-size: 11px; } }
</style>
