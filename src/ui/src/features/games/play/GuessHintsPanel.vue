<script setup lang="ts">
import type { HintsInfo } from "../model/game";
import HintMenuItem from "./HintMenuItem.vue";
import { useGuessHints } from "./useGuessHints";

const props = defineProps<{
  hintsInfo: HintsInfo;
  wordLength?: number | null;
  loading: boolean;
  pendingHint: "halfway" | "length" | "letter" | null;
}>();
const emit = defineEmits<{ request: [kind: "halfway" | "length" | "letter"] }>();
const {
  remainingNeighbourHints, totalNeighbourHints, remainingRevealLetterHints, totalRevealLetterHints,
  canRevealHalfwayWord, nextHalfwayWordPenalty, halfwayWordHintDisabledReason,
  canRevealWordLength, nextWordLengthPenalty, wordLengthHintDisabledReason,
  canRevealRandomLetter, nextRandomLetterPenalty, randomLetterHintDisabledReason,
  halfwayWordDescription, wordLengthDescription, randomLetterDescription,
} = useGuessHints(props);
</script>

<template>
  <div>
    <HintMenuItem title="Промежуточное слово" icon="pi pi-sort-amount-up" :description="halfwayWordDescription"
      :remaining="remainingNeighbourHints" :total="totalNeighbourHints" :score-penalty="nextHalfwayWordPenalty"
      :disabled="loading" :unavailable="!canRevealHalfwayWord" :loading="pendingHint === 'halfway'"
      :disabled-reason="halfwayWordHintDisabledReason" @activate="emit('request', 'halfway')" />
    <HintMenuItem title="Показать длину слова" icon="pi pi-eye" :description="wordLengthDescription"
      :score-penalty="nextWordLengthPenalty" :disabled="loading" :unavailable="!canRevealWordLength"
      :loading="pendingHint === 'length'" :disabled-reason="wordLengthHintDisabledReason"
      @activate="emit('request', 'length')" />
    <HintMenuItem title="Открыть случайную букву" icon="pi pi-question" :description="randomLetterDescription"
      :remaining="remainingRevealLetterHints" :total="totalRevealLetterHints"
      :score-penalty="nextRandomLetterPenalty" :disabled="loading" :unavailable="!canRevealRandomLetter"
      :loading="pendingHint === 'letter'" :disabled-reason="randomLetterHintDisabledReason"
      @activate="emit('request', 'letter')" />
  </div>
</template>
