<script setup lang="ts">
import { computed } from "vue";
import { getDailyHistory } from "../features/games/api/gameApi";
import DailyHistoryCard from "../features/games/history/DailyHistoryCard.vue";
import GroupedHistory from "../features/games/history/GroupedHistory.vue";
import { usePagedGameHistory } from "../features/games/history/usePagedGameHistory";
import type { DailyGame } from "../features/games/model/game";

type DailyGameGroup = {
  key: string;
  title: string;
  items: DailyGame[];
};

const {
  games,
  hasMore,
  loading,
  loadedOnce,
  loadFailed,
  setLoadAnchor,
  loadMore,
  retryLoad,
} = usePagedGameHistory(
  (skip, take, signal) => getDailyHistory(skip, take, { signal }),
  "История слов дня",
  "Не удалось загрузить слова дня",
);

const gameGroups = computed(() => groupGamesByMonth(games.value));

function groupGamesByMonth(dailyGames: DailyGame[]): DailyGameGroup[] {
  const groups: DailyGameGroup[] = [];

  for (const dailyGame of dailyGames) {
    if (dailyGame.isToday) {
      groups.push({
        key: `today-${dailyGame.day}`,
        title: "Сегодня",
        items: [dailyGame],
      });
      continue;
    }

    const key = dailyGame.day.slice(0, 7);
    const lastGroup = groups[groups.length - 1];

    if (lastGroup?.key === key) {
      lastGroup.items.push(dailyGame);
      continue;
    }

    groups.push({
      key,
      title: formatMonthTitle(key),
      items: [dailyGame],
    });
  }

  return groups;
}

function formatMonthTitle(monthKey: string) {
  return new Intl.DateTimeFormat("ru-RU", {
    month: "long",
    year: "numeric",
  }).format(new Date(`${monthKey}-01T00:00:00`));
}

function getGameKey(game: DailyGame) {
  return game.day;
}
</script>

<template>
  <GroupedHistory
    title-id="daily-history-title"
    title="История слов дня"
    :groups="gameGroups"
    :get-item-key="getGameKey"
    :has-more="hasMore"
    :loading="loading"
    :loaded-once="loadedOnce"
    :load-failed="loadFailed"
    error-message="Не удалось загрузить историю слов дня."
    empty-message="Слова дня пока недоступны."
    :set-load-anchor="setLoadAnchor"
    @load-more="loadMore"
    @retry="retryLoad"
  >
    <template #card="{ item: dailyGame }">
      <DailyHistoryCard :daily-game="dailyGame" :to="{ name: 'daily', params: { day: dailyGame.day } }" />
    </template>
  </GroupedHistory>
</template>
