<script setup lang="ts">
import { computed } from "vue";
import type { DisplayWord, GameState, GameWord } from "../model/game";
import DisplayWordTiles from "./DisplayWordTiles.vue";

const props = defineProps<{
  gameWord: GameWord | null;
  caption?: string | null;
  gameState?: GameState | null;
  wordLoading?: boolean;
  animationsEnabled?: boolean;
}>();

function fromSecretWord(secretWord: string): DisplayWord {
  return {
    cells: [...secretWord].map((value) => ({ value, revealed: true })),
  };
}

const secretDisplayWord = computed(() => {
  const gameWord = props.gameWord;
  return gameWord?.status === "secret" || gameWord?.status === "displayAndSecret"
    ? fromSecretWord(gameWord.secretWord)
    : null;
});
</script>

<template>
  <DisplayWordTiles v-if="gameWord === null" :display-word="null" :caption="caption"
    :game-state="gameState" :word-loading="wordLoading" :animations-enabled="animationsEnabled" />

  <DisplayWordTiles v-else-if="gameWord.status === 'legacy'" :display-word="gameWord.displayWord"
    :caption="caption" :game-state="gameState"
    :hidden-for-today="gameWord.displayWord.hideReason === 'hiddenForToday'"
    :word-loading="wordLoading" :animations-enabled="animationsEnabled" />

  <DisplayWordTiles v-else-if="gameWord.status === 'display'" :display-word="gameWord.displayWord"
    :caption="caption" :game-state="gameState"
    :hidden-for-today="gameWord.secretWordUnavailableReason === 'surrenderedHiddenForToday'"
    :word-loading="wordLoading" :animations-enabled="animationsEnabled" />

  <DisplayWordTiles v-else-if="gameWord.status === 'secret'" :display-word="secretDisplayWord"
    :caption="caption" :game-state="gameState" :word-loading="wordLoading"
    :animations-enabled="animationsEnabled" />

  <div v-else-if="gameWord.status === 'displayAndSecret'" class="game-word-display">
    <div class="game-word-display__part">
      <p class="game-word-display__label">Было загадано слово:</p>
      <DisplayWordTiles :display-word="secretDisplayWord" :game-state="gameState"
        :show-surrendered-title="false" :animations-enabled="animationsEnabled" />
    </div>
    <div v-if="gameWord.displayWord.cells?.length" class="game-word-display__part">
      <p class="game-word-display__label">На момент сдачи было открыто:</p>
      <DisplayWordTiles :display-word="gameWord.displayWord" :game-state="gameState"
        :show-surrendered-title="false" :animations-enabled="animationsEnabled" />
    </div>
  </div>

  <p v-else class="game-word-display__unavailable">Загаданное слово недоступно.</p>
</template>

<style scoped>
.game-word-display,
.game-word-display__part {
  width: 100%;
  display: flex;
  flex-direction: column;
  align-items: center;
}

.game-word-display {
  gap: 12px;
}

.game-word-display__part {
  gap: 6px;
}

.game-word-display__label,
.game-word-display__unavailable {
  margin: 0;
  color: var(--p-text-muted-color);
  font-size: 13px;
  line-height: 1.35;
  text-align: center;
}

@media (min-width: 768px) {
  .game-word-display {
    gap: 14px;
  }

  .game-word-display__label,
  .game-word-display__unavailable {
    font-size: 14px;
  }
}
</style>
