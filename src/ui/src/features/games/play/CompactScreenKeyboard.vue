<script setup lang="ts">
import Dialog from "primevue/dialog";
import { useDelayedLoading } from "../../../shared/composables/useDelayedLoading";
import UiButton from "../../../shared/ui/UiButton.vue";

const props = defineProps<{
  visible: boolean;
  disabled: boolean;
  submitting: boolean;
  infoOpen: boolean;
}>();

const disabledVisible = useDelayedLoading(() => props.disabled);
const submittingVisible = useDelayedLoading(() => props.submitting);

const emit = defineEmits<{
  append: [character: string];
  removeLast: [];
  submit: [];
  show: [];
  hide: [];
  "update:infoOpen": [value: boolean];
}>();

const keyboardRows = [
  ["й", "ц", "у", "к", "е", "н", "г", "ш", "щ", "з", "х", "ъ"],
  ["ф", "ы", "в", "а", "п", "р", "о", "л", "д", "ж", "э"],
  ["я", "ч", "с", "м", "и", "т", "ь", "б", "ю"],
] as const;

function setInfoOpen(value: boolean) {
  emit("update:infoOpen", value);
}
</script>

<template>
  <div class="compact-keyboard" :class="{ 'compact-keyboard--collapsed': !visible }" role="group"
    aria-label="Экранная клавиатура">
    <template v-if="visible">
      <div class="compact-keyboard__row">
        <button v-for="character in keyboardRows[0]" :key="character" class="compact-keyboard__key" type="button"
          :disabled="disabledVisible" @click="emit('append', character)">{{ character }}</button>
      </div>

      <div class="compact-keyboard__row compact-keyboard__row--middle">
        <button v-for="character in keyboardRows[1]" :key="character" class="compact-keyboard__key" type="button"
          :disabled="disabledVisible" @click="emit('append', character)">{{ character }}</button>
      </div>

      <div class="compact-keyboard__row compact-keyboard__row--lower">
        <button class="compact-keyboard__key" type="button" :disabled="disabledVisible" @click="emit('append', 'ё')">ё</button>
        <button v-for="character in keyboardRows[2]" :key="character" class="compact-keyboard__key" type="button"
          :disabled="disabledVisible" @click="emit('append', character)">{{ character }}</button>
        <button class="compact-keyboard__key" type="button" :disabled="disabledVisible" @click="emit('append', '-')">-</button>
        <button class="compact-keyboard__key compact-keyboard__key--action compact-keyboard__key--backspace"
          type="button" :disabled="disabledVisible" aria-label="Удалить последнюю букву" @click="emit('removeLast')">
          <i class="pi pi-delete-left" aria-hidden="true"></i>
        </button>
      </div>

      <div class="compact-keyboard__controls-row">
        <div class="compact-keyboard__utility-keys">
          <button class="compact-keyboard__key compact-keyboard__key--action compact-keyboard__key--toggle"
            type="button" aria-label="Свернуть экранную клавиатуру" @click="emit('hide')">
            <i class="pi pi-chevron-down" aria-hidden="true"></i>
          </button>
          <button class="compact-keyboard__key compact-keyboard__key--action compact-keyboard__key--help"
            type="button" aria-label="Об экранной клавиатуре" @click="setInfoOpen(true)">
            <span aria-hidden="true">?</span>
          </button>
        </div>
        <div class="compact-keyboard__special-keys">
          <UiButton class="compact-keyboard__key compact-keyboard__key--action compact-keyboard__key--submit"
            type="button" :loading="submittingVisible" :disabled="disabledVisible" loading-label="Отправка" aria-label="Отправить слово"
            title="Отправить слово" @click="emit('submit')">
            <i class="pi pi-arrow-right" aria-hidden="true"></i>
          </UiButton>
        </div>
      </div>
    </template>

    <div v-else class="compact-keyboard__collapsed-row">
      <div class="compact-keyboard__utility-keys">
        <button class="compact-keyboard__key compact-keyboard__key--action compact-keyboard__key--toggle"
          type="button" aria-label="Раскрыть экранную клавиатуру" @click="emit('show')">
          <i class="pi pi-chevron-up" aria-hidden="true"></i>
        </button>
        <button class="compact-keyboard__key compact-keyboard__key--action compact-keyboard__key--help" type="button"
          aria-label="Об экранной клавиатуре" @click="setInfoOpen(true)">
          <span aria-hidden="true">?</span>
        </button>
      </div>
    </div>
  </div>

  <Dialog :visible="infoOpen" modal dismissable-mask header="Экранная клавиатура" class="game-dialog"
    @update:visible="setInfoOpen">
    <div class="game-dialog__body">
      <p class="game-dialog__text">
        Экранная клавиатура является альтернативой стандартной клавиатуры на мобильных устройствах.
        Она содержит только необходимые для игры кнопки и занимает меньше места по высоте, оставляя больше
        пространства
        для игры.
      </p>
      <p class="game-dialog__text">
        При нажатии в поле ввода откроется ваша обычная клавиатура.
      </p>
      <p class="game-dialog__text">
        Экранную клавиатуру можно в любой момент включить или выключить через пункт «Экранная клавиатура» в меню
        <span class="compact-keyboard-info__menu-button" role="img" aria-label="Кнопка меню">
          <i class="pi pi-ellipsis-v" aria-hidden="true"></i>
        </span>
      </p>

      <div class="game-dialog__actions">
        <UiButton size="md" @click="setInfoOpen(false)">
          Понятно
        </UiButton>
      </div>
    </div>
  </Dialog>
