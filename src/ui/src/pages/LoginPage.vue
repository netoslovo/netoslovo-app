<script setup lang="ts">
import { computed, nextTick, onMounted, ref, watch } from "vue";
import {
  useRoute,
  useRouter,
  type RouteLocationRaw,
} from "vue-router";
import { getProfile } from "../features/auth/api/authApi";
import UserNameChangeDialog from "../features/auth/components/UserNameChangeDialog.vue";
import { toApiError } from "../shared/api/apiError";
import {
  isAuthenticated,
  refreshAuthState,
  requestLoginCode,
  verifyLoginCode,
} from "../features/auth/model/authSession";
import type { OtpVerifyFailureStatus } from "../features/auth/model/authSession";
import type { Profile } from "../features/auth/model/auth";
import { userNoticeCodes } from "../features/user-notices/model/userNoticeCodes";
import { useUserNoticeStore } from "../features/user-notices/model/userNoticeStore";
import { showToast } from "../shared/notifications/toastStore";
import UiInput from "../shared/ui/UiInput.vue";
import UiButton from "../shared/ui/UiButton.vue";

const router = useRouter();
const route = useRoute();
const userNotices = useUserNoticeStore();

type UiInputRef = {
  focusInput: () => void;
};

const email = ref("");
const code = ref("");
const codeInput = ref<UiInputRef | null>(null);
const emailSent = ref(false);
const requestedChallengeId = ref("");
const requestedChallengeEmail = ref("");
const emailLoading = ref(false);
const codeLoading = ref(false);
const emailError = ref<string | null>(null);
const codeError = ref<string | null>(null);
const userNamePromptProfile = ref<Profile | null>(null);
const pendingDestination = ref<RouteLocationRaw | null>(null);
const userNamePromptFinishing = ref(false);
const isChallengeEmailCurrent = computed(
  () =>
    emailSent.value &&
    normalizeEmailForComparison(email.value) ===
    normalizeEmailForComparison(requestedChallengeEmail.value),
);

watch(email, () => {
  if (emailError.value !== null) {
    validateEmail();
  }
});

watch(code, () => {
  if (codeError.value !== null) {
    validateCode();
  }
});

onMounted(() => {
  if (isAuthenticated.value) {
    void navigateAfterLogin();
    return;
  }
});

function validateEmail() {
  const value = email.value.trim();
  if (!value) {
    emailError.value = "Введите email";
    return false;
  }

  if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value)) {
    emailError.value = "Неверный email";
    return false;
  }

  emailError.value = null;
  return true;
}

function validateCode() {
  if (!code.value.trim()) {
    codeError.value = "Введите код подтверждения";
    return false;
  }

  codeError.value = null;
  return true;
}

function normalizeEmailForComparison(value: string) {
  return value.trim().toLowerCase();
}

async function sendCode() {
  if (emailLoading.value) {
    return false;
  }

  if (!validateEmail()) {
    return false;
  }

  const normalizedEmail = email.value.trim();

  emailLoading.value = true;
  try {
    const result = await requestLoginCode(normalizedEmail);
    if (result.status === "alreadyAuthenticated") {
      await finishAlreadyAuthenticated();
      return true;
    }

    email.value = normalizedEmail;
    requestedChallengeId.value = result.challengeId;
    requestedChallengeEmail.value = normalizedEmail;
    emailSent.value = true;
    code.value = "";
    codeError.value = null;
    await nextTick();
    codeInput.value?.focusInput();
    showToast({
      status: "success",
      title: "Код для входа",
      message: "Код отправлен на указанный email.",
    });
    return true;
  } catch (error) {
    const apiError = toApiError(error);
    const rateLimitExceeded = apiError.status === 429;
    showToast({
      status: "error",
      title: "Отправка кода",
      message: rateLimitExceeded
        ? "Для этого email слишком часто запрашивали код. Попробуйте ещё раз немного позже."
        : "Не удалось отправить код. Попробуйте позже.",
    });
    return false;
  } finally {
    emailLoading.value = false;
  }
}

