<script setup lang="ts">
import Dialog from "primevue/dialog";
import Checkbox from "primevue/checkbox";
import { computed, ref, watch } from "vue";
import { useRouter } from "vue-router";
import { changeUserName } from "../api/authApi";
import type { Profile } from "../model/auth";
import { toApiError } from "../../../shared/api/apiError";
import { showToast } from "../../../shared/notifications/toastStore";
import UiButton from "../../../shared/ui/UiButton.vue";
import UiInput from "../../../shared/ui/UiInput.vue";

type ChangeUserNameErrorCode =
  | "UserNotFound"
  | "InvalidUserName"
  | "UnsafeUserName"
  | "DuplicateUserName"
  | "TooFrequentAttempts"
  | "ConcurrencyFailure"
  | "Unauthorized";

const props = withDefaults(
  defineProps<{
    visible: boolean;
    profile: Profile;
    initialPrompt?: boolean;
    dismissPending?: boolean;
  }>(),
  {
    initialPrompt: false,
    dismissPending: false,
  },
);

const emit = defineEmits<{
  changed: [];
  dismissed: [doNotShowAgain: boolean];
}>();

const router = useRouter();
const confirmationOpen = ref(false);
const keepCurrentConfirmationOpen = ref(false);
const newUserName = ref("");
const pendingUserName = ref("");
const userNameError = ref<string | null>(null);
const submitting = ref(false);
const doNotShowAgain = ref(false);

const userNameChangedAt = computed(() =>
  props.profile.userNameChangedAt
    ? formatDateTime(props.profile.userNameChangedAt)
    : "Не менялось",
);

watch(
  () => props.visible,
  (visible) => {
    if (visible) {
      resetDialog();
    }
  },
);

watch(newUserName, () => {
  if (userNameError.value !== null) {
    validateUserName();
  }
});

function resetDialog() {
  confirmationOpen.value = false;
  keepCurrentConfirmationOpen.value = false;
  newUserName.value = "";
  pendingUserName.value = "";
  userNameError.value = null;
  doNotShowAgain.value = false;
}

function dismissDialog() {
  if (submitting.value || props.dismissPending) {
    return;
  }

  confirmationOpen.value = false;
  emit("dismissed", doNotShowAgain.value);
}

function requestKeepCurrentUserName() {
  if (!submitting.value) {
    keepCurrentConfirmationOpen.value = true;
  }
}

function closeKeepCurrentConfirmation() {
  if (!submitting.value && !props.dismissPending) {
    keepCurrentConfirmationOpen.value = false;
    doNotShowAgain.value = false;
  }
}

function closeConfirmationDialog() {
  if (!submitting.value) {
    confirmationOpen.value = false;
  }
}

function validateUserName() {
  if (!newUserName.value.trim()) {
    userNameError.value = "Введите имя пользователя";
    return false;
  }

  userNameError.value = null;
  return true;
}

function submitUserName() {
  if (submitting.value || !validateUserName()) {
    return;
  }

  const normalizedUserName = newUserName.value.trim();
  pendingUserName.value = normalizedUserName;
  confirmationOpen.value = true;
}

async function confirmUserNameChange() {
  if (submitting.value || !pendingUserName.value) {
    return;
  }

  submitting.value = true;
  try {
    await changeUserName(pendingUserName.value);
    confirmationOpen.value = false;
    showToast({
      status: "success",
      title: "Имя пользователя",
      message: "Имя изменено.",
    });
    emit("changed");
  } catch (error) {
    confirmationOpen.value = false;
    handleChangeUserNameError(error);
  } finally {
    submitting.value = false;
  }
}

function handleChangeUserNameError(error: unknown) {
  const apiError = toApiError(error);
  const message = getChangeUserNameErrorMessage(
    apiError.code as ChangeUserNameErrorCode | undefined,
    apiError.status,
  );

  userNameError.value = message;
  showToast({
    status: "error",
    title: "Имя пользователя",
    message,
  });

  if (apiError.status === 401 || apiError.code === "Unauthorized") {
    void router.push({ name: "login" });
  }
}

function getChangeUserNameErrorMessage(
  code: ChangeUserNameErrorCode | undefined,
  status: number | undefined,
) {
  switch (code) {
    case "InvalidUserName":
      return "Длина имени пользователя должна быть от 3 до 16 символов. Разрешены русские и латинские буквы, цифры, дефис, точка и нижнее подчёркивание.";
    case "UnsafeUserName":
      return "Это имя пользователя недоступно.";
    case "DuplicateUserName":
      return "Это имя пользователя уже занято.";
    case "TooFrequentAttempts":
      return "С последней смены имени прошло меньше 30 дней. Попробуйте позже.";
    case "ConcurrencyFailure":
      return "Произошла ошибка, попробуйте позже.";
    case "Unauthorized":
      return "Сессия истекла. Войдите снова.";
    case "UserNotFound":
      return "Профиль не найден.";
    default:
      return status === 401
        ? "Сессия истекла. Войдите снова."
        : "Произошла ошибка, попробуйте позже.";
  }
}

