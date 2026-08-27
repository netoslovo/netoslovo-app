<script setup lang="ts">
import Dialog from "primevue/dialog";
import { computed, onBeforeUnmount, ref } from "vue";
import { isApiRequestCanceled } from "../../../shared/api/apiError";
import UiButton from "../../../shared/ui/UiButton.vue";
import UiIconButton from "../../../shared/ui/UiIconButton.vue";
import UiSkeleton from "../../../shared/ui/UiSkeleton.vue";
import {
  getWordsVersionUpload,
  getWordsVersionUploadFromHistory,
} from "../api/sagasApi";
import { formatUploadDateTime, uploadStepLabels } from "../lib/wordUploads";
import type {
  WordsVersionUpload,
  WordsVersionUploadCompletedStep,
  WordsVersionUploadStepName,
} from "../model/wordUploads";
import AdminWordUploadStatus from "./AdminWordUploadStatus.vue";

const props = defineProps<{
  canCancel: boolean;
  canceling: boolean;
}>();

const emit = defineEmits<{
  cancel: [upload: WordsVersionUpload];
  close: [];
}>();

const open = ref(false);
const sagaId = ref<string | null>(null);
const upload = ref<WordsVersionUpload | null>(null);
const loading = ref(false);
const loadError = ref<string | null>(null);

let abortController: AbortController | null = null;
let requestId = 0;

const completedSteps = computed(() =>
  new Map(upload.value?.completedSteps.map((step) => [step.name, step]) ?? []),
);
const canCancelUpload = computed(() =>
  props.canCancel && upload.value?.state === "Active",
);

const steps: WordsVersionUploadStepName[] = [
  "CreateNewWordsVersion",
  "InsertWordsIndexes",
  "InsertWordDistanceMaps",
  "DownloadNewGameSources",
  "UploadNewGameSourcesVersion",
  "ActivateNewWordsVersion",
  "WaitingForPostActivationTasks",
  "UpdateSingleGames",
  "UpdateSingleGameResults",
  "InvalidateWordsCache",
  "UnloadOldWordsVersions",
];

async function show(selectedUpload: WordsVersionUpload | string) {
  sagaId.value = typeof selectedUpload === "string"
    ? selectedUpload
    : selectedUpload.sagaId;
  upload.value = typeof selectedUpload === "string" ? null : selectedUpload;
  open.value = true;
  await refresh();
}

async function refresh() {
  const selectedSagaId = sagaId.value;
  if (!selectedSagaId) return;

  abortController?.abort();
  const currentController = new AbortController();
  const currentRequest = ++requestId;
  abortController = currentController;
  loading.value = true;
  loadError.value = null;

  try {
    const activeUpload = await getWordsVersionUpload(selectedSagaId, {
      signal: currentController.signal,
    });
    const loadedUpload = activeUpload ?? await getWordsVersionUploadFromHistory(
      selectedSagaId,
      { signal: currentController.signal },
    );

    if (!open.value || sagaId.value !== selectedSagaId || currentRequest !== requestId) return;

    upload.value = loadedUpload;
    if (!loadedUpload) loadError.value = "Загрузка не найдена";
  } catch (caught) {
    if (isApiRequestCanceled(caught)) return;
    if (!open.value || sagaId.value !== selectedSagaId || currentRequest !== requestId) return;

    loadError.value = "Не удалось загрузить детали";
  } finally {
    if (abortController === currentController) abortController = null;
    if (currentRequest === requestId) loading.value = false;
  }
}

function close() {
  if (props.canceling) return;

  open.value = false;
  sagaId.value = null;
  upload.value = null;
  loadError.value = null;
  loading.value = false;
  requestId++;
  abortController?.abort();
  abortController = null;
  emit("close");
}

function requestCancel() {
  if (canCancelUpload.value && upload.value) emit("cancel", upload.value);
}

function completedStep(
  step: WordsVersionUploadStepName,
): WordsVersionUploadCompletedStep | undefined {
  return completedSteps.value.get(step);
}

function isPostActivationStep(step: WordsVersionUploadStepName) {
  return step === "UpdateSingleGames" ||
    step === "UpdateSingleGameResults" ||
    step === "InvalidateWordsCache" ||
    step === "UnloadOldWordsVersions";
}

function stepIcon(step: WordsVersionUploadStepName) {
  const completed = completedStep(step);
  if (!completed) return "pi pi-circle";
  return completed.isSuccess ? "pi pi-check" : "pi pi-times";
}

onBeforeUnmount(() => {
  requestId++;
  abortController?.abort();
});

defineExpose({ show, refresh });
</script>

