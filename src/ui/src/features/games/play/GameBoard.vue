<script setup lang="ts">
import Dialog from "primevue/dialog";
import Popover from "primevue/popover";
import Select from "primevue/select";
import { computed, nextTick, onBeforeUnmount, ref, watch } from "vue";
import { useInfoPopover } from "../../../shared/composables/useInfoPopover";
import type { Difficulty, Game, Guess } from "../model/game";
import type { DailyGameResultStatsState } from "./useDailyGameResultStats";
import type { GuessPresentationEvent } from "./useGameSession";
import UiButton from "../../../shared/ui/UiButton.vue";
import UiSkeleton from "../../../shared/ui/UiSkeleton.vue";
import DailyGameStatsCard from "./DailyGameStatsCard.vue";
import DisplayWordTiles from "./DisplayWordTiles.vue";
import GameScoreCard from "./GameScoreCard.vue";
import GameSourceRemovedDialog from "./GameSourceRemovedDialog.vue";
import GuessBar from "./GuessBar.vue";
import GuessesList from "./GuessesList.vue";
import GameplayControls, { type GameplayActions } from "./GameplayControls.vue";

type ArcadeReplayConfig = {
  difficulties: Difficulty[];
  selectedDifficultyCode: string;
  creating: boolean;
  difficultiesLoading: boolean;
  canReplay: boolean;
  selectDifficulty: (value: string) => void;
  replay: () => void;
};
export type GameBoardModeConfig =
  | { mode: "arcade"; title: string; detail: string | null; replay: ArcadeReplayConfig | null }
  | {
    mode: "daily";
    title: string;
    detail: string | null;
    stats: { state: DailyGameResultStatsState; refresh: () => Promise<void> };
  };

const props = defineProps<{
  game: Game;
  currentGuess: Guess | null;
  animatedHintWord: string | null;
  guessPresentationEvent: GuessPresentationEvent | null;
  guessing: boolean;
  finishedGameRefreshing: boolean;
  modeConfig: GameBoardModeConfig;
  actions: GameplayActions;
}>();

const resultConfig = {
  guessed: {
    label: "Слово отгадано",
    icon: "pi pi-check-circle",
  },
  surrendered: {
    label: "Вы сдались",
    icon: "pi pi-times-circle",
  },
  cancelled: {
    label: "Игра отменена",
    icon: "pi pi-ban",
  },
};

const gameSourceRemovedDialogOpen = ref(false);
const dailyStatsDialogOpen = ref(false);
const celebrateGuessed = ref(false);
const resultAnimationsReady = ref(true);
const gameBoard = ref<HTMLElement | null>(null);
const guessWord = ref("");
const displayedPresentationEvent = ref<GuessPresentationEvent | null>(null);
const displayedAnimatedHintWord = ref<string | null>(null);
const emptyDailyGameStats: DailyGameResultStatsState = {
  aggregate: { status: "loading" },
  player: { status: "idle" },
};
const arcadeConfig = computed(() => props.modeConfig.mode === "arcade" ? props.modeConfig : null);
const dailyConfig = computed(() => props.modeConfig.mode === "daily" ? props.modeConfig : null);
const dailyResultStats = computed(() => dailyConfig.value?.stats.state ?? emptyDailyGameStats);
const dailyStatsLoading = computed(() =>
  dailyConfig.value?.stats.state.aggregate.status === "loading"
  || dailyConfig.value?.stats.state.player.status === "loading",
);
const dailyResultState = computed((): Extract<Game["gameState"], "guessed" | "surrendered"> | null => {
  if (!dailyConfig.value) return null;
  return props.game.gameState === "guessed" || props.game.gameState === "surrendered" ? props.game.gameState : null;
});
let resultSequence = 0;

function usesMobileScrollLayout() {
  return window.matchMedia("(width < 1024px)").matches;
}

watch(
  () => props.game.id,
  () => {
    resultSequence += 1;
    resultAnimationsReady.value = true;
    displayedPresentationEvent.value = null;
    displayedAnimatedHintWord.value = null;
  },
  { flush: "sync" },
);

watch(
  [() => props.guessPresentationEvent, () => props.animatedHintWord],
  ([event, animatedHintWord]) => {
    displayedPresentationEvent.value = event;
    displayedAnimatedHintWord.value = animatedHintWord;
  },
  { flush: "pre" },
);

onBeforeUnmount(() => {
  resultSequence += 1;
});

