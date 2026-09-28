<script setup lang="ts">
import Popover from "primevue/popover";
import { useInfoPopover } from "../../../shared/composables/useInfoPopover";

withDefaults(
  defineProps<{
    position?: "left" | "right";
  }>(),
  {
    position: "left",
  },
);

const {
  triggerId,
  panelId,
  popoverPt,
  setPopover,
  visible,
  show,
  hide,
  onPointerDown,
  onShow,
  onHide,
} = useInfoPopover();
</script>

<template>
  <button
    class="daily-on-time-badge"
    :class="{
      'daily-on-time-badge--open': visible,
      'daily-on-time-badge--right': position === 'right',
    }"
    :id="triggerId"
    type="button"
    aria-label="Отгадано в день выхода"
    :aria-describedby="panelId"
    @mouseenter="show"
    @mouseleave="hide"
    @pointerdown="onPointerDown"
    @focus="show"
    @blur="hide"
    @click.stop
  >
    <i class="pi pi-bolt" aria-hidden="true"></i>
  </button>

  <Popover
    :ref="setPopover"
    :pt="popoverPt"
    class="daily-on-time-popover info-popover"
    @show="onShow"
    @hide="onHide"
  >
    <p class="daily-on-time-popover__content">
      Отгадано в день выхода
    </p>
  </Popover>
</template>

<style scoped>
.daily-on-time-badge {
  appearance: none;
  -webkit-tap-highlight-color: transparent;
  position: absolute;
  top: -8px;
  left: -8px;
  z-index: 3;
  width: 22px;
  height: 22px;
  border: 1px solid var(--color-streak-100);
  border-radius: 7px;
  padding: 0;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: white;
  box-shadow: 0 2px 7px rgba(25, 32, 43, 0.12);
  color: var(--color-streak-600);
  cursor: help;
  font-size: 12px;
}

.daily-on-time-badge--right {
  right: -8px;
  left: auto;
}

.daily-on-time-badge--open {
  background: var(--color-streak-50);
  color: var(--color-streak-600);
}

@media (hover: hover) and (pointer: fine) {
  .daily-on-time-badge:hover {
    background: var(--color-streak-50);
    color: var(--color-streak-600);
  }
}

.daily-on-time-badge:focus-visible {
  outline: none;
  box-shadow: var(--focus-ring-primary);
  background: var(--color-streak-50);
  color: var(--color-streak-600);
}

.daily-on-time-popover__content {
  margin: 0;
  color: var(--color-gray-700);
  font-size: var(--info-popover-font-size);
  line-height: 1.4;
}

:global(.daily-on-time-popover.p-popover) {
  max-width: min(240px, calc(100vw - 24px));
}
</style>
