<script setup lang="ts">
import { computed } from "vue";
import UiButton from "../../../shared/ui/UiButton.vue";
import UiSkeleton from "../../../shared/ui/UiSkeleton.vue";
import LeaderboardGapMark from "./LeaderboardGapMark.vue";
import LeaderboardPlace from "./LeaderboardPlace.vue";
import LeaderboardPlayerName from "./LeaderboardPlayerName.vue";
import type { LeaderboardRow } from "./leaderboardRows";

const props = withDefaults(
  defineProps<{
    rows: LeaderboardRow[] | null;
    metricLabel: string;
    metricColumnWidth: string;
    narrowMetricColumnWidth: string;
    metricSkeletonWidth: string;
    skeletonVisible: boolean;
    failed: boolean;
    retryLoading: boolean;
    refreshSkeletonVisible: boolean;
    transitionEnabled: boolean;
    transitionKey: string;
    tableLabel?: string;
    metricNavigation?: boolean;
    compactPlacePlayerGap?: boolean;
    denseOnShortViewport?: boolean;
  }>(),
  {
    tableLabel: undefined,
    metricNavigation: false,
    compactPlacePlayerGap: false,
    denseOnShortViewport: false,
  },
);

defineEmits<{
  retry: [];
}>();

const topN = 10;
const tableStyle = computed(() => ({
  "--leaderboard-metric-column-width": props.metricColumnWidth,
  "--leaderboard-narrow-metric-column-width": props.narrowMetricColumnWidth,
}));
</script>

<template>
  <div
    class="leaderboard-table__stage"
    :class="{
      'leaderboard-table__stage--dense-short': denseOnShortViewport,
      'leaderboard-table__stage--metric-navigation': metricNavigation,
      'leaderboard-table__stage--compact-place-player-gap': compactPlacePlayerGap,
    }"
    :style="tableStyle"
  >
    <Transition name="ui-skeleton-handoff-skeleton" appear>
      <div v-if="skeletonVisible" class="leaderboard-table__wrap" aria-hidden="true">
        <table class="leaderboard-table__table">
          <thead>
            <tr>
              <th scope="col">Место</th>
              <th scope="col">Игрок</th>
              <th scope="col">
                <div v-if="metricNavigation" class="leaderboard-table__metric-header">
                  <span class="leaderboard-table__metric-nav-slot">
                    <slot name="metric-skeleton-before"></slot>
                  </span>
                  <span class="leaderboard-table__metric-title">{{ metricLabel }}</span>
                  <span class="leaderboard-table__metric-nav-slot">
                    <slot name="metric-skeleton-after"></slot>
                  </span>
                </div>
                <template v-else>{{ metricLabel }}</template>
              </th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="index in topN" :key="index">
              <td>
                <div class="leaderboard-table__place-skeleton">
                  <UiSkeleton
                    :width="index <= 3 ? '26px' : '18px'"
                    :height="index <= 3 ? '26px' : '14px'"
                    :shape="index <= 3 ? 'circle' : 'rectangle'"
                  />
                </div>
              </td>
              <td>
                <div class="leaderboard-table__player-skeleton">
                  <UiSkeleton :width="index % 3 === 0 ? '72%' : '54%'" height="14px" />
                </div>
              </td>
              <td>
                <UiSkeleton
                  class="leaderboard-table__cell-skeleton"
                  :width="metricSkeletonWidth"
                  height="14px"
                />
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </Transition>

    <div v-if="failed" class="leaderboard-table__state">
      <p>Не удалось загрузить таблицу лидеров.</p>
      <UiButton size="md" variant="outlined" :loading="retryLoading" @click="$emit('retry')">
        Повторить
      </UiButton>
    </div>

    <Transition name="ui-skeleton-handoff-content" mode="out-in" appear :css="transitionEnabled">
      <div v-if="rows" :key="transitionKey" class="leaderboard-table__data">
        <div
          class="leaderboard-table__wrap"
          :aria-label="tableLabel"
        >
          <table class="leaderboard-table__table">
            <thead>
              <tr>
                <th scope="col">Место</th>
                <th scope="col">Игрок</th>
                <th scope="col">
                  <div v-if="metricNavigation" class="leaderboard-table__metric-header">
                    <span class="leaderboard-table__metric-nav-slot">
                      <slot name="metric-before"></slot>
                    </span>
                    <span class="leaderboard-table__metric-title" aria-live="polite">
                      {{ metricLabel }}
                    </span>
                    <span class="leaderboard-table__metric-nav-slot">
                      <slot name="metric-after"></slot>
                    </span>
                  </div>
                  <template v-else>{{ metricLabel }}</template>
                </th>
              </tr>
            </thead>
            <tbody>
              <template v-for="row in rows" :key="row.id">
                <tr v-if="row.kind === 'gap'" class="leaderboard-table__gap" aria-hidden="true">
                  <td><LeaderboardGapMark /></td>
                  <td colspan="2"></td>
                </tr>
                <tr v-else :class="{ 'leaderboard-table__row--current': row.current }">
                  <td>
                    <LeaderboardPlace
                      :place="row.place"
                      :emphasized="row.placeEmphasized"
                      :empty-place-info="row.emptyPlaceInfo"
                    />
                  </td>
                  <td>
                    <LeaderboardPlayerName
                      :player-name="row.playerName"
                      :emphasized="row.playerNameEmphasized"
                    />
                  </td>
                  <td>{{ row.metric }}</td>
                </tr>
              </template>
            </tbody>
          </table>
        </div>

        <div v-if="refreshSkeletonVisible" class="leaderboard-table__refresh-skeleton" aria-hidden="true">
          <UiSkeleton width="100%" height="38px" border-radius="8px" />
        </div>
      </div>
    </Transition>
  </div>
</template>

