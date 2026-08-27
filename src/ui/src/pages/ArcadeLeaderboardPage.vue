<script setup lang="ts">
import Button from "primevue/button";
import Tab from "primevue/tab";
import TabList from "primevue/tablist";
import Tabs from "primevue/tabs";
import { computed, nextTick, onBeforeUnmount, ref, watch } from "vue";
import {
  getArcadeGameTopPlayers,
  getDifficulties,
} from "../features/games/api/gameApi";
import type {
  ArcadeGameTopPlayers,
  ArcadeGameTopPlayersCurrentPlayerEntry,
  ArcadeGameTopPlayersEntry,
  Difficulty,
} from "../features/games/model/game";
import { isApiRequestCanceled } from "../shared/api/apiError";
import { useHandoffDelayedLoadingState } from "../shared/composables/useSkeletonHandoff";
import UiButton from "../shared/ui/UiButton.vue";
import UiSkeleton from "../shared/ui/UiSkeleton.vue";
import UiSkeletonHandoff from "../shared/ui/UiSkeletonHandoff.vue";
import LeaderboardShell from "../features/games/leaderboard/LeaderboardShell.vue";
import LeaderboardTable from "../features/games/leaderboard/LeaderboardTable.vue";
import {
  createLeaderboardRows,
  type LeaderboardPlayerRow,
} from "../features/games/leaderboard/leaderboardRows";
import { useLeaderboardDataTransition } from "../features/games/leaderboard/useLeaderboardDataTransition";
import { formatDurationInMinutes } from "../features/games/lib/formatDuration";

const topN = 10;
const difficulties = ref<Difficulty[]>([]);
const activeDifficultyCode = ref<string | null>(null);
const displayedDifficultyCode = ref<string | null>(null);
const loadingDifficulties = ref(true);
const difficultiesFailed = ref(false);
const topByDifficulty = ref<Record<string, DifficultyTopState>>({});
const initialLoadPending = ref(true);
const tabsElement = ref<HTMLElement | null>(null);
let loadGeneration = 0;
let loadController: AbortController | null = null;
const leaderboardMetrics = [
  { key: "games", label: "Отгадано" },
  { key: "score", label: "Ср. счёт" },
  { key: "duration", label: "Ср. время" },
] as const;
const activeMetricIndex = ref(0);

type DifficultyTopState = {
  data: ArcadeGameTopPlayers | null;
  loading: boolean;
  failed: boolean;
};

const activeTopState = computed(() =>
  activeDifficultyCode.value ? topByDifficulty.value[activeDifficultyCode.value] ?? null : null,
);

const activeTop = computed(() => activeTopState.value?.data ?? null);

const navigationLoading = computed(
  () => loadingDifficulties.value || !!activeTopState.value?.loading,
);

const displayedTop = computed(() =>
  displayedDifficultyCode.value
    ? topByDifficulty.value[displayedDifficultyCode.value]?.data ?? null
    : null,
);

const initialLoadingState = useHandoffDelayedLoadingState(initialLoadPending);
const subsequentTableLoadingState = useHandoffDelayedLoadingState(
  () =>
    !initialLoadPending.value &&
    !!activeTopState.value?.loading &&
    !activeTop.value,
);

const difficultySkeletonVisible = computed(
  () => loadingDifficulties.value && initialLoadingState.visible,
);

const tableSkeletonVisible = computed(
  () =>
    (initialLoadPending.value && initialLoadingState.visible) ||
    subsequentTableLoadingState.visible,
);

const dataTransition = useLeaderboardDataTransition(tableSkeletonVisible);

const refreshLoadingState = useHandoffDelayedLoadingState(
  () => !!activeTopState.value?.loading,
);

const refreshSkeletonVisible = computed(() =>
  refreshLoadingState.visible &&
  displayedDifficultyCode.value === activeDifficultyCode.value &&
  !!displayedTop.value,
);

