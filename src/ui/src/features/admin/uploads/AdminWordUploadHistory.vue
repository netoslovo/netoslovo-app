<script setup lang="ts">
import DatePicker from "primevue/datepicker";
import Select from "primevue/select";
import { computed, ref } from "vue";
import { usePagedList } from "../../../shared/composables/usePagedList";
import { useHandoffDelayedLoadingState } from "../../../shared/composables/useSkeletonHandoff";
import UiButton from "../../../shared/ui/UiButton.vue";
import UiSkeletonHandoff from "../../../shared/ui/UiSkeletonHandoff.vue";
import { getWordsVersionUploadHistory } from "../api/sagasApi";
import { formatLocalDate, parseLocalDate } from "../lib/date";
import {
  formatUploadDateTime,
  shortUploadSagaId,
  uploadStepLabels,
} from "../lib/wordUploads";
import type {
  WordsVersionUpload,
  WordsVersionUploadHistorySortField,
  WordsVersionUploadSortDirection,
} from "../model/wordUploads";
import AdminListSkeleton from "../common/AdminListSkeleton.vue";
import AdminWordUploadStatus from "./AdminWordUploadStatus.vue";

defineProps<{
  selectedSagaId: string | null;
}>();

const emit = defineEmits<{
  select: [upload: WordsVersionUpload];
}>();

const historyPageSize = 20;
const sortOptions = [
  { label: "Создана", value: "CreatedAt" },
  { label: "Завершена", value: "CompletedAt" },
  { label: "Версия", value: "WordsVersion" },
  { label: "Статус", value: "State" },
];
const sortDirectionOptions = [
  { label: "По убыванию", value: "Desc" },
  { label: "По возрастанию", value: "Asc" },
];
const versionFilter = ref<number | null>(null);
const fromFilter = ref("");
const toFilter = ref("");
const sortBy = ref<WordsVersionUploadHistorySortField>("CreatedAt");
const sortDirection = ref<WordsVersionUploadSortDirection>("Desc");
const appliedVersionFilter = ref<number | null>(null);
const appliedFromFilter = ref("");
const appliedToFilter = ref("");
const appliedSortBy = ref<WordsVersionUploadHistorySortField>("CreatedAt");
const appliedSortDirection = ref<WordsVersionUploadSortDirection>("Desc");
const action = ref<"reset" | "apply" | "retry" | null>(null);
const failedReset = ref(false);
const mobileFiltersOpen = ref(false);

const pagedList = usePagedList<WordsVersionUpload>({
  pageSize: historyPageSize,
  rootMargin: "260px",
  preserveItemsOnReset: true,
  fillVisibleSentinel: true,
  loadPage: async (skip, take, signal) => {
    const page = await getWordsVersionUploadHistory(
      {
        skip,
        take,
        from: dateFilterToIso(appliedFromFilter.value, false),
        to: dateFilterToIso(appliedToFilter.value, true),
        wordsVersion: appliedVersionFilter.value ?? undefined,
        sortBy: appliedSortBy.value,
        sortDirection: appliedSortDirection.value,
      },
      { signal },
    );
    return { items: page.uploads, hasMore: page.hasMore };
  },
  getErrorMessage: () => "Не удалось загрузить историю",
});

const historyUploads = pagedList.items;
const historyLoading = pagedList.loading;
const historyLoadError = pagedList.loadError;
const historyHasMore = pagedList.hasMore;
const historySentinel = pagedList.sentinel;
const loadingState = useHandoffDelayedLoadingState(historyLoading);
const fromDate = computed<Date | null>({
  get: () => fromFilter.value ? parseLocalDate(fromFilter.value) : null,
  set: (value) => {
    fromFilter.value = value instanceof Date ? formatLocalDate(value) : "";
  },
});
const toDate = computed<Date | null>({
  get: () => toFilter.value ? parseLocalDate(toFilter.value) : null,
  set: (value) => {
    toFilter.value = value instanceof Date ? formatLocalDate(value) : "";
  },
});

async function refresh() {
  await pagedList.load(true);
  failedReset.value = historyLoadError.value !== null;
}

async function applyFilters() {
  appliedVersionFilter.value = versionFilter.value;
  appliedFromFilter.value = fromFilter.value;
  appliedToFilter.value = toFilter.value;
  appliedSortBy.value = sortBy.value;
  appliedSortDirection.value = sortDirection.value;
  await refresh();
  mobileFiltersOpen.value = false;
}

