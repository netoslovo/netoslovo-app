<script setup lang="ts">
import Popover from "primevue/popover";
import { useInfoPopover } from "../../../shared/composables/useInfoPopover";

defineProps<{
  label: string;
  message: string;
}>();

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
    :id="triggerId"
    class="admin-dashboard-alert"
    :class="{ 'admin-dashboard-alert--open': visible }"
    type="button"
    :aria-label="label"
    :aria-describedby="panelId"
    @mouseenter="show"
    @mouseleave="hide"
    @pointerdown="onPointerDown"
    @focus="show"
    @blur="hide"
    @click.stop
  >
    <i class="pi pi-info-circle" aria-hidden="true"></i>
  </button>

  <Popover
    :ref="setPopover"
    :pt="popoverPt"
    class="admin-dashboard-alert__popover info-popover"
    @show="onShow"
    @hide="onHide"
  >
    <p class="admin-dashboard-alert__message">{{ message }}</p>
  </Popover>
</template>

<style scoped>
.admin-dashboard-alert {
  appearance: none;
  -webkit-tap-highlight-color: transparent;
  width: 32px;
  height: 32px;
  border: 0;
  border-radius: 8px;
  padding: 0;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: transparent;
  color: var(--color-red-600);
  cursor: help;
  font-size: 16px;
}

.admin-dashboard-alert--open {
  background: var(--color-red-100);
  color: var(--color-red-700);
}

@media (hover: hover) and (pointer: fine) {
  .admin-dashboard-alert:hover {
    background: var(--color-red-100);
    color: var(--color-red-700);
  }
}

.admin-dashboard-alert:focus-visible {
  outline: 2px solid var(--color-primary-500);
  outline-offset: 2px;
  background: var(--color-red-100);
  color: var(--color-red-700);
}

.admin-dashboard-alert__message {
  max-width: 300px;
  margin: 0;
  color: var(--color-gray-700);
  line-height: 1.45;
}
</style>
