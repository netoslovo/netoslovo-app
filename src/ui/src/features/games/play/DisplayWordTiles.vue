<script setup lang="ts">
import { computed } from "vue";
import type { DisplayWord, GameState } from "../model/game";

const props = defineProps<{
  displayWord: DisplayWord | null;
  caption?: string | null;
  gameState?: GameState | null;
  hiddenForToday?: boolean;
  showSurrenderedTitle?: boolean;
  wordLoading?: boolean;
  animationsEnabled?: boolean;
}>();

const cells = computed(() => props.displayWord?.cells ?? []);
const isUnknownLength = computed(
  () => props.displayWord !== null && cells.value.length === 0,
);
const tiles = computed(() => {
  if (props.displayWord === null || isUnknownLength.value) return [];

  return cells.value.map((cell) => {
    if (!cell.revealed || !cell.value) return null;
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
    props.showSurrenderedTitle !== false &&
    props.displayWord !== null &&
    props.hiddenForToday !== true &&
    (!isUnknownLength.value || props.wordLoading === true),
);
const showWordSkeleton = computed(
  () =>
    props.wordLoading === true &&
    props.displayWord !== null &&
    props.hiddenForToday !== true &&
    (isUnknownLength.value || hasUnrevealedCells.value),
);
const caption = computed(() => {
  if (props.caption) return props.caption;
  if (props.displayWord === null) return "";
  if (props.hiddenForToday === true) {
    return "Слово скрыто, пока активна эта игра дня. Оно откроется завтра.";
  }
  if (isUnknownLength.value) return "Про загаданное слово ничего не известно";
  if (isMaskedWord.value) return `Загадано слово из ${tileCount.value} букв`;
  return null;
});
const showFooter = computed(
  () => !(props.gameState === "cancelled" && isUnknownLength.value),
);
</script>

<template>
  <div class="display-word-tiles"
    :class="{ 'display-word-tiles--animations-disabled': animationsEnabled === false }">
    <p v-if="showSurrenderedWordTitle" class="word-tiles__surrender-title">
      Было загадано слово:
    </p>

    <p v-if="displayWord === null && caption" class="word-tiles__caption">
      {{ caption }}
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

    <div v-else-if="displayWord !== null && !isUnknownLength" class="word-tiles" :class="{
      'word-tiles--hangman': !isUnknownLength,
      'word-tiles--masked': isMaskedWord,
      'word-tiles--revealed': visibleTileCount > 0,
    }" :style="{ '--word-length': tileCount.toString() }">
      <span v-for="index in tileCount" :key="index" class="word-tile"
        :class="tiles[index - 1] ? 'word-tile--revealed' : 'word-tile--masked'">
        <span class="word-tile__glyph">
          <span v-if="tiles[index - 1]" :key="`${tiles[index - 1]}-${index}`" class="word-tile__letter">
            {{ tiles[index - 1] }}
          </span>
        </span>
        <span class="word-tile__line" aria-hidden="true"></span>
      </span>
    </div>

    <div v-if="displayWord !== null && showFooter && !showWordSkeleton" class="word-tiles__footer">
      <div class="word-tiles__info">
        <template v-if="hiddenForToday">
          <p class="word-tiles__helper">
            Слово будет показано завтра, когда завершится игра дня.
          </p>
        </template>

        <template v-else-if="isUnknownLength">
          <p class="word-tiles__helper">Нет дополнительной информации о слове.</p>
          <p v-if="gameState === 'active'" class="word-tiles__helper word-tiles__helper--hint">
            Вы можете воспользоваться подсказками
            <i class="word-tiles__helper-icon pi pi-lightbulb" aria-hidden="true"></i>
          </p>
        </template>

        <p v-else class="word-tiles__caption">{{ caption }}</p>
      </div>
    </div>
  </div>
</template>

<style scoped>
.display-word-tiles {
  --word-tile-max-width: 34px;
  --word-tile-min-height: 36px;
  --word-tile-gap: 5px;
  --word-tile-glyph-height: 22px;
  --word-tile-line-height: 2px;
  --word-tiles-caption-font-size: 14px;
  --word-tiles-helper-font-size: 13px;
  --word-tiles-helper-icon-size: 14px;
  display: contents;
}

.display-word-tiles--animations-disabled .word-tile__letter,
.display-word-tiles--animations-disabled .word-tile__skeleton {
  animation: none !important;
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

.word-tiles__caption {
  margin: 0;
  color: var(--color-gray-600);
  font-size: var(--word-tiles-caption-font-size);
  line-height: 1.4;
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
  color: var(--color-gray-600);
  font-size: var(--word-tiles-helper-icon-size);
  line-height: 1;
}

@media (min-width: 768px) {
  .display-word-tiles {
    --word-tile-max-width: 38px;
    --word-tile-min-height: 46px;
    --word-tile-gap: 6px;
    --word-tile-glyph-height: 30px;
    --word-tiles-caption-font-size: 15px;
    --word-tiles-helper-font-size: 14px;
    --word-tiles-helper-icon-size: 15px;
  }

  .word-tiles--hangman .word-tile {
    gap: 6px;
  }
}

@media (min-width: 1024px) {
  .display-word-tiles {
    --word-tile-max-width: 42px;
    --word-tile-min-height: 50px;
    --word-tile-gap: 6px;
    --word-tile-glyph-height: 34px;
    --word-tiles-caption-font-size: 15px;
  }
}

@media (prefers-reduced-motion: reduce) {
  .word-tile__letter,
  .word-tile__skeleton {
    animation: none;
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
