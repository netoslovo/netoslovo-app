<script setup lang="ts">
import { computed } from "vue";
import type { GameState } from "../model/game";

const props = defineProps<{
  difficultyName?: string | null;
  modeLabel?: string | null;
  modeDetail?: string | null;
  modeVariant?: "daily" | "arcade" | null;
  resultLabel?: string | null;
  resultIcon?: string | null;
  gameState?: GameState | null;
  celebrateGuessed?: boolean;
  animationsEnabled?: boolean;
}>();

const headerLabel = computed(() => {
  if (props.modeLabel) return props.modeLabel;
  return props.difficultyName ? `Сложность: ${props.difficultyName}` : null;
});
const modeIcon = computed(() =>
  props.modeVariant === "daily" ? "pi pi-calendar" : "pi pi-box",
);
</script>

<template>
  <section class="game-word-card" :class="[
    gameState ? `game-word-card--status-${gameState}` : undefined,
    {
      'game-word-card--celebrating': celebrateGuessed,
      'game-word-card--animations-disabled': animationsEnabled === false,
      'game-word-card--with-header-action': Boolean($slots['header-action']),
    },
  ]" aria-live="polite">
    <svg v-if="celebrateGuessed" class="game-word-card__celebration" aria-hidden="true">
      <rect class="game-word-card__celebration-border" x="0.5" y="0.5" width="calc(100% - 1px)"
        height="calc(100% - 1px)" rx="7.5" pathLength="1" />
    </svg>

    <div v-if="$slots['header-action']" class="game-word-card__header-action">
      <slot name="header-action" />
    </div>

    <p v-if="headerLabel" class="game-word-card__mode"
      :class="`game-word-card__mode--${modeVariant ?? 'arcade'}`">
      <i class="game-word-card__mode-icon" :class="modeIcon" aria-hidden="true"></i>
      {{ headerLabel }}
    </p>
    <p v-if="modeDetail" class="game-word-card__mode-detail">{{ modeDetail }}</p>
    <div class="game-word-card__separator" aria-hidden="true"></div>

    <div v-if="resultLabel" class="game-word-card__result">
      <i v-if="resultIcon" class="game-word-card__result-icon" :class="resultIcon" aria-hidden="true"></i>
      <h2 class="game-word-card__result-title">{{ resultLabel }}</h2>
    </div>

    <slot />
  </section>
</template>

<style scoped>
.game-word-card {
  --game-word-card-title-font-size: 18px;
  --game-word-card-title-icon-size: 17px;
  --game-word-card-title-gap: 7px;
  --game-word-card-detail-font-size: 14px;
  --game-word-card-result-title-font-size: 18px;
  --game-word-card-separator-margin-block-start: 4px;
  --game-word-card-separator-margin-block-end: 6px;
  --game-word-card-separator-margin-inline: 12px;
  position: relative;
  width: 100%;
  display: flex;
  flex-direction: column;
  gap: 6px;
  padding: 9px 10px;
  border: 1px solid var(--color-gray-300);
  border-radius: 8px;
  background: white;
}

.game-word-card--animations-disabled .game-word-card__celebration-border,
.game-word-card--animations-disabled .game-word-card__result-icon {
  animation: none !important;
}

.game-word-card--status-active {
  border-color: var(--color-gray-300);
}

.game-word-card--status-guessed {
  border-color: var(--color-primary-500);
}

.game-word-card--status-guessed.game-word-card--celebrating {
  border-color: var(--color-gray-300);
}

.game-word-card--status-surrendered {
  border-color: var(--color-red-600);
}

.game-word-card--status-cancelled {
  border-color: var(--color-gray-300);
  background: var(--color-gray-50);
}

.game-word-card__celebration {
  position: absolute;
  inset: 0;
  z-index: 1;
  width: 100%;
  height: 100%;
  overflow: visible;
  pointer-events: none;
}

.game-word-card__celebration-border {
  fill: none;
  stroke: var(--color-primary-500);
  stroke-width: 1;
  stroke-linecap: round;
  stroke-dasharray: 1;
  stroke-dashoffset: 1;
  animation: game-word-card-border-celebration 0.9s cubic-bezier(0.35, 0, 0.2, 1) forwards;
}