async function submitCode() {
  if (codeLoading.value) {
    return;
  }

  if (!requestedChallengeId.value || !validateCode()) {
    return;
  }

  codeLoading.value = true;
  try {
    const result = await verifyLoginCode(
      requestedChallengeId.value,
      code.value.trim(),
    );

    if (
      result.status !== "completed" &&
      result.status !== "alreadyAuthenticated"
    ) {
      handleVerifyFailure(result.status);
      return;
    }

    showToast({
      status: "success",
      title: "Вход в аккаунт",
      message: "Вы авторизованы.",
    });
    await offerUserNameBeforeNavigation();
  } catch (error) {
    const apiError = toApiError(error);
    showToast({
      status: "error",
      title: "Вход по коду",
      message:
        apiError.status === 401
          ? "Код неверный."
          : "Не удалось проверить код. Попробуйте позже.",
    });
  } finally {
    codeLoading.value = false;
  }
}

function handleVerifyFailure(status: OtpVerifyFailureStatus) {
  const message = getVerifyFailureMessage(status);

  codeError.value = message;
  showToast({
    status: "error",
    title: "Вход по коду",
    message,
  });
}

function getVerifyFailureMessage(status: OtpVerifyFailureStatus) {
  switch (status) {
    case "otpNotFound":
      return "Код не найден или срок его действия истёк. Запросите новый код.";
    case "invalidOtpState":
      return "Код неверный или уже недействителен. Проверьте код или запросите новый.";
    case "invalidGuestSession":
      return "Сессия входа изменилась. Запросите новый код.";
    case "emailAlreadyRegistered":
      return "Этот email уже был зарегистрирован другим запросом. Запросите новый код для входа.";
    case "concurrencyFailure":
      return "Вход столкнулся с другим запросом. Попробуйте ещё раз немного позже.";
    case "getOrAddUserError":
      return "Не удалось завершить вход. Попробуйте позже.";
  }
}

async function finishAlreadyAuthenticated() {
  showToast({
    status: "success",
    title: "Вход в аккаунт",
    message: "Вы авторизованы.",
  });
  await navigateAfterLogin();
}

async function navigateAfterLogin() {
  await router.replace(getPostLoginDestination());
}

async function offerUserNameBeforeNavigation() {
  const destination = getPostLoginDestination();

  try {
    const profile = await getProfile();
    if (
      profile.userNameChangedAt === null &&
      await userNotices.shouldShowUserNotice(userNoticeCodes.defaultUserName)
    ) {
      pendingDestination.value = destination;
      userNamePromptProfile.value = profile;
      return;
    }
  } catch {
    // The profile check must not block login navigation.
  }

  await router.replace(destination);
}

function getPostLoginDestination(): RouteLocationRaw {
  const returnTo = route.query.returnTo;
  return typeof returnTo === "string" &&
    returnTo.startsWith("/") &&
    !returnTo.startsWith("//")
    ? returnTo
    : { name: "home" };
}

function dismissUserNamePrompt(doNotShowAgain: boolean) {
  void finishUserNamePrompt(false, doNotShowAgain);
}

function handleUserNameChanged() {
  void finishUserNamePrompt(true);
}

async function finishUserNamePrompt(
  refreshSession: boolean,
  doNotShowAgain = false,
) {
  if (userNamePromptFinishing.value) {
    return;
  }

  userNamePromptFinishing.value = true;
  const destination = pendingDestination.value ?? getPostLoginDestination();

  if (doNotShowAgain) {
    try {
      await userNotices.saveRecurringUserNoticeView(
        userNoticeCodes.defaultUserName,
        true,
      );
    } catch {
      showToast({
        status: "error",
        title: "Обработка запроса",
        message: "Произошла ошибка обработки запроса, попробуйте позже",
      });
      userNamePromptFinishing.value = false;
      return;
    }
  } else {
    void userNotices.saveRecurringUserNoticeView(
      userNoticeCodes.defaultUserName,
      false,
    ).catch(() => undefined);
  }

  pendingDestination.value = null;
  userNamePromptProfile.value = null;

  if (refreshSession) {
    try {
      await refreshAuthState();
    } catch {
      showToast({
        status: "error",
        title: "Обновление профиля",
        message: "Имя изменено, но актуальные данные не загрузились. Обновите страницу.",
      });
    }
  }

  await router.replace(destination);
}
</script>

