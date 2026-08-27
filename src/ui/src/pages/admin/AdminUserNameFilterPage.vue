<script setup lang="ts">
import Dialog from "primevue/dialog";
import { computed, onBeforeUnmount, onMounted, ref } from "vue";
import { isApiRequestCanceled, toApiError } from "../../shared/api/apiError";
import { useHandoffDelayedLoadingState } from "../../shared/composables/useSkeletonHandoff";
import { showToast } from "../../shared/notifications/toastStore";
import UiButton from "../../shared/ui/UiButton.vue";
import UiSkeleton from "../../shared/ui/UiSkeleton.vue";
import UiSkeletonHandoff from "../../shared/ui/UiSkeletonHandoff.vue";
import { authState } from "../../features/auth/model/authSession";
import { adminPermissions, hasPermission } from "../../features/auth/model/permissions";
import {
  getCurrentUserNameFilterVersion,
  publishUserNameFilterVersion,
} from "../../features/admin/api/authAdminApi";

const currentVersion = ref<number | null>(null);
const versionInput = ref<number | null>(null);
const loading = ref(false);
const publishing = ref(false);
const loadError = ref<string | null>(null);
const confirmationOpen = ref(false);
const loadingState = useHandoffDelayedLoadingState(loading);

let abortController: AbortController | null = null;
let requestId = 0;

const canManage = computed(() =>
  hasPermission(authState.value, adminPermissions.manageUserNameFilter),
);
const validVersion = computed(() =>
  versionInput.value !== null &&
  Number.isInteger(versionInput.value) &&
  versionInput.value > 0,
);
const canPublish = computed(() =>
  canManage.value && validVersion.value && !loading.value && !publishing.value,
);
const confirmationVersion = computed(() => versionInput.value ?? 0);

async function loadVersion() {
  abortController?.abort();
  const controller = new AbortController();
  const currentRequestId = ++requestId;
  abortController = controller;
  loading.value = true;
  loadError.value = null;

  try {
    const version = await getCurrentUserNameFilterVersion({
      signal: controller.signal,
    });
    if (currentRequestId !== requestId) return;

    currentVersion.value = version;
  } catch (caught) {
    if (isApiRequestCanceled(caught) || currentRequestId !== requestId) return;
    loadError.value = "Не удалось загрузить текущую версию фильтра";
  } finally {
    if (abortController === controller) {
      abortController = null;
    }
    if (currentRequestId === requestId) {
      loading.value = false;
    }
  }
}

function requestPublish() {
  if (!canPublish.value) return;
  confirmationOpen.value = true;
}

async function confirmPublish() {
  if (!canPublish.value || versionInput.value === null) return;

  publishing.value = true;
  try {
    await publishUserNameFilterVersion(versionInput.value);
    showToast({
      status: "success",
      title: "Фильтр UserName",
      message: "Новая версия опубликована.",
    });
    confirmationOpen.value = false;
    versionInput.value = null;
    await loadVersion();
  } catch (caught) {
    const apiError = toApiError(caught);
    confirmationOpen.value = false;
    showToast({
      status: "error",
      title: "Фильтр UserName",
      message: apiError.status === 400
        ? "Укажите положительный номер версии"
        : "Не удалось опубликовать версию фильтра",
    });
  } finally {
    publishing.value = false;
  }
}

function closeConfirmation() {
  if (publishing.value) return;
  confirmationOpen.value = false;
}

onMounted(() => {
  void loadVersion();
});

onBeforeUnmount(() => {
  requestId++;
  abortController?.abort();
});
</script>

