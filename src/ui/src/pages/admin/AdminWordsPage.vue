<script setup lang="ts">
import Dialog from "primevue/dialog";
import Select from "primevue/select";
import { computed, onBeforeUnmount, onMounted, ref } from "vue";
import { reviewDailyGameSource } from "../../features/admin/api/adminApi";
import { useAdminReviews } from "../../features/admin/words/useAdminReviews";
import { isApiRequestCanceled, toApiError } from "../../shared/api/apiError";
import { useDelayedLoadingState } from "../../shared/composables/useDelayedLoading";
import { useHandoffDelayedLoadingState } from "../../shared/composables/useSkeletonHandoff";
import type { DailyGameSourceReview, GameSourceReviewSortField, SortDirection } from "../../features/admin/model/admin";
import { adminPermissions, hasPermission } from "../../features/auth/model/permissions";
import UiButton from "../../shared/ui/UiButton.vue";
import AdminListSkeleton from "../../features/admin/common/AdminListSkeleton.vue";
import UiSkeletonHandoff from "../../shared/ui/UiSkeletonHandoff.vue";
import { getDifficulties } from "../../features/games/api/gameApi";
import type { Difficulty } from "../../features/games/model/game";
import { authState } from "../../features/auth/model/authSession";
import { showToast } from "../../shared/notifications/toastStore";

const difficulties = ref<Difficulty[]>([]);
const difficultyLoadFailed = ref(false);
const approvalOptions = [
  { label: "Все", value: "all" },
  { label: "Одобрено", value: "approved" },
  { label: "Отклонено", value: "rejected" },
];
const latestVersionOptions = [
  { label: "Все", value: "all" },
  { label: "Да", value: "latest" },
  { label: "Нет", value: "outdated" },
];
const scheduleOptions = [
  { label: "Все", value: "all" },
  { label: "Назначено", value: "scheduled" },
  { label: "Не назначено", value: "unscheduled" },
];
const sortOptions = [
  { label: "Изменено", value: "UpdatedAt" },
  { label: "Рассмотрено", value: "CreatedAt" },
  { label: "ID источника", value: "GameSourceId" },
];
const sortDirectionOptions = [
  { label: "По убыванию", value: "Desc" },
  { label: "По возрастанию", value: "Asc" },
];

const defaultFilters = {
  word: "",
  difficulty: "",
  approval: "all",
  latestVersion: "all",
  schedule: "all",
  sortBy: "UpdatedAt" as GameSourceReviewSortField,
  sortDirection: "Desc" as SortDirection,
};

const wordFilter = ref(defaultFilters.word);
const difficultyFilter = ref(defaultFilters.difficulty);
const approvalFilter = ref(defaultFilters.approval);
const latestVersionFilter = ref(defaultFilters.latestVersion);
const scheduleFilter = ref(defaultFilters.schedule);
const sortBy = ref<GameSourceReviewSortField>(defaultFilters.sortBy);
const sortDirection = ref<SortDirection>(defaultFilters.sortDirection);
const appliedWordFilter = ref(wordFilter.value);
const appliedDifficultyFilter = ref(difficultyFilter.value);
const appliedApprovalFilter = ref(approvalFilter.value);
const appliedLatestVersionFilter = ref(latestVersionFilter.value);
const appliedScheduleFilter = ref(scheduleFilter.value);
const appliedSortBy = ref(sortBy.value);
const appliedSortDirection = ref(sortDirection.value);
const pendingReview = ref<DailyGameSourceReview | null>(null);
const savingReview = ref(false);
const listAction = ref<"reset" | "apply" | "retry" | null>(null);
const mobileFiltersOpen = ref(false);
let difficultyAbortController: AbortController | null = null;

const { reviews, loading, initialLoading, loadError, sentinel, load, retryLoadMore } =
  useAdminReviews(() => ({
    sortBy: appliedSortBy.value,
    sortDirection: appliedSortDirection.value,
    difficultyCodeFilter: appliedDifficultyFilter.value || undefined,
    existsInLatestVersionFilter: appliedLatestVersionFilter.value === "all" ? undefined : appliedLatestVersionFilter.value === "latest",
    approvedFilter: appliedApprovalFilter.value === "all" ? undefined : appliedApprovalFilter.value === "approved",
    scheduledFilter: appliedScheduleFilter.value === "all" ? undefined : appliedScheduleFilter.value === "scheduled",
    wordFilter: appliedWordFilter.value.trim() || undefined,
  }));