</template>

<style scoped>
.compact-keyboard {
  position: fixed;
  z-index: 20;
  bottom: 0;
  left: 50%;
  width: min(100%, var(--container-sm));
  transform: translateX(-50%);
  display: flex;
  flex-direction: column;
  gap: 4px;
  padding: 8px 12px calc(8px + env(safe-area-inset-bottom));
  border-top: 1px solid var(--color-gray-300);
  border-radius: 10px 10px 0 0;
  background: var(--color-gray-100);
}

:global(.game-board:has(.compact-keyboard)) {
  padding-bottom: calc(166px + env(safe-area-inset-bottom));
}

:global(.game-board:has(.compact-keyboard--collapsed)) {
  padding-bottom: calc(50px + env(safe-area-inset-bottom));
}

.compact-keyboard__collapsed-row,
.compact-keyboard__row {
  --compact-keyboard-gap: 3px;
  min-width: 0;
  display: flex;
  align-items: center;
  gap: var(--compact-keyboard-gap);
}

.compact-keyboard__controls-row {
  min-width: 0;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 3px;
}

.compact-keyboard__row--middle {
  padding-inline: calc(4.166667% + 0.125px);
}

.compact-keyboard__key {
  flex: 1 1 0;
  min-width: 0;
  height: 34px;
  padding: 0;
  border: 1px solid var(--color-gray-300);
  border-radius: 6px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: white;
  color: var(--color-gray-800);
  font-size: 14px;
  font-weight: 500;
  line-height: 1;
  text-transform: uppercase;
  cursor: pointer;
  touch-action: manipulation;
}

@media (hover: hover) and (pointer: fine) {
  .compact-keyboard__key:active {
    border-color: var(--color-primary-500);
    background: var(--color-primary-50);
    color: var(--color-primary-700);
  }
}

.compact-keyboard__key:focus-visible {
  outline: 2px solid var(--color-primary-600);
  outline-offset: 1px;
}

.compact-keyboard__key:disabled {
  cursor: wait;
  opacity: 0.55;
}

.compact-keyboard__key--action {
  flex: 0 0 14%;
  border-color: var(--color-gray-400);
  background: var(--color-gray-50);
  color: var(--color-gray-700);
  font-size: 15px;
}

.compact-keyboard__key--backspace {
  flex: 1 1 0;
}

.compact-keyboard__special-keys {
  width: calc(25% - 2.25px);
  height: 34px;
  display: flex;
  gap: 3px;
}

.compact-keyboard__utility-keys {
  flex: 0 0 calc(33.333333% - 2px);
  height: 34px;
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 3px;
}

.compact-keyboard__key--toggle {
  grid-column: span 1;
  width: auto;
}

.compact-keyboard__key--help {
  grid-column: span 1;
}

.compact-keyboard__key--help>span {
  font-size: 1.35em;
  font-weight: 500;
  line-height: 1;
}

.compact-keyboard__key--submit {
  flex: 1 1 0;
  border-color: var(--p-primary-color);
  background: var(--p-primary-color);
  color: var(--p-primary-contrast-color);
}

.compact-keyboard__key--submit.p-button {
  min-width: 0;
  min-height: 0;
  padding: 0;
  font-weight: 500;
}

.compact-keyboard__key--submit.p-button:disabled {
  opacity: 0.75;
}

.compact-keyboard__key--submit.p-button:not(:disabled):active {
  border-color: var(--p-primary-active-color);
  background: var(--p-primary-active-color);
  color: var(--p-primary-contrast-color);
}

.compact-keyboard__key--submit.p-button:focus-visible {
  color: var(--p-primary-contrast-color);
}

