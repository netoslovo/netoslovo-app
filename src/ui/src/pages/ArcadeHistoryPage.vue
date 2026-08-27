<script setup lang="ts">
import { computed } from "vue";
import { getArcadeHistory } from "../features/games/api/gameApi";
import ArcadeGameCard from "../features/games/history/ArcadeGameCard.vue";
import GroupedHistory from "../features/games/history/GroupedHistory.vue";
import { usePagedGameHistory } from "../features/games/history/usePagedGameHistory";
import type { ArcadeGame } from "../features/games/model/game";

type ArcadeGameGroup = {
  key: string;
  title: string;
  items: ArcadeGame[];
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
  (skip, take, signal) => getArcadeHistory(skip, take, { signal }),
  "История случайных игр",
  "Не удалось загрузить историю случайных игр",
);

const gameGroups = computed(() => groupGamesByLocalDay(games.value));

function groupGamesByLocalDay(arcadeGames: ArcadeGame[]): ArcadeGameGroup[] {
  const groups: ArcadeGameGroup[] = [];

  for (const arcadeGame of arcadeGames) {
    const date = new Date(arcadeGame.createdAt);
    const key = formatLocalDateKey(date);
    const lastGroup = groups[groups.length - 1];

    if (lastGroup?.key === key) {
      lastGroup.items.push(arcadeGame);
      continue;
    }

    groups.push({
      key,
      title: formatDayTitle(date),
      items: [arcadeGame],
    });
  }

  return groups;
}

function formatLocalDateKey(date: Date) {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");

  return `${year}-${month}-${day}`;
}

function formatDayTitle(date: Date) {
  return new Intl.DateTimeFormat("ru-RU", {
    day: "numeric",
    month: "long",
    year: "numeric",
  }).format(date);
}

function getGameKey(game: ArcadeGame) {
  return game.id;
}
</script>

<template>
  <GroupedHistory
    title-id="arcade-history-title"
    title="История случайных слов"
    :groups="gameGroups"
    :get-item-key="getGameKey"
    :has-more="hasMore"
    :loading="loading"
    :loaded-once="loadedOnce"
    :load-failed="loadFailed"
    error-message="Не удалось загрузить историю случайных игр."
    empty-message="У вас пока нет игр со случайным словом"
    :set-load-anchor="setLoadAnchor"
    @load-more="loadMore"
    @retry="retryLoad"
  >
    <template #card="{ item: arcadeGame }">
      <ArcadeGameCard
        :arcade-game="arcadeGame"
        :to="{
          name: 'arcade-game',
          params: { id: arcadeGame.id },
          query: { from: 'arcade-history' },
        }"
      />
    </template>
  </GroupedHistory>
</template>