function formatDateTime(value: string) {
  return new Intl.DateTimeFormat("ru-RU", {
    day: "numeric",
    month: "long",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  }).format(new Date(value));
}
</script>

<template>
  <Dialog :visible="visible" modal :dismissable-mask="!submitting && !dismissPending"
    :closable="!submitting && !dismissPending" :close-on-escape="!submitting && !dismissPending"
    :content-props="{ 'aria-busy': submitting || dismissPending ? 'true' : undefined }"
    :header="initialPrompt ? 'Придумайте имя пользователя' : 'Сменить имя пользователя'" class="game-dialog"
    @update:visible="(value) => !value && dismissDialog()">
    <form class="username-dialog" novalidate @submit.prevent="submitUserName">
      <div v-if="initialPrompt" class="username-dialog__intro">
        <p>Вам было автоматически присвоено имя:</p>
        <div class="username-card username-card--initial">
          <div class="username-card__value">
            <span>{{ profile.userName }}</span>
          </div>
        </div>
        <p>Вы можете установить своё имя пользователя: оно будет
          отображаться в таблицах лучших игроков и в вашем профиле.</p>
      </div>

      <div v-else class="username-card">
        <div class="username-card__value">
          <span class="username-card__prefix">@</span>
          <span>{{ profile.userName }}</span>
        </div>
        <div class="username-card__meta">
          <span>Последнее изменение</span>
          <strong>{{ userNameChangedAt }}</strong>
        </div>
      </div>
      <UiInput v-model="newUserName" label="Новое имя пользователя" floating-label name="userName" type="text"
        placeholder="Новое имя пользователя" :max-length="16" :disabled="submitting" :error="userNameError"
        show-error-message>
        <template #left>@</template>
      </UiInput>

      <div class="username-policy">
        <i class="pi pi-info-circle" aria-hidden="true"></i>
        <span>Имя пользователя должно содержать от 3 до 16 символов. Разрешены строчные и заглавные буквы русского и
          латинского алфавитов, цифры, дефис (-), точка (.) и нижнее подчёркивание (_).</span>
      </div>

      <div class="username-policy">
        <i class="pi pi-info-circle" aria-hidden="true"></i>
        <span>Имя пользователя можно менять не чаще чем раз в 30 дней.</span>
      </div>

      <div v-if="initialPrompt" class="username-dialog__actions username-dialog__actions--initial">
        <UiButton variant="outlined" :disabled="submitting" @click="requestKeepCurrentUserName">
          Не менять
        </UiButton>
        <UiButton type="submit" :loading="submitting">
          Сохранить
        </UiButton>
      </div>

      <div v-else class="username-dialog__actions">
        <UiButton variant="outlined" :disabled="submitting" @click="dismissDialog">
          Отмена
        </UiButton>
        <UiButton type="submit" :loading="submitting">
          Сохранить
        </UiButton>
      </div>
    </form>
  </Dialog>

  <Dialog :visible="keepCurrentConfirmationOpen" modal :dismissable-mask="!dismissPending" :closable="!dismissPending"
    :close-on-escape="!dismissPending" header="Текущее имя пользователя" class="game-dialog"
    @update:visible="(value) => !value && closeKeepCurrentConfirmation()">
    <div class="username-dialog">
      <p class="username-dialog__text">
        У вас останется имя пользователя <strong>{{ profile.userName }}</strong>. Его всегда можно изменить в профиле.
      </p>

      <div class="username-dialog__do-not-show">
        <Checkbox input-id="username-prompt-do-not-show" v-model="doNotShowAgain" binary :disabled="dismissPending" />
        <label for="username-prompt-do-not-show">Больше не предлагать смену имени</label>
      </div>

      <div class="username-dialog__actions">
        <UiButton :loading="dismissPending" @click="dismissDialog">
          Продолжить
        </UiButton>
      </div>
    </div>
  </Dialog>

  <Dialog :visible="confirmationOpen" modal :dismissable-mask="!submitting" :closable="!submitting"
    :close-on-escape="!submitting" :content-props="{ 'aria-busy': submitting ? 'true' : undefined }"
    header="Подтвердить смену имени" class="game-dialog"
    @update:visible="(value) => !value && closeConfirmationDialog()">
    <div class="username-dialog">
      <p class="username-dialog__text">
        Вы уверены, что хотите изменить имя пользователя?
      </p>

      <div class="username-change">
        <div class="username-change__item">
          <span>Сейчас</span>
          <strong>{{ profile.userName }}</strong>
        </div>
        <i class="pi pi-arrow-right" aria-hidden="true"></i>
        <div class="username-change__item">
          <span>Будет</span>
          <strong>{{ pendingUserName }}</strong>
        </div>
      </div>

      <p class="username-dialog__note">
        Следующая смена будет доступна через 30 дней.
      </p>

      <div class="username-dialog__actions">
        <UiButton variant="outlined" :disabled="submitting" @click="closeConfirmationDialog">
          Отмена
        </UiButton>
        <UiButton :loading="submitting" @click="confirmUserNameChange">
          Подтвердить
        </UiButton>
      </div>
    </div>
  </Dialog>
