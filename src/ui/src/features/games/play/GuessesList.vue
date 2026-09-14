<script setup lang="ts">
import { ref, watch, type ComponentPublicInstance } from "vue";
import type { Guess } from "../model/game";
import type { GuessPresentationEvent } from "./useGameSession";
import GuessBar from "./GuessBar.vue";

const props = withDefaults(defineProps<{
  active?: boolean;
  registerGuess?: (key: string, element: Element | ComponentPublicInstance | null) => void;
  gameId: string;
  currentGuess: Guess | null;
  guesses: Guess[];
  animatedHintWord: string | null;
  presentationEvent: GuessPresentationEvent | null;
}>(), { active: true });

function getGuessKey(guess: Guess) {
  return `${props.gameId}:${guess.word}`;
}

const revealRestoredGuesses = ref(props.guesses.length > 0 && props.active !== false
  && !window.matchMedia("(prefers-reduced-motion: reduce)").matches);
watch(() => props.active, (active) => {
  if (active === false) revealRestoredGuesses.value = false;
}, { flush: "sync" });

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
      :class="{ 'guesses-list__section--restored': revealRestoredGuesses }"
      @animationend.self="revealRestoredGuesses = false" @animationcancel.self="revealRestoredGuesses = false">
      <p class="guesses-list__label">
        Попыток: {{ guesses.length }}
      </p>
      <div class="guesses-list__items">
        <div v-for="guess in guesses" :key="getGuessKey(guess)" :ref="(element) => registerGuess?.(getGuessKey(guess), element)">
          <GuessBar :active="active" :word="guess.word" :value="guess.distance"
            :fill-percentage="guess.fillPercentage" :hint="guess.source === 'hint'"
            :animate-fill-on-mount="presentationEvent?.word === guess.word && presentationEvent.kind === 'insert'"
            :fill-animation-key="presentationEvent?.word === guess.word ? presentationEvent.id : 0"
            :fill-animation-start="0"
            :animate-hint-reveal="guess.word === animatedHintWord" :feedback="getFeedback(guess)"
            :highlighted="guess.word === currentGuess?.word" />
        </div>
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
  min-width: 0;
  padding-block: 4px;
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

@media (width < 1024px) {
  .guesses-list__section--restored {
    animation-name: guesses-list-fade;
  }
}

@keyframes guesses-list-fade {
  from { opacity: 0; }
  to { opacity: 1; }
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