const {
  triggerId: dailyStatsInfoTriggerId,
  panelId: dailyStatsInfoPanelId,
  popoverPt: dailyStatsInfoPopoverPt,
  setPopover: setDailyStatsInfoPopover,
  visible: dailyStatsInfoPopoverVisible,
  show: showDailyStatsInfo,
  hide: hideDailyStatsInfo,
  onPointerDown: onDailyStatsInfoPointerDown,
  onShow: onDailyStatsInfoPopoverShow,
  onHide: onDailyStatsInfoPopoverHide,
} = useInfoPopover();

function showGameSourceRemovedDialog() {
  gameSourceRemovedDialogOpen.value = true;
}

function showDailyStatsDialog() {
  dailyStatsDialogOpen.value = true;
}

watch(
  () => ({ id: props.game.id, state: props.game.gameState }),
  async (current, previous) => {
    const gameJustFinished = previous?.id === current.id
      && previous.state === "active"
      && current.state !== "active";

    if (previous?.id === current.id && previous.state === "active" && current.state === "cancelled") {
      showGameSourceRemovedDialog();
    }

    if (gameJustFinished) {
      const sequence = ++resultSequence;
      celebrateGuessed.value = false;
      resultAnimationsReady.value = false;
      displayedPresentationEvent.value = null;
      displayedAnimatedHintWord.value = null;
      await nextTick();
      if (usesMobileScrollLayout()) gameBoard.value?.scrollTo({ top: 0, behavior: "instant" });
      if (sequence === resultSequence) {
        resultAnimationsReady.value = true;
        if (props.game.gameState === "guessed") celebrateGuessed.value = true;
      }
    } else if (previous?.id !== current.id || current.state === "active") {
      resultSequence += 1;
      celebrateGuessed.value = false;
      resultAnimationsReady.value = true;
    }

  },
);
</script>

