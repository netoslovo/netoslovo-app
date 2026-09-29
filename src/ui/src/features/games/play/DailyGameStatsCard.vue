<script setup lang="ts">
import Popover from "primevue/popover";
import { computed, ref } from "vue";
import {
  useInfoPopover,
  useInfoPopoverSemantics,
} from "../../../shared/composables/useInfoPopover";
import UiSkeleton from "../../../shared/ui/UiSkeleton.vue";
import type { DailyGameResultStatsState } from "./useDailyGameResultStats";
import { formatDuration } from "../lib/formatDuration";
import type { GameState } from "../model/game";

type StatsRowKey = "score" | "attempts" | "duration";

type PopoverRef = {
  show: (event: Event, target: HTMLElement) => void;
  hide: () => void;
};

const props = withDefaults(
  defineProps<{
    resultState: Extract<GameState, "active" | "guessed" | "surrendered">;
    stats: DailyGameResultStatsState;
    embedded?: boolean;
    collapsible?: boolean;
    defaultOpen?: boolean;
    showHeader?: boolean;
    showRefresh?: boolean;
    refresh?: () => Promise<void>;
    playerName?: string | null;
  }>(),
  {
    embedded: false,
    collapsible: false,
    defaultOpen: true,
    showHeader: true,
    showRefresh: true,
  },
);

const statsOpen = ref(props.defaultOpen);
const needsPlayerStats = computed(() => props.resultState === "guessed");
const gameStats = computed(() =>
  props.stats.aggregate.status === "available" ? props.stats.aggregate.data : null,
);
const playerStats = computed(() =>
  props.stats.player.status === "available" ? props.stats.player.data : null,
);
const hasFailedStats = computed(() =>
  props.stats.aggregate.status === "failed" ||
  (needsPlayerStats.value && props.stats.player.status === "failed"),
);
const hasUnavailableStats = computed(() =>
  needsPlayerStats.value && props.stats.player.status === "delayed",
);
const aggregateStatsPending = computed(() =>
  ["idle", "loading"].includes(props.stats.aggregate.status),
);
const playerStatsPending = computed(() =>
  needsPlayerStats.value && ["idle", "loading"].includes(props.stats.player.status),
);
const statsLoading = computed(() =>
  props.stats.aggregate.status === "loading" || props.stats.player.status === "loading",
);
const statsStatusMessage = computed(() => {
  if (hasFailedStats.value) return "Не удалось загрузить статистику";
  if (hasUnavailableStats.value) return "Статистика пока недоступна";
  return null;
});
const statsPresentationPending = computed(() =>
  !statsStatusMessage.value && (aggregateStatsPending.value || playerStatsPending.value),
);

const rows = computed(() => {
  const aggregate = gameStats.value;
  const player = playerStats.value;

  return [
    {
      key: "score" as const,
      icon: "pi pi-star",
      label: "Счёт",
      personalValue: player ? player.score.toString() : null,
      comparison: player ? formatComparison(player.scoreBetterThanPercent) : null,
      benchmarkValue: aggregate?.medianScore.toString() ?? null,
    },
    {
      key: "attempts" as const,
      icon: "pi pi-list-check",
      label: "Попытки",
      personalValue: player ? player.attemptsCount.toString() : null,
      comparison: player ? formatComparison(player.attemptsCountBetterThanPercent) : null,
      benchmarkValue: aggregate?.medianAttempts.toString() ?? null,
    },
    {
      key: "duration" as const,
      icon: "pi pi-clock",
      label: "Время",
      personalValue: player ? formatDuration(player.duration) : null,
      comparison: player ? formatComparison(player.durationBetterThanPercent) : null,
      benchmarkValue: aggregate ? formatDuration(aggregate.medianDuration) : null,
    },
  ];
});