</template>

<style scoped>
.username-dialog {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.username-dialog__intro,
.username-dialog__text,
.username-dialog__note {
  margin: 0;
  color: var(--color-gray-600);
  font-size: var(--game-dialog-body-font-size);
  line-height: 1.45;
}

.username-dialog__intro {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.username-dialog__intro p {
  margin: 0;
}

.username-dialog__note {
  font-size: 13px;
}

.username-card {
  display: flex;
  align-items: stretch;
  justify-content: space-between;
  gap: 12px;
  padding: 14px;
  border: 1px solid var(--color-gray-200);
  border-radius: 12px;
  background: var(--color-gray-50);
}

.username-card__value {
  min-width: 0;
  display: flex;
  align-items: center;
  gap: 8px;
  color: var(--color-gray-900);
  font-size: 20px;
  font-weight: 500;
  line-height: 1.25;
  overflow-wrap: anywhere;
}

.username-card--initial .username-card__value {
  margin: 0 auto;
}

.username-card__value span:last-child {
  min-width: 0;
  overflow-wrap: anywhere;
}

.username-card__prefix {
  color: var(--color-primary-600);
}

.username-card__meta {
  min-width: 140px;
  display: flex;
  flex-direction: column;
  justify-content: center;
  gap: 3px;
  color: var(--p-text-muted-color);
  font-size: 13px;
  line-height: 1.3;
  text-align: right;
}

.username-card__meta strong {
  color: var(--color-gray-700);
  font-size: 14px;
  font-weight: 500;
}

.username-policy {
  display: flex;
  align-items: flex-start;
  gap: 8px;
  color: var(--p-text-muted-color);
  font-size: 13px;
  line-height: 1.4;
}

.username-policy .pi {
  margin-top: 2px;
  color: var(--p-text-muted-color);
  font-size: 13px;
}

.username-change {
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto minmax(0, 1fr);
  align-items: center;
  gap: 10px;
  padding: 14px;
  border: 1px solid var(--color-gray-200);
  border-radius: 12px;
  background: var(--color-gray-50);
}

.username-change>.pi {
  color: var(--color-primary-600);
  font-size: 14px;
}

.username-change__item {
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.username-change__item span {
  color: var(--p-text-muted-color);
  font-size: 13px;
  line-height: 1.2;
}

.username-change__item strong {
  color: var(--color-gray-900);
  font-size: 16px;
  line-height: 1.25;
  overflow-wrap: anywhere;
}

.username-dialog__actions {
  display: flex;
  gap: 10px;
}

.username-dialog__actions>.ui-button {
  flex: 1;
}

.username-dialog__do-not-show {
  display: flex;
  align-items: center;
  gap: 8px;
  color: var(--color-gray-700);
  font-size: 14px;
}

.username-dialog__do-not-show label {
  cursor: pointer;
}

@media (min-width: 481px) {
  .username-change {
    display: flex;
    justify-content: space-evenly;
    gap: 0;
    padding-inline: 0;
  }

  .username-change__item:last-child {
    align-items: flex-start;
    text-align: left;
  }
}

@media (max-width: 480px) {
  .username-card {
    flex-direction: column;
    align-items: center;
    text-align: center;
  }

  .username-card__value,
  .username-policy {
    justify-content: center;
  }

  .username-card__meta {
    min-width: 0;
    text-align: center;
  }

  .username-policy {
    text-align: center;
  }

  .username-dialog__actions {
    flex-direction: column;
  }

  .username-dialog__actions--initial {
    flex-direction: column-reverse;
  }

  .username-change {
    grid-template-columns: 1fr;
    justify-items: center;
    text-align: center;
  }

  .username-change>.pi {
    transform: rotate(90deg);
  }
}
</style>
