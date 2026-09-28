<script lang="ts">
let closeActivePlayerNamePopover: (() => void) | null = null;
</script>

<script setup lang="ts">
import Popover from "primevue/popover";
import { onBeforeUnmount } from "vue";
import { useInfoPopover } from "../../../shared/composables/useInfoPopover";

defineProps<{
  playerName: string;
  emphasized?: boolean;
}>();

const {
  triggerId: fullNameTriggerId,
  panelId: fullNamePanelId,
  popoverPt: fullNamePopoverPt,
  setPopover: setFullNamePopover,
  visible: fullNamePopoverVisible,
  show: showFullName,
  hide: hideFullName,
  onShow: onFullNamePopoverShow,
  onHide: onFullNamePopoverHide,
} = useInfoPopover();

function closeFullName() {
  hideFullName();
  if (closeActivePlayerNamePopover === closeFullName) closeActivePlayerNamePopover = null;
}

function toggleFullName(event: Event) {
  if (closeActivePlayerNamePopover === closeFullName) {
    closeFullName();
    return;
  }

  closeActivePlayerNamePopover?.();
  closeActivePlayerNamePopover = closeFullName;
  showFullName(event);
}

function handleFullNamePopoverHide() {
  onFullNamePopoverHide();
  if (closeActivePlayerNamePopover === closeFullName) closeActivePlayerNamePopover = null;
}

onBeforeUnmount(() => {
  if (closeActivePlayerNamePopover === closeFullName) closeActivePlayerNamePopover = null;
});
</script>

<template>
  <span class="leaderboard-player-name" :class="{ 'leaderboard-player-name--emphasized': emphasized }">
    <button
      :id="fullNameTriggerId"
      class="leaderboard-player-name__content leaderboard-player-name__content--interactive"
      type="button"
      :aria-describedby="fullNamePanelId"
      :aria-controls="fullNamePanelId"
      :aria-expanded="fullNamePopoverVisible"
      @click.stop="toggleFullName"
    >
      {{ playerName }}
    </button>

    <Popover
      :ref="setFullNamePopover"
      :pt="fullNamePopoverPt"
      class="leaderboard-player-name__popover info-popover"
      @show="onFullNamePopoverShow"
      @hide="handleFullNamePopoverHide"
    >
      <span class="leaderboard-player-name__full-name">{{ playerName }}</span>
    </Popover>
  </span>
</template>

<style scoped>
.leaderboard-player-name {
  width: 100%;
  min-width: 0;
  max-width: 100%;
  display: block;
  color: var(--color-gray-900);
  font-weight: 400;
}

.leaderboard-player-name--emphasized {
  font-weight: var(--leaderboard-emphasis-font-weight);
}

.leaderboard-player-name__content {
  width: auto;
  min-width: 0;
  max-width: 100%;
  display: inline-block;
  overflow: hidden;
  text-overflow: ellipsis;
  vertical-align: top;
  white-space: nowrap;
}

.leaderboard-player-name__content--interactive {
  padding: 0;
  border: 0;
  background: transparent;
  color: inherit;
  font: inherit;
  text-align: left;
  cursor: pointer;
}

.leaderboard-player-name__content--interactive:focus-visible {
  border-radius: 2px;
  outline: none;
  box-shadow: var(--focus-ring-primary);
}

.leaderboard-player-name__full-name {
  display: block;
  max-width: min(320px, calc(100vw - 48px));
  color: var(--color-gray-700);
  font-size: var(--info-popover-font-size);
  line-height: 1.45;
  overflow-wrap: anywhere;
}
</style>