<template>
  <Dialog
    :visible="open"
    modal
    :dismissable-mask="!canceling"
    :closable="!canceling"
    :close-on-escape="!canceling"
    :header="upload ? `Детали загрузки версии ${upload.wordsVersion}` : 'Детали загрузки'"
    class="admin-dialog upload-details-dialog"
    @update:visible="(value) => !value && close()"
  >
    <div v-if="loadError" class="inline-error upload-details-dialog__state">
      <span>{{ loadError }}</span>
      <UiButton v-if="sagaId" variant="outlined" :loading="loading" @click="refresh">
        Повторить
      </UiButton>
    </div>
    <div
      v-else-if="loading && !upload"
      class="upload-details-skeleton upload-details-dialog__state"
      aria-hidden="true"
    >
      <UiSkeleton width="96px" height="24px" border-radius="999px" />
      <UiSkeleton width="62%" height="22px" />
      <UiSkeleton width="100%" height="14px" />
      <ol class="steps">
        <li v-for="step in 6" :key="step">
          <UiSkeleton width="20px" height="20px" shape="circle" />
          <UiSkeleton :width="step % 2 === 0 ? '58%' : '72%'" height="14px" />
        </li>
      </ol>
    </div>
    <article v-else-if="upload" class="upload-details">
      <div class="upload-details__summary">
        <div class="upload-details__status">
          <AdminWordUploadStatus :state="upload.state" />
          <UiIconButton
            size="sm"
            label="Обновить детали"
            loading-label="Обновление деталей"
            class="upload-details__refresh"
            :loading="loading"
            @click="refresh"
          >
            <i class="pi pi-refresh" aria-hidden="true"></i>
          </UiIconButton>
        </div>
        <div class="upload-details__current-step">
          <strong>{{ uploadStepLabels[upload.currentStep] }}</strong>
        </div>
        <code :title="upload.sagaId">{{ upload.sagaId }}</code>
        <small>
          Создана: {{ formatUploadDateTime(upload.createdAt) }} ·
          Завершена: {{ formatUploadDateTime(upload.completedAt) }}
        </small>
      </div>

      <ol class="steps">
        <li
          v-for="step in steps"
          :key="step"
          :class="{
            'steps__item--done': completedStep(step)?.isSuccess,
            'steps__item--failed': completedStep(step)?.isSuccess === false,
            'steps__item--current': upload.currentStep === step && upload.state === 'Active',
            'steps__item--nested': isPostActivationStep(step),
          }"
        >
          <i :class="stepIcon(step)" aria-hidden="true"></i>
          <span>{{ uploadStepLabels[step] }}</span>
        </li>
      </ol>

      <div class="dialog-actions">
        <UiButton
          v-if="canCancelUpload"
          variant="outlined"
          class="cancel-upload-button"
          :disabled="loading || canceling"
          @click="requestCancel"
        >
          Отменить загрузку
        </UiButton>
      </div>
    </article>
    <div v-else class="empty-state upload-details-dialog__state">
      Выберите загрузку, чтобы увидеть прогресс.
    </div>
  </Dialog>
</template>

<style scoped>
.upload-details,
.upload-details-skeleton {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.upload-details__summary {
  padding: 10px 0;
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: 6px;
}

.upload-details__status,
.upload-details__current-step {
  max-width: 100%;
  display: flex;
  align-items: center;
  gap: 6px;
}

.upload-details__current-step strong {
  min-width: 0;
  color: var(--color-gray-900);
  font-size: 18px;
  overflow-wrap: anywhere;
}

.upload-details__summary code {
  max-width: 100%;
  color: var(--p-text-muted-color);
  font-family: ui-monospace, SFMono-Regular, Menlo, Consolas, monospace;
  font-size: 12px;
  overflow-wrap: anywhere;
}

.upload-details__summary small {
  color: var(--p-text-muted-color);
}

.steps {
  margin: 0;
  padding: 0;
  display: grid;
  gap: 6px;
  list-style: none;
}

.steps li {
  min-height: 34px;
  border: 1px solid var(--color-gray-100);
  border-radius: 8px;
  padding: 7px 9px;
  display: flex;
  align-items: center;
  gap: 8px;
  color: var(--p-text-muted-color);
  font-size: 14px;
}

.steps__item--nested {
  margin-left: 28px;
}

.steps .steps__item--done {
  background: var(--color-primary-50);
  color: #18563e;
}

.steps .steps__item--failed {
  border-color: var(--color-red-100);
  background: #fff5f5;
  color: var(--color-red-700);
  font-weight: 500;
}

.steps .steps__item--current {
  border-color: var(--color-primary-200);
  background: var(--color-primary-50);
  color: var(--color-primary-700);
  font-weight: 500;
}

.inline-error,
.empty-state {
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

.upload-details-dialog__state {
  min-width: min(520px, calc(100vw - 48px));
}

:global(.upload-details-dialog.p-dialog) {
  width: min(640px, calc(100vw - 32px));
}

:global(.upload-details-dialog.p-dialog .p-dialog-content) {
  max-height: min(680px, calc(100vh - 160px));
  overflow-y: auto;
}

:global(.upload-details-dialog.p-dialog .upload-details__refresh.ui-icon-button.p-button) {
  width: 28px;
  min-width: 28px;
  height: 28px;
  border-color: transparent;
  background: transparent;
  color: var(--p-surface-500);
}

:global(.upload-details-dialog.p-dialog .upload-details__refresh.ui-icon-button.p-button:not(:disabled):hover) {
  border-color: transparent;
  background: var(--color-gray-50);
  color: var(--color-primary-700);
}

:global(.upload-details-dialog.p-dialog .cancel-upload-button.ui-button.p-button) {
  border-color: var(--color-red-600);
  background: var(--color-red-600);
  color: white;
}

:global(.upload-details-dialog.p-dialog .cancel-upload-button.ui-button.p-button .p-button-label),
:global(.upload-details-dialog.p-dialog .cancel-upload-button.ui-button.p-button .p-button-icon) {
  color: white;
}

@media (hover: hover) and (pointer: fine) {
  :global(.upload-details-dialog.p-dialog .cancel-upload-button.ui-button.p-button:not(:disabled):hover) {
    border-color: var(--color-red-700);
    background: var(--color-red-700);
    color: white;
  }
}

:global(.upload-details-dialog.p-dialog .cancel-upload-button.ui-button.p-button:not(:disabled):active) {
  border-color: var(--color-red-700);
  background: var(--color-red-700);
  color: white;
}
</style>