const initialLoadingState = useHandoffDelayedLoadingState(initialLoading);
const listLoadingState = useDelayedLoadingState(loading);

const canReview = computed(() =>
  hasPermission(authState.value, adminPermissions.setSourceReview),
);
const difficultyOptions = computed(() => [
  { label: difficultyLoadFailed.value ? "Фильтр недоступен" : "Все", value: "" },
  ...difficulties.value.map((item) => ({ label: item.name, value: item.code })),
]);

async function applyFilters() {
  appliedWordFilter.value = wordFilter.value;
  appliedDifficultyFilter.value = difficultyFilter.value;
  appliedApprovalFilter.value = approvalFilter.value;
  appliedLatestVersionFilter.value = latestVersionFilter.value;
  appliedScheduleFilter.value = scheduleFilter.value;
  appliedSortBy.value = sortBy.value;
  appliedSortDirection.value = sortDirection.value;
  await load(true);
  mobileFiltersOpen.value = false;
}

async function resetFilters() {
  wordFilter.value = defaultFilters.word;
  difficultyFilter.value = defaultFilters.difficulty;
  approvalFilter.value = defaultFilters.approval;
  latestVersionFilter.value = defaultFilters.latestVersion;
  scheduleFilter.value = defaultFilters.schedule;
  sortBy.value = defaultFilters.sortBy;
  sortDirection.value = defaultFilters.sortDirection;
  await applyFilters();
}

async function runListAction(
  action: "reset" | "apply" | "retry",
  task: () => Promise<void>,
) {
  if (loading.value) return;

  listAction.value = action;
  try {
    await task();
  } finally {
    listAction.value = null;
  }
}

async function confirmReviewChange() {
  const item = pendingReview.value;
  if (!item) return;
  savingReview.value = true;
  try {
    await reviewDailyGameSource(item.gameSource.gameSourceId, !item.approved);
    showToast({ status: "success", title: "Решение", message: "Статус решения обновлён." });
    pendingReview.value = null;
    await load(true);
  } catch (caught) {
    const apiError = toApiError(caught);
    showToast({
      status: "error",
      title: "Решение",
      message: apiError.code === "AlreadyAssignedToGameLocked"
        ? "Слово назначено в расписании и не может быть отклонено"
        : "Не удалось обновить решение",
    });
  } finally {
    savingReview.value = false;
  }
}

function closeReviewDialog() {
  if (!savingReview.value) {
    pendingReview.value = null;
  }
}

async function loadDifficulties() {
  difficultyAbortController?.abort();
  const abortController = new AbortController();
  difficultyAbortController = abortController;
  difficultyLoadFailed.value = false;

  try {
    difficulties.value = await getDifficulties({ signal: abortController.signal });
  } catch (caught) {
    if (isApiRequestCanceled(caught)) {
      return;
    }

    difficulties.value = [];
    difficultyFilter.value = "";
    appliedDifficultyFilter.value = "";
    difficultyLoadFailed.value = true;
    showToast({ status: "error", title: "Сложность", message: "Не удалось загрузить список сложностей." });
  } finally {
    if (difficultyAbortController === abortController) {
      difficultyAbortController = null;
    }
  }
}

onMounted(() => {
  void loadDifficulties();
});

onBeforeUnmount(() => {
  difficultyAbortController?.abort();
});
</script>