const statsDescription = computed(() =>
  props.resultState === "active"
    ? "Пока текущая игра активна, в статистике отображаются средние результаты других игроков. После завершения игры появятся ваш результат и его сравнение с другими игроками."
    : props.playerName
      ? `В статистике отображается результат игрока ${props.playerName}, его сравнение с остальными, а также средние результаты других игроков.`
      : "В статистике отображается ваш результат игры, его сравнение с остальными, а также средние результаты других игроков.",
);

const {
  triggerId: statsInfoTriggerId,
  panelId: statsInfoPanelId,
  popoverPt: statsInfoPopoverPt,
  setPopover: setStatsInfoPopover,
  visible: statsInfoPopoverVisible,
  show: showStatsInfo,
  hide: hideStatsInfo,
  onPointerDown: onStatsInfoPointerDown,
  onShow: onStatsInfoPopoverShow,
  onHide: onStatsInfoPopoverHide,
} = useInfoPopover();

const benchmarkInfoPopover = ref<PopoverRef | null>(null);
const openBenchmarkInfoKey = ref<StatsRowKey | null>(null);
const benchmarkInfoSemantics = useInfoPopoverSemantics();

function showBenchmarkInfo(key: StatsRowKey, event: Event) {
  openBenchmarkInfoKey.value = key;

  if (event.currentTarget instanceof HTMLElement) {
    benchmarkInfoPopover.value?.show(event, event.currentTarget);
  }
}

function hideBenchmarkInfo() {
  benchmarkInfoPopover.value?.hide();
}

function onBenchmarkInfoPointerDown(key: StatsRowKey, event: PointerEvent) {
  event.preventDefault();
  if (event.pointerType === "mouse") return;

  event.stopPropagation();
  if (openBenchmarkInfoKey.value === key) hideBenchmarkInfo();
  else showBenchmarkInfo(key, event);
}

function onBenchmarkInfoPopoverHide() {
  openBenchmarkInfoKey.value = null;
}

function getPersonalSkeletonWidth(key: StatsRowKey) {
  return key === "duration" ? "82px" : "32px";
}

function getBenchmarkSkeletonWidth(key: StatsRowKey) {
  return key === "duration" ? "82px" : "34px";
}

function formatComparison(percent: number) {
  return `лучше, чем у ${percent}% игроков`;
}

</script>

