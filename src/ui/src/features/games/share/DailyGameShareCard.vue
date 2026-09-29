<script setup lang="ts">
import Dialog from "primevue/dialog";
import { computed, onBeforeUnmount, ref, watch } from "vue";
import { useRouter } from "vue-router";
import { isApiRequestCanceled } from "../../../shared/api/apiError";
import { getAppOrigin } from "../../../shared/config/app";
import { showToast } from "../../../shared/notifications/toastStore";
import UiButton from "../../../shared/ui/UiButton.vue";
import UiIconButton from "../../../shared/ui/UiIconButton.vue";
import UiSkeleton from "../../../shared/ui/UiSkeleton.vue";
import {
  getDailyGameShare,
  shareDailyGame,
  unshareDailyGame,
} from "../api/gameApi";

const props = defineProps<{
  gameId: string;
  day: string;
}>();
type CardState = "loading" | "missing" | "ready" | "failed";
type CopyFeedbackTarget = "field" | "button";

const router = useRouter();
const state = ref<CardState>("loading");
const publicId = ref<string | null>(null);
const revokeConfirmationOpen = ref(false);
const creating = ref(false);
const revoking = ref(false);
const nativeShareFailed = ref(false);
const fieldCopied = ref(false);
const buttonCopied = ref(false);
const copying = ref(false);
const sharing = ref(false);
const shareSent = ref(false);
const createError = ref(false);
const copyError = ref(false);
let fieldCopiedTimeout: ReturnType<typeof setTimeout> | undefined;
let buttonCopiedTimeout: ReturnType<typeof setTimeout> | undefined;
let shareSentTimeout: ReturnType<typeof setTimeout> | undefined;
let controller: AbortController | null = null;
let requestId = 0;

const shareUrl = computed(() => {
  if (!publicId.value) return "";
  const path = router.resolve({
    name: "shared-daily",
    params: { publicId: publicId.value },
  }).href;
  return `${getAppOrigin()}${path}`;
});
const nativeShareData = computed<ShareData>(() => ({
  text: `Мой результат в игре дня за ${formatDay(props.day)}\n\n${shareUrl.value}`,
}));
const nativeShareAvailable = computed(() => {
  if (typeof navigator.share !== "function" || nativeShareFailed.value) return false;
  if (typeof navigator.canShare !== "function" || !shareUrl.value) return true;
  return navigator.canShare(nativeShareData.value);
});
watch(() => props.gameId, () => {
  void loadShare();
}, { immediate: true });

onBeforeUnmount(() => {
  requestId += 1;
  controller?.abort();
  clearTimeout(fieldCopiedTimeout);
  clearTimeout(buttonCopiedTimeout);
  clearTimeout(shareSentTimeout);
});

function resetCopyFeedback() {
  clearTimeout(fieldCopiedTimeout);
  clearTimeout(buttonCopiedTimeout);
  fieldCopied.value = false;
  buttonCopied.value = false;
  copyError.value = false;
}

async function loadShare() {
  controller?.abort();
  const abortController = new AbortController();
  controller = abortController;
  const currentRequestId = ++requestId;
  state.value = "loading";
  publicId.value = null;
  revokeConfirmationOpen.value = false;
  creating.value = false;
  revoking.value = false;
  sharing.value = false;
  shareSent.value = false;
  clearTimeout(shareSentTimeout);
  copying.value = false;
  nativeShareFailed.value = false;
  createError.value = false;
  resetCopyFeedback();

  try {
    const existingPublicId = await getDailyGameShare(props.gameId, {
      signal: abortController.signal,
    });
    if (currentRequestId !== requestId) return;
    publicId.value = existingPublicId;
    state.value = existingPublicId ? "ready" : "missing";
  } catch (error) {
    if (isApiRequestCanceled(error) || currentRequestId !== requestId) return;
    state.value = "failed";
  } finally {
    if (controller === abortController) controller = null;
  }
}

