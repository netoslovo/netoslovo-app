<script setup lang="ts">
import Popover from "primevue/popover";
import { computed } from "vue";
import { useInfoPopover } from "../../../shared/composables/useInfoPopover";
import type { DisplayWord, GameWord } from "../model/game";

const props = defineProps<{
  gameWord: GameWord | null;
}>();

function fromSecretWord(secretWord: string): DisplayWord {
  return {
    cells: [...secretWord].map((value) => ({ value, revealed: true })),
  };
}

const displayWord = computed((): DisplayWord | null => {
  const gameWord = props.gameWord;
  if (gameWord === null || gameWord.status === "unavailable") return null;
  if (gameWord.status === "secret" || gameWord.status === "displayAndSecret") {
    return fromSecretWord(gameWord.secretWord);
  }
  return gameWord.displayWord;
});
const cells = computed(() => displayWord.value?.cells ?? []);

const isHiddenForToday = computed(
  () => props.gameWord?.status === "legacy"
    ? props.gameWord.displayWord.hideReason === "hiddenForToday"
    : props.gameWord?.status === "display"
      && props.gameWord.secretWordUnavailableReason === "surrenderedHiddenForToday",
);
const isUnavailable = computed(() => props.gameWord?.status === "unavailable");

const showHiddenInfo = computed(
  () => isHiddenForToday.value && cells.value.length > 0,
);

const {
  triggerId: hiddenInfoTriggerId,
  panelId: hiddenInfoPanelId,
  popoverPt: hiddenInfoPopoverPt,
  setPopover: setHiddenInfoPopover,
  visible: hiddenInfoPopoverVisible,
  show: showHiddenInfoPopover,
  hide: hideHiddenInfoPopover,
  onPointerDown: onHiddenInfoPointerDown,
  onShow: onHiddenInfoPopoverShow,
  onHide: onHiddenInfoPopoverHide,
} = useInfoPopover();

const tiles = computed(() =>
  cells.value.map((cell) => {
    if (!cell.revealed || !cell.value) {
      return null;
    }

    const value = cell.value.toUpperCase();
    return value === " " ? null : value;
  }),
);

const ariaLabel = computed(() => {
  if (!displayWord.value) {
    return null;
  }

  if (isHiddenForToday.value) {
    return "Слово скрыто, пока активна эта игра дня";
  }

  if (cells.value.length === 0) {
    return "Длина слова неизвестна";
  }

  return `Слово из ${cells.value.length} букв`;
});
</script>

<template>
  <div v-if="displayWord" class="history-display-word" :aria-label="ariaLabel ?? undefined">
    <span v-if="isHiddenForToday && cells.length === 0" class="history-display-word__unknown-caption">
      Слово скрыто до завтра
    </span>

    <span v-else-if="cells.length === 0" class="history-display-word__unknown-caption">
      Нет доп. информации о слове
    </span>

    <template v-if="cells.length > 0">
      <span v-for="(tile, index) in tiles" :key="index" class="history-display-word__tile"
        :class="{ 'history-display-word__tile--revealed': tile }">
        <span class="history-display-word__letter">
          {{ tile ?? "" }}
        </span>
      </span>
    </template>

    <button v-if="showHiddenInfo" class="history-display-word__info"
      :class="{ 'history-display-word__info--open': hiddenInfoPopoverVisible }" type="button"
      :id="hiddenInfoTriggerId" :aria-describedby="hiddenInfoPanelId" aria-label="Почему слово скрыто"
      @mouseenter="showHiddenInfoPopover" @mouseleave="hideHiddenInfoPopover"
      @pointerdown="onHiddenInfoPointerDown" @focus="showHiddenInfoPopover" @blur="hideHiddenInfoPopover" @click.stop>
      <i class="pi pi-info-circle" aria-hidden="true"></i>
    </button>

    <Popover :ref="setHiddenInfoPopover" :pt="hiddenInfoPopoverPt" class="history-display-word__popover info-popover"
      @show="onHiddenInfoPopoverShow" @hide="onHiddenInfoPopoverHide">
      <p class="history-display-word__popover-content">
        Слово скрыто до завтра
      </p>
    </Popover>
  </div>
  <span v-else-if="isUnavailable" class="history-display-word__unknown-caption">
    Слово недоступно
  </span>
  <span v-else class="history-display-word__unknown-caption">
    Игра не начата
  </span>
</template>

<style scoped>
.history-display-word {
  --history-display-word-tile-size: 19px;
  --history-display-word-max-width: 260px;
  min-width: 0;
  width: 100%;
  max-width: min(100%, var(--history-display-word-max-width));
  display: flex;
  align-items: flex-end;
  justify-content: flex-start;
  flex-wrap: wrap;
  gap: 4px;
}

.history-display-word__tile {
  width: var(--history-display-word-tile-size);
  height: 22px;
  border-bottom: 1px solid var(--color-gray-400);
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.history-display-word__tile--revealed {
  border-bottom-color: var(--color-gray-800);
}

.history-display-word__letter {
  color: var(--color-gray-800);
  font-size: 14px;
  font-weight: 500;
  line-height: 1;
}

.history-display-word__info {
  width: 22px;
  min-width: 22px;
  height: 22px;
  border: 0;
  border-radius: 50%;
  padding: 0;
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

.history-display-word__info--open {
  background: var(--color-gray-100);
  color: var(--color-gray-800);
}

@media (hover: hover) and (pointer: fine) {
  .history-display-word__info:hover {
    background: var(--color-gray-100);
    color: var(--color-gray-800);
  }
}

.history-display-word__info:focus-visible {
  outline: none;
  box-shadow: var(--focus-ring-primary);
}

.history-display-word__info .pi {
  font-size: 14px;
  line-height: 1;
}

.history-display-word__popover-content {
  max-width: min(240px, calc(100vw - 24px));
  margin: 0;
  color: var(--color-gray-700);
  font-size: var(--info-popover-font-size);
  line-height: 1.35;
}

.history-display-word__unknown-caption {
  min-height: 22px;
  display: inline-flex;
  align-items: center;
  color: var(--p-text-muted-color);
  font-size: 12px;
  font-weight: 400;
  line-height: 1.2;
}

@media (max-width: 360px) {
  .history-display-word {
    --history-display-word-tile-size: 17px;
    gap: 3px;
  }
}

@media (max-width: 520px) {
  .history-display-word {
    justify-content: center;
  }
}
</style>
