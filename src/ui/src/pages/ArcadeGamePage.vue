<script setup lang="ts">
import { computed } from "vue";
import { useRoute, useRouter } from "vue-router";
import GameBoard from "../features/games/play/GameBoard.vue";
import GameBoardSkeleton from "../features/games/play/GameBoardSkeleton.vue";
import { useGameSession } from "../features/games/play/useGameSession";
import { useGuestLoginPrompt } from "../features/games/play/useGuestLoginPrompt";
import GuestLoginPrompt from "../features/games/components/GuestLoginPrompt.vue";
import { useHandoffDelayedLoadingState } from "../shared/composables/useSkeletonHandoff";
import UiSkeletonHandoff from "../shared/ui/UiSkeletonHandoff.vue";

const route = useRoute();
const router = useRouter();
const gameId = computed(() => typeof route.params.id === "string" ? route.params.id : null);
const openedFromHistory = computed(() => route.query.from === "arcade-history");
const replayAvailable = computed(() => !openedFromHistory.value);
const session = useGameSession(
  "arcade",
  gameId,
  () => void router.replace({ name: "home" }),
  replayAvailable,
);
const guestLoginPrompt = useGuestLoginPrompt(computed(() => session.game.value?.gameState ?? null));
const loadingState = useHandoffDelayedLoadingState(session.loading);
const actions = {
  submitGuess: session.submitGuess,
  surrender: session.surrenderGame,
  revealHalfwayWord: session.revealHalfwayWordGame,
  revealWordLength: session.revealWordLengthGame,
  revealRandomLetter: session.revealRandomLetterGame,
};
const modeConfig = computed(() => ({
  mode: "arcade" as const,
  title: "Случайное слово",
  detail: session.game.value ? `Сложность: ${session.game.value.difficulty.name}` : null,
  replay: openedFromHistory.value ? null : {
    difficulties: session.difficulties.value,
    selectedDifficultyCode: session.selectedDifficultyCode.value,
    creating: session.creating.value,
    difficultiesLoading: session.replayDifficultiesLoading.value,
    canReplay: session.canReplay.value,
    selectDifficulty: (value: string) => { session.selectedDifficultyCode.value = value; },
    replay,
  },
}));

function replay() {
  return guestLoginPrompt.runBeforeLeaving(() =>
    session.replay((game) => router.push({ name: "arcade-game", params: { id: game.id } })),
  );
}
</script>

<template>
  <h1 class="visually-hidden">Случайное слово</h1>
  <UiSkeletonHandoff :skeleton-visible="loadingState.visible" :content-visible="loadingState.ready">
    <template #skeleton>
      <GameBoardSkeleton />
    </template>
    <GameBoard
      v-if="session.game.value"
      :game="session.game.value"
      :current-guess="session.currentGuess.value"
      :animated-hint-word="session.animatedHintWord.value"
      :guess-presentation-event="session.guessPresentationEvent.value"
      :guessing="session.guessing.value"
      :finished-game-refreshing="session.finishedGameRefreshing.value"
      :mode-config="modeConfig"
      :actions="actions"
    />
  </UiSkeletonHandoff>
  <GuestLoginPrompt :visible="guestLoginPrompt.visible.value" @continue="guestLoginPrompt.continueNavigation"
    @login="guestLoginPrompt.requestLogin" />
</template>
