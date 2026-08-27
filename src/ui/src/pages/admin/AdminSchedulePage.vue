<script setup lang="ts">
import Dialog from "primevue/dialog";
import DatePicker from "primevue/datepicker";
import Select from "primevue/select";
import { computed, ref } from "vue";
import { assignSourceToDay, getSourceByWord, reviewDailyGameSource, unassignSourceFromDay } from "../../features/admin/api/adminApi";
import { useAdminSchedule } from "../../features/admin/schedule/useAdminSchedule";
import { toApiError } from "../../shared/api/apiError";
import { useDelayedLoadingState } from "../../shared/composables/useDelayedLoading";
import { useHandoffDelayedLoadingState } from "../../shared/composables/useSkeletonHandoff";
import type { AdminGameSourceWithReview, DailyGameSchedule, SortDirection } from "../../features/admin/model/admin";
import { addDays, formatAdminDate, formatLocalDate, formatWeekday, parseLocalDate } from "../../features/admin/lib/date";
import { adminPermissions, hasPermission } from "../../features/auth/model/permissions";
import { useAdminSourceSearch } from "../../features/admin/common/useAdminSourceSearch";
import AdminAutoAssignDialog from "../../features/admin/schedule/AdminAutoAssignDialog.vue";
import UiButton from "../../shared/ui/UiButton.vue";
import UiIconButton from "../../shared/ui/UiIconButton.vue";
import AdminListSkeleton from "../../features/admin/common/AdminListSkeleton.vue";
import UiSkeletonHandoff from "../../shared/ui/UiSkeletonHandoff.vue";
import { authState } from "../../features/auth/model/authSession";
import { showToast } from "../../shared/notifications/toastStore";

const today = formatLocalDate(new Date());
const assignedOptions = [
  { label: "Все дни", value: "all" },
  { label: "Назначенные", value: "assigned" },
  { label: "Пустые", value: "empty" },
];
const sortDirectionOptions = [
  { label: "Сначала ранние", value: "Asc" },
  { label: "Сначала поздние", value: "Desc" },
];
const defaultFilters = {
  from: addDays(today, -7),
  to: addDays(today, 7),
  assigned: "all",
  sortDirection: "Asc" as SortDirection,
};
const from = ref(defaultFilters.from);
const to = ref(defaultFilters.to);
const assignedFilter = ref(defaultFilters.assigned);
const sortDirection = ref<SortDirection>(defaultFilters.sortDirection);
const appliedFrom = ref(from.value);
const appliedTo = ref(to.value);
const appliedAssignedFilter = ref(assignedFilter.value);
const appliedSortDirection = ref(sortDirection.value);
const revealedDays = ref(new Set<string>());
const revealAll = ref(false);
const spoilerDialogOpen = ref(false);
const autoAssignOpen = ref(false);
const assignmentDay = ref<DailyGameSchedule | null>(null);
const foundSource = ref<AdminGameSourceWithReview | null>(null);
const assigning = ref(false);
const unassignDay = ref<DailyGameSchedule | null>(null);
const unassigning = ref(false);
const listAction = ref<"reset" | "apply" | "retry" | null>(null);
const mobileFiltersOpen = ref(false);

const {
  schedules,
  loading,
  initialLoading,
  loadError,
  sentinel,
  load: loadPage,
  retryLoadMore,
} = useAdminSchedule(() => ({
  from: appliedFrom.value,
  to: appliedTo.value,
  sortDirection: appliedSortDirection.value,
  assignedFilter: appliedAssignedFilter.value === "all" ? undefined : appliedAssignedFilter.value === "assigned",
}));
const initialLoadingState = useHandoffDelayedLoadingState(initialLoading);
const listLoadingState = useDelayedLoadingState(loading);

const canAssign = computed(() => hasPermission(authState.value, adminPermissions.assignSource));
const canReview = computed(() => hasPermission(authState.value, adminPermissions.setSourceReview));
const canUnassign = computed(() => hasPermission(authState.value, adminPermissions.unassignSource));
const emptyCount = computed(() => schedules.value.filter((item) => !item.gameSource).length);
const {
  searchWord: assignmentWord,
  searchError: assignmentError,
  searching,
  search: searchWord,
  cancel: cancelSearch,
} = useAdminSourceSearch({
  emptyMessage: "Введите слово целиком",
  notFoundMessage: "Слово не найдено",
  errorMessage: "Не удалось проверить слово",
  load: (word, signal) => getSourceByWord(word, { signal }),
  onBeforeSearch: () => {
    foundSource.value = null;
  },
  onFound: (result) => {
    foundSource.value = result;
  },
});
const fromDate = computed<Date | null>({
  get: () => (from.value ? parseLocalDate(from.value) : null),
  set: (value) => {
    if (value instanceof Date) {
      from.value = formatLocalDate(value);
    }
  },
});
const toDate = computed<Date | null>({
  get: () => (to.value ? parseLocalDate(to.value) : null),
  set: (value) => {
    if (value instanceof Date) {
      to.value = formatLocalDate(value);
    }
  },
});

