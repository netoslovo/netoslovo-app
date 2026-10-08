<script setup lang="ts">
import { computed, ref } from "vue";
import type { SharedGameSpoilersHideReason, SharedGuess } from "../model/game";
import SharedGuessBar from "./SharedGuessBar.vue";

const props = defineProps<{
  guesses: SharedGuess[];
  hideReason: SharedGameSpoilersHideReason | null;
}>();

type SortMode = "distance" | "order";

const sortMode = ref<SortMode>("distance");
const sortLabel = computed(() => sortMode.value === "distance" ? "По близости" : "По порядку");
const sortToggleLabel = computed(() =>
  `Сортировка: ${sortLabel.value.toLocaleLowerCase("ru-RU")}. Переключить на ${sortMode.value === "distance" ? "порядок попыток" : "близость"}`,
);

const sortedGuesses = computed(() => [...props.guesses].sort((left, right) =>
  sortMode.value === "distance"
    ? left.distance - right.distance
    : right.order - left.order,
));

const hiddenWordsExplanation = computed(() =>
  props.hideReason === "hiddenForToday"
    ? "Слова в попытках станут доступны позже, когда завершится текущая игра дня."
    : props.hideReason === "viewerGameNotFinished"
      ? "Слово попытки откроется после завершения вашей игры."
      : "Названия попыток скрыты.",
);

</script>

<template>
  <section class="shared-guesses" aria-label="Попытки игрока">
    <div v-if="guesses.length" class="shared-guesses__heading">
      <p class="shared-guesses__label">Попыток: {{ guesses.length }}</p>
      <button type="button" class="shared-guesses__sort" :aria-label="sortToggleLabel"
        :title="sortToggleLabel" @click="sortMode = sortMode === 'distance' ? 'order' : 'distance'">
        <i class="pi pi-sort-alt" aria-hidden="true"></i>
        {{ sortLabel }}
      </button>
    </div>

    <div v-if="guesses.length" class="shared-guesses__items">
      <SharedGuessBar v-for="guess in sortedGuesses" :key="guess.order" :guess="guess"
        :hidden-word-explanation="hiddenWordsExplanation" />
    </div>
    <p v-else class="shared-guesses__empty">В этой игре не было попыток</p>
  </section>
</template>

<style scoped>
.shared-guesses {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.shared-guesses__heading {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
}

.shared-guesses__label,
.shared-guesses__empty {
  margin: 0;
  color: var(--p-text-muted-color);
  font-size: 13px;
  line-height: 1.3;
}

.shared-guesses__empty {
  text-align: center;
}

.shared-guesses__sort {
  appearance: none;
  -webkit-tap-highlight-color: transparent;
  flex-shrink: 0;
  display: inline-flex;
  align-items: center;
  gap: 5px;
  min-height: 28px;
  border: 0;
  border-radius: 6px;
  padding: 3px 5px;
  background: transparent;
  color: var(--color-gray-600);
  cursor: pointer;
  font: inherit;
  font-size: 12px;
  line-height: 1.2;
}

.shared-guesses__sort .pi {
  font-size: 12px;
}

.shared-guesses__sort:focus-visible {
  outline: none;
  box-shadow: var(--focus-ring-primary);
}

.shared-guesses__items {
  display: flex;
  flex-direction: column;
  gap: 2px;
  padding-block: 4px;
}

@media (min-width: 768px) {
  .shared-guesses__items {
    gap: 3px;
  }
}

@media (hover: hover) and (pointer: fine) {
  .shared-guesses__sort:hover {
    background: var(--color-gray-100);
    color: var(--color-gray-800);
  }
}
</style>
