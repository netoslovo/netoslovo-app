<script setup lang="ts">
import Popover from "primevue/popover";
import { computed } from "vue";
import { type RouteLocationRaw, useRouter } from "vue-router";
import { useInfoPopover } from "../../../shared/composables/useInfoPopover";
import UiButton from "../../../shared/ui/UiButton.vue";
import type { DailyGame } from "../model/game";
import {
  formatDailyGameDate,
  getDailyGameStatus,
  getDisabledDailyGameStatus,
  wasGuessedOnReleaseDay,
} from "../lib/dailyGamePresentation";
import DailyOnTimeBadge from "../components/DailyOnTimeBadge.vue";
import { isCardNavigationClick } from "../lib/cardNavigation";

const router = useRouter();

const props = withDefaults(
  defineProps<{
    dailyGame: DailyGame;
    disabled?: boolean;
    disabledLabel?: string;
    to?: RouteLocationRaw;
  }>(),
  {
    disabled: false,
    disabledLabel: "Недоступно",
    to: undefined,
  },
);

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

const config = computed(() =>
  props.disabled
    ? getDisabledDailyGameStatus(props.disabledLabel)
    : getDailyGameStatus(props.dailyGame),
);
const title = computed(() => `Сегодня, ${formatDailyGameDate(props.dailyGame.day)}`);
const showOnTimeBadge = computed(() => wasGuessedOnReleaseDay(props.dailyGame));
const actionVariant = computed<"primary" | "soft">(() =>
  config.value.status === "surrendered" || config.value.status === "cancelled"
    ? "soft"
    : "primary",
);

function toggleStatus(event: Event) {
  if (statusPopoverVisible.value) {
    hideStatus();
    return;
  }

  showStatus(event);
}

function showDisabledStatus(event: Event) {
  if (props.disabled) showStatus(event);
}

function hideDisabledStatus() {
  if (props.disabled) hideStatus();
}

function onDisabledCardPointerDown(event: PointerEvent) {
  if (!props.disabled) return;

  event.preventDefault();
  if (event.pointerType !== "mouse") toggleStatus(event);
}

function onDisabledCardKeyDown(event: KeyboardEvent) {
  if (props.disabled) toggleStatus(event);
}

function openCard(event: MouseEvent) {
  if (props.disabled || !props.to || !isCardNavigationClick(event)) return;
  void router.push(props.to);
}

</script>

<template>
  <article
    class="daily-today-card"
    :class="[
      `daily-today-card--status-${config.status}`,
      {
        'daily-today-card--disabled': disabled,
        'game-card--clickable': !disabled && to,
      },
    ]"
    :role="disabled ? 'button' : undefined"
    :tabindex="disabled ? 0 : undefined"
    :id="disabled ? statusTriggerId : undefined"
    :aria-label="disabled ? disabledLabel : undefined"
    :aria-describedby="disabled ? statusPanelId : undefined"
    @mouseenter="showDisabledStatus"
    @mouseleave="hideDisabledStatus"
    @pointerdown="onDisabledCardPointerDown"
    @click="openCard"
    @focus="showDisabledStatus"
    @blur="hideDisabledStatus"
    @keydown.enter.prevent="onDisabledCardKeyDown"
    @keydown.space.prevent="onDisabledCardKeyDown"
  >
    <DailyOnTimeBadge v-if="showOnTimeBadge" />

    <div class="daily-today-card__body">
      <span
        v-if="disabled"
        class="daily-today-card__status daily-today-card__status--disabled"
        :class="{ 'daily-today-card__status--open': statusPopoverVisible }"
        aria-hidden="true"
      >
        <i :class="config.icon" aria-hidden="true"></i>
      </span>
      <button
        v-else
        class="daily-today-card__status"
        :class="[
          `daily-today-card__status--${config.status}`,
          { 'daily-today-card__status--open': statusPopoverVisible },
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

      <div class="daily-today-card__text">
        <h3 class="daily-today-card__title">{{ title }}</h3>
        <p class="daily-today-card__meta">{{ config.label }}</p>
      </div>
    </div>

    <UiButton v-if="!disabled && to" class="daily-today-card__action" size="md" :variant="actionVariant" :to="to"
      :aria-label="`${config.action}: ${title}`" :title="config.action">
      <i class="pi pi-arrow-right" aria-hidden="true"></i>
    </UiButton>
    <span v-else class="daily-today-card__action daily-today-card__action--disabled" aria-hidden="true">
      <i class="pi pi-arrow-right" aria-hidden="true"></i>
    </span>

    <Popover
      :ref="setStatusPopover"
      :pt="statusPopoverPt"
      class="daily-today-card__popover info-popover"
      @show="onStatusPopoverShow"
      @hide="onStatusPopoverHide"
    >
      <div class="daily-today-card__popover-content">{{ config.label }}</div>
    </Popover>
  </article>
</template>

<style scoped>
.daily-today-card {
  position: relative;
  min-width: 0;
  container-type: inline-size;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
  padding: 10px;
  border: 1px solid var(--color-gray-200);
  border-radius: 8px;
  background: rgba(255, 255, 255, 0.92);
}

.daily-today-card--status-active,
.daily-today-card--status-guessed {
  background: white;
}

