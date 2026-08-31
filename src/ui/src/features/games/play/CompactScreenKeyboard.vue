<script setup lang="ts">
import Dialog from "primevue/dialog";
import { useDelayedLoading } from "../../../shared/composables/useDelayedLoading";
import UiButton from "../../../shared/ui/UiButton.vue";

const props = defineProps<{
  visible: boolean;
  disabled: boolean;
  infoOpen: boolean;
}>();

const disabledVisible = useDelayedLoading(() => props.disabled);

const emit = defineEmits<{
  append: [character: string];
  removeLast: [];
  show: [];
  hide: [];
  close: [event: MouseEvent];
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
      <div class="compact-keyboard__controls-row">
        <div class="compact-keyboard__toolbar" role="group" aria-label="Управление экранной клавиатурой">
          <button class="compact-keyboard__key compact-keyboard__toolbar-button compact-keyboard__toolbar-button--close"
            type="button" aria-label="Закрыть экранную клавиатуру" title="Закрыть" @click="emit('close', $event)">
            <i class="pi pi-times" aria-hidden="true"></i>
          </button>
          <button class="compact-keyboard__key compact-keyboard__toolbar-button" type="button"
            aria-label="Свернуть экранную клавиатуру" title="Свернуть" @click="emit('hide')">
            <i class="pi pi-chevron-down" aria-hidden="true"></i>
          </button>
          <button class="compact-keyboard__key compact-keyboard__toolbar-button" type="button"
            aria-label="Справка об экранной клавиатуре" title="Справка" @click="setInfoOpen(true)">
            <i class="pi pi-question" aria-hidden="true"></i>
          </button>
        </div>
      </div>

      <div class="compact-keyboard__row">
        <button v-for="character in keyboardRows[0]" :key="character"
          class="compact-keyboard__key compact-keyboard__key--character" type="button" :disabled="disabledVisible"
          @click="emit('append', character)">
          <span class="compact-keyboard__key-preview" aria-hidden="true">{{ character }}</span>
          <span>{{ character }}</span>
        </button>
      </div>

      <div class="compact-keyboard__row compact-keyboard__row--middle">
        <button v-for="character in keyboardRows[1]" :key="character"
          class="compact-keyboard__key compact-keyboard__key--character" type="button" :disabled="disabledVisible"
          @click="emit('append', character)">
          <span class="compact-keyboard__key-preview" aria-hidden="true">{{ character }}</span>
          <span>{{ character }}</span>
        </button>
      </div>

      <div class="compact-keyboard__row compact-keyboard__row--lower">
        <button class="compact-keyboard__key compact-keyboard__key--character" type="button"
          :disabled="disabledVisible" @click="emit('append', 'ё')">
          <span class="compact-keyboard__key-preview" aria-hidden="true">ё</span>
          <span>ё</span>
        </button>
        <button v-for="character in keyboardRows[2]" :key="character"
          class="compact-keyboard__key compact-keyboard__key--character" type="button" :disabled="disabledVisible"
          @click="emit('append', character)">
          <span class="compact-keyboard__key-preview" aria-hidden="true">{{ character }}</span>
          <span>{{ character }}</span>
        </button>
        <button class="compact-keyboard__key compact-keyboard__key--character" type="button"
          :disabled="disabledVisible" @click="emit('append', '-')">
          <span class="compact-keyboard__key-preview" aria-hidden="true">-</span>
          <span>-</span>
        </button>
        <button class="compact-keyboard__key compact-keyboard__key--action compact-keyboard__key--backspace"
          type="button" :disabled="disabledVisible" aria-label="Удалить последнюю букву" @click="emit('removeLast')">
          <i class="pi pi-delete-left" aria-hidden="true"></i>
        </button>
      </div>

    </template>

    <div v-else class="compact-keyboard__collapsed-row">
      <div class="compact-keyboard__toolbar" role="group" aria-label="Управление экранной клавиатурой">
        <button class="compact-keyboard__key compact-keyboard__toolbar-button compact-keyboard__toolbar-button--close"
          type="button" aria-label="Закрыть экранную клавиатуру" title="Закрыть" @click="emit('close', $event)">
          <i class="pi pi-times" aria-hidden="true"></i>
        </button>
        <button class="compact-keyboard__key compact-keyboard__toolbar-button" type="button"
          aria-label="Развернуть экранную клавиатуру" title="Развернуть" @click="emit('show')">
          <i class="pi pi-chevron-up" aria-hidden="true"></i>
        </button>
        <button class="compact-keyboard__key compact-keyboard__toolbar-button" type="button"
          aria-label="Справка об экранной клавиатуре" title="Справка" @click="setInfoOpen(true)">
          <i class="pi pi-question" aria-hidden="true"></i>
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
        Пока экранная клавиатура включена, поле ввода показывает набранное слово, но не открывает обычную клавиатуру.
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
  width: 100%;
  display: flex;
  flex-direction: column;
  gap: 4px;
  padding: 6px 12px calc(6px + env(safe-area-inset-bottom));
  border-top: 1px solid var(--color-gray-300);
  border-inline: 1px solid var(--color-gray-300);
  border-radius: 10px 10px 0 0;
  background: var(--color-gray-100);
}

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
  justify-content: flex-start;
}

