<script setup lang="ts">
import { computed, onBeforeUnmount, ref, watch } from "vue";
import { useRoute } from "vue-router";
import { isApiRequestCanceled, toApiError } from "../shared/api/apiError";
import { useHandoffDelayedLoadingState } from "../shared/composables/useSkeletonHandoff";
import UiButton from "../shared/ui/UiButton.vue";
import UiSkeleton from "../shared/ui/UiSkeleton.vue";
import UiSkeletonHandoff from "../shared/ui/UiSkeletonHandoff.vue";
import { getSharedDailyGame } from "../features/games/api/gameApi";
import type { GameState, SharedDailyGame } from "../features/games/model/game";
import DailyGameStatsCard from "../features/games/play/DailyGameStatsCard.vue";
import DisplayWordTiles from "../features/games/play/DisplayWordTiles.vue";
import GameBoardSkeleton from "../features/games/play/GameBoardSkeleton.vue";
import GameScoreCard from "../features/games/play/GameScoreCard.vue";
import GameWordCard from "../features/games/play/GameWordCard.vue";
import type { DailyGameResultStatsState } from "../features/games/play/useDailyGameResultStats";
import SharedGuessesList from "../features/games/share/SharedGuessesList.vue";

const route = useRoute();
const publicId = computed(() => typeof route.params.publicId === "string" ? route.params.publicId : null);
const game = ref<SharedDailyGame | null>(null);
const loading = ref(true);
const notFound = ref(false);
const failed = ref(false);
const loadingState = useHandoffDelayedLoadingState(loading);
let controller: AbortController | null = null;
let requestId = 0;

const formattedDay = computed(() => game.value
  ? new Intl.DateTimeFormat("ru-RU", {
    day: "numeric",
    month: "long",
    year: "numeric",
  }).format(new Date(`${game.value.day}T00:00:00`))
  : null,
);
const resultPresentation = computed(() => getResultPresentation(game.value?.gameState ?? null));
const dailyResultState = computed((): "guessed" | "surrendered" | null => {
  const state = game.value?.gameState;
  return state === "guessed" || state === "surrendered" ? state : null;
});
const statsState = computed((): DailyGameResultStatsState => ({
  aggregate: game.value?.gameStats
    ? { status: "available", data: game.value.gameStats }
    : { status: "insufficient" },
  player: game.value?.playerStats
    ? { status: "available", data: game.value.playerStats }
    : game.value?.gameState === "guessed"
      ? { status: "delayed" }
      : { status: "idle" },
}));
const hiddenWordCaption = computed(() => {
  if (game.value?.spoilersHideReason === "hiddenForToday") {
    return "Загаданное слово и слова попыток будут доступны завтра, когда завершится игра дня.";
  }

  if (game.value?.spoilersHideReason === "viewerGameNotFinished") {
    return "Загаданное слово и слова попыток откроются после того, как вы завершите свою игру за этот день.";
  }

  return null;
});
watch(publicId, () => void load(), { immediate: true });

onBeforeUnmount(() => {
  requestId += 1;
  controller?.abort();
});

async function load() {
  controller?.abort();
  const id = publicId.value;
  const abortController = new AbortController();
  controller = abortController;
  const currentRequestId = ++requestId;
  loading.value = true;
  failed.value = false;
  notFound.value = false;
  game.value = null;

  if (!id) {
    notFound.value = true;
    loading.value = false;
    return;
  }

  try {
    const sharedGame = await getSharedDailyGame(id, { signal: abortController.signal });
    if (currentRequestId !== requestId) return;
    game.value = sharedGame;
  } catch (error) {
    if (isApiRequestCanceled(error) || currentRequestId !== requestId) return;
    if (toApiError(error).status === 404) notFound.value = true;
    else failed.value = true;
  } finally {
    if (currentRequestId === requestId) loading.value = false;
    if (controller === abortController) controller = null;
  }
}

