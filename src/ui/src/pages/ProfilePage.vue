<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from "vue";
import { getProfile } from "../features/auth/api/authApi";
import UserNameChangeDialog from "../features/auth/components/UserNameChangeDialog.vue";
import { refreshAuthState } from "../features/auth/model/authSession";
import type { Profile } from "../features/auth/model/auth";
import { isApiRequestCanceled, toApiError } from "../shared/api/apiError";
import { useHandoffDelayedLoadingState } from "../shared/composables/useSkeletonHandoff";
import { showToast } from "../shared/notifications/toastStore";
import UiButton from "../shared/ui/UiButton.vue";
import UiSkeleton from "../shared/ui/UiSkeleton.vue";
import UiSkeletonHandoff from "../shared/ui/UiSkeletonHandoff.vue";

const profile = ref<Profile | null>(null);
const loading = ref(true);
const loadingState = useHandoffDelayedLoadingState(loading);
const profileRefreshing = ref(false);
const loadingError = ref<string | null>(null);
const dialogOpen = ref(false);
let abortController: AbortController | null = null;

const createdAt = computed(() =>
  profile.value?.createdAt ? formatDateTime(profile.value.createdAt) : "",
);

onMounted(() => {
  void loadProfile();
});

onBeforeUnmount(() => {
  abortController?.abort();
});

async function loadProfile() {
  abortController?.abort();
  const currentAbortController = new AbortController();
  abortController = currentAbortController;
  loading.value = true;
  loadingError.value = null;

  try {
    const loadedProfile = await getProfile({
      signal: currentAbortController.signal,
    });
    profile.value = loadedProfile;
  } catch (error) {
    if (isApiRequestCanceled(error)) {
      return;
    }

    const apiError = toApiError(error);
    loadingError.value =
      apiError.status === 404
        ? "Профиль не найден"
        : "Не удалось загрузить профиль";
    showToast({
      status: "error",
      title: "Профиль",
      message: loadingError.value,
    });
  } finally {
    if (abortController === currentAbortController) {
      loading.value = false;
      abortController = null;
    }
  }
}

function openChangeUserNameDialog() {
  if (!profile.value) {
    return;
  }

  dialogOpen.value = true;
}

function closeChangeUserNameDialog() {
  dialogOpen.value = false;
}

function handleUserNameChanged() {
  dialogOpen.value = false;
  profileRefreshing.value = true;
  void syncProfileAfterUserNameChange();
}

