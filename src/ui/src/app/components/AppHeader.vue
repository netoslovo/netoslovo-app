<script setup lang="ts">
import { computed, ref } from "vue";
import { RouterLink, useRouter } from "vue-router";
import { authState, logoutUser } from "../../features/auth/model/authSession";
import { adminRoles, hasRole } from "../../features/auth/model/permissions";
import { showToast } from "../../shared/notifications/toastStore";
import UiMenu from "../../shared/ui/UiMenu.vue";
import type { UiMenuItem } from "../../shared/ui/UiMenu.vue";

const props = defineProps<{
  actionsDisabled?: boolean;
  backVisible?: boolean;
}>();

const emit = defineEmits<{
  back: [];
  "logout-pending-change": [value: boolean];
}>();

const router = useRouter();
const loggingOut = ref(false);
const currentUserLabel = computed(
  () => authState.value?.userName ?? authState.value?.email ?? "",
);
const isAuthenticated = computed(
  () => authState.value?.isAuthenticated === true,
);
const canOpenAdmin = computed(() =>
  hasRole(authState.value, adminRoles.admin),
);
const actionsLocked = computed(() => props.actionsDisabled === true || loggingOut.value);
const headerMenuItems = computed<UiMenuItem[]>(() => {
  const items: UiMenuItem[] = [
    {
      label: "На главную",
      icon: "pi pi-home",
      to: { name: "home" },
      disabled: actionsLocked.value,
    },
    { separator: true },
  ];

  if (!isAuthenticated.value) {
    items.push({
      label: "Войти",
      icon: "pi pi-sign-in",
      to: { name: "login" },
      disabled: actionsLocked.value,
    });
    return items;
  }

  const accountItems: UiMenuItem[] = [
    {
      label: "Профиль",
      icon: "pi pi-user",
      to: { name: "profile" },
      disabled: actionsLocked.value,
    },
  ];

  if (canOpenAdmin.value) {
    accountItems.push({
      label: "Администрирование",
      icon: "pi pi-cog",
      to: { name: "admin-home" },
      disabled: actionsLocked.value,
    });
  }

  accountItems.push({
    label: "Выйти",
    icon: "pi pi-sign-out",
    loading: loggingOut.value,
    disabled: actionsLocked.value,
    closeOnActivate: false,
    activate: (_event, close) => void onLogout(close),
  });

  items.push({
    label: currentUserLabel.value,
    items: accountItems,
  });

  return items;
});

async function onLogout(closeMenu: () => void) {
  if (actionsLocked.value) {
    return;
  }

  loggingOut.value = true;
  emit("logout-pending-change", true);
  try {
    await logoutUser();
    closeMenu();
    await router.push({ name: "home" });
  } catch {
    showToast({
      status: "error",
      title: "Выход из аккаунта",
      message: "Не удалось завершить сеанс. Попробуйте ещё раз.",
    });
  } finally {
    loggingOut.value = false;
    emit("logout-pending-change", false);
  }
}

function onBackClick(event: MouseEvent) {
  if (actionsLocked.value) {
    return;
  }

  (event.currentTarget as HTMLButtonElement).blur();
  emit("back");
}

function onLogoClick(event: MouseEvent) {
  if (!actionsLocked.value) {
    return;
  }

  event.preventDefault();
}

</script>

<template>
  <header class="app-header">
    <div class="app-header__container">
      <button v-if="backVisible" class="app-header__back" type="button" aria-label="Назад" :disabled="actionsLocked"
        @click="onBackClick">
        <i class="pi pi-arrow-left" aria-hidden="true"></i>
      </button>
      <div v-else class="app-header__back-spacer" aria-hidden="true"></div>

      <RouterLink class="game-logo-button" :to="{ name: 'home' }" aria-label="На главную" :aria-disabled="actionsLocked"
        :tabindex="actionsLocked ? -1 : undefined" @click="onLogoClick">
        <img class="game-logo" src="/logo.svg" alt="нетослово" />
      </RouterLink>

      <div class="app-header__actions">
        <UiMenu :items="headerMenuItems" :disabled="actionsLocked" />
      </div>
    </div>
  </header>
</template>

<style scoped>
.app-header {
  position: sticky;
  top: 0;
  z-index: 10;
  flex-shrink: 0;
  min-height: calc(var(--app-header-height) + env(safe-area-inset-top));
  padding-top: env(safe-area-inset-top);
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--p-primary-color);
  color: var(--p-primary-contrast-color);
}

.app-header__container {
  width: min(100%, var(--container-sm));
  min-height: var(--app-header-height);
  display: grid;
  grid-template-columns: var(--app-header-side-size) minmax(0, 1fr) var(--app-header-side-size);
  align-items: center;
  gap: 10px;
  padding-left: max(12px, env(safe-area-inset-left));
  padding-right: max(12px, env(safe-area-inset-right));
}

.app-header__actions {
  min-width: 0;
  display: flex;
  align-items: center;
  justify-content: flex-end;
  justify-self: end;
  gap: 8px;
}

.app-header__back,
.app-header__back-spacer {
  width: var(--app-header-side-size);
  height: var(--app-header-side-size);
}

.app-header__back {
  border: 0;
  border-radius: 8px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: transparent;
  color: var(--p-primary-100);
  cursor: pointer;
  transition:
    background-color 0.16s ease,
    transform 0.1s ease;
}

.app-header__back:active {
  transform: translateY(1px);
}

.app-header__back:disabled {
  opacity: 0.55;
  cursor: default;
  transform: none;
}

.app-header__back:focus-visible {
  outline: none;
  box-shadow: var(--focus-ring-inverse);
}

.app-header__back .pi {
  font-size: 18px;
}

.game-logo-button {
  border: 0;
  padding: 4px 0;
  display: inline-flex;
  align-items: center;
  flex-shrink: 0;
  justify-self: center;
  background: transparent;
  cursor: pointer;
  transition:
    opacity 0.16s ease,
    transform 0.1s ease;
}

.game-logo-button:hover {
  opacity: 0.88;
}

.game-logo-button[aria-disabled="true"] {
  opacity: 0.55;
  cursor: default;
  pointer-events: none;
}

.game-logo-button:active {
  transform: translateY(1px);
}

.game-logo-button:focus-visible {
  outline: none;
  box-shadow: var(--focus-ring-inverse);
}

.game-logo {
  display: block;
  width: min(var(--game-logo-width), calc(100vw - 96px));
  height: auto;
  object-fit: contain;
}

@media (min-width: 768px) {
  .app-header__actions {
    grid-column: 3;
  }
}

@media (hover: hover) and (pointer: fine) {
  .app-header__back:hover {
    background: rgba(255, 255, 255, 0.14);
  }
}
</style>