async function load(reset = false) {
  if (!appliedFrom.value || !appliedTo.value || appliedFrom.value > appliedTo.value) {
    loadError.value = "Укажите корректный диапазон дат";
    return;
  }
  await loadPage(reset);
}

async function applyFilters() {
  appliedFrom.value = from.value;
  appliedTo.value = to.value;
  appliedAssignedFilter.value = assignedFilter.value;
  appliedSortDirection.value = sortDirection.value;
  await resetAndLoad();
  mobileFiltersOpen.value = false;
}

async function resetFilters() {
  from.value = defaultFilters.from;
  to.value = defaultFilters.to;
  assignedFilter.value = defaultFilters.assigned;
  sortDirection.value = defaultFilters.sortDirection;
  await applyFilters();
}

async function resetAndLoad() {
  revealedDays.value = new Set();
  revealAll.value = false;
  await load(true);
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

function toggleDayReveal(day: string) {
  const next = new Set(revealedDays.value);
  if (next.has(day)) {
    next.delete(day);
  } else {
    next.add(day);
  }
  revealedDays.value = next;
}

function requestRevealAll() {
  if (revealAll.value) {
    revealAll.value = false;
    revealedDays.value = new Set();
  } else {
    spoilerDialogOpen.value = true;
  }
}

function confirmRevealAll() {
  revealAll.value = true;
  spoilerDialogOpen.value = false;
}

function openAssignment(item: DailyGameSchedule) {
  cancelSearch();
  assignmentDay.value = item;
  assignmentWord.value = "";
  foundSource.value = null;
  assignmentError.value = null;
}

function closeAssignment() {
  if (!assigning.value) {
    cancelSearch();
    assignmentDay.value = null;
  }
}

async function assignFoundSource() {
  const day = assignmentDay.value;
  const result = foundSource.value;
  if (!day || !result) return;
  assigning.value = true;
  assignmentError.value = null;
  let approvedDuringRequest = false;
  try {
    if (result.approved !== true) {
      if (!canReview.value) {
        assignmentError.value = "Нет разрешения на одобрение этого слова";
        return;
      }
      await reviewDailyGameSource(result.gameSource.gameSourceId, true);
      approvedDuringRequest = true;
    }
    await assignSourceToDay(day.day, result.gameSource.gameSourceId);
    showToast({ status: "success", title: "Расписание", message: `Слово назначено на ${formatAdminDate(day.day, true)}.` });
    assignmentDay.value = null;
    await load(true);
  } catch (caught) {
    const apiError = toApiError(caught);
    assignmentError.value = approvedDuringRequest
      ? "Слово одобрено, но назначить его не удалось. Повторите назначение."
      : apiError.code === "ConcurrencyConflict"
        ? "Слово уже используется или расписание изменилось. Обновите список."
        : "Не удалось назначить слово";
  } finally {
    assigning.value = false;
  }
}

async function confirmUnassign() {
  if (!unassignDay.value) return;
  unassigning.value = true;
  try {
    await unassignSourceFromDay(unassignDay.value.day);
    showToast({ status: "success", title: "Расписание", message: "Назначение удалено." });
    unassignDay.value = null;
    await load(true);
  } catch {
    showToast({ status: "error", title: "Расписание", message: "Не удалось снять назначение." });
  } finally {
    unassigning.value = false;
  }
}

function closeUnassignDialog() {
  if (!unassigning.value) {
    unassignDay.value = null;
  }
}

</script>

<template>
  <div class="admin-page">
    <header class="admin-page__header">
      <div><p class="admin-page__eyebrow">Слова дня</p><h1>Расписание</h1><p class="admin-page__description">Слова скрыты, пока вы сами их не откроете.</p></div>
      <UiButton v-if="canAssign" variant="outlined" @click="autoAssignOpen = true">Автоназначить</UiButton>
    </header>

    <UiButton
      class="admin-filters-toggle"
      size="sm"
      variant="soft"
      :aria-expanded="mobileFiltersOpen"
      aria-controls="schedule-filters"
      @click="mobileFiltersOpen = !mobileFiltersOpen"
    >
      <i class="pi pi-filter" aria-hidden="true"></i>
      <span>{{ mobileFiltersOpen ? "Скрыть" : "Фильтры" }}</span>
    </UiButton>

    <section
      id="schedule-filters"
      class="admin-filters schedule-filters admin-collapsible-filters"
      :class="{ 'admin-collapsible-filters--closed': !mobileFiltersOpen }"
    >
      <div class="admin-field schedule-filter-field">
        <label for="schedule-from-date">С даты</label>
        <DatePicker
          v-model="fromDate"
          input-id="schedule-from-date"
          class="admin-date-picker"
          date-format="yy-mm-dd"
          show-icon
          icon-display="input"
          fluid
        />
      </div>
      <div class="admin-field schedule-filter-field">
        <label for="schedule-to-date">По дату</label>
        <DatePicker
          v-model="toDate"
          input-id="schedule-to-date"
          class="admin-date-picker"
          date-format="yy-mm-dd"
          show-icon
          icon-display="input"
          fluid
        />
      </div>
      <div class="admin-field"><label for="schedule-assigned-filter">Показывать</label><Select
          v-model="assignedFilter" input-id="schedule-assigned-filter" class="admin-select" overlay-class="admin-select-overlay"
          :options="assignedOptions" option-label="label" option-value="value" fluid /></div>
      <div class="admin-field"><label for="schedule-order-filter">Порядок</label><Select
          v-model="sortDirection" input-id="schedule-order-filter" class="admin-select" overlay-class="admin-select-overlay"
          :options="sortDirectionOptions" option-label="label" option-value="value" fluid /></div>
      <div class="admin-actions schedule-filters__actions">
        <UiButton variant="outlined" :loading="listAction === 'reset'" :disabled="loading" @click="runListAction('reset', resetFilters)">Сбросить</UiButton>
        <UiButton :loading="listAction === 'apply'" :disabled="loading" @click="runListAction('apply', applyFilters)">Применить</UiButton>
      </div>
    </section>

    <div class="schedule-toolbar">
      <span>Загружено: {{ schedules.length }} · пустых: {{ emptyCount }}</span>
      <button type="button" class="admin-native-action" @click="requestRevealAll"><i :class="revealAll ? 'pi pi-eye-slash' : 'pi pi-eye'" aria-hidden="true"></i>{{ revealAll ? "Скрыть все слова" : "Показать все слова" }}</button>
    </div>

    <UiSkeletonHandoff
      :skeleton-visible="initialLoadingState.visible"
      :content-visible="initialLoadingState.ready"
    >
      <template #skeleton>
        <AdminListSkeleton :count="5" :columns="2" />
      </template>
      <div v-if="loadError && schedules.length === 0" class="empty-state"><p>{{ loadError }}</p><UiButton variant="outlined" :loading="listAction === 'retry'" @click="runListAction('retry', () => load(true))">Повторить</UiButton></div>
      <section v-else class="schedule-list">
      <article v-for="item in schedules" :key="item.day" class="schedule-card" :class="{ 'schedule-card--empty': !item.gameSource, 'schedule-card--locked': item.locked, 'schedule-card--today': item.day === today }">
        <div class="schedule-card__date"><strong>{{ formatAdminDate(item.day) }}</strong><span>{{ formatWeekday(item.day) }}</span><small v-if="item.day === today" class="schedule-card__today">Сегодня</small><small v-if="item.locked">Зафиксировано</small></div>
        <div class="schedule-card__source">
          <template v-if="item.gameSource">
            <strong>{{ revealAll || revealedDays.has(item.day) ? item.gameSource.word : "Назначено" }}</strong>
            <span>{{ revealAll || revealedDays.has(item.day) ? item.gameSource.difficulty.name : "Слово скрыто" }}</span>
          </template>
          <template v-else><strong>Не назначено</strong><span>Пустой день можно заполнить независимо от даты</span></template>
        </div>
        <div class="schedule-card__actions">
          <UiIconButton v-if="item.gameSource" size="sm" :label="revealedDays.has(item.day) ? 'Скрыть слово' : 'Показать слово'" @click="toggleDayReveal(item.day)"><i :class="revealedDays.has(item.day) ? 'pi pi-eye-slash' : 'pi pi-eye'" aria-hidden="true"></i></UiIconButton>
          <UiButton v-if="canAssign && (!item.gameSource || !item.locked)" variant="outlined" @click="openAssignment(item)">{{ item.gameSource ? "Заменить" : "Назначить" }}</UiButton>
          <UiButton v-if="canUnassign && item.gameSource && !item.locked" class="schedule-card__unassign" variant="ghost" @click="unassignDay = item">Снять</UiButton>
        </div>
      </article>
      <div v-if="schedules.length === 0" class="empty-state">В выбранном диапазоне нет подходящих дней.</div>
      <div ref="sentinel" class="sentinel">
        <AdminListSkeleton v-if="listLoadingState.visible" :count="1" :columns="2" />
        <template v-else-if="loadError">
          <span>{{ loadError }}</span>
          <UiButton variant="outlined" :loading="listAction === 'retry'" @click="runListAction('retry', retryLoadMore)">Повторить</UiButton>
        </template>
      </div>
      </section>
    </UiSkeletonHandoff>

    <Dialog :visible="assignmentDay !== null" modal :closable="!assigning" :close-on-escape="!assigning"
      :content-props="{ 'aria-busy': assigning ? 'true' : undefined }"
      :header="assignmentDay?.gameSource ? 'Заменить назначение' : 'Назначить слово'" class="admin-dialog" @update:visible="(value) => !value && closeAssignment()">
      <form class="assignment-form" @submit.prevent="searchWord">
        <p v-if="assignmentDay">Дата: <strong>{{ formatAdminDate(assignmentDay.day, true) }}</strong></p>
        <div class="assignment-form__field"><label for="schedule-assignment-word">Введите слово целиком</label><div><input id="schedule-assignment-word" v-model="assignmentWord" type="search" :disabled="assigning" placeholder="Слово" /><UiButton type="submit" variant="outlined" :loading="searching" :disabled="assigning">Проверить</UiButton></div></div>
        <div v-if="foundSource" class="found-source"><div><strong>{{ foundSource.gameSource.word }}</strong><span>{{ foundSource.gameSource.difficulty.name }}</span></div><span class="found-source__status" :class="{ 'found-source__status--approved': foundSource.approved === true }">{{ foundSource.approved === true ? "Одобрено" : foundSource.approved === false ? "Отклонено" : "Не рассмотрено" }}</span></div>
        <p v-if="foundSource?.approved !== true" class="assignment-warning">При продолжении слово будет сначала одобрено, затем назначено.</p>
        <p v-if="assignmentDay?.gameSource" class="assignment-warning">Текущее будущее назначение будет заменено.</p>
        <p v-if="assignmentError" class="assignment-error">{{ assignmentError }}</p>
        <div class="dialog-actions"><UiButton v-if="foundSource" :loading="assigning" :disabled="foundSource.approved !== true && !canReview" @click="assignFoundSource">{{ foundSource.approved === true ? "Назначить" : "Одобрить и назначить" }}</UiButton><UiButton variant="outlined" :disabled="assigning" @click="closeAssignment">Отмена</UiButton></div>
      </form>
    </Dialog>

    <Dialog :visible="unassignDay !== null" modal :closable="!unassigning" :close-on-escape="!unassigning" :content-props="{ 'aria-busy': unassigning ? 'true' : undefined }" header="Снять назначение?" class="admin-dialog" @update:visible="(value) => !value && closeUnassignDialog()"><p class="confirm-text">Будущее назначение на {{ unassignDay ? formatAdminDate(unassignDay.day, true) : "" }} будет удалено. Одобрение слова сохранится.</p><div class="dialog-actions"><UiButton :loading="unassigning" @click="confirmUnassign">Снять</UiButton><UiButton variant="outlined" :disabled="unassigning" @click="closeUnassignDialog">Отмена</UiButton></div></Dialog>
    <Dialog v-model:visible="spoilerDialogOpen" modal header="Показать все слова?" class="admin-dialog"><p class="confirm-text">Это раскроет слова выбранного диапазона и может испортить вашу игру.</p><div class="dialog-actions"><UiButton @click="confirmRevealAll">Показать</UiButton><UiButton variant="outlined" @click="spoilerDialogOpen = false">Отмена</UiButton></div></Dialog>
    <AdminAutoAssignDialog v-model:visible="autoAssignOpen" :initial-from="from" :initial-to="to" @completed="load(true)" />
  </div>
</template>

<style scoped>
.schedule-filters { padding: 12px; display: grid; grid-template-columns: repeat(4,minmax(130px,1fr)) auto; align-items: end; gap: 9px; }
.schedule-filters label,
.schedule-filter-field { display: flex; flex-direction: column; gap: 5px; color: var(--color-gray-600); font-size: 13px; }
.schedule-filters__actions { display: flex; justify-content: flex-end; gap: 8px; }
.schedule-toolbar { min-height: 42px; display: flex; align-items: center; justify-content: space-between; flex-wrap: wrap; gap: 10px; color: var(--p-text-muted-color); font-size: 14px; }
.schedule-toolbar span { min-width: 0; }
.schedule-toolbar button { border: 0; display: inline-flex; align-items: center; gap: 7px; background: transparent; color: var(--color-primary-700); text-align: left; cursor: pointer; }
.schedule-list { display: flex; flex-direction: column; gap: 8px; }
.schedule-card { min-height: 78px; border: 1px solid var(--color-gray-200); border-radius: 8px; padding: 12px; display: grid; grid-template-columns: 150px minmax(0,1fr) auto; align-items: center; gap: 14px; background: white; }
.schedule-card--empty { border-style: dashed; background: var(--color-gray-50); }
.schedule-card--locked { background: #fbfbfb; }
.schedule-card--today { border-color: var(--color-primary-600); box-shadow: inset 3px 0 0 var(--color-primary-600); }
.schedule-card__date,.schedule-card__source { min-width: 0; display: flex; flex-direction: column; gap: 3px; }
.schedule-card__date span,.schedule-card__date small,.schedule-card__source span { color: var(--p-text-muted-color); font-size: 13px; }
.schedule-card__date small { color: var(--color-red-700); }
.schedule-card__date .schedule-card__today { color: var(--color-primary-700); font-weight: 500; }
.schedule-card__source strong { font-size: 17px; }
.schedule-card--empty .schedule-card__source strong { color: var(--color-red-700); }
.schedule-card__actions { min-width: 0; display: flex; align-items: center; justify-content: flex-end; flex-wrap: wrap; gap: 6px; }
.schedule-card__unassign.p-button { background: var(--color-red-100); color: var(--color-red-700); }
.schedule-card__unassign.p-button:not(:disabled):hover { background: var(--color-red-200); color: var(--color-red-700); }
.loading,.empty-state,.sentinel { min-height: 110px; display: flex; align-items: center; justify-content: center; gap: 10px; color: var(--color-gray-600); }
.assignment-form { min-width: min(520px,calc(100vw - 48px)); display: flex; flex-direction: column; gap: 14px; }
.assignment-form > p { margin: 0; }
.assignment-form__field { display: flex; flex-direction: column; gap: 6px; color: var(--color-gray-600); font-size: 14px; }
.assignment-form__field > div { display: flex; gap: 8px; }
.assignment-form input { flex: 1; min-width: 0; padding: 0 10px; }
.found-source { border: 1px solid var(--color-gray-200); border-radius: 8px; padding: 12px; display: flex; align-items: center; justify-content: space-between; gap: 10px; }
.found-source > div { display: flex; flex-direction: column; gap: 3px; }
.found-source > div span { color: var(--p-text-muted-color); font-size: 13px; }
.found-source__status { border-radius: 999px; padding: 5px 9px; background: var(--color-red-100); color: var(--color-red-700); font-size: 12px; font-weight: 500; }
.found-source__status--approved { background: var(--color-green-200); color: #18563e; }
.assignment-warning { border-radius: 8px; padding: 10px; background: var(--color-gray-50); color: var(--color-gray-700); font-size: 14px; }
.assignment-error { color: var(--color-red-700); }
.confirm-text { max-width: 440px; line-height: 1.5; }
@container admin-content (max-width: 1120px) { .schedule-filters { grid-template-columns: repeat(2,minmax(0,1fr)); } .schedule-filters__actions { grid-column: 1 / -1; } }
@container admin-content (max-width: 760px) { .schedule-card { grid-template-columns: 125px minmax(0,1fr); } .schedule-card__actions { grid-column: 1 / -1; justify-content: flex-start; } }
@container admin-content (max-width: 560px) { .schedule-filters { grid-template-columns: 1fr; } .schedule-filters__actions { justify-content: stretch; } .schedule-filters__actions :deep(.p-button) { flex: 1; } .schedule-card { grid-template-columns: 1fr; gap: 9px; } .schedule-card__actions { grid-column: auto; flex-wrap: wrap; } .assignment-form__field > div { flex-direction: column; } }
</style>
