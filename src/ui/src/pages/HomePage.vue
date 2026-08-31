<script setup lang="ts">
import { computed } from "vue";
import { isAuthenticated } from "../features/auth/model/authSession";
import GameHome from "../features/games/home/GameHome.vue";
import { useGameHome } from "../features/games/home/useGameHome";
import { useHandoffDelayedLoadingState } from "../shared/composables/useSkeletonHandoff";
import UiSkeleton from "../shared/ui/UiSkeleton.vue";
import UiSkeletonHandoff from "../shared/ui/UiSkeletonHandoff.vue";

const {
  arcadeGame,
  dailyGames,
  dailyStreak,
  arcadeFailed,
  todayFailed,
  dailyHistoryFailed,
  loading,
  authFailed,
  statusMessage,
  retry,
} = useGameHome();
const loadingState = useHandoffDelayedLoadingState(loading);
const contentVisible = computed(() =>
  loadingState.ready || arcadeGame.value !== null || dailyGames.value.length > 0 || statusMessage.value !== null,
);
</script>

<template>
  <h1 class="visually-hidden">нетослово.рф</h1>
  <UiSkeletonHandoff :skeleton-visible="loadingState.visible" :content-visible="contentVisible">
    <template #skeleton>
      <div class="home-skeleton" aria-hidden="true">
        <section class="home-skeleton__card" v-if="!isAuthenticated">
          <div class="home-skeleton__header">
            <UiSkeleton width="82px" height="20px" />
          </div>
          <UiSkeleton height="40px" border-radius="8px" />
        </section>
        <section class="home-skeleton__card home-skeleton__card--daily">
          <div class="home-skeleton__header">
            <UiSkeleton width="112px" height="20px" />
            <UiSkeleton width="88px" height="28px" border-radius="8px" />
          </div>
          <div class="home-skeleton__today">
            <UiSkeleton width="40px" height="40px" border-radius="8px" />
            <UiSkeleton width="38%" height="18px" />
            <UiSkeleton width="40px" height="40px" border-radius="8px" />
          </div>
          <UiSkeleton width="42%" height="16px" />
          <div class="home-skeleton__week">
            <UiSkeleton v-for="index in 6" :key="index" class="home-skeleton__week-game" height="auto"
              border-radius="8px" />
          </div>
          <UiSkeleton height="40px" border-radius="8px" />
          <UiSkeleton height="40px" border-radius="8px" />
        </section>
        <section class="home-skeleton__card">
          <div class="home-skeleton__header">
            <UiSkeleton width="148px" height="20px" />
          </div>
          <UiSkeleton v-if="arcadeGame?.state === 'active'" height="40px" border-radius="8px" />
          <UiSkeleton height="40px" border-radius="8px" />
          <UiSkeleton height="40px" border-radius="8px" />
          <UiSkeleton height="40px" border-radius="8px" />
        </section>
        <UiSkeleton class="home-skeleton__footer" width="76px" height="22px" />
      </div>
    </template>
    <GameHome
      :authenticated="isAuthenticated"
      :login-locked="authFailed"
      :arcade-locked="arcadeFailed"
      :today-failed="todayFailed"
      :status-message="statusMessage"
      :has-game="arcadeGame?.state === 'active'"
      :daily-games="dailyGames"
      :daily-streak="dailyStreak"
      :daily-history-failed="dailyHistoryFailed"
      :daily-loading="loading"
      :status-retry-available="!!statusMessage"
      :continue-game-to="arcadeGame ? { name: 'arcade-game', params: { id: arcadeGame.id } } : undefined"
      @retry-status="retry"
    />
  </UiSkeletonHandoff>
</template>

<style scoped>
.home-skeleton {
  flex: 1;
  min-height: 0;
  width: min(100%, 438px);
  margin-inline: auto;
  display: flex;
  flex-direction: column;
  justify-content: center;
  gap: 12px;
  overflow-y: auto;
  padding: 12px 0;
}

.home-skeleton__card {
  border: 1px solid var(--color-gray-200);
  border-radius: 16px;
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 12px;
  background: rgba(255, 255, 255, 0.92);
}

.home-skeleton__card--daily {
  padding: 18px;
}

.home-skeleton__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}

.home-skeleton__today {
  border: 1px solid var(--color-gray-200);
  border-radius: 8px;
  padding: 10px;
  display: grid;
  grid-template-columns: 40px minmax(0, 1fr) 40px;
  align-items: center;
  gap: 10px;
}

.home-skeleton__week {
  display: grid;
  grid-template-columns: repeat(6, minmax(0, 1fr));
  gap: 6px;
}

.home-skeleton__week-game {
  aspect-ratio: 1;
}

.home-skeleton__footer {
  margin-inline: auto;
}

@media (max-width: 480px) {
  .home-skeleton {
    padding: 2px 0 8px;
  }

  .home-skeleton__card {
    border-radius: 12px;
    padding: 12px;
  }

  .home-skeleton__card--daily {
    padding: 14px;
  }

  .home-skeleton__week {
    grid-template-columns: repeat(5, minmax(0, 1fr));
    gap: 5px;
  }

  .home-skeleton__week-game:first-child {
    display: none;
  }
}
</style>
