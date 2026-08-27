<script setup lang="ts">
import { useRouter } from "vue-router";
import ArcadeCreateScreen from "../features/games/create/ArcadeCreateScreen.vue";
import { useArcadeCreation } from "../features/games/create/useArcadeCreation";
import { useHandoffDelayedLoadingState } from "../shared/composables/useSkeletonHandoff";
import UiSkeleton from "../shared/ui/UiSkeleton.vue";
import UiSkeletonHandoff from "../shared/ui/UiSkeletonHandoff.vue";

const router = useRouter();
const creation = useArcadeCreation((game) =>
  router.push({ name: "arcade-game", params: { id: game.id } }),
);
const loadingState = useHandoffDelayedLoadingState(creation.loading);
</script>

<template>
  <UiSkeletonHandoff :skeleton-visible="loadingState.visible" :content-visible="loadingState.ready">
    <template #skeleton>
      <div class="arcade-create-skeleton" aria-hidden="true">
        <section class="arcade-create-skeleton__panel">
          <UiSkeleton width="42%" height="22px" />
          <div class="arcade-create-skeleton__field">
            <UiSkeleton width="28%" height="15px" />
            <UiSkeleton height="42px" border-radius="8px" />
          </div>
          <UiSkeleton height="48px" border-radius="8px" />
        </section>
      </div>
    </template>
    <ArcadeCreateScreen
      v-model="creation.selectedDifficultyCode.value"
      :difficulties="creation.difficulties.value"
      :creating="creation.creating.value"
      :can-start-arcade-game="creation.canStart.value"
      :gameplay-locked="creation.gameplayLocked.value"
      @create-game="creation.create"
    />
  </UiSkeletonHandoff>
</template>

<style scoped>
.arcade-create-skeleton {
  flex: 1;
  min-height: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  overflow-y: auto;
  padding: 12px 0;
}

.arcade-create-skeleton__panel {
  width: min(100%, 380px);
  border: 1px solid var(--color-gray-200);
  border-radius: 16px;
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 12px;
  background: rgba(255, 255, 255, 0.94);
}

.arcade-create-skeleton__field {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

@media (min-width: 768px) {
  .arcade-create-skeleton__panel {
    padding: 18px;
  }
}

@media (max-height: 760px) {
  .arcade-create-skeleton__panel {
    gap: 10px;
    padding: 12px;
  }
}

@media (max-width: 480px) {
  .arcade-create-skeleton {
    padding: 2px 0 8px;
  }

  .arcade-create-skeleton__panel {
    gap: 10px;
    padding: 12px;
    border-radius: 12px;
  }
}
</style>