@media (hover: hover) and (pointer: fine) {
  .compact-keyboard__key--submit.p-button:not(:disabled):hover {
    border-color: var(--p-primary-hover-color);
    background: var(--p-primary-hover-color);
    color: var(--p-primary-contrast-color);
  }
}

.compact-keyboard__key--action .pi {
  font-size: 15px;
}

.compact-keyboard__key--action.compact-keyboard__key--backspace .pi {
  font-size: 20px;
}

.compact-keyboard-info__menu-button {
  width: 28px;
  height: 28px;
  margin-left: 3px;
  border: 1px solid var(--color-gray-300);
  border-radius: 6px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  vertical-align: middle;
  background: white;
  color: var(--color-gray-700);
}

.compact-keyboard-info__menu-button .pi {
  font-size: 13px;
}

.game-dialog__body {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.game-dialog__text {
  margin: 0;
  color: var(--color-gray-600);
  font-size: var(--game-dialog-body-font-size);
  line-height: 1.45;
}

.game-dialog__actions {
  display: flex;
  gap: 10px;
  padding-top: 4px;
}

.game-dialog__actions>.ui-button {
  flex: 1;
}

@media (max-width: 480px) {
  .compact-keyboard {
    gap: 5px;
    padding: 8px 6px calc(8px + env(safe-area-inset-bottom));
  }

  :global(.game-board:has(.compact-keyboard)) {
    padding-bottom: calc(194px + env(safe-area-inset-bottom));
  }

  :global(.game-board:has(.compact-keyboard--collapsed)) {
    padding-bottom: calc(56px + env(safe-area-inset-bottom));
  }

  .compact-keyboard__collapsed-row,
  .compact-keyboard__row {
    --compact-keyboard-gap: 2px;
    gap: 2px;
  }

  .compact-keyboard__controls-row,
  .compact-keyboard__special-keys,
  .compact-keyboard__utility-keys {
    gap: 2px;
  }

  .compact-keyboard__special-keys {
    width: calc(25% - 1.5px);
  }

  .compact-keyboard__row--middle {
    padding-inline: calc(4.166667% + 0.083333px);
  }

  .compact-keyboard__key,
  .compact-keyboard__special-keys,
  .compact-keyboard__utility-keys {
    height: 40px;
  }

  .compact-keyboard__key {
    font-size: 15px;
  }

  .compact-keyboard__key--toggle {
    padding-inline: 0;
  }

  .compact-keyboard__utility-keys {
    flex-basis: calc(33.333333% - 1.333333px);
  }
}

@media (min-width: 481px) and (max-width: 767px) {
  :global(.game-board:has(.compact-keyboard)) {
    padding-bottom: calc(198px + env(safe-area-inset-bottom));
  }

  :global(.game-board:has(.compact-keyboard--collapsed)) {
    padding-bottom: calc(60px + env(safe-area-inset-bottom));
  }

  .compact-keyboard__key,
  .compact-keyboard__special-keys,
  .compact-keyboard__utility-keys {
    height: 42px;
  }

  .compact-keyboard__key {
    font-size: 16px;
  }

  .compact-keyboard__utility-keys {
    flex-basis: calc(33.333333% - 2px);
  }
}

@media (min-width: 768px) {
  .compact-keyboard {
    gap: 6px;
    padding: 10px 14px calc(10px + env(safe-area-inset-bottom));
    border-radius: 12px 12px 0 0;
  }

  :global(.game-board:has(.compact-keyboard)) {
    padding-bottom: calc(210px + env(safe-area-inset-bottom));
  }

  :global(.game-board:has(.compact-keyboard--collapsed)) {
    padding-bottom: calc(62px + env(safe-area-inset-bottom));
  }

  .compact-keyboard__collapsed-row,
  .compact-keyboard__row {
    --compact-keyboard-gap: 4px;
    gap: 4px;
  }

  .compact-keyboard__controls-row,
  .compact-keyboard__special-keys,
  .compact-keyboard__utility-keys {
    gap: 4px;
  }

  .compact-keyboard__special-keys {
    width: calc(25% - 3px);
  }

  .compact-keyboard__row--middle {
    padding-inline: calc(4.166667% + 0.166667px);
  }

  .compact-keyboard__key,
  .compact-keyboard__special-keys,
  .compact-keyboard__utility-keys {
    height: 42px;
  }

  .compact-keyboard__key {
    border-radius: 8px;
    font-size: 16px;
  }

  .compact-keyboard__key--action,
  .compact-keyboard__key--action .pi {
    font-size: 17px;
  }

  .compact-keyboard__key--toggle {
    width: auto;
  }

  .compact-keyboard__utility-keys {
    flex-basis: calc(33.333333% - 2.666667px);
  }
}
</style>
