<script setup lang="ts">
import Popover from "primevue/popover";
import { computed, onBeforeUnmount, ref, watch } from "vue";
import { useInfoPopover } from "../../../shared/composables/useInfoPopover";

const props = withDefaults(
  defineProps<{
    active?: boolean;
    word: string;
    value: number;
    fillPercentage: number;
    current?: boolean;
    highlighted?: boolean;
    hint?: boolean;
    animateHintReveal?: boolean;
    animateFillOnMount?: boolean;
    fillAnimationStart?: number;
    fillAnimationKey?: number;
    feedback?: "insert" | "repeat" | null;
  }>(),
  {
    active: true,
    current: false,
    highlighted: false,
    hint: false,
    animateHintReveal: false,
    animateFillOnMount: false,
    fillAnimationStart: 0,
    fillAnimationKey: 0,
    feedback: null,
  },
);

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
watch(
  () => props.hint,
  (hint) => {
    if (!hint) {
      hideHintInfo();
      onHintPopoverHide();
    }
  },
);

const barShell = ref<HTMLElement | null>(null);
let repeatAnimation: Animation | null = null;

const fillAnimating = ref(false);
const hintAnimating = ref(false);
let consumedEvent: number | undefined;

watch(
  [() => props.fillAnimationKey, () => props.active],
  () => {
    repeatAnimation?.cancel();
    repeatAnimation = null;
    fillAnimating.value = false;
    hintAnimating.value = false;
    const fresh = consumedEvent !== props.fillAnimationKey;
    consumedEvent = props.fillAnimationKey;
    if (!props.active || !fresh || window.matchMedia("(prefers-reduced-motion: reduce)").matches) return;
    fillAnimating.value = props.animateFillOnMount;
    hintAnimating.value = props.animateHintReveal && !props.feedback;
    if (props.feedback !== "repeat") return;

    repeatAnimation = barShell.value?.animate(
      [
        { transform: "translateX(0)", offset: 0 },
        { transform: "translateX(-3px)", offset: 0.35 },
        { transform: "translateX(2px)", offset: 0.7 },
        { transform: "translateX(0)", offset: 1 },
      ],
      { duration: 240, easing: "ease-out" },
    ) ?? null;
  },
  { immediate: true, flush: "post" },
);

onBeforeUnmount(() => repeatAnimation?.cancel());

const toneClass = computed(() =>
  props.fillPercentage < 25
    ? "guess-bar--red"
    : props.fillPercentage < 50
      ? "guess-bar--orange"
      : props.fillPercentage < 80
        ? "guess-bar--yellow"
        : "guess-bar--green",
);

const fillStyle = computed(() => ({
  width: `${props.fillPercentage}%`,
  "--guess-fill-start-scale": props.fillPercentage > 0
    ? String(props.fillAnimationStart / props.fillPercentage)
    : "0",
}));

</script>

<template>
  <div
    ref="barShell"
    class="guess-bar-shell"
    @animationend.self="hintAnimating = false" @animationcancel.self="hintAnimating = false"
    :class="{
      'guess-bar-shell--revealed': hintAnimating,
    }"
  >
    <div
      class="guess-bar"
      :class="[
        toneClass,
        {
          'guess-bar--current': current,
          'guess-bar--highlighted': highlighted,
        },
      ]"
    >
      <div
        :key="fillAnimationKey"
        class="guess-bar__fill"
        :class="{ 'guess-bar__fill--revealed': fillAnimating }"
        :style="fillStyle"
        @animationend="fillAnimating = false" @animationcancel="fillAnimating = false"
      ></div>
      <span class="guess-bar__word">{{ word }}</span>
      <span class="guess-bar__value">{{ value }}</span>
    </div>
    <button
      v-if="hint"
      class="guess-bar__hint-marker"
      :class="{ 'guess-bar__hint-marker--open': hintPopoverVisible }"
      type="button"
      :id="hintTriggerId"
      :aria-describedby="hintPanelId"
      aria-label="Подсказка"
      @mouseenter="showHintInfo"
      @mouseleave="hideHintInfo"
      @pointerdown="onHintPointerDown"
      @focus="showHintInfo"
      @blur="hideHintInfo"
      @click.stop
    >
      <i class="pi pi-lightbulb" aria-hidden="true"></i>
    </button>
    <Popover
      v-if="hint"
      :ref="setHintPopover"
      :pt="hintPopoverPt"
      class="guess-bar__hint-popover info-popover"
      @show="onHintPopoverShow"
      @hide="onHintPopoverHide"
    >
      <p class="guess-bar__hint-popover-content">
        Слово открыто подсказкой
      </p>
    </Popover>
  </div>
