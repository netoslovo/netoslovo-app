<script setup lang="ts">
import Dialog from "primevue/dialog";
import { computed, ref } from "vue";
import { toApiError } from "../../shared/api/apiError";
import { showToast } from "../../shared/notifications/toastStore";
import UiButton from "../../shared/ui/UiButton.vue";
import { authState } from "../../features/auth/model/authSession";
import { adminPermissions, hasPermission } from "../../features/auth/model/permissions";
import {
  cancelWordsVersionUpload,
  createWordsVersionUpload,
} from "../../features/admin/api/sagasApi";
import type { WordsVersionUpload } from "../../features/admin/model/wordUploads";
import AdminActiveWordUploads from "../../features/admin/uploads/AdminActiveWordUploads.vue";
import AdminWordUploadDetailsDialog from "../../features/admin/uploads/AdminWordUploadDetailsDialog.vue";
import AdminWordUploadHistory from "../../features/admin/uploads/AdminWordUploadHistory.vue";

type RefreshableSection = {
  refresh: () => Promise<void>;
};

type DetailsDialog = RefreshableSection & {
  show: (upload: WordsVersionUpload | string) => Promise<void>;
};

const versionInput = ref<number | null>(null);
const submitting = ref(false);
const refreshing = ref(false);
const selectedSagaId = ref<string | null>(null);
const cancelCandidate = ref<WordsVersionUpload | null>(null);
const canceling = ref(false);
const activeSection = ref<RefreshableSection | null>(null);
const historySection = ref<RefreshableSection | null>(null);
const detailsDialog = ref<DetailsDialog | null>(null);

const canUpload = computed(() =>
  hasPermission(authState.value, adminPermissions.uploadWordsVersion),
);
const canCancel = computed(() =>
  hasPermission(authState.value, adminPermissions.cancelWordsVersionUpload),
);
const validVersion = computed(() =>
  versionInput.value !== null &&
  Number.isInteger(versionInput.value) &&
  versionInput.value > 0,
);
const canStartUpload = computed(() =>
  canUpload.value && validVersion.value && !submitting.value,
);

async function startUpload() {
  if (!canStartUpload.value || versionInput.value === null) return;

  submitting.value = true;
  try {
    const sagaId = await createWordsVersionUpload(versionInput.value);
    showToast({ status: "success", title: "Загрузка слов", message: "Загрузка запущена." });
    await refreshAll();
    await openUploadDetails(sagaId);
  } catch (caught) {
    const apiError = toApiError(caught);
    showToast({
      status: "error",
      title: "Загрузка слов",
      message: apiError.status === 409 || apiError.code === "AlreadyRunning"
        ? "Загрузка слов уже выполняется"
        : "Не удалось запустить загрузку слов",
    });
  } finally {
    submitting.value = false;
  }
}

async function refreshAll() {
  if (refreshing.value) return;

  refreshing.value = true;
  try {
    await Promise.all([
      activeSection.value?.refresh(),
      historySection.value?.refresh(),
    ]);
  } finally {
    refreshing.value = false;
  }
}

async function openUploadDetails(upload: WordsVersionUpload | string) {
  selectedSagaId.value = typeof upload === "string" ? upload : upload.sagaId;
  await detailsDialog.value?.show(upload);
}

function closeUploadDetails() {
  selectedSagaId.value = null;
}

function requestCancelUpload(upload: WordsVersionUpload) {
  if (!canCancel.value || upload.state !== "Active") return;
  cancelCandidate.value = upload;
}

async function confirmCancelUpload() {
  const upload = cancelCandidate.value;
  if (!upload) return;

  canceling.value = true;
  try {
    await cancelWordsVersionUpload(upload.sagaId);
    showToast({ status: "success", title: "Загрузка слов", message: "Запрошена отмена загрузки." });
    cancelCandidate.value = null;
    await refreshAll();
    if (selectedSagaId.value === upload.sagaId) await detailsDialog.value?.refresh();
  } catch (caught) {
    const apiError = toApiError(caught);
    showToast({
      status: "error",
      title: "Загрузка слов",
      message: apiError.status === 404
        ? "Загрузка уже не активна"
        : "Не удалось отменить загрузку",
    });
    await refreshAll();
    if (selectedSagaId.value === upload.sagaId) await detailsDialog.value?.refresh();
  } finally {
    canceling.value = false;
  }
}