<template>
  <div class="login-page">
    <article class="login-page__card">
      <header class="login-page__header">
        <h1 class="login-page__title">Вход</h1>
      </header>

      <form class="login-form__section" autocomplete="on" novalidate @submit.prevent="sendCode">
        <div class="login-form__intro">
          <div class="login-form__title">Вход по email</div>
          <div class="login-form__description">
            Отправим одноразовый код на вашу почту для входа без пароля.
          </div>
        </div>
        <UiInput v-model="email" class="login-form__input login-form__input--email" label="Email"
          input-id="login-email" name="email" type="email" autocomplete="username"
          placeholder="name@example.com" floating-label :error="emailError"
          :disabled="emailLoading || codeLoading" show-error-message>
          <template #left>@</template>
        </UiInput>
        <UiButton class="login-form__submit" type="submit" :loading="emailLoading"
          :disabled="emailLoading || codeLoading || isChallengeEmailCurrent">
          {{ isChallengeEmailCurrent ? "Код отправлен" : "Отправить код" }}
        </UiButton>
        <UiButton v-if="isChallengeEmailCurrent" class="login-form__secondary" type="submit" variant="outlined"
          :loading="emailLoading" :disabled="codeLoading">
          Отправить новый код
        </UiButton>
      </form>

      <form v-if="emailSent" class="login-form__section login-form__section--confirm" @submit.prevent="submitCode">
        <div class="login-form__intro">
          <div class="login-form__title">Подтверждение</div>
          <div class="login-form__description">
            Введите код из письма, отправленного на {{ requestedChallengeEmail }}.
          </div>
        </div>
        <UiInput ref="codeInput" v-model="code" class="login-form__input login-form__input--code" label="Введите код" name="code"
          type="text" autocomplete="one-time-code" input-mode="numeric" placeholder="Введите код" floating-label
          :error="codeError" :disabled="codeLoading" show-error-message>
          <template #left>#</template>
        </UiInput>
        <UiButton class="login-form__submit" type="submit" :loading="codeLoading"
          :disabled="emailLoading || codeLoading">
          Подтвердить вход
        </UiButton>
      </form>
    </article>
  </div>

  <UserNameChangeDialog
    v-if="userNamePromptProfile"
    visible
    :profile="userNamePromptProfile"
    initial-prompt
    :dismiss-pending="userNamePromptFinishing"
    @dismissed="dismissUserNamePrompt"
    @changed="handleUserNameChanged"
  />
</template>

<style scoped>
.login-page {
  flex: 1;
  min-height: 0;
  display: flex;
  justify-content: center;
  padding-top: 16px;
}

.login-page__card {
  width: min(100%, 420px);
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.login-page__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}

.login-page__title {
  margin: 0;
  color: var(--color-gray-900);
  font-size: 28px;
  font-weight: 500;
  line-height: 1.15;
}

.login-form__section {
  display: flex;
  flex-direction: column;
  gap: 12px;
  padding: 16px;
  border: 1px solid var(--color-gray-200);
  border-radius: 8px;
  background: rgba(255, 255, 255, 0.92);
  box-shadow: 0 10px 24px rgba(25, 32, 43, 0.06);
}

.login-form__section--confirm {
  border-color: var(--color-primary-100);
}

.login-form__input {
  min-width: 0;
}

.login-form__intro {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.login-form__title {
  color: var(--color-gray-800);
  font-size: 18px;
  font-weight: 500;
}

.login-form__description {
  color: var(--color-gray-600);
  font-size: 14px;
  line-height: 1.45;
}

.login-form__submit.ui-button.p-button {
  width: 100%;
  font-size: 16px;
}

.login-form__secondary.ui-button.p-button {
  width: 100%;
}

.login-form__input :deep(.ui-input-wrap.p-inputgroup .ui-input.p-inputtext) {
  font-size: 16px;
}

.login-form__input--code :deep(.ui-input-wrap.p-inputgroup .ui-input.p-inputtext) {
  letter-spacing: 0.08em;
}

.login-form__input--code :deep(.ui-input-wrap.p-inputgroup .ui-input.p-inputtext::placeholder) {
  letter-spacing: normal;
}
</style>