<template>
  <div class="admin-page">
    <header class="admin-page__header">
      <div>
        <p class="admin-page__eyebrow">Слова дня</p>
        <h1>Рассмотренные слова</h1>
      </div>
    </header>

    <UiButton
      class="admin-filters-toggle"
      size="sm"
      variant="soft"
      :aria-expanded="mobileFiltersOpen"
      aria-controls="reviewed-words-filters"
      @click="mobileFiltersOpen = !mobileFiltersOpen"
    >
      <i class="pi pi-filter" aria-hidden="true"></i>
      <span>{{ mobileFiltersOpen ? "Скрыть" : "Фильтры" }}</span>
    </UiButton>

    <section
      id="reviewed-words-filters"
      class="admin-filters filters admin-collapsible-filters"
      :class="{ 'admin-collapsible-filters--closed': !mobileFiltersOpen }"
    >
      <label class="admin-field"><span>Слово целиком</span><input v-model="wordFilter" type="search"
          placeholder="Например, пример" /></label>
      <div class="admin-field"><label for="reviewed-difficulty-filter">Сложность</label><Select
          v-model="difficultyFilter" input-id="reviewed-difficulty-filter" class="admin-select" overlay-class="admin-select-overlay"
          :options="difficultyOptions" option-label="label" option-value="value"
          :disabled="difficultyLoadFailed" fluid /></div>
      <div class="admin-field"><label for="reviewed-status-filter">Статус</label><Select
          v-model="approvalFilter" input-id="reviewed-status-filter" class="admin-select" overlay-class="admin-select-overlay"
          :options="approvalOptions" option-label="label" option-value="value" fluid /></div>
      <div class="admin-field"><label for="reviewed-version-filter">Есть в последней версии</label><Select
          v-model="latestVersionFilter" input-id="reviewed-version-filter" class="admin-select" overlay-class="admin-select-overlay"
          :options="latestVersionOptions" option-label="label" option-value="value" fluid /></div>
      <div class="admin-field"><label for="reviewed-schedule-filter">Слово дня</label><Select
          v-model="scheduleFilter" input-id="reviewed-schedule-filter" class="admin-select" overlay-class="admin-select-overlay"
          :options="scheduleOptions" option-label="label" option-value="value" fluid /></div>
      <div class="admin-field"><label for="reviewed-sort-filter">Сортировка</label><Select
          v-model="sortBy" input-id="reviewed-sort-filter" class="admin-select" overlay-class="admin-select-overlay"
          :options="sortOptions" option-label="label" option-value="value" fluid /></div>
      <div class="admin-field"><label for="reviewed-order-filter">Порядок</label><Select
          v-model="sortDirection" input-id="reviewed-order-filter" class="admin-select" overlay-class="admin-select-overlay"
          :options="sortDirectionOptions" option-label="label" option-value="value" fluid /></div>
      <div class="admin-actions filters__actions">
        <UiButton variant="outlined" :loading="listAction === 'reset'" :disabled="loading"
          @click="runListAction('reset', resetFilters)">Сбросить</UiButton>
        <UiButton :loading="listAction === 'apply'" :disabled="loading"
          @click="runListAction('apply', applyFilters)">Применить</UiButton>
      </div>
    </section>

    <UiSkeletonHandoff
      :skeleton-visible="initialLoadingState.visible"
      :content-visible="initialLoadingState.ready"
    >
      <template #skeleton>
        <AdminListSkeleton :count="5" :columns="2" />
      </template>
      <div v-if="loadError && reviews.length === 0" class="empty-state">
        <p>{{ loadError }}</p>
        <UiButton variant="outlined" :loading="listAction === 'retry'"
          @click="runListAction('retry', () => load(true))">Повторить</UiButton>
      </div>
      <section v-else class="review-list">
      <article v-for="item in reviews" :key="item.gameSource.gameSourceId" class="review-card">
        <div class="review-card__main">
          <div><strong>{{ item.gameSource.word }}</strong><span>{{ item.gameSource.difficulty.name }} · #{{
            item.gameSource.gameSourceId }}</span></div>
          <div class="review-card__statuses">
            <span v-if="!item.existsInLatestVersion" class="review-status review-status--outdated">Устарело</span>
            <span class="review-status"
              :class="item.scheduled ? 'review-status--scheduled' : 'review-status--unscheduled'">{{ item.scheduled ?
                "Слово дня: назначено" : "Слово дня: нет" }}</span>
            <span class="review-status"
              :class="item.approved ? 'review-status--approved' : 'review-status--rejected'">{{ item.approved ?
                "Одобрено" : "Отклонено" }}</span>
          </div>
        </div>
        <div class="review-card__meta">
          <span>Изменено {{ new Date(item.updatedAt).toLocaleString('ru-RU') }}</span>
          <UiButton v-if="canReview" variant="outlined" @click="pendingReview = item">{{ item.approved ? "Отклонить" :
            "Одобрить" }}</UiButton>
        </div>
      </article>
      <div v-if="reviews.length === 0" class="empty-state">По выбранным фильтрам ничего не найдено.</div>
      <div ref="sentinel" class="sentinel">
        <AdminListSkeleton v-if="listLoadingState.visible" :count="1" :columns="2" />
        <template v-else-if="loadError">
          <span>{{ loadError }}</span>
          <UiButton variant="outlined" :loading="listAction === 'retry'"
            @click="runListAction('retry', retryLoadMore)">Повторить</UiButton>
        </template>
      </div>
      </section>
    </UiSkeletonHandoff>

    <Dialog :visible="pendingReview !== null" modal :closable="!savingReview"
      :close-on-escape="!savingReview"
      :content-props="{ 'aria-busy': savingReview ? 'true' : undefined }"
      header="Изменить решение?" class="admin-dialog"
      @update:visible="(value) => !value && closeReviewDialog()">
      <p v-if="pendingReview" class="confirm-text">
        {{ pendingReview.approved ? "Отклонить" : "Одобрить" }} слово «{{ pendingReview.gameSource.word }}»?
        <span v-if="pendingReview.approved">Если оно уже назначено, backend запретит действие.</span>
      </p>
      <div class="dialog-actions">
        <UiButton :loading="savingReview" @click="confirmReviewChange">
          Подтвердить
        </UiButton>
        <UiButton variant="outlined" :disabled="savingReview" @click="closeReviewDialog">Отмена</UiButton>
      </div>
    </Dialog>
  </div>
