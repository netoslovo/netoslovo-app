<template>
  <ToastStack />
  <div class="app-shell" :aria-busy="appBusy">
    <AppHeader :actions-disabled="headerActionsDisabled" :back-visible="headerBackVisible" @back="goBack"
      @logout-pending-change="logoutPending = $event" />
    <main ref="appMain" class="app-main" tabindex="-1" :aria-label="pageTitle">
      <section class="app-container" :class="{ 'app-container--wide': route.meta.wideContent }"
        :aria-busy="appContentBlocked">
        <UiSkeletonHandoff
          :skeleton-visible="appContentLoadingState.visible"
          :content-visible="appContentLoadingState.ready"
        >
          <template #skeleton>
            <div class="global-loading" role="status">
              <span class="global-loading__spinner" aria-hidden="true"></span>
              <span class="visually-hidden">Загрузка страницы</span>
            </div>
          </template>
          <template #default>
            <RouterView />
          </template>
        </UiSkeletonHandoff>
      </section>
    </main>
  </div>
</template>

<script setup lang="ts">
import { computed, nextTick, ref, watch } from "vue";
import { RouterView, useRoute, useRouter } from "vue-router";
import { getRoutePageIdentity, getRoutePageTitle, routeNavigationLoading } from "./app/router";
import AppHeader from "./app/components/AppHeader.vue";
import ToastStack from "./app/components/ToastStack.vue";
import UiSkeletonHandoff from "./shared/ui/UiSkeletonHandoff.vue";
import { useDelayedLoadingState } from "./shared/composables/useDelayedLoading";
import { provideSkeletonHandoff } from "./shared/composables/useSkeletonHandoff";
import { authBootstrapState, bootstrapAuthState } from "./features/auth/model/authSession";

const route = useRoute();
const router = useRouter();
const appMain = ref<HTMLElement | null>(null);
const logoutPending = ref(false);
const pageTitle = computed(() => getRoutePageTitle(route));
const headerBackVisible = computed(
  () => route.meta.showBack === true && Boolean(window.history.state?.back),
);
const contentLoading = computed(
  () => authBootstrapState.value === "loading" || route.matched.length === 0,
);
const appContentBlocked = computed(
  () => logoutPending.value || contentLoading.value || routeNavigationLoading.value,
);
const appContentLoadingState = useDelayedLoadingState(appContentBlocked);
provideSkeletonHandoff(appContentLoadingState.visible);
const headerActionsDisabled = computed(
  () => logoutPending.value || authBootstrapState.value !== "ready" || contentLoading.value || routeNavigationLoading.value,
);
const appBusy = computed(() => appContentBlocked.value || logoutPending.value);

watch(
  () => getRoutePageIdentity(route),
  async () => {
    await nextTick();
    appMain.value?.focus({ preventScroll: true });
  },
  { flush: "post" },
);

function goBack() {
  router.back();
}

if (authBootstrapState.value !== "ready") {
  void bootstrapAuthState().catch(() => undefined);
}
</script>

<style scoped>
.app-shell {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

.app-main {
  flex: 1;
  width: 100%;
  min-width: 0;
  min-height: 0;
  display: flex;
  justify-content: center;
}

.app-main:focus {
  outline: none;
}

.app-container {
  position: relative;
  flex: 1;
  width: 100%;
  min-width: 0;
  max-width: var(--container-sm);
  display: flex;
  flex-direction: column;
  padding: 10px 16px 24px;
}

.app-container--wide {
  max-width: 1180px;
}

.global-loading {
  flex: 1;
  min-height: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--color-gray-50);
}

.global-loading__spinner {
  width: 32px;
  height: 32px;
  border: 3px solid var(--color-gray-200);
  border-top-color: var(--color-gray-500);
  border-radius: 50%;
  animation: global-loading-spin .75s linear infinite;
}

@keyframes global-loading-spin {
  to {
    transform: rotate(360deg);
  }
}

@media (prefers-reduced-motion: reduce) {
  .global-loading__spinner {
    animation-duration: 1.5s;
  }
}

@media (min-width: 768px) {
  .app-container {
    padding-top: 20px;
  }
}
</style>