<template>
  <div ref="gameBoard" class="game-board" :class="{ 'game-board--active': game.gameState === 'active' }">
    <template v-if="game.gameState !== 'active'">
      <div class="game-board__win">
        <DisplayWordTiles :display-word="game.displayWord" :difficulty-name="game.difficulty.name"
          :mode-label="modeConfig.title" :mode-detail="modeConfig.detail" :mode-variant="modeConfig.mode"
          :result-label="resultConfig[game.gameState].label" :result-icon="resultConfig[game.gameState].icon"
          :game-state="game.gameState" :word-loading="finishedGameRefreshing"
          :celebrate-guessed="celebrateGuessed" :animations-enabled="resultAnimationsReady" />
        <section v-if="dailyResultState" class="game-result-summary-card" aria-label="Итоги игры дня">
          <GameScoreCard :score="game.score" :score-details="game.scoreDetails" embedded />
          <div class="game-result-summary-card__divider" aria-hidden="true"></div>
          <DailyGameStatsCard :result-state="dailyResultState" :stats="dailyResultStats"
            :refresh="dailyConfig?.stats.refresh" embedded collapsible />
        </section>
        <GameScoreCard v-else :score="game.score" :score-details="game.scoreDetails" />

        <section v-if="arcadeConfig?.replay" class="game-result-actions">
          <h3 class="game-result-actions__title">Играть ещё</h3>
          <div v-if="arcadeConfig.replay.difficultiesLoading" class="game-result-replay-skeleton" aria-live="polite"
            aria-label="Загрузка сложностей">
            <UiSkeleton width="72px" height="14px" />
            <UiSkeleton width="100%" height="42px" border-radius="8px" />
            <UiSkeleton width="100%" height="44px" border-radius="8px" />
          </div>
          <form v-else class="game-result-replay-options" @submit.prevent="arcadeConfig.replay.replay">
            <label class="game-result-replay-options__difficulty">
              <span class="game-result-replay-options__label">Сложность</span>
              <Select class="game-result-replay-options__select" :model-value="arcadeConfig.replay.selectedDifficultyCode"
                :options="arcadeConfig.replay.difficulties" option-label="name" option-value="code"
                :disabled="arcadeConfig.replay.creating || !arcadeConfig.replay.canReplay"
                @update:model-value="arcadeConfig.replay.selectDifficulty" />
            </label>
            <UiButton type="submit" size="md" :loading="arcadeConfig.replay.creating"
              :disabled="!arcadeConfig.replay.canReplay">
              Начать игру
            </UiButton>
          </form>
          <div v-if="!arcadeConfig.replay.difficultiesLoading && !arcadeConfig.replay.creating && !arcadeConfig.replay.canReplay"
            class="game-result-actions__status">
            Сложности недоступны
          </div>
        </section>
      </div>
      <GuessesList :key="game.id" :game-id="game.id" :current-guess="currentGuess" :guesses="game.allGuesses"
        :animated-hint-word="displayedAnimatedHintWord" :presentation-event="displayedPresentationEvent" />
    </template>

    <GameplayControls v-else :key="game.id" v-model="guessWord"
      :game-id="game.id" :presentation-event="guessPresentationEvent"
      :dialog-open="dailyStatsDialogOpen || gameSourceRemovedDialogOpen"
      :loading="guessing" :word-length="game.displayWord.cells?.length ?? null"
      :hints-info="game.hintsInfo" :actions="actions"
      :show-statistics="dailyConfig ? showDailyStatsDialog : undefined">
      <template #word>
        <div class="game-board__word" data-game-scroll-anchor="word-card">
          <DisplayWordTiles :display-word="game.displayWord" :difficulty-name="game.difficulty.name"
            :mode-label="modeConfig.title" :mode-detail="modeConfig.detail" :mode-variant="modeConfig.mode"
            :game-state="game.gameState" />
        </div>
      </template>
      <template #summary>
        <GameScoreCard class="guess-card__score" :score="game.score" :score-details="game.scoreDetails" embedded />
        <div v-if="currentGuess" class="guess-card__latest">
          <p class="guess-card__label">Последняя попытка</p>
          <GuessBar :fill-animation-key="guessPresentationEvent?.id ?? 0" :word="currentGuess.word" :value="currentGuess.distance"
            :fill-percentage="currentGuess.fillPercentage" :hint="currentGuess.source === 'hint'"
            :animate-fill-on-mount="guessPresentationEvent?.word === currentGuess.word"
            :fill-animation-start="guessPresentationEvent?.previousFillPercentage ?? 0" />
        </div>
      </template>
      <template #guesses>
        <div class="game-board__guesses">
          <GuessesList :key="game.id" :game-id="game.id" :current-guess="currentGuess"
            :guesses="game.allGuesses" :animated-hint-word="displayedAnimatedHintWord"
            :presentation-event="displayedPresentationEvent" />
        </div>
      </template>
    </GameplayControls>

    <GameSourceRemovedDialog :visible="gameSourceRemovedDialogOpen" @close="gameSourceRemovedDialogOpen = false" />

    <Dialog v-model:visible="dailyStatsDialogOpen" modal dismissable-mask class="game-dialog daily-stats-game-dialog">
      <template #header>
        <div class="daily-stats-dialog__header">
          <span>Статистика</span>
          <button class="daily-stats-dialog__info"
            :class="{ 'daily-stats-dialog__info--open': dailyStatsInfoPopoverVisible }" type="button"
            :id="dailyStatsInfoTriggerId" aria-label="О статистике дня"
            :aria-describedby="dailyStatsInfoPanelId" @mouseenter="showDailyStatsInfo"
            @mouseleave="hideDailyStatsInfo"
            @pointerdown="onDailyStatsInfoPointerDown" @focus="showDailyStatsInfo" @blur="hideDailyStatsInfo"
            @click.stop>
            <i class="pi pi-info-circle" aria-hidden="true"></i>
          </button>
        </div>
      </template>

      <div class="daily-stats-dialog">
        <DailyGameStatsCard v-if="dailyConfig" result-state="active" :stats="dailyConfig.stats.state" embedded
          :show-header="false" :show-refresh="false" :refresh="dailyConfig.stats.refresh" />

        <div class="daily-stats-dialog__actions">
          <UiButton v-if="dailyConfig" variant="outlined" :loading="dailyStatsLoading"
            loading-label="Обновление статистики" @click="dailyConfig.stats.refresh">
            <i class="pi pi-refresh" aria-hidden="true"></i>
            Обновить
          </UiButton>
        </div>
      </div>

      <Popover :ref="setDailyStatsInfoPopover" :pt="dailyStatsInfoPopoverPt" class="daily-stats-dialog__popover info-popover"
        @show="onDailyStatsInfoPopoverShow" @hide="onDailyStatsInfoPopoverHide">
        <p class="daily-stats-dialog__popover-text">
          Пока текущая игра активна, в статистике отображаются средние результаты других игроков. После завершения игры
          появятся ваш результат и его сравнение с результатами других игроков.
        </p>
      </Popover>
    </Dialog>
  </div>
