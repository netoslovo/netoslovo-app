<script setup lang="ts">
import { computed } from "vue";
import type { DisplayWord, GameState } from "../model/game";

const props = defineProps<{
  displayWord: DisplayWord | null;
  difficultyName?: string | null;
  modeLabel?: string | null;
  modeDetail?: string | null;
  modeVariant?: "daily" | "arcade" | null;
  caption?: string | null;
  resultLabel?: string | null;
  resultIcon?: string | null;
  gameState?: GameState | null;
  wordLoading?: boolean;
  celebrateGuessed?: boolean;
  animationsEnabled?: boolean;
}>();

const cells = computed(() => props.displayWord?.cells ?? []);

const isHiddenForToday = computed(
  () => props.displayWord?.hideReason === "hiddenForToday",
);

const isUnknownLength = computed(
  () => props.displayWord !== null && cells.value.length === 0,
);

const tiles = computed(() => {
  if (props.displayWord === null || isUnknownLength.value) {
    return [];
  }

  return cells.value.map((cell) => {
    if (!cell.revealed || !cell.value) {
      return null;
    }

    const value = cell.value.toUpperCase();
    return value === " " ? null : value;
  });
});

const tileCount = computed(() => Math.max(cells.value.length, 1));
const skeletonTileCount = 6;

const visibleTileCount = computed(
  () => tiles.value.filter((tile) => tile !== null).length,
);

const hasUnrevealedCells = computed(
  () => cells.value.some((cell) => !cell.revealed || cell.value === null),
);

const isMaskedWord = computed(
  () => tiles.value.length > 0 && visibleTileCount.value === 0,
);

const showSurrenderedWordTitle = computed(
  () =>
    props.gameState === "surrendered" &&
    props.displayWord !== null &&
    !isHiddenForToday.value &&
    (!isUnknownLength.value || props.wordLoading === true),
);

const showWordSkeleton = computed(
  () =>
    props.wordLoading === true &&
    props.displayWord !== null &&
    !isHiddenForToday.value &&
    (isUnknownLength.value || hasUnrevealedCells.value),
);

const caption = computed(() => {
  if (props.caption) {
    return props.caption;
  }

  if (props.displayWord === null) {
    return "";
  }

  if (isHiddenForToday.value) {
    return "Слово скрыто, пока активна эта игра дня. Оно откроется завтра.";
  }

  if (isUnknownLength.value) {
    return "Про загаданное слово ничего не известно";
  }

  if (isMaskedWord.value) {
    return `Загадано слово из ${tileCount.value} букв`;
  }

  return null;
});

const showFooter = computed(
  () => !(props.gameState === "cancelled" && isUnknownLength.value),
);

const headerLabel = computed(() => {
  if (props.modeLabel) {
    return props.modeLabel;
  }

  return props.difficultyName ? `Сложность: ${props.difficultyName}` : null;
});

const modeIcon = computed(() =>
  props.modeVariant === "daily" ? "pi pi-calendar" : "pi pi-box",
);
</script>

