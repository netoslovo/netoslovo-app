<script setup lang="ts">
import { computed } from "vue";
import { useRoute } from "vue-router";
import GameBoard from "../features/games/play/GameBoard.vue";
import GameBoardSkeleton from "../features/games/play/GameBoardSkeleton.vue";
import { useDailyGameResultStats } from "../features/games/play/useDailyGameResultStats";
import { useGameSession } from "../features/games/play/useGameSession";
import { useGuestProgressNotice } from "../features/games/play/useGuestProgressNotice";
import GuestProgressNotice from "../features/games/components/GuestProgressNotice.vue";
import { useHandoffDelayedLoadingState } from "../shared/composables/useSkeletonHandoff";
import UiSkeletonHandoff from "../shared/ui/UiSkeletonHandoff.vue";
import DailyGameShareDialog from "../features/games/share/DailyGameShareDialog.vue";

const route = useRoute();
const day = computed(() => typeof route.params.day === "string" ? route.params.day : null);
const session = useGameSession("daily", day);
const guestProgressNotice = useGuestProgressNotice(session.game);
const loadingState = useHandoffDelayedLoadingState(session.loading);
const statsEnabled = computed(() => day.value !== null);
const includePlayerStats = computed(() => session.game.value?.gameState === "guessed");
const resultStats = useDailyGameResultStats(day, statsEnabled, includePlayerStats);
const formattedDay = computed(() => day.value
  ? new Intl.DateTimeFormat("ru-RU", { day: "numeric", month: "long", year: "numeric" }).format(new Date(`${day.value}T00:00:00`))
  : null,
);
const actions = {
  submitGuess: session.submitGuess,
  surrender: session.surrenderGame,
  revealHalfwayWord: session.revealHalfwayWordGame,
  revealWordLength: session.revealWordLengthGame,
  revealRandomLetter: session.revealRandomLetterGame,
};
const modeConfig = computed(() => ({
  mode: "daily" as const,
  title: "Слово дня",
  detail: formattedDay.value,
  stats: {
    state: resultStats.state.value,
    refresh: resultStats.load,
  },
}));
</script>

<template>
  <h1 class="visually-hidden">Слово дня</h1>
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
      :finished-game-data-state="session.finishedGameDataState.value"
      :mode-config="modeConfig"
      :actions="actions"
    >
      <template #result-header-action>
        <DailyGameShareDialog v-if="day" :game-id="session.game.value.id" :day="day"
          :attention-enabled="session.game.value.gameState === 'guessed'" />
      </template>
      <template #result-notice>
        <GuestProgressNotice v-if="guestProgressNotice.visible.value" :saving="guestProgressNotice.saving.value"
          @dismiss="guestProgressNotice.dismiss"
          @login="guestProgressNotice.requestLogin" />
      </template>
    </GameBoard>
    <div v-else class="daily-preview">
      {{ session.unavailable.value ? "Слово дня недоступно." : "Не удалось открыть слово дня." }}
    </div>
  </UiSkeletonHandoff>
</template>

<style scoped>
.daily-preview { flex: 1; display: flex; align-items: center; justify-content: center; }
.daily-preview { color: var(--color-gray-600); font-size: 15px; text-align: center; }
</style>