.daily-today-card--status-active,
.daily-today-card--status-disabled,
.daily-today-card--status-notStarted,
.daily-today-card--status-cancelled {
  border-color: var(--color-gray-300);
}

.daily-today-card--status-disabled,
.daily-today-card--status-notStarted,
.daily-today-card--status-cancelled {
  background: var(--color-gray-50);
}

.daily-today-card--status-guessed {
  border-color: var(--color-primary-500);
}

.daily-today-card--status-surrendered {
  border-color: var(--color-red-600);
}

.daily-today-card--disabled {
  border-color: var(--color-gray-200);
  background: var(--color-gray-50);
  cursor: pointer;
}

.daily-today-card--disabled .daily-today-card__body,
.daily-today-card--disabled .daily-today-card__action {
  pointer-events: none;
}

.daily-today-card--disabled :deep(*) {
  transition: none !important;
  animation: none !important;
}

.daily-today-card--disabled:focus-visible {
  outline: none;
  box-shadow: var(--focus-ring-primary);
}

@media (hover: hover) and (pointer: fine) {
  .daily-today-card--disabled:hover {
    border-color: var(--color-gray-300);
    background: var(--color-gray-100);
  }

  .daily-today-card.game-card--clickable:hover {
    background: var(--color-gray-50);
  }

  .daily-today-card--status-notStarted.game-card--clickable:hover,
  .daily-today-card--status-cancelled.game-card--clickable:hover {
    background: var(--color-gray-100);
  }
}

.daily-today-card__body {
  min-width: 0;
  display: flex;
  align-items: center;
  gap: 8px;
}

.daily-today-card__status {
  width: 40px;
  min-width: 40px;
  height: 40px;
  border: 0;
  border-radius: 8px;
  padding: 0;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: transparent;
  color: var(--p-text-muted-color);
  cursor: help;
  transition: color 0.16s ease, background-color 0.16s ease;
}

.daily-today-card__status--disabled {
  color: var(--color-gray-500);
  cursor: inherit;
}

.daily-today-card__status--guessed {
  color: var(--color-primary-600);
}

.daily-today-card__status--surrendered {
  color: var(--color-red-600);
}

.daily-today-card__status--open,
.daily-today-card__status--active.daily-today-card__status--open,
.daily-today-card__status--guessed.daily-today-card__status--open,
.daily-today-card__status--notStarted.daily-today-card__status--open,
.daily-today-card__status--surrendered.daily-today-card__status--open,
.daily-today-card__status--cancelled.daily-today-card__status--open {
  background: var(--color-gray-100);
}

@media (hover: hover) and (pointer: fine) {
  .daily-today-card__status:hover {
    background: var(--color-primary-50);
  }

  .daily-today-card__status--active:hover,
  .daily-today-card__status--guessed:hover,
  .daily-today-card__status--notStarted:hover,
  .daily-today-card__status--surrendered:hover,
  .daily-today-card__status--disabled:hover {
    background: var(--color-gray-100);
  }
}

.daily-today-card__status:focus-visible {
  outline: none;
  box-shadow: var(--focus-ring-primary);
}

.daily-today-card__status .pi {
  font-size: 20px;
  line-height: 1;
}

.daily-today-card__text {
  min-width: 0;
  flex: 1 1 auto;
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.daily-today-card__title,
.daily-today-card__meta {
  margin: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.daily-today-card__title {
  color: var(--color-gray-900);
  font-size: 18px;
  font-weight: 500;
  line-height: 1.2;
}

.daily-today-card__meta {
  color: var(--color-gray-600);
  font-size: 13px;
  line-height: 1.3;
}

.daily-today-card__action.ui-button {
  width: 40px;
  min-width: 40px;
  min-height: 40px;
  padding-inline: 0;
}

.daily-today-card--status-surrendered .daily-today-card__action.ui-button.ui-button--soft.p-button,
.daily-today-card--status-cancelled .daily-today-card__action.ui-button.ui-button--soft.p-button {
  border-color: transparent;
  background: transparent;
  color: var(--color-gray-700);
}

.daily-today-card--status-surrendered .daily-today-card__action.ui-button.ui-button--soft.p-button:not(:disabled):is(:hover, :active),
.daily-today-card--status-cancelled .daily-today-card__action.ui-button.ui-button--soft.p-button:not(:disabled):is(:hover, :active) {
  border-color: transparent;
  background: transparent;
  color: var(--color-gray-800);
}

@media (hover: hover) and (pointer: fine) {
  .daily-today-card__action.ui-button.ui-button--soft:hover .pi {
    transform: scale(1.08);
  }
}

.daily-today-card--status-surrendered .daily-today-card__action.ui-button:focus-visible,
.daily-today-card--status-cancelled .daily-today-card__action.ui-button:focus-visible {
  border-color: transparent;
  box-shadow: var(--focus-ring-neutral);
}

.daily-today-card__action--disabled {
  width: 40px;
  min-width: 40px;
  height: 40px;
  border: 1px solid var(--color-gray-300);
  border-radius: 8px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: var(--color-gray-200);
  color: var(--color-gray-500);
}

.daily-today-card__action .pi {
  font-size: 20px;
  transition: transform 0.16s ease;
}

.daily-today-card__popover-content {
  margin: 0;
  color: var(--color-gray-700);
  font-size: var(--info-popover-font-size);
  line-height: 1.35;
}
</style>