async function resetFilters() {
  versionFilter.value = null;
  fromFilter.value = "";
  toFilter.value = "";
  sortBy.value = "CreatedAt";
  sortDirection.value = "Desc";
  await applyFilters();
}

async function retry() {
  if (failedReset.value) {
    await refresh();
    return;
  }

  await pagedList.retry();
}

async function runAction(
  currentAction: "reset" | "apply" | "retry",
  task: () => Promise<void>,
) {
  if (historyLoading.value) return;

  action.value = currentAction;
  try {
    await task();
  } finally {
    action.value = null;
  }
}

function dateFilterToIso(value: string, endOfDay: boolean) {
  if (!value) return undefined;
  return new Date(
    `${value}T${endOfDay ? "23:59:59.999" : "00:00:00.000"}`,
  ).toISOString();
}

defineExpose({ refresh });
</script>

<template>
  <section class="admin-panel panel history-panel">
    <div class="admin-panel__header panel__header">
      <h2>История загрузок</h2>
      <UiButton
        class="admin-filters-toggle"
        size="sm"
        variant="soft"
        :aria-expanded="mobileFiltersOpen"
        aria-controls="upload-history-filters"
        @click="mobileFiltersOpen = !mobileFiltersOpen"
      >
        <i class="pi pi-filter" aria-hidden="true"></i>
        <span>{{ mobileFiltersOpen ? "Скрыть" : "Фильтры" }}</span>
      </UiButton>
    </div>

    <section
      id="upload-history-filters"
      class="history-filters admin-collapsible-filters"
      :class="{ 'admin-collapsible-filters--closed': !mobileFiltersOpen }"
    >
      <label class="admin-field">
        <span>Версия</span>
        <input v-model.number="versionFilter" type="number" min="1" step="1" />
      </label>
      <div class="admin-field history-filter-field">
        <label for="upload-history-from-date">С даты</label>
        <DatePicker
          v-model="fromDate"
          input-id="upload-history-from-date"
          class="admin-date-picker"
          date-format="yy-mm-dd"
          show-icon
          icon-display="input"
          fluid
        />
      </div>
      <div class="admin-field history-filter-field">
        <label for="upload-history-to-date">По дату</label>
        <DatePicker
          v-model="toDate"
          input-id="upload-history-to-date"
          class="admin-date-picker"
          date-format="yy-mm-dd"
          show-icon
          icon-display="input"
          fluid
        />
      </div>
      <div class="admin-field">
        <label for="upload-history-sort">Сортировка</label>
        <Select v-model="sortBy" input-id="upload-history-sort" class="admin-select" overlay-class="admin-select-overlay"
          :options="sortOptions" option-label="label" option-value="value" fluid />
      </div>
      <div class="admin-field">
        <label for="upload-history-order">Порядок</label>
        <Select v-model="sortDirection" input-id="upload-history-order" class="admin-select" overlay-class="admin-select-overlay"
          :options="sortDirectionOptions" option-label="label" option-value="value" fluid />
      </div>
      <div class="admin-actions history-filters__actions">
        <UiButton
          variant="outlined"
          :loading="action === 'reset'"
          :disabled="historyLoading"
          @click="runAction('reset', resetFilters)"
        >
          Сбросить
        </UiButton>
        <UiButton
          :loading="action === 'apply'"
          :disabled="historyLoading"
          @click="runAction('apply', applyFilters)"
        >
          Применить
        </UiButton>
      </div>
    </section>

    <UiSkeletonHandoff
      :skeleton-visible="loadingState.visible && historyUploads.length === 0"
      :content-visible="loadingState.ready || historyUploads.length > 0"
    >
      <template #skeleton>
        <AdminListSkeleton :count="5" :columns="5" />
      </template>
      <div
        v-if="historyLoadError && historyUploads.length === 0"
        class="inline-error"
      >
        <span>{{ historyLoadError }}</span>
        <UiButton
          variant="outlined"
          :loading="action === 'retry'"
          @click="runAction('retry', retry)"
        >
          Повторить
        </UiButton>
      </div>
      <div v-else-if="historyUploads.length === 0" class="empty-state">
        История загрузок пуста.
      </div>
      <div v-else class="history-list">
        <div class="history-list__header" aria-hidden="true">
          <span>Версия</span>
          <span>Статус</span>
          <span>Текущий шаг</span>
          <span>Создана</span>
          <span>Завершена</span>
          <span>ID загрузки</span>
        </div>
        <article
          v-for="upload in historyUploads"
          :key="upload.sagaId"
          class="history-row"
          :class="{ 'history-row--selected': upload.sagaId === selectedSagaId }"
        >
          <button type="button" class="admin-native-action" @click="emit('select', upload)">
            <strong>Версия {{ upload.wordsVersion }}</strong>
            <AdminWordUploadStatus :state="upload.state" />
            <span>{{ uploadStepLabels[upload.currentStep] }}</span>
            <span>{{ formatUploadDateTime(upload.createdAt) }}</span>
            <span>{{ formatUploadDateTime(upload.completedAt) }}</span>
            <code :title="upload.sagaId">{{ shortUploadSagaId(upload.sagaId) }}</code>
          </button>
        </article>
      </div>
    </UiSkeletonHandoff>

    <div
      v-if="historyLoading && historyUploads.length > 0 || historyLoadError || historyHasMore"
      class="history-more"
    >
      <AdminListSkeleton
        v-if="loadingState.visible && historyUploads.length > 0"
        :count="1"
        :columns="5"
      />
      <template v-else-if="historyLoadError">
        <span>{{ historyLoadError }}</span>
        <UiButton
          variant="outlined"
          :loading="action === 'retry'"
          @click="runAction('retry', retry)"
        >
          Повторить
        </UiButton>
      </template>
      <div
        v-else-if="historyHasMore"
        ref="historySentinel"
        class="history-more__sentinel"
        aria-hidden="true"
      ></div>
    </div>
  </section>
