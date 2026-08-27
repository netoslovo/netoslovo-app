<script setup lang="ts">
import Dialog from "primevue/dialog";
import DatePicker from "primevue/datepicker";
import { computed, onBeforeUnmount, ref, watch } from "vue";
import {
  autoAssignSources,
  getApprovedUnassignedDailySourcesCount,
  getUnassignedDailyScheduleCount,
} from "../api/adminApi";
import { isApiRequestCanceled, toApiError } from "../../../shared/api/apiError";
import { countDays, formatLocalDate, parseLocalDate } from "../lib/date";
import { showToast } from "../../../shared/notifications/toastStore";
import UiButton from "../../../shared/ui/UiButton.vue";
import UiSkeleton from "../../../shared/ui/UiSkeleton.vue";

const MAX_AUTO_ASSIGN_DAYS = 365;

const props = withDefaults(
  defineProps<{
    visible: boolean;
    initialFrom: string;
    initialTo: string;
  }>(),
  {},
);

const emit = defineEmits<{
  "update:visible": [value: boolean];
  completed: [];
}>();

const from = ref(props.initialFrom);
const to = ref(props.initialTo);
const submitting = ref(false);
const countingEmptyDays = ref(false);
const emptyDaysCount = ref<number | null>(null);
const countingApprovedSources = ref(false);
const approvedSourcesCount = ref<number | null>(null);
const error = ref<string | null>(null);
let countRequestId = 0;
let countTimeoutId: number | null = null;
let countAbortController: AbortController | null = null;
let approvedSourcesRequestId = 0;
let approvedSourcesAbortController: AbortController | null = null;

const intervalDays = computed(() =>
  from.value && to.value && from.value <= to.value
    ? countDays(from.value, to.value)
    : 0,
);
const validInterval = computed(
  () => intervalDays.value > 0 && intervalDays.value <= MAX_AUTO_ASSIGN_DAYS,
);
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

watch(
  () => props.visible,
  (visible) => {
    if (!visible) {
      cancelEmptyDaysCount();
      cancelApprovedSourcesCount();
      return;
    }

    from.value = props.initialFrom;
    to.value = props.initialTo;
    error.value = null;
    void loadApprovedSourcesCount();
    scheduleEmptyDaysCount();
  },
);

watch([from, to], () => {
  if (!props.visible) return;
  scheduleEmptyDaysCount();
});

function close() {
  if (!submitting.value) {
    cancelEmptyDaysCount();
    cancelApprovedSourcesCount();
    emit("update:visible", false);
  }
}

function scheduleEmptyDaysCount() {
  cancelEmptyDaysCount();

  countTimeoutId = window.setTimeout(() => {
    countTimeoutId = null;
    void loadEmptyDaysCount();
  }, 250);
}

async function loadEmptyDaysCount() {
  const requestId = ++countRequestId;
  const abortController = new AbortController();
  countAbortController = abortController;
  emptyDaysCount.value = null;

  if (!validInterval.value) {
    countAbortController = null;
    return;
  }

  countingEmptyDays.value = true;

  try {
    const count = await getUnassignedDailyScheduleCount(
      from.value,
      to.value,
      {
        signal: abortController.signal,
      },
    );

    if (requestId === countRequestId) {
      emptyDaysCount.value = count;
    }
  } catch (caught) {
    if (isApiRequestCanceled(caught)) {
      return;
    }

    if (requestId === countRequestId) {
      emptyDaysCount.value = null;
      showToast({
        status: "error",
        title: "Автоназначение",
        message: "Не удалось загрузить данные.",
      });
    }
  } finally {
    if (requestId === countRequestId) {
      countingEmptyDays.value = false;
    }
    if (countAbortController === abortController) {
      countAbortController = null;
    }
  }
}

async function loadApprovedSourcesCount() {
  const requestId = ++approvedSourcesRequestId;
  const abortController = new AbortController();
  approvedSourcesAbortController = abortController;
  approvedSourcesCount.value = null;
  countingApprovedSources.value = true;

  try {
    const count = await getApprovedUnassignedDailySourcesCount({
      signal: abortController.signal,
    });

    if (requestId === approvedSourcesRequestId) {
      approvedSourcesCount.value = count;
    }
  } catch (caught) {
    if (isApiRequestCanceled(caught)) {
      return;
    }

    if (requestId === approvedSourcesRequestId) {
      approvedSourcesCount.value = null;
      showToast({
        status: "error",
        title: "Автоназначение",
        message: "Не удалось загрузить данные.",
      });
    }
  } finally {
    if (requestId === approvedSourcesRequestId) {
      countingApprovedSources.value = false;
    }
    if (approvedSourcesAbortController === abortController) {
      approvedSourcesAbortController = null;
    }
  }
}