async function createShare() {
  if (creating.value) return;
  controller?.abort();
  controller = null;
  const currentRequestId = ++requestId;
  creating.value = true;
  createError.value = false;
  try {
    const createdPublicId = await shareDailyGame(props.gameId);
    if (currentRequestId !== requestId) return;
    publicId.value = createdPublicId;
    state.value = "ready";
  } catch {
    if (currentRequestId !== requestId) return;
    state.value = "missing";
    createError.value = true;
  } finally {
    if (currentRequestId === requestId) creating.value = false;
  }
}

async function openNativeShare() {
  if (!shareUrl.value || sharing.value || typeof navigator.share !== "function") return;
  const currentRequestId = requestId;
  clearTimeout(shareSentTimeout);
  shareSent.value = false;
  sharing.value = true;
  try {
    await navigator.share(nativeShareData.value);
    if (currentRequestId !== requestId) return;
    shareSent.value = true;
    shareSentTimeout = setTimeout(() => { shareSent.value = false; }, 2400);
  } catch (error) {
    if (currentRequestId !== requestId) return;
    if (error instanceof DOMException && error.name === "AbortError") return;
    nativeShareFailed.value = true;
    showToast({
      status: "info",
      title: "Поделиться",
      message: "Системное меню недоступно. Скопируйте ссылку вручную.",
    });
  } finally {
    if (currentRequestId === requestId) sharing.value = false;
  }
}

async function copyShareUrl(feedbackTarget: CopyFeedbackTarget) {
  if (!shareUrl.value || copying.value) return;
  const currentRequestId = requestId;
  copying.value = true;
  copyError.value = false;
  if (feedbackTarget === "field") {
    clearTimeout(fieldCopiedTimeout);
    fieldCopied.value = false;
  } else {
    clearTimeout(buttonCopiedTimeout);
    buttonCopied.value = false;
  }
  try {
    await navigator.clipboard.writeText(shareUrl.value);
    if (currentRequestId !== requestId) return;
    if (feedbackTarget === "field") {
      fieldCopied.value = true;
      fieldCopiedTimeout = setTimeout(() => { fieldCopied.value = false; }, 1400);
    } else {
      buttonCopied.value = true;
      buttonCopiedTimeout = setTimeout(() => { buttonCopied.value = false; }, 1400);
    }
  } catch {
    if (currentRequestId !== requestId) return;
    copyError.value = true;
  } finally {
    if (currentRequestId === requestId) copying.value = false;
  }
}

function requestRevoke() {
  revokeConfirmationOpen.value = true;
}

async function revokeShare() {
  const currentRequestId = ++requestId;
  revoking.value = true;
  try {
    await unshareDailyGame(props.gameId);
    if (currentRequestId !== requestId) return;
    publicId.value = null;
    state.value = "missing";
    revokeConfirmationOpen.value = false;
    nativeShareFailed.value = false;
    shareSent.value = false;
    clearTimeout(shareSentTimeout);
    resetCopyFeedback();
    showToast({
      status: "success",
      title: "Публикация удалена",
      message: "Результат больше недоступен по ссылке.",
    });
  } catch {
    if (currentRequestId !== requestId) return;
    showToast({
      status: "error",
      title: "Удаление публикации",
      message: "Не удалось удалить публикацию. Попробуйте ещё раз.",
    });
  } finally {
    if (currentRequestId === requestId) revoking.value = false;
  }
}

function formatDay(day: string) {
  return new Intl.DateTimeFormat("ru-RU", {
    day: "numeric",
    month: "long",
    year: "numeric",
  }).format(new Date(`${day}T00:00:00`));
}
</script>

