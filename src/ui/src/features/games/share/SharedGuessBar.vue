<script setup lang="ts">
import Popover from "primevue/popover";
import { computed } from "vue";
import { useInfoPopover } from "../../../shared/composables/useInfoPopover";
import type { SharedGuess } from "../model/game";

const props = defineProps<{
  guess: SharedGuess;
  hiddenWordExplanation: string;
}>();

const toneClass = computed(() =>
  props.guess.fillPercentage < 25
    ? "shared-guess-bar--red"
    : props.guess.fillPercentage < 50
      ? "shared-guess-bar--orange"
      : props.guess.fillPercentage < 80
        ? "shared-guess-bar--yellow"
        : "shared-guess-bar--green",
);

const fillStyle = computed(() => ({ width: `${props.guess.fillPercentage}%` }));

const {
  triggerId: hiddenWordTriggerId,
  panelId: hiddenWordPanelId,
  popoverPt: hiddenWordPopoverPt,
  setPopover: setHiddenWordPopover,
  visible: hiddenWordPopoverVisible,
  show: showHiddenWordInfo,
  hide: hideHiddenWordInfo,
  onPointerDown: onHiddenWordPointerDown,
  onShow: onHiddenWordPopoverShow,
  onHide: onHiddenWordPopoverHide,
} = useInfoPopover();

const {
  triggerId: hintTriggerId,
  panelId: hintPanelId,
  popoverPt: hintPopoverPt,
  setPopover: setHintPopover,
  visible: hintPopoverVisible,
  show: showHintInfo,
  hide: hideHintInfo,
  onPointerDown: onHintPointerDown,
  onShow: onHintPopoverShow,
  onHide: onHintPopoverHide,
} = useInfoPopover();
</script>

<template>
  <div class="shared-guess-bar-shell">
    <div class="shared-guess-bar" :class="toneClass">
      <div class="shared-guess-bar__fill" :style="fillStyle"></div>
      <span v-if="guess.word !== null" class="shared-guess-bar__word">{{ guess.word }}</span>
      <button v-else :id="hiddenWordTriggerId" type="button" class="shared-guess-bar__hidden-word"
        :class="{ 'shared-guess-bar__hidden-word--open': hiddenWordPopoverVisible }"
        aria-label="Почему слово скрыто" :aria-describedby="hiddenWordPanelId"
        @mouseenter="showHiddenWordInfo" @mouseleave="hideHiddenWordInfo"
        @pointerdown="onHiddenWordPointerDown" @focus="showHiddenWordInfo" @blur="hideHiddenWordInfo"
        @click.stop>
        <i class="pi pi-eye-slash" aria-hidden="true"></i>
      </button>
      <span class="shared-guess-bar__value">{{ guess.distance }}</span>
    </div>

    <button v-if="guess.source === 'hint'" :id="hintTriggerId" type="button"
      class="shared-guess-bar__hint-marker"
      :class="{ 'shared-guess-bar__hint-marker--open': hintPopoverVisible }"
      :aria-describedby="hintPanelId" aria-label="Подсказка" @mouseenter="showHintInfo"
      @mouseleave="hideHintInfo" @pointerdown="onHintPointerDown" @focus="showHintInfo"
      @blur="hideHintInfo" @click.stop>
      <i class="pi pi-lightbulb" aria-hidden="true"></i>
    </button>

    <Popover v-if="guess.word === null" :ref="setHiddenWordPopover" :pt="hiddenWordPopoverPt"
      class="shared-guess-bar__popover info-popover" @show="onHiddenWordPopoverShow"
      @hide="onHiddenWordPopoverHide">
      <p class="shared-guess-bar__popover-content">{{ hiddenWordExplanation }}</p>
    </Popover>

    <Popover v-if="guess.source === 'hint'" :ref="setHintPopover" :pt="hintPopoverPt"
      class="shared-guess-bar__popover info-popover" @show="onHintPopoverShow" @hide="onHintPopoverHide">
      <p class="shared-guess-bar__popover-content">Слово открыто подсказкой</p>
    </Popover>
  </div>
</template>

<style scoped>
.shared-guess-bar-shell {
  width: 100%;
  position: relative;
}

.shared-guess-bar {
  height: 38px;
  width: 100%;
  position: relative;
  display: flex;
  align-items: center;
  overflow: clip;
  border: 1px solid var(--color-gray-600);
  border-radius: 8px;
  background: white;
}

.shared-guess-bar__fill {
  height: 100%;
  border-radius: 4px;
}

.shared-guess-bar--red .shared-guess-bar__fill {
  background: var(--color-red-200);
}

.shared-guess-bar--orange .shared-guess-bar__fill {
  background: var(--color-orange-200);
}

.shared-guess-bar--yellow .shared-guess-bar__fill {
  background: var(--color-yellow-300);
}

.shared-guess-bar--green .shared-guess-bar__fill {
  background: var(--color-green-200);
}

.shared-guess-bar__word,
.shared-guess-bar__hidden-word,
.shared-guess-bar__value {
  position: absolute;
  color: var(--color-black);
  font-size: 18px;
}

.shared-guess-bar__word,
.shared-guess-bar__hidden-word {
  left: var(--guess-content-offset, 10px);
}

.shared-guess-bar__hidden-word {
  appearance: none;
  -webkit-tap-highlight-color: transparent;
  border: 0;
  border-radius: 5px;
  padding: 3px;
  display: inline-flex;
  align-items: center;
  background: transparent;
  color: var(--color-gray-600);
  cursor: help;
}

.shared-guess-bar__hidden-word--open {
  background: color-mix(in srgb, white 72%, transparent);
  color: var(--color-primary-700);
}

.shared-guess-bar__hidden-word .pi {
  font-size: 17px;
}

.shared-guess-bar__value {
  right: 10px;
}

.shared-guess-bar__hint-marker {
  appearance: none;
  -webkit-tap-highlight-color: transparent;
  position: absolute;
  top: -8px;
  right: -8px;
  z-index: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  width: 22px;
  height: 22px;
  border: 1px solid var(--color-orange-200);
  padding: 0;
  border-radius: 7px;
  background: white;
  box-shadow: 0 2px 7px rgba(25, 32, 43, 0.12);
  color: #8a6a36;
  cursor: help;
  font-size: 12px;
}

.shared-guess-bar__hint-marker--open {
  background: #fff6d8;
  color: #6f5427;
}

.shared-guess-bar__hidden-word:focus-visible,
.shared-guess-bar__hint-marker:focus-visible {
  outline: none;
  box-shadow: var(--focus-ring-primary);
}

.shared-guess-bar__popover-content {
  max-width: 280px;
  margin: 0;
  color: var(--color-gray-700);
  font-size: var(--info-popover-font-size);
  line-height: 1.45;
}

:global(.shared-guess-bar__popover.p-popover) {
  max-width: min(310px, calc(100vw - 24px));
}

@media (hover: hover) and (pointer: fine) {
  .shared-guess-bar__hidden-word:hover {
    background: color-mix(in srgb, white 72%, transparent);
    color: var(--color-primary-700);
  }

  .shared-guess-bar__hint-marker:hover {
    background: #fff6d8;
    color: #6f5427;
  }
}
</style>