</template>

<style scoped>
.panel {
  min-width: 0;
  padding: 12px;
}

.panel__header {
  min-height: 34px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}

.history-filters {
  margin: 8px 0 12px;
  display: grid;
  grid-template-columns: repeat(5, minmax(0, 1fr));
  align-items: end;
  gap: 9px;
}

.history-filters__actions {
  grid-column: 1 / -1;
  display: flex;
  justify-content: flex-end;
  gap: 8px;
}

.history-list {
  --history-status-width: 96px;
  --history-columns:
    minmax(90px, 0.8fr) var(--history-status-width) minmax(160px, 1.2fr) minmax(170px, 1fr) minmax(170px, 1fr) minmax(110px, 0.8fr);
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.history-row {
  border: 1px solid var(--color-gray-200);
  border-radius: 8px;
  background: white;
}

.history-row--selected {
  border-color: var(--color-primary-300);
  outline: 1px solid var(--color-primary-100);
  outline-offset: -1px;
}

.history-row button {
  width: 100%;
  border: 0;
  padding: 11px;
  display: grid;
  grid-template-columns: var(--history-columns);
  align-items: center;
  gap: 10px;
  background: transparent;
  color: var(--color-gray-700);
  text-align: left;
  cursor: pointer;
}

.history-list__header {
  padding: 0 11px;
  display: grid;
  grid-template-columns: var(--history-columns);
  align-items: center;
  gap: 10px;
  color: var(--p-text-muted-color);
  font-size: 12px;
  font-weight: 500;
}

.history-row :deep(.upload-status) {
  width: var(--history-status-width);
  text-align: center;
}

.history-row strong {
  color: var(--color-gray-900);
}

.history-row span {
  min-width: 0;
  color: var(--p-text-muted-color);
  font-size: 13px;
}

.history-row code {
  max-width: 100%;
  color: var(--p-text-muted-color);
  font-family: ui-monospace, SFMono-Regular, Menlo, Consolas, monospace;
  font-size: 12px;
  overflow-wrap: anywhere;
}

.empty-state,
.inline-error,
.history-more {
  min-height: 58px;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  color: var(--color-gray-600);
}

.inline-error {
  color: var(--color-red-700);
}

@container admin-content (max-width: 1080px) {
  .history-filters,
  .history-row button {
    grid-template-columns: repeat(3, minmax(0, 1fr));
  }

  .history-list__header {
    display: none;
  }
}

@container admin-content (max-width: 680px) {
  .history-filters,
  .history-row button {
    grid-template-columns: 1fr;
  }

  .history-filters__actions {
    align-items: stretch;
    flex-direction: column;
  }

  .history-filters__actions :deep(.p-button) {
    width: 100%;
  }
}
</style>