function closeCancelUploadDialog() {
  if (!canceling.value) cancelCandidate.value = null;
}
</script>

<template>
  <div class="admin-page">
    <header class="admin-page__header">
      <div>
        <p class="admin-page__eyebrow">Словарь</p>
        <h1>Загрузка слов</h1>
        <p class="admin-page__description">Запуск версий и контроль саг загрузки.</p>
      </div>
      <UiButton variant="outlined" :loading="refreshing" @click="refreshAll">
        Обновить
      </UiButton>
    </header>

    <section class="admin-panel upload-start">
      <form class="upload-start__form" @submit.prevent="startUpload">
        <label class="admin-field">
          <span>Версия слов</span>
          <input
            v-model.number="versionInput"
            type="number"
            min="1"
            step="1"
            inputmode="numeric"
            placeholder="Например, 42"
          />
        </label>
        <UiButton type="submit" :loading="submitting" :disabled="!canStartUpload">
          Запустить
        </UiButton>
      </form>
      <p v-if="!canUpload" class="upload-start__hint">
        У вашей учетной записи нет права на запуск загрузки.
      </p>
    </section>

    <AdminActiveWordUploads
      ref="activeSection"
      :selected-saga-id="selectedSagaId"
      @select="openUploadDetails"
    />
    <AdminWordUploadHistory
      ref="historySection"
      :selected-saga-id="selectedSagaId"
      @select="openUploadDetails"
    />
    <AdminWordUploadDetailsDialog
      ref="detailsDialog"
      :can-cancel="canCancel"
      :canceling="canceling"
      @cancel="requestCancelUpload"
      @close="closeUploadDetails"
    />

    <Dialog
      :visible="cancelCandidate !== null"
      modal
      :closable="!canceling"
      :close-on-escape="!canceling"
      :content-props="{ 'aria-busy': canceling ? 'true' : undefined }"
      header="Отменить загрузку?"
      class="admin-dialog"
      @update:visible="(value) => !value && closeCancelUploadDialog()"
    >
      <p v-if="cancelCandidate" class="confirm-text">
        Загрузка версии {{ cancelCandidate.wordsVersion }} будет остановлена.
      </p>
      <div class="dialog-actions">
        <UiButton variant="outlined" :disabled="canceling" @click="closeCancelUploadDialog">
          Закрыть
        </UiButton>
        <UiButton class="cancel-upload-button" :loading="canceling" @click="confirmCancelUpload">
          Отменить
        </UiButton>
      </div>
    </Dialog>
  </div>
</template>

<style scoped>
.upload-start {
  padding: 12px;
}

.upload-start__form {
  display: grid;
  grid-template-columns: minmax(180px, 320px) auto;
  align-items: end;
  gap: 10px;
}

.upload-start__hint {
  margin: 10px 0 0;
  color: var(--p-text-muted-color);
  font-size: 14px;
}

.confirm-text {
  max-width: 430px;
  color: var(--color-gray-700);
  line-height: 1.5;
}

:global(.admin-dialog.p-dialog .cancel-upload-button.ui-button.p-button) {
  border-color: var(--color-red-600);
  background: var(--color-red-600);
  color: white;
}

:global(.admin-dialog.p-dialog .cancel-upload-button.ui-button.p-button .p-button-label),
:global(.admin-dialog.p-dialog .cancel-upload-button.ui-button.p-button .p-button-icon) {
  color: white;
}

@media (hover: hover) and (pointer: fine) {
  :global(.admin-dialog.p-dialog .cancel-upload-button.ui-button.p-button:not(:disabled):hover) {
    border-color: var(--color-red-700);
    background: var(--color-red-700);
    color: white;
  }
}

:global(.admin-dialog.p-dialog .cancel-upload-button.ui-button.p-button:not(:disabled):active) {
  border-color: var(--color-red-700);
  background: var(--color-red-700);
  color: white;
}

@container admin-content (max-width: 680px) {
  .upload-start__form {
    grid-template-columns: 1fr;
  }
}
</style>
