<script setup lang="ts">
import Popover from "primevue/popover";
import { computed } from "vue";
import { useInfoPopover } from "../../../shared/composables/useInfoPopover";

const props = defineProps<{
  place: number | null;
  emptyPlaceInfo?: string;
  emphasized?: boolean;
}>();

const medalClass = computed(() =>
  props.place !== null && props.place >= 1 && props.place <= 3
    ? `leaderboard-place--${props.place}`
    : null,
);

const {
  triggerId: emptyPlaceTriggerId,
  panelId: emptyPlacePanelId,
  popoverPt: emptyPlacePopoverPt,
  setPopover: setEmptyPlacePopover,
  visible: emptyPlacePopoverVisible,
  show: showEmptyPlaceInfo,
  hide: hideEmptyPlaceInfo,
  onPointerDown: onEmptyPlaceInfoPointerDown,
  onShow: onEmptyPlacePopoverShow,
  onHide: onEmptyPlacePopoverHide,
} = useInfoPopover();
</script>

<template>
  <span v-if="place !== null" class="leaderboard-place"
    :class="[medalClass, { 'leaderboard-place--emphasized': emphasized }]">{{ place }}</span>

  <template v-else-if="emptyPlaceInfo">
    <button class="leaderboard-place__info"
      :class="{ 'leaderboard-place__info--open': emptyPlacePopoverVisible }" type="button"
      :id="emptyPlaceTriggerId" aria-label="Почему нет места в таблице лидеров"
      :aria-describedby="emptyPlacePanelId" @mouseenter="showEmptyPlaceInfo" @mouseleave="hideEmptyPlaceInfo"
      @pointerdown="onEmptyPlaceInfoPointerDown" @focus="showEmptyPlaceInfo" @blur="hideEmptyPlaceInfo" @click.stop>
      <i class="pi pi-info-circle" aria-hidden="true"></i>
    </button>

    <Popover :ref="setEmptyPlacePopover" :pt="emptyPlacePopoverPt" class="leaderboard-place__popover info-popover"
      @show="onEmptyPlacePopoverShow" @hide="onEmptyPlacePopoverHide">
      <p class="leaderboard-place__popover-text">{{ emptyPlaceInfo }}</p>
    </Popover>
  </template>

  <span v-else class="leaderboard-place">—</span>
</template>

<style scoped>
.leaderboard-place {
  width: 26px;
  height: 26px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-variant-numeric: tabular-nums;
}

.leaderboard-place--emphasized {
  font-weight: var(--leaderboard-emphasis-font-weight);
}

.leaderboard-place__info {
  width: 30px;
  height: 30px;
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
    color 0.16s ease,
    background-color 0.16s ease;
}

.leaderboard-place__info i {
  font-size: 16px;
  line-height: 1;
}

.leaderboard-place__info--open {
  background: var(--color-primary-100);
  color: var(--color-primary-600);
}

@media (hover: hover) and (pointer: fine) {
  .leaderboard-place__info:hover {
    background: var(--color-primary-100);
    color: var(--color-primary-600);
  }
}

.leaderboard-place__info:focus-visible {
  outline: 2px solid var(--color-primary-600);
  outline-offset: 2px;
}

.leaderboard-place__popover-text {
  max-width: 240px;
  margin: 0;
  color: var(--color-gray-700);
  font-size: var(--info-popover-font-size);
  line-height: 1.45;
}

.leaderboard-place--1,
.leaderboard-place--2,
.leaderboard-place--3 {
  border: 1px solid var(--leaderboard-place-border);
  border-radius: 50%;
  background: var(--leaderboard-place-background);
  color: var(--leaderboard-place-color);
  font-weight: var(--leaderboard-emphasis-font-weight);
  line-height: 1;
}

.leaderboard-place--1 {
  --leaderboard-place-background: #fff7df;
  --leaderboard-place-border: #d8b75a;
  --leaderboard-place-color: #80610d;
}

.leaderboard-place--2 {
  --leaderboard-place-background: #f4f7fa;
  --leaderboard-place-border: #b9c4cf;
  --leaderboard-place-color: #55616d;
}

.leaderboard-place--3 {
  --leaderboard-place-background: #fbf3eb;
  --leaderboard-place-border: #d5b28f;
  --leaderboard-place-color: #855322;
}
</style>