<template>
  <section class="share-card" aria-label="Публикация результата"
    :aria-busy="state === 'loading' || creating || revoking || copying || sharing">
    <div class="share-card__content">
      <div v-if="state === 'loading'" class="share-card__loading">
        <div aria-hidden="true" class="share-card__loading-skeletons">
          <UiSkeleton width="150px" height="15px" border-radius="5px" />
          <UiSkeleton width="100%" height="34px" border-radius="8px" />
        </div>
        <span class="visually-hidden" role="status">Проверка статуса публикации</span>
      </div>

      <template v-else>
        <div v-if="state === 'failed'" class="share-card__failure">
          <p class="share-card__error" role="alert">Не удалось проверить статус публикации.</p>
        </div>

        <template v-else-if="state === 'missing'">
          <p v-if="createError" class="share-card__error" role="alert">
            Не удалось опубликовать результат. Попробуйте ещё раз.
          </p>
        </template>

        <template v-else>
          <div class="share-card__status-row">
            <p class="share-card__status" role="status">
              <i class="pi pi-check-circle" aria-hidden="true"></i>
              Результат опубликован
            </p>
          </div>

          <div class="share-card__link-row">
            <div class="share-card__link-field">
              <label class="visually-hidden" for="daily-share-url">Ссылка на результат</label>
              <input id="daily-share-url" class="share-card__link" :value="shareUrl" readonly
                :aria-describedby="copyError ? 'daily-share-copy-error' : undefined" />
              <UiIconButton class="share-card__link-copy" size="sm"
                :label="fieldCopied ? 'Ссылка скопирована' : 'Скопировать ссылку'" :disabled="revoking || sharing"
                :aria-busy="copying" @click="copyShareUrl('field')">
                <Transition name="share-card-feedback" mode="out-in">
                  <i :key="fieldCopied ? 'copied' : 'copy'" :class="fieldCopied ? 'pi pi-check' : 'pi pi-copy'"
                    aria-hidden="true"></i>
                </Transition>
              </UiIconButton>
            </div>
            <UiIconButton class="share-card__revoke" size="sm" label="Удалить публикацию результата"
              loading-label="Удаление публикации" :loading="revoking" :disabled="copying || sharing"
              @click="requestRevoke">
              <i class="pi pi-trash" aria-hidden="true"></i>
            </UiIconButton>
          </div>

          <p v-if="copyError" id="daily-share-copy-error" class="share-card__error" role="alert">
            Не удалось скопировать ссылку. Выделите её и скопируйте вручную.
          </p>
        </template>

        <div class="share-card__policies">
          <div class="share-card__policy">
            <i class="pi pi-info-circle" aria-hidden="true"></i>
            <span>
              Опубликованный результат доступен любому, у кого есть ссылка. Вместе с ним будут видны ваше
              имя пользователя, попытки и статистика. Публикацию можно удалить в любой момент.
            </span>
          </div>
          <div class="share-card__policy">
            <i class="pi pi-info-circle" aria-hidden="true"></i>
            <span>Загаданное слово и слова попыток останутся скрытыми от тех, кто ещё не завершил игру дня.</span>
          </div>
        </div>

        <div v-if="state === 'failed'" class="share-card__actions">
          <UiButton variant="outlined" @click="loadShare">
            <i class="pi pi-refresh" aria-hidden="true"></i>
            Повторить
          </UiButton>
        </div>

        <div v-else-if="state === 'missing'" class="share-card__actions">
          <UiButton :loading="creating" loading-label="Публикация результата" @click="createShare()">
            <i class="pi pi-share-alt" aria-hidden="true"></i>
            Опубликовать результат
          </UiButton>
        </div>

        <div v-else class="share-card__actions">
          <UiButton v-if="nativeShareAvailable" :loading="sharing" loading-label="Открытие меню отправки"
            :disabled="revoking || copying" @click="openNativeShare">
            <Transition name="share-card-feedback" mode="out-in">
              <span :key="shareSent ? 'sent' : 'share'" class="share-card__share-feedback">
                <i :class="shareSent ? 'pi pi-check' : 'pi pi-share-alt'" aria-hidden="true"></i>
                {{ shareSent ? "Отправлено" : "Поделиться" }}
              </span>
            </Transition>
          </UiButton>
          <UiButton class="share-card__copy" :variant="nativeShareAvailable ? 'outlined' : 'primary'"
            :disabled="revoking || sharing" :aria-busy="copying" @click="copyShareUrl('button')">
            <span class="share-card__copy-content">
              <span class="share-card__copy-sizer" aria-hidden="true">
                <i class="pi pi-copy"></i>
                Скопировать ссылку
              </span>
              <Transition name="share-card-feedback" mode="out-in">
                <span :key="buttonCopied ? 'copied' : 'copy'" class="share-card__copy-feedback">
                  <i :class="buttonCopied ? 'pi pi-check' : 'pi pi-copy'" aria-hidden="true"></i>
                  {{ buttonCopied ? "Скопировано" : "Скопировать ссылку" }}
                </span>
              </Transition>
            </span>
          </UiButton>
        </div>
      </template>
    </div>
  </section>

  <Dialog :visible="revokeConfirmationOpen" modal :dismissable-mask="!revoking" :closable="!revoking"
    :close-on-escape="!revoking" :content-props="{ 'aria-busy': revoking ? 'true' : undefined }"
    header="Удалить публикацию результата?" class="game-dialog"
    @update:visible="value => { if (!revoking) revokeConfirmationOpen = value; }">
    <div class="share-dialog-confirmation">
      <p class="share-dialog-confirmation__text">
        Ссылка на результат перестанет работать. Позже результат можно будет опубликовать снова.
      </p>
      <div class="share-dialog-confirmation__actions">
        <UiButton variant="outlined" :disabled="revoking" @click="revokeConfirmationOpen = false">Назад</UiButton>
        <UiButton class="share-card__danger" :loading="revoking" loading-label="Удаление публикации"
          @click="revokeShare">
          Удалить</UiButton>
      </div>
    </div>
  </Dialog>

  <span class="visually-hidden" role="status">{{ fieldCopied ? "Ссылка скопирована" : "" }}</span>
  <span class="visually-hidden" role="status">{{ buttonCopied ? "Ссылка скопирована" : "" }}</span>
  <span class="visually-hidden" role="status">{{ shareSent ? "Результат отправлен" : "" }}</span>
