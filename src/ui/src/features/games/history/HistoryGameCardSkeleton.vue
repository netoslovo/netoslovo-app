<script setup lang="ts">
import UiSkeleton from "../../../shared/ui/UiSkeleton.vue";

withDefaults(
  defineProps<{
    count?: number;
    showGroupTitle?: boolean;
  }>(),
  {
    count: 4,
    showGroupTitle: true,
  },
);
</script>

<template>
  <div class="history-game-card-skeleton" aria-hidden="true">
    <div v-if="showGroupTitle" class="history-game-card-skeleton__group-title">
      <UiSkeleton width="116px" height="14px" />
    </div>
    <article v-for="index in count" :key="index" class="history-game-card-skeleton__card">
      <div class="history-game-card-skeleton__desktop">
        <UiSkeleton width="34px" height="34px" shape="circle" />
        <div class="history-game-card-skeleton__text">
          <UiSkeleton width="72%" height="16px" />
          <UiSkeleton width="48%" height="13px" />
        </div>
        <UiSkeleton width="96px" height="40px" border-radius="8px" />
      </div>

      <div class="history-game-card-skeleton__mobile">
        <UiSkeleton width="40px" height="40px" border-radius="8px" />
        <div class="history-game-card-skeleton__text">
          <UiSkeleton width="72%" height="16px" />
          <UiSkeleton width="48%" height="13px" />
        </div>
        <UiSkeleton width="40px" height="40px" border-radius="8px" />
        <div class="history-game-card-skeleton__word">
          <UiSkeleton v-for="tileIndex in 6" :key="tileIndex" width="19px" height="22px" border-radius="3px" />
        </div>
      </div>
    </article>
  </div>
</template>

<style scoped>
.history-game-card-skeleton {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.history-game-card-skeleton__card {
  border: 1px solid var(--color-gray-200);
  border-radius: 8px;
  padding: 10px;
  background: rgba(255, 255, 255, 0.92);
}

.history-game-card-skeleton__desktop {
  display: flex;
  align-items: center;
  gap: 10px;
}

.history-game-card-skeleton__text {
  min-width: 0;
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 7px;
}

.history-game-card-skeleton__mobile {
  display: none;
}

@media (max-width: 640px) {
  .history-game-card-skeleton__group-title,
  .history-game-card-skeleton__card {
    width: min(100%, 360px);
    margin-inline: auto;
  }

  .history-game-card-skeleton__desktop {
    display: none;
  }

  .history-game-card-skeleton__mobile {
    display: grid;
    grid-template-columns: 40px minmax(0, 1fr) 40px;
    grid-template-areas:
      "status text action"
      "word word word";
    align-items: center;
    gap: 8px 10px;
  }

  .history-game-card-skeleton__mobile > :first-child {
    grid-area: status;
  }

  .history-game-card-skeleton__mobile .history-game-card-skeleton__text {
    grid-area: text;
  }

  .history-game-card-skeleton__mobile > :nth-child(3) {
    grid-area: action;
  }

  .history-game-card-skeleton__word {
    grid-area: word;
    min-width: 0;
    display: flex;
    justify-content: center;
    gap: 4px;
  }
}

@media (min-width: 768px) {
  .history-game-card-skeleton {
    gap: 14px;
  }
}
</style>