<template>
  <section class="daily-game-stats-card"
    :class="{ 'daily-game-stats-card--embedded': embedded, 'daily-game-stats-card--headerless': !showHeader }"
    aria-label="Статистика дня">
    <div v-if="showHeader" class="daily-game-stats-card__header">
      <button v-if="collapsible" class="daily-game-stats-card__toggle" type="button" :aria-expanded="statsOpen"
        aria-controls="daily-game-stats-content" aria-label="Показать или скрыть статистику"
        @click="statsOpen = !statsOpen">
        <span class="daily-game-stats-card__toggle-icon" aria-hidden="true">
          <i class="pi pi-chevron-down"></i>
        </span>
      </button>

      <div class="daily-game-stats-card__title-group">
        <span class="daily-game-stats-card__title">Статистика</span>
        <button v-if="!showRefresh || !refresh || (collapsible && !statsOpen)"
          class="daily-game-stats-card__header-info"
          :class="{ 'daily-game-stats-card__header-info--open': statsInfoPopoverVisible }" type="button"
          :id="statsInfoTriggerId" aria-label="О статистике дня" :aria-describedby="statsInfoPanelId"
          @mouseenter="showStatsInfo" @mouseleave="hideStatsInfo"
          @pointerdown="onStatsInfoPointerDown" @focus="showStatsInfo" @blur="hideStatsInfo" @click.stop>
          <i class="pi pi-info-circle" aria-hidden="true"></i>
        </button>
        <button v-if="(!collapsible || statsOpen) && showRefresh && refresh"
          class="daily-game-stats-card__header-refresh" type="button"
          :disabled="statsLoading" :aria-label="statsLoading ? 'Обновление статистики' : 'Обновить статистику'"
          :title="statsLoading ? 'Обновление статистики' : 'Обновить статистику'" @click.stop="refresh">
          <i class="pi pi-refresh" :class="{ 'pi-spin': statsLoading }" aria-hidden="true"></i>
        </button>
      </div>
    </div>

    <template v-if="!collapsible || statsOpen">
      <div id="daily-game-stats-content" class="daily-game-stats-card__body" :aria-busy="statsLoading">
        <div v-if="statsStatusMessage" class="daily-game-stats-card__overlay">
          <p>{{ statsStatusMessage }}</p>
        </div>

        <div class="daily-game-stats-card__rows" :class="{ 'daily-game-stats-card__rows--blocked': statsStatusMessage }"
          :aria-disabled="statsStatusMessage ? 'true' : undefined">
          <div v-for="row in rows" :key="row.key" class="daily-game-stats-card__row">
            <UiSkeleton v-if="statsPresentationPending"
              class="daily-game-stats-card__row-icon-skeleton"
              :width="'var(--daily-game-stats-card-row-icon-size)'"
              :height="'var(--daily-game-stats-card-row-icon-size)'" border-radius="8px" aria-hidden="true" />
            <span v-else class="daily-game-stats-card__row-icon" aria-hidden="true">
              <i :class="row.icon"></i>
            </span>

            <div class="daily-game-stats-card__row-content">
              <span class="daily-game-stats-card__metric daily-game-stats-card__metric--personal">
                <span class="daily-game-stats-card__metric-label">{{ row.label }}:</span>
                <template v-if="statsPresentationPending">
                  <UiSkeleton :width="getPersonalSkeletonWidth(row.key)" height="1em" />
                  <UiSkeleton width="142px" height="0.9em" />
                </template>
                <span v-else-if="statsStatusMessage" class="daily-game-stats-card__metric-missing">—</span>
                <template v-else-if="row.personalValue">
                  <strong class="daily-game-stats-card__metric-value">
                    {{ row.personalValue }}
                  </strong>
                </template>
                <span v-else-if="needsPlayerStats" class="daily-game-stats-card__metric-missing">—</span>
                <span v-else class="daily-game-stats-card__metric-missing">
                  Не отгадано
                </span>
                <span v-if="row.comparison" class="daily-game-stats-card__comparison">
                  ({{ row.comparison }})
                </span>
              </span>

              <span class="daily-game-stats-card__metric">
                <span class="daily-game-stats-card__metric-label">В среднем у других:</span>
                <span v-if="statsPresentationPending" class="daily-game-stats-card__benchmark-value">
                  <UiSkeleton :width="getBenchmarkSkeletonWidth(row.key)" height="1em" />
                </span>
                <span v-else-if="statsStatusMessage" class="daily-game-stats-card__metric-missing">—</span>
                <span v-else-if="row.benchmarkValue" class="daily-game-stats-card__benchmark-value">
                  <strong class="daily-game-stats-card__metric-value">{{ row.benchmarkValue }}</strong>
                </span>
                <span v-else-if="stats.aggregate.status === 'insufficient'"
                  class="daily-game-stats-card__metric-missing daily-game-stats-card__missing-with-info">
                  <span>Мало данных</span>
                  <button class="daily-game-stats-card__benchmark-info"
                    :class="{ 'daily-game-stats-card__benchmark-info--open': openBenchmarkInfoKey === row.key }"
                    :id="`${benchmarkInfoSemantics.triggerId}-${row.key}`" type="button"
                    aria-label="Почему результат других игроков пока не показан"
                    :aria-describedby="benchmarkInfoSemantics.panelId"
                    @mouseenter="showBenchmarkInfo(row.key, $event)" @mouseleave="hideBenchmarkInfo"
                    @pointerdown="onBenchmarkInfoPointerDown(row.key, $event)"
                    @focus="showBenchmarkInfo(row.key, $event)" @blur="hideBenchmarkInfo" @click.stop>
                    <i class="pi pi-question-circle" aria-hidden="true"></i>
                  </button>
                </span>
                <span v-else class="daily-game-stats-card__metric-missing">—</span>
              </span>
            </div>
          </div>
        </div>
      </div>

    </template>

    <Popover v-if="showHeader" :ref="setStatsInfoPopover" :pt="statsInfoPopoverPt"
      class="daily-game-stats-card__popover info-popover" @show="onStatsInfoPopoverShow" @hide="onStatsInfoPopoverHide">
      <p class="daily-game-stats-card__popover-text">
        {{ statsDescription }}
      </p>
    </Popover>

    <Popover ref="benchmarkInfoPopover" :pt="benchmarkInfoSemantics.popoverPt"
      class="daily-game-stats-card__popover info-popover" @hide="onBenchmarkInfoPopoverHide">
      <p class="daily-game-stats-card__popover-text">
        Пока слово отгадало мало игроков. Когда данных станет больше, здесь появится средний результат других игроков.
      </p>
    </Popover>

  </section>