function getResultPresentation(state: GameState | null) {
  switch (state) {
    case "guessed":
      return { label: "Слово отгадано", icon: "pi pi-check-circle" };
    case "surrendered":
      return { label: "Игрок сдался", icon: "pi pi-times-circle" };
    case "cancelled":
      return { label: "Игра отменена", icon: "pi pi-ban" };
    default:
      return { label: "Результат игры", icon: "pi pi-calendar" };
  }
}
</script>

<template>
  <h1 class="visually-hidden">Результат слова дня</h1>

  <UiSkeletonHandoff :skeleton-visible="loadingState.visible" :content-visible="loadingState.ready">
    <template #skeleton>
      <GameBoardSkeleton>
        <template #context>
          <div class="shared-daily-context-skeleton__message">
            <UiSkeleton width="30px" height="30px" border-radius="50%" />
            <UiSkeleton class="shared-daily-context-skeleton__text" width="280px" height="15px" />
          </div>
          <div class="shared-daily-context-skeleton__actions">
            <UiSkeleton class="shared-daily-context-skeleton__action" width="190px" height="34px" border-radius="8px" />
          </div>
        </template>
      </GameBoardSkeleton>
    </template>

    <div v-if="game" class="shared-daily-result">
      <aside class="shared-daily-context" aria-label="Опубликованный результат">
        <div class="shared-daily-context__message">
          <span class="shared-daily-context__icon" aria-hidden="true">
            <i class="pi pi-share-alt"></i>
          </span>
          <p class="shared-daily-context__title">
            Отображется результат игрока {{ game.playerName }}
          </p>
        </div>
        <div class="shared-daily-context__actions">
          <UiButton class="shared-daily-context__action" size="sm" variant="soft"
            :to="{ name: 'daily', params: { day: game.day } }">
            Открыть эту игру
            <i class="pi pi-arrow-right" aria-hidden="true"></i>
          </UiButton>
        </div>
      </aside>

      <div class="shared-daily-result__summary">
        <GameWordCard mode-label="Слово дня" :mode-detail="formattedDay" mode-variant="daily"
          :result-label="resultPresentation.label" :result-icon="resultPresentation.icon" :game-state="game.gameState"
          :animations-enabled="false">
          <DisplayWordTiles :display-word="game.displayWord" :caption="hiddenWordCaption" :game-state="game.gameState"
            :animations-enabled="false" />
        </GameWordCard>

        <section v-if="dailyResultState" class="shared-daily-result__result-card" aria-label="Итоги игры дня">
          <GameScoreCard :score="game.score" :score-details="game.scoreDetails" embedded />
          <div class="shared-daily-result__divider" aria-hidden="true"></div>
          <DailyGameStatsCard :result-state="dailyResultState" :stats="statsState" :player-name="game.playerName"
            embedded collapsible :show-refresh="false" />
        </section>
        <GameScoreCard v-else :score="game.score" :score-details="game.scoreDetails" />
      </div>

      <SharedGuessesList :guesses="game.allGuesses" :hide-reason="game.spoilersHideReason" />
    </div>

    <section v-else-if="notFound" class="shared-daily-state">
      <i class="shared-daily-state__icon pi pi-link" aria-hidden="true"></i>
      <h2 class="shared-daily-state__title">Ссылка недоступна</h2>
      <p class="shared-daily-state__text">Возможно, владелец отключил её, или адрес указан неверно.</p>
      <UiButton :to="{ name: 'home' }">На главную</UiButton>
    </section>

    <section v-else-if="failed" class="shared-daily-state">
      <i class="shared-daily-state__icon pi pi-exclamation-circle" aria-hidden="true"></i>
      <h2 class="shared-daily-state__title">Не удалось открыть результат</h2>
      <p class="shared-daily-state__text">Проверьте соединение и попробуйте ещё раз.</p>
      <UiButton @click="load"><i class="pi pi-refresh" aria-hidden="true"></i>Повторить</UiButton>
    </section>
  </UiSkeletonHandoff>
</template>