function cancelEmptyDaysCount() {
  countRequestId++;

  if (countTimeoutId !== null) {
    window.clearTimeout(countTimeoutId);
    countTimeoutId = null;
  }

  countAbortController?.abort();
  countAbortController = null;
  countingEmptyDays.value = false;
}

function cancelApprovedSourcesCount() {
  approvedSourcesRequestId++;
  approvedSourcesAbortController?.abort();
  approvedSourcesAbortController = null;
  countingApprovedSources.value = false;
}

onBeforeUnmount(() => {
  cancelEmptyDaysCount();
  cancelApprovedSourcesCount();
});

async function submit() {
  if (!from.value || !to.value || from.value > to.value) {
    error.value = "Укажите корректный диапазон дат";
    return;
  }

  if (intervalDays.value > MAX_AUTO_ASSIGN_DAYS) {
    error.value = "Диапазон автоназначения не может превышать 365 дней";
    return;
  }

  submitting.value = true;
  error.value = null;
  try {
    await autoAssignSources(from.value, to.value);
    showToast({ status: "success", title: "Автоназначение", message: "Пустые дни заполнены." });
    emit("update:visible", false);
    emit("completed");
  } catch (caught) {
    const apiError = toApiError(caught);
    error.value =
      apiError.code === "NotEnoughApprovedSources"
        ? "Недостаточно свободных одобренных слов для всего диапазона"
        : "Не удалось выполнить автоназначение";
  } finally {
    submitting.value = false;
  }
}
</script>

<template>
  <Dialog
    :visible="visible"
    modal
    :dismissable-mask="!submitting"
    :closable="!submitting"
    :close-on-escape="!submitting"
    :content-props="{ 'aria-busy': submitting ? 'true' : undefined }"
    header="Автоназначение"
    class="admin-dialog"
    @update:visible="(value) => !value && close()"
  >
    <form class="auto-assign" @submit.prevent="submit">
      <p class="auto-assign__hint">
        Будут заполнены только пустые дни. Существующие назначения не изменятся,
        а слова останутся скрытыми.
      </p>
      <div class="auto-assign__dates">
        <div class="admin-field auto-assign__date-field">
          <label for="auto-assign-from-date">С даты</label>
          <DatePicker
            v-model="fromDate"
            input-id="auto-assign-from-date"
            class="admin-date-picker"
            date-format="yy-mm-dd"
            show-icon
            icon-display="input"
            fluid
            required
          />
        </div>
        <div class="admin-field auto-assign__date-field">
          <label for="auto-assign-to-date">По дату</label>
          <DatePicker
            v-model="toDate"
            input-id="auto-assign-to-date"
            class="admin-date-picker"
            date-format="yy-mm-dd"
            show-icon
            icon-display="input"
            fluid
            required
          />
        </div>
      </div>
      <div class="auto-assign__summary">
        <span>Дней в диапазоне: <strong>{{ intervalDays }}</strong></span>
        <span>
          Пустых в выбранном диапазоне:
          <UiSkeleton v-if="countingEmptyDays" class="auto-assign__count-skeleton" width="34px" height="1em" />
          <strong v-else-if="emptyDaysCount !== null">{{ emptyDaysCount }}</strong>
          <strong v-else>не удалось посчитать</strong>
        </span>
        <span>
          Свободных одобренных слов:
          <UiSkeleton v-if="countingApprovedSources" class="auto-assign__count-skeleton" width="34px" height="1em" />
          <strong v-else-if="approvedSourcesCount !== null">{{ approvedSourcesCount }}</strong>
          <strong v-else>не удалось посчитать</strong>
        </span>
      </div>
      <p v-if="error" class="auto-assign__error">{{ error }}</p>
      <div class="admin-actions auto-assign__actions">
        <UiButton type="submit" :loading="submitting">Назначить</UiButton>
        <UiButton variant="outlined" :disabled="submitting" @click="close">
          Отмена
        </UiButton>
      </div>
    </form>
  </Dialog>
</template>

<style scoped>
.auto-assign {
  min-width: min(480px, calc(100vw - 48px));
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.auto-assign__count-skeleton {
  display: inline-flex;
  vertical-align: middle;
}

.auto-assign__hint {
  margin: 0;
  color: var(--color-gray-600);
  line-height: 1.5;
}

.auto-assign__dates {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
}

.auto-assign__date-field {
  display: flex;
  flex-direction: column;
  gap: 6px;
  color: var(--color-gray-700);
  font-size: 14px;
}

.auto-assign__summary {
  border-radius: 8px;
  padding: 12px;
  display: flex;
  flex-wrap: wrap;
  gap: 8px 20px;
  background: var(--color-primary-50);
  color: var(--color-gray-700);
}

.auto-assign__error {
  margin: 0;
  color: var(--color-red-700);
}

.auto-assign__actions {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
}

@media (max-width: 520px) {
  .auto-assign__dates {
    grid-template-columns: 1fr;
  }
}
</style>