</template>

<style scoped>
.daily-game-stats-card {
  --daily-game-stats-card-padding-block: 7px;
  --daily-game-stats-card-padding-inline-start: 10px;
  --daily-game-stats-card-padding-inline-end: 9px;
  --daily-game-stats-card-title-font-size: 17px;
  --daily-game-stats-card-row-font-size: 14px;
  --daily-game-stats-card-value-font-size: 14px;
  --daily-game-stats-card-row-icon-size: 30px;
  --daily-game-stats-card-info-size: 26px;
  --daily-game-stats-card-info-icon-size: 14px;
  width: 100%;
  display: flex;
  flex-direction: column;
  gap: 7px;
  padding:
    var(--daily-game-stats-card-padding-block) var(--daily-game-stats-card-padding-inline-end) var(--daily-game-stats-card-padding-block) var(--daily-game-stats-card-padding-inline-start);
  border: 1px solid var(--color-gray-200);
  border-radius: 8px;
  background: white;
}

.daily-game-stats-card--embedded {
  padding: 0;
  border: 0;
  border-radius: 0;
  background: transparent;
}

.daily-game-stats-card--headerless {
  gap: 0;
}

.daily-game-stats-card__header {
  position: relative;
  min-height: 28px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
}

.daily-game-stats-card__title-group {
  position: relative;
  z-index: 1;
  min-width: 0;
  display: flex;
  align-items: center;
  gap: 2px;
  pointer-events: none;
}

.daily-game-stats-card__title {
  color: var(--p-text-muted-color);
  font-size: var(--daily-game-stats-card-title-font-size);
  font-weight: 400;
  line-height: 1.1;
}

.daily-game-stats-card__body {
  position: relative;
}

.daily-game-stats-card__rows {
  display: flex;
  flex-direction: column;
  gap: 0;
}

.daily-game-stats-card__rows--blocked {
  opacity: 0.34;
  pointer-events: none;
  user-select: none;
}

.daily-game-stats-card__row {
  min-width: 0;
  display: grid;
  grid-template-columns: var(--daily-game-stats-card-row-icon-size) minmax(0, 1fr);
  align-items: center;
  gap: 8px;
  padding: 8px 0;
  border-top: 1px solid var(--color-gray-50);
}

.daily-game-stats-card__row:first-child {
  border-top: 0;
  padding-top: 4px;
}

.daily-game-stats-card__row-icon {
  width: var(--daily-game-stats-card-row-icon-size);
  height: var(--daily-game-stats-card-row-icon-size);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border-radius: 8px;
  background: var(--color-gray-50);
  color: var(--p-surface-500);
  box-shadow: inset 0 0 0 1px var(--color-gray-100);
}

.daily-game-stats-card__row-icon .pi {
  font-size: 15px;
  line-height: 1;
}

.daily-game-stats-card__row-icon,
.daily-game-stats-card__metric-value,
.daily-game-stats-card__comparison,
.daily-game-stats-card__metric-missing {
  animation: daily-game-stats-card-fade-in 0.18s ease-out;
}