</template>

<style scoped>
.game-board,
.game-board__win {
  width: 100%;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.game-board__word {
  order: 1;
}

.game-board__guesses {
  order: 4;
}

.guess-card__latest {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.guess-card__label {
  margin: 0;
  color: var(--p-text-muted-color);
  font-size: 12px;
  line-height: 1.3;
}

.game-result-summary-card {
  min-width: 0;
  overflow: hidden;
  display: flex;
  flex-direction: column;
  gap: 8px;
  padding: 8px 9px;
  border: 1px solid var(--color-gray-200);
  border-radius: 8px;
  background: white;
}

.game-result-summary-card__divider {
  height: 1px;
  background: var(--color-gray-100);
}

.daily-stats-dialog__header {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  color: var(--color-gray-900);
  font-size: 18px;
  font-weight: 400;
  line-height: 1.2;
}

.daily-stats-dialog__info {
  width: 28px;
  min-width: 28px;
  height: 28px;
  padding: 0;
  border: 0;
  border-radius: 50%;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: transparent;
  color: var(--p-surface-500);
  cursor: help;
  transition: color 0.16s ease, background-color 0.16s ease;
}

.daily-stats-dialog__info i {
  font-size: 15px;
  line-height: 1;
}

.daily-stats-dialog__info--open {
  background: var(--color-primary-50);
  color: var(--color-primary-700);
}

.daily-stats-dialog {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.daily-stats-dialog__actions {
  display: flex;
  justify-content: flex-start;
}

.daily-stats-dialog__actions>.ui-button {
  width: 100%;
  font-size: 14px;
}

.daily-stats-dialog__popover-text {
  max-width: 280px;
  margin: 0;
  color: var(--color-gray-700);
  font-size: var(--info-popover-font-size);
  line-height: 1.45;
}

:global(.daily-stats-game-dialog.p-dialog) {
  width: min(calc(100vw - 24px), 560px);
}

:global(.daily-stats-dialog__popover.p-popover) {
  max-width: min(310px, calc(100vw - 24px));
}

@media (hover: hover) and (pointer: fine) {
  .daily-stats-dialog__info:hover {
    background: var(--color-primary-50);
    color: var(--color-primary-700);
  }
}

.daily-stats-dialog__info:focus-visible {
  outline: 2px solid var(--color-primary-600);
  outline-offset: 2px;
}

.game-result-actions {
  display: flex;
  flex-direction: column;
  gap: 9px;
  padding: 9px 10px;
  border: 1px solid var(--color-gray-300);
  border-radius: 8px;
  background: white;
}

.game-result-actions__title {
  margin: 0;
  color: var(--color-gray-900);
  font-size: 16px;
  font-weight: 500;
  text-align: center;
}

.game-result-replay-options {
  width: min(100%, 300px);
  margin-inline: auto;
  display: flex;
  flex-direction: column;
  gap: 9px;
}

.game-result-replay-skeleton {
  width: min(100%, 300px);
  margin-inline: auto;
  display: flex;
  flex-direction: column;
  gap: 9px;
}

.game-result-replay-options__difficulty {
  display: flex;
  flex-direction: column;
  gap: 8px;
  color: var(--color-gray-700);
}

.game-result-replay-options__label {
  color: var(--color-gray-600);
  font-size: 14px;
}

.game-result-replay-options__select.p-select {
  width: 100%;
  min-height: 42px;
  border-radius: 8px;
}

.game-result-replay-options>.ui-button {
  width: 100%;
}

.game-result-actions__status {
  color: var(--p-text-muted-color);
  font-size: 13px;
  text-align: center;
}

@media (min-width: 768px) {

  .game-board {
    gap: 12px;
  }

  .game-board__win {
    gap: 10px;
  }

  .game-result-summary-card {
    gap: 10px;
    padding: 10px 12px;
  }

  .game-result-actions {
    gap: 10px;
    padding: 12px;
  }

  .daily-stats-dialog__actions>.ui-button {
    width: auto;
    font-size: 16px;
  }
}

@media (width < 1024px) {
  .game-board {
    flex: 1;
    min-height: 0;
    overflow-y: auto;
    overscroll-behavior: none;
    padding: 8px 0 max(12px, var(--app-visual-viewport-safe-bottom));
  }

  .game-board--active {
    overflow: hidden;
    padding: 0;
    gap: 0;
  }

  .game-board__word,
  .game-board__guesses {
    order: initial;
  }
}
</style>