.compact-keyboard__collapsed-row {
  min-width: 0;
  display: flex;
  align-items: center;
  justify-content: flex-start;
}

.compact-keyboard__row--middle {
  padding-inline: calc(4.166667% + 0.125px);
}

.compact-keyboard__key {
  flex: 1 1 0;
  min-width: 0;
  height: 36px;
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

.compact-keyboard__key--character {
  position: relative;
}

.compact-keyboard__key-preview {
  position: absolute;
  z-index: 1;
  bottom: calc(100% - 7px);
  left: 50%;
  width: calc(100% + 16px);
  height: 54px;
  border: 1px solid var(--color-primary-300);
  border-radius: 9px 9px 11px 11px;
  display: none;
  align-items: center;
  justify-content: center;
  transform: translateX(-50%);
  background: var(--color-primary-50);
  box-shadow: 0 3px 9px rgb(0 0 0 / 18%);
  color: var(--color-primary-700);
  font-size: 24px;
  font-weight: 600;
  pointer-events: none;
}

.compact-keyboard__key--character:active {
  z-index: 2;
  border-color: var(--color-primary-500);
  background: var(--color-primary-50);
  color: var(--color-primary-700);
}

.compact-keyboard__key--character:active .compact-keyboard__key-preview {
  display: inline-flex;
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

.compact-keyboard__toolbar {
  display: flex;
  justify-content: flex-start;
  gap: 3px;
}

.compact-keyboard__key.compact-keyboard__toolbar-button {
  flex: 0 0 32px;
  width: 32px;
  height: 32px;
  border-color: transparent;
  border-radius: 6px;
  background: transparent;
  color: var(--color-gray-600);
}

.compact-keyboard__toolbar-button .pi {
  font-size: 13px;
}

.compact-keyboard__toolbar-button:active {
  border-color: var(--color-gray-300);
  background: white;
  color: var(--color-gray-800);
}

.compact-keyboard__toolbar-button--close:active {
  border-color: var(--color-red-200);
  background: var(--color-red-100);
  color: var(--color-red-700);
}

@media (hover: hover) and (pointer: fine) {
  .compact-keyboard__toolbar-button:hover {
    border-color: var(--color-gray-300);
    background: white;
    color: var(--color-gray-800);
  }

  .compact-keyboard__toolbar-button--close:hover {
    border-color: var(--color-red-200);
    background: var(--color-red-100);
    color: var(--color-red-700);
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
    gap: 4px;
    padding: 6px 6px calc(6px + env(safe-area-inset-bottom));
  }

  .compact-keyboard__row {
    --compact-keyboard-gap: 2px;
    gap: 2px;
  }

  .compact-keyboard__toolbar {
    gap: 2px;
  }

  .compact-keyboard__row--middle {
    padding-inline: calc(4.166667% + 0.083333px);
  }

  .compact-keyboard__key {
    height: 42px;
    font-size: 15px;
  }

}

@media (min-width: 481px) and (max-width: 767px) {
  .compact-keyboard__key {
    height: 44px;
    font-size: 16px;
  }

}

@media (min-width: 768px) {
  .compact-keyboard {
    gap: 5px;
    padding: 8px 14px calc(8px + env(safe-area-inset-bottom));
    border-radius: 12px 12px 0 0;
  }

  .compact-keyboard__row {
    --compact-keyboard-gap: 4px;
    gap: 4px;
  }

  .compact-keyboard__toolbar {
    gap: 4px;
  }

  .compact-keyboard__row--middle {
    padding-inline: calc(4.166667% + 0.166667px);
  }

  .compact-keyboard__key {
    height: 44px;
    border-radius: 8px;
    font-size: 16px;
  }

  .compact-keyboard__key--action,
  .compact-keyboard__key--action .pi {
    font-size: 17px;
  }

}

.compact-keyboard.compact-keyboard--collapsed {
  padding: 3px 8px calc(3px + env(safe-area-inset-bottom));
}
</style>
