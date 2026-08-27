<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref } from "vue";
import { isApiRequestCanceled } from "../../../shared/api/apiError";
import { useHandoffDelayedLoadingState } from "../../../shared/composables/useSkeletonHandoff";
import UiButton from "../../../shared/ui/UiButton.vue";
import UiSkeletonHandoff from "../../../shared/ui/UiSkeletonHandoff.vue";
import { getWordsVersionUploads } from "../api/sagasApi";
import {
  formatUploadDateTime,
  shortUploadSagaId,
  uploadStepLabels,
} from "../lib/wordUploads";
import type { WordsVersionUpload } from "../model/wordUploads";
import AdminListSkeleton from "../common/AdminListSkeleton.vue";
import AdminWordUploadStatus from "./AdminWordUploadStatus.vue";

defineProps<{
  selectedSagaId: string | null;
}>();

const emit = defineEmits<{
  select: [upload: WordsVersionUpload];
}>();

const activePageSize = 50;
const uploads = ref<WordsVersionUpload[]>([]);
const loading = ref(false);
const loadError = ref<string | null>(null);
const loadingState = useHandoffDelayedLoadingState(loading);

let abortController: AbortController | null = null;
let requestId = 0;

async function refresh() {
  abortController?.abort();
  const currentController = new AbortController();
  const currentRequest = ++requestId;
  abortController = currentController;
  loading.value = true;
  loadError.value = null;

  try {
    const page = await getWordsVersionUploads(
      { skip: 0, take: activePageSize },
      { signal: currentController.signal },
    );
    if (currentRequest !== requestId) return;

    uploads.value = page.uploads;
  } catch (caught) {
    if (isApiRequestCanceled(caught) || currentRequest !== requestId) return;
    loadError.value = "Не удалось загрузить активные загрузки";
  } finally {
    if (abortController === currentController) abortController = null;
    if (currentRequest === requestId) loading.value = false;
  }
}

onMounted(() => void refresh());

onBeforeUnmount(() => {
  requestId++;
  abortController?.abort();
});

defineExpose({ refresh });
</script>

<template>
  <section class="admin-panel panel active-uploads-panel">
    <div class="admin-panel__header panel__header">
      <h2>Активные загрузки</h2>
      <span>{{ uploads.length }}</span>
    </div>

    <UiSkeletonHandoff
      :skeleton-visible="loadingState.visible"
      :content-visible="loadingState.ready"
    >
      <template #skeleton>
        <AdminListSkeleton :count="2" :columns="3" />
      </template>
      <div v-if="loadError" class="inline-error">
        <span>{{ loadError }}</span>
        <UiButton variant="outlined" :loading="loading" @click="refresh">
          Повторить
        </UiButton>
      </div>
      <div v-else-if="uploads.length === 0" class="empty-state">
        Активных загрузок нет.
      </div>
      <div v-else class="upload-list">
        <article
          v-for="upload in uploads"
          :key="upload.sagaId"
          class="upload-card"
          :class="{ 'upload-card--selected': upload.sagaId === selectedSagaId }"
        >
          <button type="button" class="admin-native-action upload-card__body" @click="emit('select', upload)">
            <span class="upload-card__title">Версия {{ upload.wordsVersion }}</span>
            <span class="upload-card__id" :title="upload.sagaId">
              {{ shortUploadSagaId(upload.sagaId) }}
            </span>
            <span class="upload-card__meta">{{ uploadStepLabels[upload.currentStep] }}</span>
            <span class="upload-card__meta">
              Запущено: {{ formatUploadDateTime(upload.createdAt) }}
            </span>
            <AdminWordUploadStatus :state="upload.state" />
          </button>
        </article>
      </div>
    </UiSkeletonHandoff>
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

.panel__header span {
  color: var(--p-text-muted-color);
  font-size: 13px;
  font-weight: 500;
}

.upload-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.upload-card {
  border: 1px solid var(--color-gray-200);
  border-radius: 8px;
  background: white;
}

.upload-card--selected {
  border-color: var(--color-primary-300);
  outline: 1px solid var(--color-primary-100);
  outline-offset: -1px;
}

.upload-card__body {
  width: 100%;
  border: 0;
  padding: 11px;
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: 4px;
  background: transparent;
  color: var(--color-gray-700);
  text-align: left;
  cursor: pointer;
}

.upload-card__title {
  color: var(--color-gray-900);
  font-size: 17px;
  font-weight: 500;
}

.upload-card__id {
  max-width: 100%;
  color: var(--p-text-muted-color);
  font-family: ui-monospace, SFMono-Regular, Menlo, Consolas, monospace;
  font-size: 12px;
  overflow-wrap: anywhere;
}

.upload-card__meta {
  color: var(--p-text-muted-color);
  font-size: 13px;
}

.empty-state,
.inline-error {
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
</style>
