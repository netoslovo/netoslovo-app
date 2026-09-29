<script setup lang="ts">
import { computed } from "vue";
import { RouterLink, type RouteLocationRaw } from "vue-router";
import type { DailyGame } from "../model/game";
import {
  formatDailyGameDate,
  getDailyGameStatus,
  wasGuessedOnReleaseDay,
} from "../lib/dailyGamePresentation";
import DailyOnTimeBadge from "../components/DailyOnTimeBadge.vue";

const props = defineProps<{
  dailyGame: DailyGame;
  to: RouteLocationRaw;
}>();

const config = computed(() => getDailyGameStatus(props.dailyGame));
const date = computed(() => formatDailyGameDate(props.dailyGame.day));
const showOnTimeBadge = computed(() => wasGuessedOnReleaseDay(props.dailyGame));

</script>

<template>
  <article class="daily-compact-tile" :class="`daily-compact-tile--status-${config.status}`">
    <DailyOnTimeBadge v-if="showOnTimeBadge" position="right" />

    <RouterLink class="daily-compact-tile__action" :to="to"
      :aria-label="`${config.action}: ${date}. ${config.label}`" :title="`${date}: ${config.label}`">
        <span class="daily-compact-tile__date">{{ date }}</span>
        <span class="daily-compact-tile__status" aria-hidden="true">
          <i :class="config.icon"></i>
        </span>
    </RouterLink>
  </article>
</template>

<style scoped>
.daily-compact-tile {
  position: relative;
  min-width: 0;
  width: 100%;
  aspect-ratio: 1;
  container-type: inline-size;
  border: 1px solid var(--color-gray-200);
  border-radius: 8px;
  background: rgba(255, 255, 255, 0.92);
}

.daily-compact-tile--status-active,
.daily-compact-tile--status-notStarted,
.daily-compact-tile--status-cancelled {
  border-color: var(--color-gray-300);
}

.daily-compact-tile--status-active,
.daily-compact-tile--status-guessed {
  background: white;
}

.daily-compact-tile--status-notStarted,
.daily-compact-tile--status-cancelled {
  background: var(--color-gray-50);
}

.daily-compact-tile--status-guessed {
  border-color: var(--color-primary-500);
}

.daily-compact-tile--status-surrendered {
  border-color: var(--color-red-600);
}

.daily-compact-tile__action {
  width: 100%;
  height: 100%;
  border: 0;
  border-radius: inherit;
  padding: 4px 2px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 2px;
  background: transparent;
  color: var(--color-gray-700);
  cursor: pointer;
  text-decoration: none;
}

.daily-compact-tile__action:focus-visible {
  outline: none;
  box-shadow: var(--focus-ring-primary);
  background: var(--color-gray-100);
}

@media (hover: hover) and (pointer: fine) {
  .daily-compact-tile__action:hover {
    background: var(--color-gray-50);
  }

  .daily-compact-tile--status-notStarted .daily-compact-tile__action:hover,
  .daily-compact-tile--status-cancelled .daily-compact-tile__action:hover {
    background: var(--color-gray-100);
  }
}

.daily-compact-tile__date {
  max-width: 100%;
  color: var(--color-gray-700);
  font-size: 11px;
  font-weight: 500;
  line-height: 1.1;
  text-align: center;
  overflow: hidden;
  text-overflow: ellipsis;
  text-transform: lowercase;
  white-space: nowrap;
}

.daily-compact-tile__status {
  width: 20px;
  height: 20px;
  flex: 0 0 20px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.daily-compact-tile__status .pi {
  color: var(--p-surface-500);
  font-size: 16px;
}

.daily-compact-tile--status-guessed .daily-compact-tile__status .pi {
  color: var(--color-primary-600);
}

.daily-compact-tile--status-surrendered .daily-compact-tile__status .pi {
  color: var(--color-red-600);
}

@container (min-width: 56px) {
  .daily-compact-tile__action {
    padding: 5px 2px;
    gap: 3px;
  }

  .daily-compact-tile__date {
    font-size: 12px;
  }

  .daily-compact-tile__status {
    width: 24px;
    height: 24px;
    flex-basis: 24px;
  }

  .daily-compact-tile__status .pi {
    font-size: 17px;
  }
}

@container (min-width: 62px) {
  .daily-compact-tile__action {
    padding: 7px 3px 6px;
    gap: 4px;
  }

  .daily-compact-tile__status {
    width: 28px;
    height: 28px;
    flex-basis: 28px;
  }

  .daily-compact-tile__status .pi {
    font-size: 18px;
  }
}
</style>