</template>

<style scoped>
.filters {
  padding: 12px;
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  align-items: end;
  gap: 9px;
}

.filters label:first-child {
  grid-column: span 2;
}

.filters__actions {
  grid-column: 1 / -1;
  display: flex;
  gap: 8px;
  justify-content: flex-end;
}

.review-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.review-card {
  border: 1px solid var(--color-gray-200);
  border-radius: 8px;
  padding: 12px;
  background: white;
}

.review-card__main,
.review-card__meta {
  min-width: 0;
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 12px;
}

.review-card__main>div {
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 3px;
}

.review-card__main strong {
  font-size: 17px;
}

.review-card__main span,
.review-card__meta {
  color: var(--p-text-muted-color);
  font-size: 13px;
}

.review-card__meta {
  margin-top: 12px;
  padding-top: 10px;
  border-top: 1px solid var(--color-gray-100);
}

.review-card__meta>span {
  min-width: 0;
}

.review-card__statuses {
  display: flex;
  flex-wrap: wrap;
  justify-content: flex-end;
  gap: 6px;
}

.review-card__statuses .review-status {
  border-radius: 999px;
  padding: 5px 9px;
  font-size: 12px;
  font-weight: 500;
}

.review-card__statuses .review-status--approved {
  background: var(--color-green-200);
  color: #18563e;
}

.review-card__statuses .review-status--rejected {
  background: var(--color-red-100);
  color: var(--color-red-700);
}

.review-card__statuses .review-status--outdated {
  background: var(--color-gray-100);
  color: var(--color-gray-600);
}

.review-card__statuses .review-status--scheduled {
  background: var(--color-primary-100);
  color: var(--color-primary-700);
}

.review-card__statuses .review-status--unscheduled {
  background: var(--color-gray-100);
  color: var(--color-gray-600);
}

.loading,
.empty-state,
.sentinel {
  min-height: 100px;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  color: var(--color-gray-600);
}

.confirm-text {
  max-width: 440px;
  display: flex;
  flex-direction: column;
  gap: 8px;
  line-height: 1.5;
}

.confirm-text span {
  color: var(--p-text-muted-color);
  font-size: 14px;
}

@container admin-content (max-width: 1040px) {
  .filters {
    grid-template-columns: repeat(3, minmax(0, 1fr));
  }
}

@container admin-content (max-width: 760px) {
  .filters {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .filters label:first-child {
    grid-column: 1 / -1;
  }
}

@container admin-content (max-width: 520px) {
  .filters {
    grid-template-columns: 1fr;
  }

  .filters label:first-child {
    grid-column: auto;
  }

  .review-card__meta {
    align-items: flex-start;
    flex-direction: column;
  }

  .filters__actions {
    justify-content: stretch;
  }

  .filters__actions :deep(.p-button) {
    flex: 1;
  }
}
</style>
