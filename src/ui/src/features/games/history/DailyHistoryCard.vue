<script setup lang="ts">
import Popover from "primevue/popover";
import { computed } from "vue";
import type { RouteLocationRaw } from "vue-router";
import { useInfoPopover } from "../../../shared/composables/useInfoPopover";
import UiButton from "../../../shared/ui/UiButton.vue";
import type { DailyGame } from "../model/game";
import {
  formatDailyGameDate,
  getDailyGameStatus,
  wasGuessedOnReleaseDay,
} from "../lib/dailyGamePresentation";
import DailyOnTimeBadge from "../components/DailyOnTimeBadge.vue";
import HistoryDisplayWord from "./HistoryDisplayWord.vue";

const props = defineProps<{
  dailyGame: DailyGame;
  to: RouteLocationRaw;
}>();

const {
  triggerId: statusTriggerId,
  panelId: statusPanelId,
  popoverPt: statusPopoverPt,
  setPopover: setStatusPopover,
  visible: statusPopoverVisible,
  show: showStatus,
  hide: hideStatus,
  onPointerDown: onStatusPointerDown,
  onShow: onStatusPopoverShow,
  onHide: onStatusPopoverHide,
} = useInfoPopover();

const config = computed(() => getDailyGameStatus(props.dailyGame));
const title = computed(() => formatDailyGameDate(props.dailyGame.day));
const showOnTimeBadge = computed(() => wasGuessedOnReleaseDay(props.dailyGame));
</script>

<template>
  <article class="daily-history-card" :class="`daily-history-card--status-${config.status}`">
    <DailyOnTimeBadge v-if="showOnTimeBadge" />

    <div class="daily-history-card__layout">
      <div class="daily-history-card__body">
        <div class="daily-history-card__status-zone">
          <button
            class="daily-history-card__status"
            :class="[
              `daily-history-card__status--${config.status}`,
              { 'daily-history-card__status--open': statusPopoverVisible },
            ]"
            :id="statusTriggerId"
            type="button"
            :aria-label="config.label"
            :aria-describedby="statusPanelId"
            @mouseenter="showStatus"
            @mouseleave="hideStatus"
            @pointerdown="onStatusPointerDown"
            @focus="showStatus"
            @blur="hideStatus"
            @click.stop
          >
            <i :class="config.icon" aria-hidden="true"></i>
          </button>
        </div>

        <div class="daily-history-card__text">
          <h3 class="daily-history-card__title">{{ title }}</h3>
          <p class="daily-history-card__meta">Слово дня</p>
        </div>

        <div class="daily-history-card__word">
          <HistoryDisplayWord :display-word="dailyGame.word" />
        </div>
      </div>

      <div class="daily-history-card__action-zone">
        <UiButton class="daily-history-card__action" size="md" variant="soft" :to="to">
          <span>Открыть</span>
          <i class="pi pi-arrow-right" aria-hidden="true"></i>
        </UiButton>
      </div>
    </div>

    <Popover
      :ref="setStatusPopover"
      :pt="statusPopoverPt"
      class="daily-history-card__popover info-popover"
      @show="onStatusPopoverShow"
      @hide="onStatusPopoverHide"
    >
      <div class="daily-history-card__popover-content">{{ config.label }}</div>
    </Popover>
  </article>
</template>

<style scoped>
.daily-history-card {
  --daily-history-info-width: 84px;
  position: relative;
  min-width: 0;
  container-type: inline-size;
  display: flex;
  gap: 10px;
  padding: 10px;
  border: 1px solid var(--color-gray-200);
  border-radius: 8px;
  background: rgba(255, 255, 255, 0.92);
}

.daily-history-card--status-active,
.daily-history-card--status-notStarted,
.daily-history-card--status-cancelled {
  border-color: var(--color-gray-300);
}

.daily-history-card--status-active,
.daily-history-card--status-guessed {
  background: white;
}

.daily-history-card--status-notStarted,
.daily-history-card--status-cancelled {
  background: var(--color-gray-50);
}

.daily-history-card--status-guessed {
  border-color: var(--color-primary-500);
}

.daily-history-card--status-surrendered {
  border-color: var(--color-red-600);
}

.daily-history-card__layout {
  min-width: 0;
  width: 100%;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
}

.daily-history-card__body {
  min-width: 0;
  flex: 1 1 auto;
  display: flex;
  align-items: center;
}