<style scoped>
.leaderboard-table__stage {
  min-width: 0;
  display: grid;
}

.leaderboard-table__stage > * {
  grid-area: 1 / 1;
}

.leaderboard-table__state {
  min-height: 120px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 10px;
  color: var(--p-text-muted-color);
  font-size: 14px;
  line-height: 1.4;
  text-align: center;
}

.leaderboard-table__state p {
  margin: 0;
}

.leaderboard-table__data {
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: inherit;
}

.leaderboard-table__wrap {
  overflow: hidden;
  border: 1px solid var(--color-gray-200);
  border-radius: 8px;
  background: rgba(255, 255, 255, 0.94);
}

.leaderboard-table__table {
  width: 100%;
  table-layout: fixed;
  border-collapse: collapse;
}

.leaderboard-table__table th,
.leaderboard-table__table td {
  min-width: 0;
  padding: 10px 12px;
  border-bottom: 1px solid var(--color-gray-100);
  color: var(--color-gray-700);
  font-size: 15px;
  line-height: 1.25;
  text-align: left;
}

.leaderboard-table__table th {
  color: var(--p-text-muted-color);
  font-weight: var(--leaderboard-emphasis-font-weight);
  vertical-align: middle;
}

.leaderboard-table__table th:first-child,
.leaderboard-table__table td:first-child {
  width: 7ch;
  color: var(--p-text-muted-color);
  text-align: center;
  font-variant-numeric: tabular-nums;
}

.leaderboard-table__stage--compact-place-player-gap .leaderboard-table__table th:first-child,
.leaderboard-table__stage--compact-place-player-gap .leaderboard-table__table td:first-child {
  padding-right: 6px;
}

.leaderboard-table__stage--compact-place-player-gap .leaderboard-table__table th:nth-child(2),
.leaderboard-table__stage--compact-place-player-gap .leaderboard-table__table td:nth-child(2) {
  padding-left: 6px;
}

.leaderboard-table__table th:last-child,
.leaderboard-table__table td:last-child {
  width: var(--leaderboard-metric-column-width);
  text-align: center;
  font-variant-numeric: tabular-nums;
}

.leaderboard-table__table td {
  overflow: hidden;
}

.leaderboard-table__table th:nth-child(2),
.leaderboard-table__table td:nth-child(2) {
  padding-inline: 8px;
}

.leaderboard-table__table td:nth-child(2) {
  white-space: normal;
}

.leaderboard-table__table td:last-child {
  font-weight: 400;
  white-space: nowrap;
}

.leaderboard-table__metric-header {
  width: 100%;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 2px;
  font-size: 15px;
  line-height: 1.1;
  text-align: center;
}

.leaderboard-table__metric-title {
  flex: 1;
  white-space: nowrap;
}

.leaderboard-table__metric-nav-slot {
  width: 26px;
  height: 26px;
  flex: none;
  display: flex;
  align-items: center;
  justify-content: center;
}

.leaderboard-table__metric-nav-slot :deep(.p-button) {
  width: 26px;
  height: 26px;
  flex: none;
  padding: 0;
}

.leaderboard-table__metric-nav-slot :deep(.p-button-icon) {
  font-size: 10px;
}

.leaderboard-table__player-skeleton {
  display: flex;
  flex-direction: column;
  gap: 3px;
}

.leaderboard-table__cell-skeleton {
  margin-inline: auto;
}

.leaderboard-table__place-skeleton {
  width: 26px;
  height: 26px;
  margin-inline: auto;
  display: flex;
  align-items: center;
  justify-content: center;
}

.leaderboard-table__table tbody tr:last-child td {
  border-bottom: 0;
}

.leaderboard-table__row--current td {
  background: color-mix(in srgb, var(--color-primary-100) 32%, white);
}

.leaderboard-table__row--current td:first-child {
  border-left: 3px solid var(--color-primary-400);
}

.leaderboard-table__gap td {
  color: var(--p-text-muted-color);
  text-align: center;
  vertical-align: middle;
}

.leaderboard-table__refresh-skeleton {
  min-height: 44px;
  display: flex;
  align-items: center;
}

@media (min-width: 768px) {
  .leaderboard-table__table th,
  .leaderboard-table__table td {
    padding: 12px 14px;
    font-size: 16px;
  }

  .leaderboard-table__metric-header {
    font-size: 16px;
  }

  .leaderboard-table__table th:nth-child(2),
  .leaderboard-table__table td:nth-child(2) {
    padding-inline: 10px;
  }
}

@media (max-height: 760px) {
  .leaderboard-table__stage--dense-short .leaderboard-table__table th,
  .leaderboard-table__stage--dense-short .leaderboard-table__table td {
    padding-block: 8px;
  }
}

@media (max-height: 640px) {
  .leaderboard-table__stage--dense-short .leaderboard-table__table th,
  .leaderboard-table__stage--dense-short .leaderboard-table__table td {
    padding-block: 7px;
  }
}

@media (max-width: 480px) {
  .leaderboard-table__table th,
  .leaderboard-table__table td {
    padding: 7px 8px;
    font-size: 14px;
  }

  .leaderboard-table__table th:last-child,
  .leaderboard-table__table td:last-child {
    width: var(--leaderboard-narrow-metric-column-width);
  }

  .leaderboard-table__stage--metric-navigation .leaderboard-table__table th:last-child {
    padding-inline: 3px;
  }

  .leaderboard-table__metric-header {
    font-size: 14px;
  }

  .leaderboard-table__table th:nth-child(2),
  .leaderboard-table__table td:nth-child(2) {
    padding-inline: 6px;
  }

  .leaderboard-table__metric-nav-slot,
  .leaderboard-table__metric-nav-slot :deep(.p-button) {
    width: 22px;
    height: 22px;
  }
}
</style>