<template>
  <div v-if="displayWord === null" class="word-tiles-panel word-tiles-panel--empty" aria-hidden="true"></div>

  <section v-else class="word-tiles-panel" :class="[
    gameState ? `word-tiles-panel--status-${gameState}` : undefined,
    {
      'word-tiles-panel--celebrating': celebrateGuessed,
      'word-tiles-panel--animations-disabled': animationsEnabled === false,
    },
  ]" aria-live="polite">
    <svg v-if="celebrateGuessed" class="word-tiles-panel__celebration" aria-hidden="true">
      <rect class="word-tiles-panel__celebration-border" x="0.5" y="0.5" width="calc(100% - 1px)"
        height="calc(100% - 1px)" rx="7.5" pathLength="1" />
    </svg>

    <p v-if="headerLabel" class="word-tiles__difficulty" :class="`word-tiles__difficulty--${modeVariant ?? 'arcade'}`">
      <i class="word-tiles__difficulty-icon" :class="modeIcon" aria-hidden="true"></i>
      {{ headerLabel }}
    </p>
    <p v-if="modeDetail" class="word-tiles__mode-detail">
      {{ modeDetail }}
    </p>
    <div class="word-tiles__separator" aria-hidden="true"></div>
    <div v-if="resultLabel" class="word-tiles__result">
      <i v-if="resultIcon" class="word-tiles__result-icon" :class="resultIcon" aria-hidden="true"></i>
      <h2 class="word-tiles__result-title">
        {{ resultLabel }}
      </h2>
    </div>

    <p v-if="showSurrenderedWordTitle" class="word-tiles__surrender-title">
      Было загадано слово:
    </p>

    <div v-if="showWordSkeleton" class="word-tiles word-tiles--hangman word-tiles--loading"
      :style="{ '--word-length': skeletonTileCount.toString() }" aria-hidden="true">
      <span v-for="index in skeletonTileCount" :key="index" class="word-tile word-tile--loading">
        <span class="word-tile__glyph">
          <span class="word-tile__skeleton"></span>
        </span>

        <span class="word-tile__line" aria-hidden="true"></span>
      </span>
    </div>

    <div v-else-if="!isUnknownLength" class="word-tiles" :class="{
      'word-tiles--hangman': !isUnknownLength,
      'word-tiles--masked': isMaskedWord,
      'word-tiles--revealed': visibleTileCount > 0,
    }" :style="{ '--word-length': tileCount.toString() }">
      <span v-for="index in tileCount" :key="index" class="word-tile" :class="tiles[index - 1] ? 'word-tile--revealed' : 'word-tile--masked'
        ">
        <span class="word-tile__glyph">
          <span v-if="tiles[index - 1]" class="word-tile__letter" :key="`${tiles[index - 1]}-${index}`">
            {{ tiles[index - 1] }}
          </span>
        </span>

        <span class="word-tile__line" aria-hidden="true"></span>
      </span>
    </div>

    <div v-if="showFooter && !showWordSkeleton" class="word-tiles__footer">
      <div class="word-tiles__info">
        <template v-if="isHiddenForToday">
          <p class="word-tiles__helper">
            Слово будет показано завтра, когда завершится игра дня.
          </p>
        </template>

        <template v-else-if="isUnknownLength">
          <p class="word-tiles__helper">
            Нет дополнительной информации о слове.
          </p>

          <p class="word-tiles__helper word-tiles__helper--hint">
            Вы можете воспользоваться подсказками
            <i class="word-tiles__helper-icon pi pi-lightbulb" aria-hidden="true"></i>
          </p>
        </template>

        <p v-else class="word-tiles__caption">{{ caption }}</p>
      </div>
    </div>
  </section>
</template>