.daily-game-stats-card__row-icon-skeleton {
  align-self: center;
}

.daily-game-stats-card__row-content {
  min-width: 0;
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: 5px;
  color: var(--color-gray-700);
  font-size: var(--daily-game-stats-card-row-font-size);
  line-height: 1.25;
}

.daily-game-stats-card__metric {
  min-width: 0;
  display: inline-flex;
  align-items: baseline;
  flex-wrap: wrap;
  gap: 3px 4px;
}

.daily-game-stats-card__metric-label {
  color: var(--color-gray-700);
  font-size: var(--daily-game-stats-card-value-font-size);
  font-weight: 400;
  line-height: 1.2;
}

.daily-game-stats-card__metric:not(.daily-game-stats-card__metric--personal) .daily-game-stats-card__metric-label {
  color: var(--p-text-muted-color);
  font-weight: 400;
}

.daily-game-stats-card__metric-value {
  min-width: 0;
  color: var(--color-gray-900);
  font-size: var(--daily-game-stats-card-value-font-size);
  font-weight: 500;
  line-height: 1;
  font-variant-numeric: tabular-nums;
  white-space: nowrap;
}

.daily-game-stats-card__comparison {
  color: var(--color-gray-600);
  font-size: var(--daily-game-stats-card-row-font-size);
  font-weight: 400;
  line-height: 1.15;
}

.daily-game-stats-card__benchmark-value {
  min-width: 0;
  display: inline-flex;
  align-items: baseline;
  flex-wrap: wrap;
  gap: 4px;
  color: var(--p-text-muted-color);
  font-size: var(--daily-game-stats-card-row-font-size);
  line-height: 1.15;
}

.daily-game-stats-card__metric-missing {
  color: var(--p-surface-500);
  font-size: var(--daily-game-stats-card-row-font-size);
  font-weight: 400;
  line-height: 1.2;
}

.daily-game-stats-card__missing-with-info {
  display: inline-flex;
  align-items: center;
  gap: 3px;
}

.daily-game-stats-card__overlay {
  position: absolute;
  inset: 0;
  z-index: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 16px;
  border-radius: 8px;
  background: rgba(255, 255, 255, 0.68);
  color: var(--color-gray-700);
  text-align: center;
}

.daily-game-stats-card__overlay p {
  margin: 0;
  font-size: var(--daily-game-stats-card-value-font-size);
  font-weight: 500;
  line-height: 1.35;
}

.daily-game-stats-card__header-info,
.daily-game-stats-card__header-refresh,
.daily-game-stats-card__benchmark-info,
.daily-game-stats-card__toggle {
  border: 0;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: transparent;
  color: var(--p-surface-500);
  cursor: help;
  transition:
    color 0.16s ease,
    background-color 0.16s ease;
}

.daily-game-stats-card__header-info,
.daily-game-stats-card__header-refresh {
  width: var(--daily-game-stats-card-info-size);
  min-width: var(--daily-game-stats-card-info-size);
  height: var(--daily-game-stats-card-info-size);
  padding: 0;
  border-radius: 50%;
  pointer-events: auto;
}

.daily-game-stats-card__header-refresh {
  cursor: pointer;
}

.daily-game-stats-card__header-refresh:disabled {
  cursor: wait;
  opacity: 0.65;
}

.daily-game-stats-card__benchmark-info {
  width: 18px;
  min-width: 18px;
  height: 18px;
  padding: 0;
  border-radius: 6px;
  color: var(--p-surface-500);
  line-height: 1;
}

.daily-game-stats-card__toggle {
  position: absolute;
  inset: 0;
  width: 100%;
  min-width: 0;
  height: 100%;
  padding: 0;
  border-radius: 8px;
  justify-content: flex-end;
  cursor: pointer;
}

.daily-game-stats-card__toggle-icon {
  width: 28px;
  min-width: 28px;
  height: 28px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border-radius: 8px;
  transition:
    color 0.16s ease,
    background-color 0.16s ease;
}