async function syncProfileAfterUserNameChange() {
  try {
    const [updatedProfile] = await Promise.all([
      getProfile(),
      refreshAuthState(),
    ]);
    profile.value = updatedProfile;
  } catch {
    showToast({
      status: "error",
      title: "Обновление профиля",
      message: "Имя изменено, но актуальные данные не загрузились. Обновите страницу.",
    });
  } finally {
    profileRefreshing.value = false;
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
  <UiSkeletonHandoff
    :skeleton-visible="loadingState.visible || profileRefreshing"
    :content-visible="loadingState.ready && !profileRefreshing"
  >
    <template #skeleton>
      <div class="profile-skeleton" aria-hidden="true">
        <article class="profile-skeleton__content">
          <section class="profile-skeleton__hero">
            <div class="profile-skeleton__top">
              <UiSkeleton width="74px" height="14px" />
              <UiSkeleton width="168px" height="14px" />
            </div>
            <div class="profile-skeleton__main">
              <UiSkeleton width="58px" height="58px" border-radius="16px" />
              <div class="profile-skeleton__text">
                <UiSkeleton width="180px" height="28px" />
                <UiSkeleton width="240px" height="16px" />
              </div>
            </div>
          </section>
        </article>
      </div>
    </template>

    <div class="profile-page">
      <article v-if="profile" class="profile-page__content">
        <section class="profile-hero">
          <div class="profile-hero__top">
            <p class="profile-hero__eyebrow">Профиль</p>
            <p class="profile-hero__created">Создан {{ createdAt }}</p>
          </div>

        <div class="profile-hero__main">
          <div class="profile-hero__avatar" aria-hidden="true">
            <i class="pi pi-user"></i>
          </div>

          <div class="profile-hero__content">
            <div class="profile-hero__name-row">
              <h1 class="profile-page__title">{{ profile.userName }}</h1>
              <button class="profile-hero__edit" type="button" aria-label="Изменить имя пользователя"
                title="Изменить имя пользователя" @click="openChangeUserNameDialog">
                <i class="pi pi-pencil" aria-hidden="true"></i>
              </button>
            </div>
            <p class="profile-hero__email">{{ profile.email }}</p>
          </div>
        </div>
      </section>
    </article>

    <article v-else class="profile-page__content">
      <section class="profile-section profile-section--center">
        <h1 class="profile-page__title">Профиль</h1>
        <p class="profile-page__error">{{ loadingError }}</p>
        <UiButton variant="outlined" :loading="loading" @click="loadProfile">
          Повторить
        </UiButton>
      </section>
    </article>
    </div>
  </UiSkeletonHandoff>

  <UserNameChangeDialog
    v-if="profile"
    :visible="dialogOpen"
    :profile="profile"
    @dismissed="closeChangeUserNameDialog"
    @changed="handleUserNameChanged"
  />
</template>

<style scoped>
.profile-skeleton {
  flex: 1;
  min-height: 0;
  display: flex;
  justify-content: center;
  padding: 12px 0;
}

.profile-skeleton__content {
  width: min(100%, 520px);
}

.profile-skeleton__hero {
  padding: 18px;
  border: 1px solid var(--color-gray-200);
  border-radius: 16px;
  display: flex;
  flex-direction: column;
  gap: 12px;
  background: rgba(255, 255, 255, 0.94);
  box-shadow: 0 10px 24px rgba(25, 32, 43, 0.06);
}

.profile-skeleton__top,
.profile-skeleton__main {
  display: flex;
  align-items: flex-start;
  gap: 14px;
}

.profile-skeleton__top {
  justify-content: space-between;
}

.profile-skeleton__text {
  min-width: 0;
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.profile-page {
  flex: 1;
  min-height: 0;
  display: flex;
  justify-content: center;
  padding: 12px 0;
}

.profile-page__content {
  width: min(100%, 520px);
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.profile-hero {
  display: flex;
  flex-direction: column;
  gap: 12px;
  padding: 18px;
  border: 1px solid var(--color-gray-200);
  border-radius: 16px;
  background: rgba(255, 255, 255, 0.94);
  box-shadow: 0 10px 24px rgba(25, 32, 43, 0.06);
}

.profile-hero__top {
  width: 100%;
  min-width: 0;
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 12px;
}

.profile-hero__main {
  width: 100%;
  min-width: 0;
  display: flex;
  align-items: flex-start;
  gap: 14px;
}

.profile-hero__avatar {
  width: 58px;
  min-width: 58px;
  height: 58px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border-radius: 16px;
  background: var(--color-primary-50);
  color: var(--color-primary-600);
}

.profile-hero__avatar .pi {
  font-size: 24px;
}

.profile-hero__content {
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 5px;
}

.profile-hero__name-row {
  min-width: 0;
  display: flex;
  align-items: center;
  gap: 8px;
}

.profile-hero__name-row .profile-page__title {
  min-width: 0;
}

.profile-hero__edit {
  width: 30px;
  min-width: 30px;
  height: 30px;
  border: 0;
  border-radius: 8px;
  padding: 0;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: transparent;
  color: var(--p-surface-500);
  cursor: pointer;
  flex-shrink: 0;
  transition:
    background-color 0.16s ease,
    color 0.16s ease,
    transform 0.1s ease;
}

.profile-hero__edit .pi {
  font-size: 15px;
}

.profile-hero__edit:active {
  transform: translateY(1px);
}

.profile-hero__edit:focus-visible {
  outline: 2px solid var(--color-primary-600);
  outline-offset: 2px;
}

@media (hover: hover) and (pointer: fine) {
  .profile-hero__edit:hover {
    background: var(--color-primary-50);
    color: var(--color-primary-600);
  }
}

.profile-page__title,
.profile-hero__eyebrow,
.profile-hero__email,
.profile-hero__created,
.profile-page__error {
  margin: 0;
}

.profile-page__title {
  color: var(--color-gray-900);
  font-size: 28px;
  font-weight: 500;
  line-height: 1.12;
  overflow-wrap: anywhere;
}

.profile-hero__eyebrow {
  color: var(--color-primary-700);
  font-size: 13px;
  font-weight: 500;
  line-height: 1.25;
}

.profile-hero__email,
.profile-page__error {
  color: var(--color-gray-600);
  font-size: 15px;
  line-height: 1.45;
  overflow-wrap: anywhere;
}

.profile-hero__created {
  flex-shrink: 0;
  max-width: 240px;
  color: var(--p-text-muted-color);
  font-size: 13px;
  line-height: 1.3;
  text-align: right;
}

.profile-section {
  display: flex;
  flex-direction: column;
  gap: 12px;
  padding: 16px;
  border: 1px solid var(--color-gray-200);
  border-radius: 16px;
  background: rgba(255, 255, 255, 0.94);
  box-shadow: 0 10px 24px rgba(25, 32, 43, 0.06);
}

.profile-section--center {
  align-items: center;
  text-align: center;
}

@media (min-width: 768px) {
  .profile-page {
    padding-top: 20px;
  }

  .profile-page__content {
    gap: 14px;
  }
}

@media (max-width: 480px) {
  .profile-hero {
    padding: 16px;
  }

  .profile-hero__top {
    flex-direction: column;
    gap: 4px;
  }

  .profile-hero__created {
    max-width: 100%;
    text-align: left;
  }

  .profile-page__title {
    font-size: 25px;
  }

}
</style>