</template>

<style scoped>
.share-card {
  width: 100%;
  min-width: 0;
}

.share-card__content {
  width: 100%;
  min-width: 0;
  display: flex;
  flex-direction: column;
  align-items: stretch;
  gap: 14px;
}

.share-card__status,
.share-card__error,
.share-dialog-confirmation__text {
  margin: 0;
}

.share-card__status {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 5px;
  color: var(--color-gray-600);
  font-size: 13px;
  line-height: 1.3;
}

.share-card__status .pi {
  color: var(--color-gray-500);
  font-size: 12px;
}

.share-card__loading,
.share-card__loading-skeletons {
  width: 100%;
}

.share-card__loading-skeletons {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
}

.share-card__failure {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  text-align: center;
}

.share-card__error {
  color: var(--color-red-700);
  font-size: 14px;
  line-height: 1.4;
}

.share-card__status-row {
  width: 100%;
  display: flex;
  align-items: center;
  justify-content: flex-start;
  gap: 4px;
}

.share-card__link-row {
  width: 100%;
  min-width: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
}

.share-card__link-field {
  position: relative;
  min-width: 0;
  flex: 1;
}

.share-card__link {
  box-sizing: border-box;
  width: 100%;
  min-width: 0;
  height: 34px;
  padding: 7px 42px 7px 10px;
  border: 1px solid var(--color-gray-300);
  border-radius: 8px;
  background: var(--color-gray-50);
  color: var(--color-gray-700);
  font: inherit;
  font-size: 13px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

:global(.share-card__link-copy.ui-icon-button.p-button) {
  position: absolute;
  top: 1px;
  right: 1px;
  width: 32px;
  min-width: 32px;
  height: 32px;
  border: 0;
  border-radius: 7px;
  background: transparent;
  color: var(--color-gray-600);
}

:global(.share-card__link-copy.ui-icon-button.p-button:not(:disabled):active) {
  background: var(--color-gray-100);
  color: var(--color-primary-700);
}

:global(.share-card__link-copy.ui-icon-button.p-button:not(:disabled):focus-visible) {
  box-shadow: var(--focus-ring-primary);
}

@media (hover: hover) and (pointer: fine) {
  :global(.share-card__link-copy.ui-icon-button.p-button:not(:disabled):hover) {
    background: var(--color-gray-100);
    color: var(--color-primary-700);
  }
}

.share-card__actions {
  width: 100%;
  display: grid;
  grid-template-columns: minmax(0, 1fr);
  gap: 10px;
}

.share-card__actions>.ui-button {
  width: 100%;
  min-width: 0;
}

.share-card__policies {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.share-card__policy {
  display: flex;
  align-items: flex-start;
  gap: 8px;
  color: var(--p-text-muted-color);
  font-size: 13px;
  line-height: 1.4;
}

.share-card__policy .pi {
  margin-top: 2px;
  color: var(--p-text-muted-color);
  font-size: 13px;
  line-height: 1;
}

.share-card__copy-content {
  display: inline-grid;
}

.share-card__share-feedback {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
}

.share-card__copy-sizer,
.share-card__copy-feedback {
  grid-area: 1 / 1;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  white-space: nowrap;
}

.share-card__copy-sizer {
  visibility: hidden;
}

.share-card-feedback-enter-active,
.share-card-feedback-leave-active {
  transition: opacity 0.1s ease, transform 0.1s ease;
}

.share-card-feedback-enter-from {
  opacity: 0;
  transform: translateY(2px);
}

.share-card-feedback-leave-to {
  opacity: 0;
  transform: translateY(-2px);
}

.share-card__link:focus-visible {
  border-color: var(--focus-border-primary);
  outline: none;
  box-shadow: var(--focus-ring-primary);
}

.share-dialog-confirmation {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.share-dialog-confirmation__text {
  color: var(--color-gray-600);
  font-size: var(--game-dialog-body-font-size);
  line-height: 1.45;
}

.share-dialog-confirmation__actions {
  display: flex;
  gap: 10px;
  padding-top: 4px;
}

.share-dialog-confirmation__actions>.ui-button {
  flex: 1;
}

:global(.share-card__danger.ui-button.p-button) {
  background: var(--color-red-600);
  border-color: var(--color-red-600);
  color: white;
}

:global(.share-card__danger.ui-button.p-button:not(:disabled):hover),
:global(.share-card__danger.ui-button.p-button:not(:disabled):focus),
:global(.share-card__danger.ui-button.p-button:not(:disabled):active) {
  background: var(--color-red-700);
  border-color: var(--color-red-700);
  color: white;
}

:global(.share-card__danger.ui-button.p-button:not(:disabled):focus-visible) {
  box-shadow: var(--focus-ring-danger);
}

:global(.share-card__danger.ui-button.p-button .p-button-label),
:global(.share-card__danger.ui-button.p-button .p-button-icon) {
  color: white;
}

:global(.share-card__revoke.ui-icon-button.p-button:not(:disabled):focus-visible),
:global(.share-card__revoke.ui-icon-button.p-button:not(:disabled):active) {
  border-color: var(--color-red-600);
  background: var(--color-red-100);
  color: var(--color-red-700);
  box-shadow: var(--focus-ring-danger);
}

@media (hover: hover) and (pointer: fine) {
  :global(.share-card__revoke.ui-icon-button.p-button:not(:disabled):hover) {
    border-color: var(--color-red-600);
    background: var(--color-red-100);
    color: var(--color-red-700);
  }
}
</style>