const activeMetric = computed(() => leaderboardMetrics[activeMetricIndex.value]);
const canShowPreviousMetric = computed(() => activeMetricIndex.value > 0);
const canShowNextMetric = computed(
  () => activeMetricIndex.value < leaderboardMetrics.length - 1,
);

const displayedRows = computed(() => {
  const top = displayedTop.value;
  if (!top) return null;

  const topRows: LeaderboardPlayerRow[] = top.top.map((entry, index) => {
    const current = isSameEntry(entry, top.playerTopInfo);
    return {
      id: `top-${index}-${entry.place}-${entry.playerName}`,
      kind: "player",
      place: entry.place,
      playerName: entry.playerName,
      metric: formatMetricValue(entry),
      current,
      placeEmphasized: current,
      playerNameEmphasized: shouldEmphasizePlayerName(entry, current),
    };
  });

  const currentPlayer = top.playerTopInfo;
  return createLeaderboardRows(
    topRows,
    {
      id: "current-player",
      kind: "player",
      place: currentPlayer.place,
      playerName: currentPlayer.playerName,
      metric: formatMetricValue(currentPlayer),
      current: true,
      placeEmphasized: currentPlayer.place !== null,
      playerNameEmphasized: shouldEmphasizePlayerName(currentPlayer, true),
      emptyPlaceInfo: "Отгадайте своё первое слово, чтобы попасть в таблицу лидеров",
    },
    topN,
  );
});

watch(tableSkeletonVisible, (visible) => {
  if (visible && !activeTop.value) displayedDifficultyCode.value = null;
});

void loadDifficulties();

onBeforeUnmount(() => {
  loadGeneration += 1;
  loadController?.abort();
  loadController = null;
});

async function loadDifficulties() {
  loadController?.abort();
  const controller = new AbortController();
  loadController = controller;
  const generation = ++loadGeneration;

  loadingDifficulties.value = true;
  difficultiesFailed.value = false;
  displayedDifficultyCode.value = null;
  initialLoadPending.value = true;
  topByDifficulty.value = {};
  dataTransition.startLoad();

  try {
    const loadedDifficulties = await getDifficulties({ signal: controller.signal });
    if (!isCurrentLoad(generation, controller.signal)) return;

    difficulties.value = loadedDifficulties;
    loadingDifficulties.value = false;

    const firstDifficulty = loadedDifficulties[0];
    if (firstDifficulty) {
      activeDifficultyCode.value = firstDifficulty.code;
      await loadTop(firstDifficulty.code, generation);
      if (!isCurrentLoad(generation, controller.signal)) return;

      if (topByDifficulty.value[firstDifficulty.code]?.data) {
        dataTransition.revealData(() => {
          displayedDifficultyCode.value = firstDifficulty.code;
        });
      }
    }
  } catch (error) {
    if (isApiRequestCanceled(error) || !isCurrentLoad(generation, controller.signal)) return;
    difficultiesFailed.value = true;
  } finally {
    if (isCurrentLoad(generation, controller.signal)) {
      loadingDifficulties.value = false;
      initialLoadPending.value = false;
    }
  }
}

function selectDifficulty(difficultyCode: string | number) {
  if (typeof difficultyCode !== "string") return;
  if (activeDifficultyCode.value === difficultyCode) return;

  const state = topByDifficulty.value[difficultyCode];
  activeDifficultyCode.value = difficultyCode;
  activeMetricIndex.value = 0;
  scrollActiveTabIntoView();

  if (state?.data) {
    displayedDifficultyCode.value = difficultyCode;
    return;
  }

  dataTransition.startLoad();
  void loadTop(difficultyCode);
}

function scrollActiveTabIntoView() {
  void nextTick(() => {
    const activeTab = tabsElement.value?.querySelector<HTMLElement>(
      '[data-pc-name="tab"][data-p-active="true"]',
    );

    activeTab?.scrollIntoView({
      behavior: window.matchMedia("(prefers-reduced-motion: reduce)").matches
        ? "auto"
        : "smooth",
      block: "nearest",
      inline: "center",
    });
  });
}

