<script setup lang="ts">
import Popover from "primevue/popover";
import { useInfoPopover } from "../../../shared/composables/useInfoPopover";

defineProps<{
  title: string;
  titleId: string;
  denseOnShortViewport?: boolean;
}>();

const {
  triggerId: infoTriggerId,
  panelId: infoPanelId,
  popoverPt: infoPopoverPt,
  setPopover: setInfoPopover,
  visible: infoPopoverVisible,
  show: showInfo,
  hide: hideInfo,
  onPointerDown: onInfoPointerDown,
  onShow: onInfoPopoverShow,
  onHide: onInfoPopoverHide,
} = useInfoPopover();
</script>

<template>
  <section
    class="leaderboard-shell"
    :class="{ 'leaderboard-shell--dense-short': denseOnShortViewport }"
    :aria-labelledby="titleId"
  >
    <div class="leaderboard-shell__header">
      <div class="leaderboard-shell__title-group">
        <h1 :id="titleId" class="game-page-title">
          <span>{{ title }}</span>
        </h1>
        <button
          :id="infoTriggerId"
          class="leaderboard-shell__info"
          :class="{ 'leaderboard-shell__info--open': infoPopoverVisible }"
          type="button"
          aria-label="О таблице лидеров"
          :aria-describedby="infoPanelId"
          @mouseenter="showInfo"
          @mouseleave="hideInfo"
          @pointerdown="onInfoPointerDown"
          @focus="showInfo"
          @blur="hideInfo"
          @click.stop
        >
          <i class="pi pi-info-circle" aria-hidden="true"></i>
        </button>
      </div>

      <slot name="header-extra"></slot>
    </div>

    <Popover
      :ref="setInfoPopover"
      :pt="infoPopoverPt"
      class="leaderboard-shell__popover info-popover"
      @show="onInfoPopoverShow"
      @hide="onInfoPopoverHide"
    >
      <div class="leaderboard-shell__popover-content">
        <slot name="info"></slot>
      </div>
    </Popover>

    <div class="leaderboard-shell__content">
      <slot></slot>
    </div>
  </section>
</template>

<style scoped>
.leaderboard-shell {
  width: 100%;
  max-width: 760px;
  margin-inline: auto;
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.leaderboard-shell__header {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.leaderboard-shell__title-group {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 3px;
}

.leaderboard-shell__info {
  width: 26px;
  min-width: 26px;
  height: 26px;
  padding: 0;
  border: 0;
  border-radius: 50%;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: transparent;
  color: var(--p-surface-500);
  cursor: help;
  transition:
    border-color 0.16s ease,
    color 0.16s ease,
    background-color 0.16s ease;
}

.leaderboard-shell__info i {
  font-size: 16px;
  line-height: 1;
}

.leaderboard-shell__info--open {
  background: var(--color-primary-100);
  color: var(--color-primary-600);
}

@media (hover: hover) and (pointer: fine) {
  .leaderboard-shell__info:hover {
    background: var(--color-primary-100);
    color: var(--color-primary-600);
  }
}

.leaderboard-shell__info:focus-visible {
  outline: none;
}

.leaderboard-shell__popover-content {
  max-width: 280px;
  color: var(--color-gray-700);
  font-size: var(--info-popover-font-size);
  line-height: 1.45;
}

.leaderboard-shell__popover-content :deep(p) {
  margin: 0;
}

.leaderboard-shell__popover-content :deep(p + p) {
  margin-top: 8px;
}

.leaderboard-shell__content {
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 14px;
}

@media (min-width: 768px) {
  .leaderboard-shell,
  .leaderboard-shell__content {
    gap: 16px;
  }
}

@media (max-width: 480px) {
  .leaderboard-shell,
  .leaderboard-shell__content {
    gap: 12px;
  }
}

@media (max-height: 760px) {
  .leaderboard-shell--dense-short,
  .leaderboard-shell--dense-short .leaderboard-shell__content {
    gap: 12px;
  }
}
</style>
