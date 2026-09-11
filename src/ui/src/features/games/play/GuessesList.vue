<script setup lang="ts">
import { watch } from "vue";
import type { Guess } from "../model/game";
import type { GuessPresentationEvent } from "./useGameSession";
import GuessBar from "./GuessBar.vue";

const props = defineProps<{
  gameId: string;
  currentGuess: Guess | null;
  guesses: Guess[];
  animatedHintWord: string | null;
  presentationEvent: GuessPresentationEvent | null;
}>();

function getGuessKey(guess: Guess) {
  return `${props.gameId}:${guess.word}:${feedbackEventIds.get(guess.word) ?? 0}`;
}

const revealRestoredGuesses = props.guesses.length > 0;
const feedbackEventIds = new Map<string, number>();

watch(
  () => props.presentationEvent,
  (event) => {
    if (event) feedbackEventIds.set(event.word, event.id);
  },
  { immediate: true },
);

function getFeedback(guess: Guess) {
  if (props.presentationEvent) {
    return props.presentationEvent.word === guess.word
      ? props.presentationEvent.kind
      : null;
  }

  return null;
}
</script>

<template>
  <div class="guesses-list">
    <div v-if="guesses.length > 0" class="guesses-list__section"
      :class="{ 'guesses-list__section--restored': revealRestoredGuesses }">
      <p class="guesses-list__label">
        Попыток: {{ guesses.length }}
      </p>
      <div class="guesses-list__items">
        <GuessBar v-for="guess in guesses" :key="getGuessKey(guess)" :word="guess.word" :value="guess.distance"
          :fill-percentage="guess.fillPercentage" :hint="guess.source === 'hint'"
          :animate-hint-reveal="guess.word === animatedHintWord" :feedback="getFeedback(guess)"
          :highlighted="guess.word === currentGuess?.word" />
      </div>
    </div>
    <p v-else class="guesses-list__empty">
      Пока нет попыток
    </p>
  </div>
</template>

<style scoped>
.guesses-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.guesses-list__section {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.guesses-list__section--restored {
  transform-origin: top center;
  animation: guesses-list-settle 0.28s cubic-bezier(0.22, 0.8, 0.3, 1) both;
}

.guesses-list__items {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.guesses-list__label {
  margin: 0;
  color: var(--p-text-muted-color);
  font-size: 13px;
  line-height: 1.3;
  text-align: left;
}

.guesses-list__empty {
  margin: 0;
  color: var(--p-text-muted-color);
  font-size: 13px;
  line-height: 1.3;
  text-align: center;
}

@media (min-width: 768px) {
  .guesses-list__items {
    gap: 3px;
  }
}

@media (prefers-reduced-motion: reduce) {
  .guesses-list__section--restored {
    animation: none;
  }
}

@keyframes guesses-list-settle {
  from {
    transform: translateY(-6px) scale(0.99);
  }

  to {
    transform: translateY(0) scale(1);
  }
}
</style>