async function loadTop(difficultyCode: string, generation = loadGeneration) {
  const signal = loadController?.signal;
  if (!signal || !isCurrentLoad(generation, signal)) return;

  const previousState = topByDifficulty.value[difficultyCode];
  if (previousState?.loading) return;

  if (difficultyCode === activeDifficultyCode.value && !previousState?.data) {
    dataTransition.startLoad();
  }

  topByDifficulty.value = {
    ...topByDifficulty.value,
    [difficultyCode]: {
      data: previousState?.data ?? null,
      loading: true,
      failed: false,
    },
  };

  try {
    const data = await getArcadeGameTopPlayers(difficultyCode, topN, {
      signal,
    });
    if (!isCurrentLoad(generation, signal)) return;

    topByDifficulty.value = {
      ...topByDifficulty.value,
      [difficultyCode]: { data, loading: false, failed: false },
    };

    if (difficultyCode === activeDifficultyCode.value && !initialLoadPending.value) {
      dataTransition.revealData(() => {
        displayedDifficultyCode.value = difficultyCode;
      });
    }
  } catch (error) {
    if (isApiRequestCanceled(error) || !isCurrentLoad(generation, signal)) return;

    topByDifficulty.value = {
      ...topByDifficulty.value,
      [difficultyCode]: {
        data: previousState?.data ?? null,
        loading: false,
        failed: true,
      },
    };

    if (difficultyCode === activeDifficultyCode.value && !initialLoadPending.value) {
      displayedDifficultyCode.value = null;
    }
  }
}

function isCurrentLoad(generation: number, signal: AbortSignal) {
  return generation === loadGeneration && !signal.aborted;
}

function isSameEntry(
  entry: ArcadeGameTopPlayersEntry,
  other: ArcadeGameTopPlayersCurrentPlayerEntry,
) {
  return (
    entry.place === other.place &&
    entry.playerName === other.playerName &&
    entry.guessedGames === other.guessedGames &&
    entry.averageScore === other.averageScore &&
    entry.averageDuration === other.averageDuration
  );
}

function shouldEmphasizePlayerName(
  entry: ArcadeGameTopPlayersEntry | ArcadeGameTopPlayersCurrentPlayerEntry,
  current: boolean,
) {
  return entry.place !== null && (entry.place <= 3 || current);
}

function moveMetric(offset: number) {
  activeMetricIndex.value = Math.min(
    leaderboardMetrics.length - 1,
    Math.max(0, activeMetricIndex.value + offset),
  );
}

function formatMetricValue(
  entry: ArcadeGameTopPlayersEntry | ArcadeGameTopPlayersCurrentPlayerEntry,
) {
  if (activeMetric.value.key === "games") return entry.guessedGames;
  if (activeMetric.value.key === "score") {
    return entry.averageScore === null ? "—" : formatAverageScore(entry.averageScore);
  }

  return entry.averageDuration ? formatLeaderboardDuration(entry.averageDuration) : "—";
}

function metricSkeletonWidth() {
  if (activeMetric.value.key === "games") return "24px";
  if (activeMetric.value.key === "score") return "32px";
  return "70px";
}

function retryActiveTop() {
  if (activeDifficultyCode.value) void loadTop(activeDifficultyCode.value);
}

function formatLeaderboardDuration(value: string) {
  return formatDurationInMinutes(value, 1000);
}

function formatAverageScore(value: number) {
  return value.toFixed(1);
}
</script>