.daily-history-card__status-zone {
  width: 40px;
  min-width: 40px;
  height: 40px;
  flex: 0 0 40px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.daily-history-card__status {
  width: 40px;
  min-width: 40px;
  height: 40px;
  position: relative;
  border: 0;
  border-radius: 8px;
  padding: 0;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: transparent;
  color: var(--p-text-muted-color);
  cursor: help;
  transition: color 0.16s ease;
}

.daily-history-card__status::before {
  content: "";
  position: absolute;
  inset: 4px;
  border-radius: 8px;
  background: transparent;
  transition: background-color 0.16s ease;
}

.daily-history-card__status--guessed {
  color: var(--color-primary-600);
}

.daily-history-card__status--surrendered {
  color: var(--color-red-600);
}

.daily-history-card__status--open::before {
  background: var(--color-gray-100);
}

@media (hover: hover) and (pointer: fine) {
  .daily-history-card__status:hover::before {
    background: var(--color-primary-50);
  }

  .daily-history-card__status--active:hover::before,
  .daily-history-card__status--guessed:hover::before,
  .daily-history-card__status--notStarted:hover::before,
  .daily-history-card__status--surrendered:hover::before {
    background: var(--color-gray-100);
  }
}

.daily-history-card__status:focus-visible {
  outline: 2px solid var(--color-primary-600);
  outline-offset: 2px;
}

.daily-history-card__status .pi {
  position: relative;
  z-index: 1;
  font-size: var(--history-card-status-icon-size);
  line-height: 1;
}

.daily-history-card__text {
  min-width: 0;
  flex: 0 0 var(--daily-history-info-width);
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.daily-history-card__title,
.daily-history-card__meta {
  min-width: 0;
  margin: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.daily-history-card__title {
  color: var(--color-gray-900);
  font-size: var(--history-card-title-font-size);
  font-weight: 400;
  line-height: 1.2;
}

.daily-history-card__meta {
  color: var(--color-gray-600);
  font-size: 13px;
  line-height: 1.3;
}

.daily-history-card__word {
  position: relative;
  min-width: 0;
  flex: 1 1 148px;
  max-width: 420px;
  height: 100%;
  padding-left: 10px;
  display: flex;
  align-items: center;
  justify-content: flex-start;
}

.daily-history-card__word :deep(.history-display-word) {
  --history-display-word-max-width: 420px;
}

.daily-history-card__action-zone {
  flex: 0 0 auto;
  height: 100%;
  margin-left: auto;
  padding-left: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.daily-history-card__action.ui-button {
  width: max-content;
  min-height: 40px;
  padding-inline: 12px;
  align-items: center;
  justify-content: center;
  gap: 8px;
  font-size: 14px;
  line-height: 1;
  white-space: nowrap;
}

.daily-history-card__action .pi {
  font-size: 14px;
}

.daily-history-card__popover-content {
  margin: 0;
  color: var(--color-gray-700);
  font-size: var(--info-popover-font-size);
  line-height: 1.35;
}

@container (max-width: 460px) {
  .daily-history-card {
    padding-block: 8px;
  }

  .daily-history-card__layout {
    display: grid;
    grid-template-columns: 40px minmax(0, 1fr) max-content;
    grid-template-areas:
      "status text action"
      "word   word word";
    align-items: center;
    row-gap: 8px;
    column-gap: 0;
  }

  .daily-history-card__body {
    display: contents;
  }

  .daily-history-card__status-zone {
    grid-area: status;
  }

  .daily-history-card__text {
    grid-area: text;
    flex: 0 1 auto;
    width: 100%;
    max-width: none;
    align-items: flex-start;
    text-align: left;
  }

  .daily-history-card__title {
    width: 100%;
    font-size: 14px;
    text-align: left;
  }

  .daily-history-card__word {
    grid-area: word;
    box-sizing: border-box;
    flex-basis: auto;
    width: 100%;
    max-width: 100%;
    min-height: 22px;
    border-top: 1px solid var(--color-gray-200);
    padding-top: 6px;
    padding-left: 0;
    flex-direction: column;
    align-items: center;
    justify-content: flex-start;
  }

  .daily-history-card__word :deep(.history-display-word) {
    width: fit-content;
    max-width: 100%;
    justify-content: center;
  }

  .daily-history-card__action-zone {
    grid-area: action;
    justify-self: end;
    flex-basis: auto;
    width: max-content;
    height: auto;
    margin-left: 0;
    padding-left: 0;
  }

  .daily-history-card__action.ui-button {
    min-height: 36px;
    font-size: 13px;
  }

  .daily-history-card__action .pi {
    font-size: 13px;
  }
}

@media (max-width: 640px) {
  .daily-history-card {
    width: min(100%, 360px);
    margin-inline: auto;
  }
}

@media (max-width: 360px) {
  .daily-history-card {
    padding: 8px;
  }
}
</style>
