<script setup lang="ts">
import UiSkeleton from "../../../shared/ui/UiSkeleton.vue";
</script>

<template>
  <div class="game-board-skeleton" aria-hidden="true">
    <section class="game-board-skeleton__word-card">
      <UiSkeleton class="game-board-skeleton__mode" width="46%" height="20px" />
      <UiSkeleton class="game-board-skeleton__mode-detail" width="32%" height="15px" />
      <div class="game-board-skeleton__separator"></div>
      <UiSkeleton class="game-board-skeleton__word" width="62%" height="42px" border-radius="8px" />
    </section>

    <section class="game-board-skeleton__state-card">
      <div class="game-board-skeleton__score">
        <div class="game-board-skeleton__score-main">
          <UiSkeleton width="54px" height="18px" />
          <UiSkeleton width="28px" height="21px" />
        </div>
        <div class="game-board-skeleton__score-details">
          <UiSkeleton width="74px" height="15px" />
          <UiSkeleton width="82px" height="15px" />
        </div>
      </div>

      <div class="game-board-skeleton__state-details">
        <UiSkeleton width="96px" height="13px" />
        <UiSkeleton height="40px" border-radius="8px" />
        <UiSkeleton height="40px" border-radius="8px" />
      </div>
    </section>

    <section class="game-board-skeleton__list">
      <UiSkeleton v-for="index in 5" :key="index" height="40px" border-radius="8px" />
    </section>
  </div>
</template>

<style scoped>
.game-board-skeleton {
  width: 100%;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.game-board-skeleton__word-card,
.game-board-skeleton__state-card,
.game-board-skeleton__list {
  border: 1px solid var(--color-gray-200);
  border-radius: 8px;
  display: flex;
  flex-direction: column;
  background: white;
}

.game-board-skeleton__word-card {
  gap: 6px;
  padding: 9px 10px;
  border-color: var(--color-gray-300);
}

.game-board-skeleton__mode,
.game-board-skeleton__mode-detail {
  margin-inline: auto;
}

.game-board-skeleton__separator {
  width: calc(100% - 24px);
  height: 1px;
  margin: 4px 12px 6px;
  background: var(--color-gray-200);
}

.game-board-skeleton__word {
  margin-inline: auto;
}

.game-board-skeleton__state-card {
  gap: 8px;
  padding: 8px;
}

.game-board-skeleton__score {
  min-height: 42px;
  display: flex;
  align-items: center;
  gap: 12px;
}

.game-board-skeleton__score-main {
  flex: 0 0 auto;
  display: flex;
  align-items: center;
  gap: 7px;
}

.game-board-skeleton__score-details {
  min-width: 0;
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 12px;
}

.game-board-skeleton__state-details {
  min-width: 0;
  padding-top: 8px;
  border-top: 1px solid var(--color-gray-100);
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.game-board-skeleton__list {
  gap: 8px;
  padding-top: 2px;
  border: 0;
  background: transparent;
}

@media (min-width: 768px) {
  .game-board-skeleton {
    gap: 12px;
  }

  .game-board-skeleton__word-card {
    gap: 8px;
    padding: 12px;
  }

  .game-board-skeleton__separator {
    width: calc(100% - 28px);
    margin: 5px 14px 7px;
  }

  .game-board-skeleton__state-card {
    gap: 10px;
    padding: 10px;
  }

  .game-board-skeleton__state-details {
    padding-top: 10px;
    gap: 10px;
  }
}
</style>