<template>
  <LeaderboardShell title="Лучшие игроки случайных слов" title-id="arcade-leaderboard-title">
    <template #info>
      <p>
        В каждой сложности лучшим считается тот игрок, который отгадал больше слов.
      </p>
      <p>
        При равенстве отгаданных слов выше будет игрок с меньшим средним счётом, затем — с меньшим средним временем.
      </p>
    </template>

    <UiSkeletonHandoff class="arcade-leaderboard__tabs-handoff" :skeleton-visible="difficultySkeletonVisible"
      :content-visible="!difficultySkeletonVisible">
      <template #skeleton>
        <div class="arcade-leaderboard__tabs-skeleton" aria-hidden="true">
          <UiSkeleton width="100%" height="42px" border-radius="8px" />
        </div>
      </template>

      <div v-if="difficultiesFailed" class="arcade-leaderboard__state">
        <p>Не удалось загрузить сложности.</p>
        <UiButton size="md" variant="outlined" :loading="loadingDifficulties" @click="loadDifficulties">
          Повторить
        </UiButton>
      </div>

      <div v-else-if="difficulties.length === 0 && !loadingDifficulties" class="arcade-leaderboard__state">
        <p>Сложности пока недоступны.</p>
      </div>

      <div v-else-if="difficulties.length > 0" ref="tabsElement">
        <Tabs class="arcade-leaderboard__tabs" :value="activeDifficultyCode ?? ''" scrollable :show-navigators="true"
          :aria-busy="navigationLoading" @update:value="selectDifficulty">
          <TabList aria-label="Сложность">
            <Tab v-for="difficulty in difficulties" :key="difficulty.code" :value="difficulty.code"
              :disabled="navigationLoading">
              {{ difficulty.name }}
            </Tab>
          </TabList>
        </Tabs>
      </div>
    </UiSkeletonHandoff>

    <LeaderboardTable :rows="displayedRows" :metric-label="activeMetric.label" metric-column-width="160px"
      narrow-metric-column-width="120px" :metric-skeleton-width="metricSkeletonWidth()"
      :skeleton-visible="tableSkeletonVisible" :failed="activeTopState?.failed ?? false"
      :retry-loading="activeTopState?.loading ?? false" :refresh-skeleton-visible="refreshSkeletonVisible"
      :transition-enabled="dataTransition.enabled.value" :transition-key="displayedDifficultyCode ?? ''"
      metric-navigation compact-place-player-gap @retry="retryActiveTop">
      <template #metric-skeleton-after>
        <Button icon="pi pi-chevron-right" severity="secondary" text rounded size="small"
          aria-label="Следующий показатель" disabled tabindex="-1" />
      </template>
      <template #metric-before>
        <Button v-if="canShowPreviousMetric" icon="pi pi-chevron-left" severity="secondary" text rounded size="small"
          aria-label="Предыдущий показатель" @click.stop="moveMetric(-1)" />
      </template>
      <template #metric-after>
        <Button v-if="canShowNextMetric" icon="pi pi-chevron-right" severity="secondary" text rounded size="small"
          aria-label="Следующий показатель" @click.stop="moveMetric(1)" />
      </template>
    </LeaderboardTable>
  </LeaderboardShell>
</template>

<style scoped>
.arcade-leaderboard__state p {
  margin: 0;
}

.arcade-leaderboard__tabs {
  width: 100%;
}

.arcade-leaderboard__tabs :deep(.p-tablist-tab-list) {
  min-width: 100%;
}

.arcade-leaderboard__tabs :deep(.p-tab) {
  flex: 1 0 max-content;
  padding: clamp(9px, 1.5vw, 14px) clamp(10px, 2vw, 18px);
  font-size: var(--leaderboard-tab-font-size);
  font-weight: var(--leaderboard-emphasis-font-weight);
}

.arcade-leaderboard__state {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.arcade-leaderboard__state {
  min-height: 120px;
  align-items: center;
  justify-content: center;
  color: var(--p-text-muted-color);
  font-size: 14px;
  line-height: 1.4;
  text-align: center;
}

.arcade-leaderboard__tabs-handoff {
  flex: none;
}
</style>