</template>

<style scoped>
.guess-bar-shell {
  width: 100%;
  position: relative;
}

.guess-bar-shell--revealed {
  animation: hint-row-reveal 0.28s ease-out both;
}

.guess-bar {
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

.guess-bar--current {
  height: 50px;
  border-width: 2px;
}

.guess-bar--highlighted {
  height: 41px;
  border-width: 2px;
}

.guess-bar__fill {
  height: 100%;
  border-radius: 4px;
  transform-origin: left center;
}

.guess-bar__fill--revealed {
  animation: guess-fill-reveal 0.5s cubic-bezier(0.2, 0.8, 0.3, 1) both;
}

.guess-bar--red .guess-bar__fill {
  background: var(--color-red-200);
}

.guess-bar--orange .guess-bar__fill {
  background: var(--color-orange-200);
}

.guess-bar--yellow .guess-bar__fill {
  background: var(--color-yellow-300);
}

.guess-bar--green .guess-bar__fill {
  background: var(--color-green-200);
}

.guess-bar__word,
.guess-bar__value {
  position: absolute;
  color: var(--color-black);
  font-size: 18px;
}

.guess-bar__word {
  left: var(--guess-content-offset, 10px);
}

.guess-bar__value {
  right: 10px;
}

.guess-bar__hint-marker {
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

.guess-bar__hint-marker--open {
  background: #fff6d8;
  color: #6f5427;
}

@media (hover: hover) and (pointer: fine) {
  .guess-bar__hint-marker:hover {
    background: #fff6d8;
    color: #6f5427;
  }
}

.guess-bar__hint-marker:focus-visible {
  outline: none;
  box-shadow: var(--focus-ring-primary);
  background: #fff6d8;
  color: #6f5427;
}

.guess-bar__hint-popover-content {
  margin: 0;
  color: var(--color-gray-700);
  font-size: var(--info-popover-font-size);
  line-height: 1.4;
}

:global(.guess-bar__hint-popover.p-popover) {
  max-width: min(240px, calc(100vw - 24px));
}

@media (max-width: 480px) {
  .guess-bar--current {
    height: 48px;
  }

  .guess-bar--current .guess-bar__word,
  .guess-bar--current .guess-bar__value {
    font-size: 17px;
  }
}

@media (max-width: 359px) {
  .guess-bar--current {
    height: 44px;
  }

  .guess-bar--current .guess-bar__word,
  .guess-bar--current .guess-bar__value {
    font-size: 16px;
  }
}

@media (prefers-reduced-motion: reduce) {
  .guess-bar-shell--revealed {
    animation: none;
  }

  .guess-bar__fill--revealed {
    animation: none;
  }
}

@media (min-width: 768px) {
  .guess-bar {
    height: 42px;
  }

  .guess-bar--current {
    height: 50px;
  }

  .guess-bar--highlighted {
    height: 45px;
  }
}

@media (width < 1024px) {
  .guess-bar-shell--revealed { animation-name: hint-row-fade; }
}

@keyframes hint-row-fade {
  from { opacity: 0; }
  to { opacity: 1; }
}

@keyframes hint-row-reveal {
  from {
    transform: translateY(-4px);
  }

  to {
    transform: translateY(0);
  }
}

@keyframes guess-fill-reveal {
  from {
    transform: scaleX(var(--guess-fill-start-scale));
  }

  to {
    transform: scaleX(1);
  }
}

</style>