<style scoped>
.shared-daily-result,
.shared-daily-result__summary {
  width: 100%;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.shared-daily-context {
  width: 100%;
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 8px;
  padding: 8px 10px;
  border: 1px solid var(--color-primary-100);
  border-radius: 8px;
  background: var(--color-primary-50);
}

.shared-daily-context__message,
.shared-daily-context-skeleton__message {
  min-width: 0;
  display: flex;
  align-items: center;
  gap: 9px;
}

.shared-daily-context__icon {
  width: 30px;
  min-width: 30px;
  height: 30px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
  background: white;
  color: var(--color-primary-600);
}

.shared-daily-context__icon .pi {
  font-size: 14px;
}

.shared-daily-context-skeleton__text {
  min-width: 0;
  flex: 1;
}

.shared-daily-context__title {
  min-width: 0;
  flex: 1;
  margin: 0;
  overflow-wrap: anywhere;
  color: var(--color-gray-800);
  font-size: 13px;
  line-height: 1.35;
}

.shared-daily-context__actions,
.shared-daily-context-skeleton__actions {
  display: flex;
  justify-content: flex-start;
}

.shared-daily-context__action,
.shared-daily-context-skeleton__action {
  width: 100% !important;
}

.shared-daily-context__action .pi {
  font-size: 11px;
  line-height: 1;
}

.shared-daily-context__action.ui-button.ui-button--soft.p-button {
  border-color: transparent;
  background: var(--color-primary-100);
  color: var(--color-primary-700);
  font-weight: 500;
}

.shared-daily-context__action.ui-button.ui-button--soft.p-button:not(:disabled):is(:hover, :active) {
  border-color: transparent;
  background: var(--color-primary-200);
  color: var(--color-primary-700);
}

@media (hover: hover) and (pointer: fine) {
  .shared-daily-context__action .pi {
    transition: transform 0.16s ease;
  }

  .shared-daily-context__action:hover .pi {
    transform: translateX(2px);
  }
}

.shared-daily-result__result-card {
  min-width: 0;
  overflow: clip;
  display: flex;
  flex-direction: column;
  gap: 8px;
  padding: 8px 9px;
  border: 1px solid var(--color-gray-200);
  border-radius: 8px;
  background: white;
}

.shared-daily-result__divider {
  height: 1px;
  background: var(--color-gray-100);
}

.shared-daily-state {
  flex: 1;
  min-height: 320px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 10px;
  text-align: center;
  padding: 24px 16px;
}

.shared-daily-state__icon {
  color: var(--color-gray-500);
  font-size: 34px;
}

.shared-daily-state__title,
.shared-daily-state__text {
  margin: 0;
}

.shared-daily-state__title {
  color: var(--color-gray-900);
  font-size: 20px;
  font-weight: 500;
}

.shared-daily-state__text {
  max-width: 420px;
  color: var(--color-gray-600);
  font-size: 15px;
  line-height: 1.45;
}

@media (min-width: 768px) {
  .shared-daily-result {
    gap: 12px;
  }

  .shared-daily-result__summary {
    gap: 10px;
  }

  .shared-daily-result__result-card {
    gap: 10px;
    padding: 10px 12px;
  }

  .shared-daily-context__action {
    width: auto !important;
    min-width: 140px;
  }

  .shared-daily-context-skeleton__action {
    width: 200px !important;
  }

  .shared-daily-context,
  :deep(.game-board-skeleton__context) {
    flex-direction: row;
    align-items: center;
    justify-content: space-between;
    gap: 16px;
  }

  .shared-daily-context__message,
  .shared-daily-context-skeleton__message {
    flex: 1;
  }

  .shared-daily-context__actions,
  .shared-daily-context-skeleton__actions {
    flex: 0 0 auto;
  }
}

@media (width < 1024px) {
  .shared-daily-result {
    flex: 1;
    min-height: 0;
    overflow-y: auto;
    overflow-x: hidden;
    padding: 8px max(16px, env(safe-area-inset-right)) max(12px, var(--app-visual-viewport-safe-bottom)) max(16px, env(safe-area-inset-left));
  }

  .shared-daily-result__summary,
  .shared-daily-context,
  .shared-daily-result>.shared-guesses {
    flex-shrink: 0;
    width: 100%;
    max-width: calc(var(--container-sm) - 32px);
    margin-inline: auto;
  }
}
</style>