<style scoped>
.word-tiles-panel {
  --word-tile-max-width: 34px;
  --word-tile-min-height: 36px;
  --word-tile-gap: 5px;
  --word-tile-glyph-height: 22px;
  --word-tile-line-height: 2px;
  --word-tiles-title-font-size: 18px;
  --word-tiles-title-icon-size: 17px;
  --word-tiles-title-gap: 7px;
  --word-tiles-detail-font-size: 14px;
  --word-tiles-caption-font-size: 14px;
  --word-tiles-result-title-font-size: 18px;
  --word-tiles-helper-font-size: 13px;
  --word-tiles-helper-icon-size: 14px;
  --word-tiles-separator-margin-block-start: 4px;
  --word-tiles-separator-margin-block-end: 6px;
  --word-tiles-separator-margin-inline: 12px;
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

.word-tiles-panel--empty {
  display: none;
}

.word-tiles-panel--animations-disabled .word-tiles-panel__celebration-border,
.word-tiles-panel--animations-disabled .word-tiles__result-icon,
.word-tiles-panel--animations-disabled .word-tile__letter,
.word-tiles-panel--animations-disabled .word-tile__skeleton {
  animation: none !important;
}

.word-tiles-panel--status-active {
  border-color: var(--color-gray-300);
}

.word-tiles-panel--status-guessed {
  border-color: var(--color-primary-500);
}

.word-tiles-panel--status-guessed.word-tiles-panel--celebrating {
  border-color: var(--color-gray-300);
}

.word-tiles-panel__celebration {
  position: absolute;
  inset: 0;
  z-index: 1;
  width: 100%;
  height: 100%;
  overflow: visible;
  pointer-events: none;
}

.word-tiles-panel__celebration-border {
  fill: none;
  stroke: var(--color-primary-500);
  stroke-width: 1;
  stroke-linecap: round;
  stroke-dasharray: 1;
  stroke-dashoffset: 1;
  animation: word-tiles-panel-border-celebration 0.9s cubic-bezier(0.35, 0, 0.2, 1) forwards;
}

.word-tiles-panel--celebrating .word-tiles__result-icon {
  animation: word-tiles-result-icon-celebration 0.36s 0.58s cubic-bezier(0.2, 0.8, 0.3, 1.25) both;
}

.word-tiles-panel--status-surrendered {
  border-color: var(--color-red-600);
}

.word-tiles-panel--status-cancelled {
  border-color: var(--color-gray-300);
  background: var(--color-gray-50);
}

.word-tiles {
  width: 100%;
  min-width: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-wrap: nowrap;
  gap: 8px var(--word-tile-gap);
}

.word-tiles__surrender-title {
  margin: 0;
  color: var(--p-text-muted-color);
  font-size: var(--word-tiles-helper-font-size);
  line-height: 1.35;
  text-align: center;
}

.word-tiles--hangman {
  align-items: flex-end;
}

.word-tile {
  color: var(--color-gray-700);
  user-select: none;
}

.word-tiles--hangman .word-tile {
  flex: 1 1 0;
  width: auto;
  max-width: var(--word-tile-max-width);
  min-width: 0;
  min-height: var(--word-tile-min-height);
  display: inline-flex;
  flex-direction: column;
  align-items: center;
  justify-content: flex-end;
  gap: 4px;
}

.word-tile__glyph {
  min-height: var(--word-tile-glyph-height);
  width: 100%;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.word-tile__letter {
  color: var(--color-gray-800);
  font-size: clamp(14px, calc(68vw / var(--word-length, 1)), var(--resolved-word-font-size));
  font-weight: 500;
  line-height: 1;
  animation: revealed-letter 0.42s cubic-bezier(0.2, 0.8, 0.3, 1.2) both;
}

.word-tile__line {
  width: 100%;
  height: var(--word-tile-line-height);
  border-radius: 999px;
  background: var(--color-gray-800);
}

.word-tile--masked {
  color: var(--p-text-muted-color);
}

.word-tile--revealed {
  color: var(--color-gray-800);
}

.word-tiles--loading .word-tile__line {
  background: var(--color-gray-300);
}

.word-tile__skeleton {
  width: 72%;
  height: calc(var(--word-tile-glyph-height) * 0.68);
  border-radius: 6px;
  background: linear-gradient(
    90deg,
    var(--color-gray-100) 0%,
    var(--color-gray-200) 42%,
    var(--color-gray-100) 82%
  );
  background-size: 220% 100%;
  animation: word-tile-skeleton 1.05s ease-in-out infinite;
}

.word-tiles__footer {
  display: flex;
  justify-content: center;
}

.word-tiles__info {
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 2px;
  align-items: center;
}

.word-tiles__difficulty,
.word-tiles__mode-detail,
.word-tiles__caption {
  margin: 0;
  color: var(--color-gray-600);
  font-size: var(--word-tiles-caption-font-size);
  line-height: 1.4;
  text-align: center;
}

.word-tiles__difficulty {
  width: fit-content;
  max-width: 100%;
  margin-inline: auto;
  padding: 0;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: var(--word-tiles-title-gap);
  color: var(--color-gray-900);
  font-size: var(--word-tiles-title-font-size);
  font-weight: 500;
}

.word-tiles__difficulty-icon {
  color: var(--p-surface-500);
  font-size: var(--word-tiles-title-icon-size);
  line-height: 1;
}

.word-tiles__mode-detail {
  margin-top: 0;
  color: var(--color-gray-700);
  font-size: var(--word-tiles-detail-font-size);
  font-weight: 400;
}

.word-tiles__result {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--word-tiles-title-gap);
  color: var(--color-gray-900);
}

.word-tiles__result-icon {
  color: var(--p-surface-500);
  font-size: var(--word-tiles-title-icon-size);
  line-height: 1;
}

.word-tiles__result-title {
  margin: 0;
  font-size: var(--word-tiles-result-title-font-size);
  font-weight: 500;
  line-height: 1.15;
  text-align: center;
}

.word-tiles__helper {
  margin: 0;
  color: var(--p-text-muted-color);
  font-size: var(--word-tiles-helper-font-size);
  line-height: 1.35;
  text-align: center;
}

.word-tiles__helper--hint {
  display: flex;
  align-items: center;
  justify-content: center;
  flex-wrap: wrap;
  gap: 6px;
}

.word-tiles__helper-icon {
  position: relative;
  top: -1px;
  color: var(--color-primary-600);
  font-size: var(--word-tiles-helper-icon-size);
  line-height: 1;
}

.word-tiles__separator {
  width: calc(100% - var(--word-tiles-separator-margin-inline) * 2);
  height: 1px;
  margin: var(--word-tiles-separator-margin-block-start) var(--word-tiles-separator-margin-inline) var(--word-tiles-separator-margin-block-end);
  background: var(--color-gray-200);
}

@media (min-width: 768px) {
  .word-tiles-panel {
    --word-tile-max-width: 38px;
    --word-tile-min-height: 46px;
    --word-tile-gap: 6px;
    --word-tile-glyph-height: 30px;
    --word-tiles-title-font-size: 20px;
    --word-tiles-title-icon-size: 18px;
    --word-tiles-title-gap: 8px;
    --word-tiles-detail-font-size: 15px;
    --word-tiles-caption-font-size: 15px;
    --word-tiles-result-title-font-size: 20px;
    --word-tiles-helper-font-size: 14px;
    --word-tiles-helper-icon-size: 15px;
    --word-tiles-separator-margin-block-start: 5px;
    --word-tiles-separator-margin-block-end: 7px;
    --word-tiles-separator-margin-inline: 14px;
    gap: 8px;
    padding: 12px;
  }

  .word-tiles--hangman .word-tile {
    gap: 6px;
  }
}

@media (min-width: 1024px) {
  .word-tiles-panel {
    --word-tile-max-width: 42px;
    --word-tile-min-height: 50px;
    --word-tile-gap: 6px;
    --word-tile-glyph-height: 34px;
    --word-tiles-title-font-size: 21px;
    --word-tiles-title-icon-size: 19px;
    --word-tiles-detail-font-size: 16px;
    --word-tiles-caption-font-size: 15px;
    --word-tiles-result-title-font-size: 21px;
  }
}

@media (prefers-reduced-motion: reduce) {
  .word-tile__letter,
  .word-tile__skeleton {
    animation: none;
  }

  .word-tiles-panel--status-guessed.word-tiles-panel--celebrating {
    border-color: var(--color-primary-500);
  }

  .word-tiles-panel__celebration {
    display: none;
  }

  .word-tiles-panel--celebrating .word-tiles__result-icon {
    animation: none;
  }
}

@keyframes word-tiles-panel-border-celebration {
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

@keyframes word-tiles-result-icon-celebration {
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

@keyframes revealed-letter {
  0% {
    opacity: 0;
    transform: translateY(10px) scale(0.84);
  }

  70% {
    opacity: 1;
    transform: translateY(-3px) scale(1.08);
  }

  100% {
    opacity: 1;
    transform: translateY(0) scale(1);
  }
}

@keyframes word-tile-skeleton {
  from {
    background-position: 120% 0;
  }

  to {
    background-position: -120% 0;
  }
}
</style>