<template>
  <div class="admin-page user-name-filter" :aria-busy="loading">
    <header class="admin-page__header">
      <div>
        <p class="admin-page__eyebrow">Пользователи</p>
        <h1>Фильтр UserName</h1>
        <p class="admin-page__description">Версия фильтра безопасных имен пользователей.</p>
      </div>
      <UiButton
        class="user-name-filter__refresh"
        variant="outlined"
        :loading="loading"
        @click="loadVersion()"
      >
        Обновить
      </UiButton>
    </header>

    <section class="admin-panel panel">
      <div class="admin-panel__header panel__header">
        <h2>Текущая версия</h2>
      </div>

      <UiSkeletonHandoff
        :skeleton-visible="loadingState.visible && currentVersion === null"
        :content-visible="loadingState.ready || currentVersion !== null"
      >
        <template #skeleton>
          <div class="version-skeleton" aria-hidden="true">
            <UiSkeleton width="96px" height="44px" border-radius="8px" />
            <UiSkeleton width="52%" height="14px" />
          </div>
        </template>
        <div v-if="loadError" class="inline-error">
          <span>{{ loadError }}</span>
          <UiButton variant="outlined" :loading="loading" @click="loadVersion()">Повторить</UiButton>
        </div>
        <div v-else class="version-summary">
          <strong>{{ currentVersion ?? "Не загружено" }}</strong>
          <span>Опубликованная версия фильтра UserName</span>
        </div>
      </UiSkeletonHandoff>
    </section>

    <section class="admin-panel publish-panel">
      <form class="publish-form" @submit.prevent="requestPublish">
        <label class="admin-field">
          <span>Новая версия</span>
          <input
            v-model.number="versionInput"
            type="number"
            min="1"
            step="1"
            inputmode="numeric"
            placeholder="Например, 7"
            :disabled="loading || publishing"
          />
        </label>
        <UiButton type="submit" :loading="publishing" :disabled="!canPublish">
          Опубликовать
        </UiButton>
      </form>
      <p v-if="!canManage" class="publish-panel__hint">
        У вашей учетной записи нет права на управление фильтром UserName.
      </p>
    </section>

    <Dialog
      :visible="confirmationOpen"
      modal
      header="Опубликовать версию?"
      class="admin-dialog"
      :closable="!publishing"
      :close-on-escape="!publishing"
      :content-props="{ 'aria-busy': publishing ? 'true' : undefined }"
      @update:visible="(value) => !value && closeConfirmation()"
    >
      <div>
        <p class="confirm-text">
          Версия {{ confirmationVersion }} будет опубликована как новая версия фильтра UserName.
        </p>
      </div>
      <div class="dialog-actions">
        <UiButton :loading="publishing" loading-label="Публикация" @click="confirmPublish">
          Опубликовать
        </UiButton>
        <UiButton variant="outlined" :disabled="publishing" @click="closeConfirmation">
          Отмена
        </UiButton>
      </div>
    </Dialog>

    <div v-if="loading" class="user-name-filter__loading-overlay" aria-hidden="true"></div>
  </div>
</template>

<style scoped>
.user-name-filter {
  position: relative;
}

.user-name-filter__refresh {
  position: relative;
  z-index: 2;
}

.user-name-filter__loading-overlay {
  position: absolute;
  inset: -4px;
  z-index: 1;
  border-radius: 10px;
  background: color-mix(in srgb, white 64%, transparent);
  cursor: wait;
}

.panel__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 12px;
}

.version-summary {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.version-summary strong {
  color: var(--color-gray-900);
  font-size: 42px;
  line-height: 1;
}

.version-summary span {
  color: var(--p-text-muted-color);
}

.version-skeleton {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.publish-form {
  display: grid;
  grid-template-columns: minmax(180px, 320px) auto;
  align-items: end;
  gap: 10px;
}

.publish-panel__hint {
  margin: 8px 0 0;
  color: var(--p-text-muted-color);
  font-size: 13px;
}

.confirm-text {
  margin: 0 0 16px;
  color: var(--color-gray-700);
}

@container admin-content (max-width: 560px) {
  .publish-form {
    grid-template-columns: 1fr;
  }

  .publish-form :deep(.p-button) {
    width: 100%;
  }

}
</style>
