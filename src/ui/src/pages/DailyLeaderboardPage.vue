<script setup lang="ts">
import Tab from "primevue/tab";
import TabList from "primevue/tablist";
import Tabs from "primevue/tabs";
import { computed, onBeforeUnmount, ref } from "vue";
import {
  getDailyGameCurrentStreakTop,
  getDailyGameLongestStreakTop,
} from "../features/games/api/gameApi";
import type {
  DailyGameStreakTop,
  DailyGameStreakTopCurrentPlayerEntry,
  DailyGameStreakTopEntry,
} from "../features/games/model/game";
import DailyStreakBadge from "../features/games/components/DailyStreakBadge.vue";
import { getDailyStreakTier } from "../features/games/lib/dailyStreakPresentation";
import LeaderboardShell from "../features/games/leaderboard/LeaderboardShell.vue";
import LeaderboardTable from "../features/games/leaderboard/LeaderboardTable.vue";
import {
  createLeaderboardRows,
  type LeaderboardPlayerRow,
} from "../features/games/leaderboard/leaderboardRows";
import { isApiRequestCanceled } from "../shared/api/apiError";
import { useHandoffDelayedLoadingState } from "../shared/composables/useSkeletonHandoff";

type LeaderboardMode = "current" | "longest";

const topN = 10;
const mode = ref<LeaderboardMode>("current");
const tops = ref<Record<LeaderboardMode, DailyGameStreakTop | null>>({
  current: null,
  longest: null,
});
const loading = ref(false);
const loadFailed = ref(false);
const loadingState = useHandoffDelayedLoadingState(loading);
let requestId = 0;
let controller: AbortController | null = null;

const modeTitle = computed(() =>
  mode.value === "current" ? "Активная серия" : "Лучшая серия",
);

const top = computed(() =>
  tops.value[mode.value],
);

const initialSkeletonVisible = computed(() =>
  loadingState.visible && !top.value && !loadFailed.value,
);

const refreshSkeletonVisible = computed(() =>
  loadingState.visible && !!top.value,
);

const displayedRows = computed(() => {
  const displayedTop = top.value;
  if (!displayedTop) return null;

  const topRows: LeaderboardPlayerRow[] = displayedTop.top.map((entry) => {
    const current = isSameEntry(entry, displayedTop.currentPlayerTopInfo);
    return {
      id: `${mode.value}-${entry.place}-${entry.playerName}-${entry.streak}`,
      kind: "player",
      place: entry.place,
      playerName: entry.playerName,
      metric: entry.streak,
      current,
      placeEmphasized: current,
      playerNameEmphasized: current,
    };
  });

  const currentPlayer = displayedTop.currentPlayerTopInfo;
  return createLeaderboardRows(
    topRows,
    {
      id: "current-player",
      kind: "player",
      place: currentPlayer.place,
      playerName: currentPlayer.playerName,
      metric: currentPlayer.streak,
      current: true,
      placeEmphasized: currentPlayer.place !== null,
      playerNameEmphasized: true,
      emptyPlaceInfo: "Начните серию, чтобы попасть в таблицу лидеров",
    },
    topN,
  );
});

void load();
onBeforeUnmount(() => controller?.abort());

async function load() {
  controller?.abort();
  const abortController = new AbortController();
  controller = abortController;
  const currentRequest = ++requestId;
  loading.value = true;

  try {
    const [current, longest] = await Promise.all([
      getDailyGameCurrentStreakTop(topN, { signal: abortController.signal }),
      getDailyGameLongestStreakTop(topN, { signal: abortController.signal }),
    ]);

    if (currentRequest !== requestId) return;
    loadFailed.value = false;
    tops.value = { current, longest };
  } catch (error) {
    if (isApiRequestCanceled(error) || currentRequest !== requestId) return;
    loadFailed.value = true;
    tops.value = { current: null, longest: null };
  } finally {
    if (currentRequest === requestId) loading.value = false;
    if (controller === abortController) controller = null;
  }
}

function setMode(nextMode: string | number) {
  if (nextMode !== "current" && nextMode !== "longest") return;
  if (mode.value === nextMode) return;
  mode.value = nextMode;
}

function isSameEntry(
  entry: DailyGameStreakTopEntry,
  other: DailyGameStreakTopCurrentPlayerEntry,
) {
  return (
    entry.place === other.place &&
    entry.playerName === other.playerName &&
    entry.streak === other.streak
  );
}

</script>

<template>
  <LeaderboardShell title="Лучшие игроки слова дня" title-id="daily-leaderboard-title" dense-on-short-viewport>
    <template #header-extra>
      <Tabs class="daily-leaderboard__tabs" :value="mode" :show-navigators="false" :aria-busy="loading"
        @update:value="setMode">
        <TabList aria-label="Тип серии">
          <Tab value="current" :disabled="loading">Активная серия</Tab>
          <Tab value="longest" :disabled="loading">Лучшая серия</Tab>
        </TabList>
      </Tabs>
    </template>

    <template #info>
      <p>
        Лучшим считается игрок с наиболее длинной серией отгаданных слов дня.
      </p>
      <p>
        Активная серия — сколько дней подряд вы уже отгадываете слово в день его выхода без пропусков.
      </p>
      <p>
        Лучшая серия — самый длинный такой отрезок за всё время, даже если сейчас он уже прерван.
      </p>
    </template>

    <LeaderboardTable :rows="displayedRows" metric-label="Серия" metric-column-width="82px"
      narrow-metric-column-width="64px" metric-skeleton-width="28px" :skeleton-visible="initialSkeletonVisible"
      :failed="loadFailed" :retry-loading="loading" :refresh-skeleton-visible="refreshSkeletonVisible"
      :transition-key="mode" :table-label="modeTitle" :decorate-top-places="false" dense-on-short-viewport
      @retry="load">
      <template #metric="{ value }">
        <DailyStreakBadge :value="value" :tier="getDailyStreakTier(Number(value))" :label="`Серия ${value}`" />
      </template>
    </LeaderboardTable>
  </LeaderboardShell>
</template>

<style scoped>
.daily-leaderboard__tabs {
  width: 100%;
}

.daily-leaderboard__tabs :deep(.p-tablist-tab-list) {
  width: 100%;
}

.daily-leaderboard__tabs :deep(.p-tab) {
  flex: 1 1 0;
  min-width: 0;
  padding: clamp(9px, 1.5vw, 14px) clamp(10px, 2vw, 18px);
  font-size: var(--leaderboard-tab-font-size);
  font-weight: var(--leaderboard-emphasis-font-weight);
}

</style>