.daily-game-stats-card__toggle-icon i {
  font-size: 14px;
  line-height: 1;
  transition: transform 0.16s ease;
}

.daily-game-stats-card__toggle[aria-expanded="true"] .daily-game-stats-card__toggle-icon i {
  transform: rotate(180deg);
}

.daily-game-stats-card__header-info i,
.daily-game-stats-card__header-refresh i {
  font-size: var(--daily-game-stats-card-info-icon-size);
  line-height: 1;
}

.daily-game-stats-card__benchmark-info i {
  font-size: 13px;
  line-height: 1;
}

.daily-game-stats-card__header-info--open,
.daily-game-stats-card__benchmark-info--open {
  background: var(--color-primary-50);
  color: var(--color-primary-700);
}

@media (hover: hover) and (pointer: fine) {

  .daily-game-stats-card__header-info:hover,
  .daily-game-stats-card__header-refresh:not(:disabled):hover,
  .daily-game-stats-card__benchmark-info:hover {
    background: var(--color-primary-50);
    color: var(--color-primary-700);
  }

  .daily-game-stats-card__toggle-icon:hover {
    background: var(--color-primary-50);
    color: var(--color-primary-700);
  }
}

.daily-game-stats-card__header-info:focus-visible,
.daily-game-stats-card__header-refresh:focus-visible,
.daily-game-stats-card__benchmark-info:focus-visible,
.daily-game-stats-card__toggle:focus-visible {
  outline: none;
  box-shadow: var(--focus-ring-primary);
}

.daily-game-stats-card__popover-text {
  max-width: 280px;
  margin: 0;
  color: var(--color-gray-700);
  font-size: var(--info-popover-font-size);
  line-height: 1.45;
}

:global(.daily-game-stats-card__popover.p-popover) {
  max-width: min(310px, calc(100vw - 24px));
}

@keyframes daily-game-stats-card-fade-in {
  from {
    opacity: 0;
  }

  to {
    opacity: 1;
  }
}

@media (max-width: 767px) {
  .daily-game-stats-card {
    --daily-game-stats-card-row-font-size: 13px;
    --daily-game-stats-card-value-font-size: 14px;
    gap: 6px;
  }

  .daily-game-stats-card__benchmark-value {
    gap: 3px;
  }

  .daily-game-stats-card--headerless .daily-game-stats-card__row {
    padding-inline: 0;
  }

  .daily-game-stats-card--headerless .daily-game-stats-card__row-content {
    font-size: 13px;
  }
}

@media (max-width: 359px) {
  .daily-game-stats-card {
    --daily-game-stats-card-row-icon-size: 28px;
    --daily-game-stats-card-row-font-size: 12px;
  }

  .daily-game-stats-card__row {
    padding: 7px 0;
  }
}

@media (min-width: 768px) {
  .daily-game-stats-card {
    --daily-game-stats-card-padding-block: 8px;
    --daily-game-stats-card-padding-inline-start: 12px;
    --daily-game-stats-card-padding-inline-end: 10px;
    --daily-game-stats-card-title-font-size: 18px;
    --daily-game-stats-card-row-font-size: 14px;
    --daily-game-stats-card-value-font-size: 15px;
    --daily-game-stats-card-row-icon-size: 30px;
    --daily-game-stats-card-info-size: 28px;
    --daily-game-stats-card-info-icon-size: 15px;
    gap: 8px;
  }

  .daily-game-stats-card__row {
    gap: 10px;
    padding: 9px 0;
  }

}

@media (min-width: 1024px) {
  .daily-game-stats-card {
    --daily-game-stats-card-padding-block: 9px;
    --daily-game-stats-card-title-font-size: 19px;
    --daily-game-stats-card-row-font-size: 14px;
    --daily-game-stats-card-value-font-size: 15px;
    --daily-game-stats-card-row-icon-size: 30px;
    --daily-game-stats-card-info-size: 30px;
    --daily-game-stats-card-info-icon-size: 16px;
  }

}
</style>