.game-word-card--celebrating .game-word-card__result-icon {
  animation: game-word-card-result-icon-celebration 0.36s 0.58s cubic-bezier(0.2, 0.8, 0.3, 1.25) both;
}

.game-word-card__header-action {
  position: absolute;
  top: 7px;
  right: 8px;
  z-index: 2;
}

.game-word-card--with-header-action .game-word-card__mode {
  padding-inline: 38px;
}

.game-word-card__mode,
.game-word-card__mode-detail {
  margin: 0;
  text-align: center;
}

.game-word-card__mode {
  width: fit-content;
  max-width: 100%;
  margin-inline: auto;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: var(--game-word-card-title-gap);
  color: var(--color-gray-900);
  font-size: var(--game-word-card-title-font-size);
  font-weight: 500;
  line-height: 1.15;
}

.game-word-card__mode-icon {
  color: var(--p-surface-500);
  font-size: var(--game-word-card-title-icon-size);
  line-height: 1;
}

.game-word-card__mode-detail {
  color: var(--color-gray-700);
  font-size: var(--game-word-card-detail-font-size);
  font-weight: 400;
  line-height: 1.4;
}

.game-word-card__separator {
  width: calc(100% - var(--game-word-card-separator-margin-inline) * 2);
  height: 1px;
  margin: var(--game-word-card-separator-margin-block-start) var(--game-word-card-separator-margin-inline) var(--game-word-card-separator-margin-block-end);
  background: var(--color-gray-200);
}

.game-word-card__result {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--game-word-card-title-gap);
  color: var(--color-gray-900);
}

.game-word-card__result-icon {
  color: var(--p-surface-500);
  font-size: var(--game-word-card-title-icon-size);
  line-height: 1;
}

.game-word-card__result-title {
  margin: 0;
  font-size: var(--game-word-card-result-title-font-size);
  font-weight: 500;
  line-height: 1.15;
  text-align: center;
}

@media (min-width: 768px) {
  .game-word-card {
    --game-word-card-title-font-size: 20px;
    --game-word-card-title-icon-size: 18px;
    --game-word-card-title-gap: 8px;
    --game-word-card-detail-font-size: 15px;
    --game-word-card-result-title-font-size: 20px;
    --game-word-card-separator-margin-block-start: 5px;
    --game-word-card-separator-margin-block-end: 7px;
    --game-word-card-separator-margin-inline: 14px;
    gap: 8px;
    padding: 12px;
  }

  .game-word-card__header-action {
    top: 9px;
    right: 10px;
  }
}

@media (min-width: 1024px) {
  .game-word-card {
    --game-word-card-title-font-size: 21px;
    --game-word-card-title-icon-size: 19px;
    --game-word-card-detail-font-size: 16px;
    --game-word-card-result-title-font-size: 21px;
  }
}

@media (prefers-reduced-motion: reduce) {
  .game-word-card--status-guessed.game-word-card--celebrating {
    border-color: var(--color-primary-500);
  }

  .game-word-card__celebration {
    display: none;
  }

  .game-word-card--celebrating .game-word-card__result-icon {
    animation: none;
  }
}

@keyframes game-word-card-border-celebration {
  0% {
    stroke-dashoffset: 1;
    filter: drop-shadow(0 0 0 rgba(34, 197, 94, 0));
  }

  68% {
    stroke-dashoffset: 0;
    filter: drop-shadow(0 0 0 rgba(34, 197, 94, 0));
  }

  84% {
    stroke-dashoffset: 0;
    filter: drop-shadow(0 0 5px rgba(34, 197, 94, 0.42));
  }

  100% {
    stroke-dashoffset: 0;
    filter: drop-shadow(0 0 0 rgba(34, 197, 94, 0));
  }
}

@keyframes game-word-card-result-icon-celebration {
  0% {
    opacity: 0;
    transform: scale(0.72);
  }

  70% {
    opacity: 1;
    transform: scale(1.12);
  }

  100% {
    opacity: 1;
    transform: scale(1);
  }
}
</style>
