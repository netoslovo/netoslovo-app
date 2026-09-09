<script lang="ts">
let closeActiveClosestWordPopover: (() => void) | null = null;
</script>

<script setup lang="ts">
import Popover from "primevue/popover";
import { onBeforeUnmount } from "vue";
import { useInfoPopover } from "../../../shared/composables/useInfoPopover";

defineProps<{
  word: string;
}>();

const {
  triggerId,
  panelId,
  popoverPt,
  setPopover,
  visible,
  show,
  hide,
  onShow,
  onHide,
} = useInfoPopover();

function close() {
  hide();
  if (closeActiveClosestWordPopover === close) closeActiveClosestWordPopover = null;
}

function toggle(event: Event) {
  if (closeActiveClosestWordPopover === close) {
    close();
    return;
  }

  closeActiveClosestWordPopover?.();
  closeActiveClosestWordPopover = close;
  show(event);
}

function handleHide() {
  onHide();
  if (closeActiveClosestWordPopover === close) closeActiveClosestWordPopover = null;
}

onBeforeUnmount(() => {
  if (closeActiveClosestWordPopover === close) closeActiveClosestWordPopover = null;
});
</script>

<template>
  <span class="moderation-closest-word">
    <button
      :id="triggerId"
      class="moderation-closest-word__content"
      type="button"
      :aria-describedby="panelId"
      :aria-controls="panelId"
      :aria-expanded="visible"
      @click.stop="toggle"
    >
      {{ word }}
    </button>

    <Popover
      :ref="setPopover"
      :pt="popoverPt"
      class="moderation-closest-word__popover info-popover"
      @show="onShow"
      @hide="handleHide"
    >
      <span class="moderation-closest-word__full">{{ word }}</span>
    </Popover>
  </span>
</template>

<style scoped>
.moderation-closest-word {
  display: block;
}

.moderation-closest-word__content {
  width: 100%;
  min-width: 0;
  padding: 0;
  border: 0;
  display: block;
  overflow: hidden;
  background: transparent;
  color: inherit;
  cursor: pointer;
  font: inherit;
  text-align: left;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.moderation-closest-word__content:focus-visible {
  border-radius: 2px;
  outline: 2px solid var(--color-primary-600);
  outline-offset: 2px;
}

.moderation-closest-word__full {
  display: block;
  max-width: min(320px, calc(100vw - 48px));
  color: var(--color-gray-700);
  font-size: var(--info-popover-font-size);
  line-height: 1.45;
  overflow-wrap: anywhere;
}
</style>
