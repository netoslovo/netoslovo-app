<script setup lang="ts">
import Popover from "primevue/popover";
import { nextTick, onBeforeUnmount, onMounted, ref, watch } from "vue";
import UiIconButton from "../../../shared/ui/UiIconButton.vue";
import UiInput from "../../../shared/ui/UiInput.vue";

type InputRef = {
  $el?: HTMLElement;
  focusInput: () => void;
  blurInput: () => void;
};

type PopoverRef = {
  show: (event: Event, target?: HTMLElement) => void;
  hide: () => void;
};

const props = defineProps<{
  modelValue: string;
  loading: boolean;
  inputLocked: boolean;
  submit: (word: string) => Promise<{ clear: boolean }>;
}>();

const emit = defineEmits<{
  "update:modelValue": [value: string];
}>();

const inputField = ref<InputRef | null>(null);
const lockedInputPopover = ref<PopoverRef | null>(null);
const error = ref<string | null>(null);
const hasInvalidSubmitAttempt = ref(false);
const invalidSubmitCount = ref(0);
const localSubmitting = ref(false);
const suppressClearedEmptySubmit = ref(false);

watch(() => props.modelValue, (value) => {
  const normalizedValue = value.trim();
  if (normalizedValue) suppressClearedEmptySubmit.value = false;

  error.value = validateWord(normalizedValue, hasInvalidSubmitAttempt.value);
  if (error.value === null) hasInvalidSubmitAttempt.value = false;
});

watch(() => props.inputLocked, (locked) => {
  if (locked) {
    inputField.value?.blurInput();
    return;
  }

  lockedInputPopover.value?.hide();
});

onMounted(() => {
  document.addEventListener("pointerdown", clearRequiredErrorOutsideInput);
  if (!props.inputLocked) void focusInput();
});

onBeforeUnmount(() => {
  document.removeEventListener("pointerdown", clearRequiredErrorOutsideInput);
});

function clearRequiredErrorOutsideInput(event: PointerEvent) {
  if (
    error.value !== "Введите слово"
    || !(event.target instanceof Node)
    || inputField.value?.$el?.contains(event.target)
  ) return;

  error.value = null;
  hasInvalidSubmitAttempt.value = false;
}

async function focusInput() {
  if (props.inputLocked) return;

  await nextTick();
  if (props.inputLocked) return;
  inputField.value?.focusInput();
}

async function onSubmit() {
  if (props.loading || localSubmitting.value) return;

  const normalizedWord = props.modelValue.trim();

  if (!normalizedWord && suppressClearedEmptySubmit.value) {
    suppressClearedEmptySubmit.value = false;
    error.value = null;
    hasInvalidSubmitAttempt.value = false;
    await focusInput();
    return;
  }

  const validationError = validateWord(normalizedWord, true);
  if (validationError) {
    hasInvalidSubmitAttempt.value = true;
    error.value = validationError;
    invalidSubmitCount.value += 1;
    await focusInput();
    return;
  }

  error.value = null;
  hasInvalidSubmitAttempt.value = false;
  localSubmitting.value = true;

  try {
    const result = await props.submit(normalizedWord);
    if (result.clear) {
      emit("update:modelValue", "");
      suppressClearedEmptySubmit.value = true;
    }
  } finally {
    localSubmitting.value = false;
    await focusInput();
  }
}

function getCurrentTargetElement(event: Event) {
  return event.currentTarget instanceof HTMLElement ? event.currentTarget : null;
}

function showInputError(event: Event, showError: (target?: HTMLElement | null) => void) {
  showError(getCurrentTargetElement(event));
}

function toggleInputError(
  event: Event,
  visible: boolean,
  showError: (target?: HTMLElement | null) => void,
  hideError: () => void,
) {
  if (visible) {
    hideError();
    return;
  }

  showInputError(event, showError);
}

function showLockedInputInfo(event: PointerEvent) {
  if (!props.inputLocked) return;

  event.preventDefault();
  const target = getCurrentTargetElement(event);
  if (target) lockedInputPopover.value?.show(event, target);
}

function validateWord(value: string, showRequired: boolean) {
  if (!value) return showRequired ? "Введите слово" : null;
  if (!/^[а-яА-ЯёЁ-]+$/.test(value)) return "Введите слово на русском языке";
  if (value.length > 25) return "Слово не должно быть длиннее 25 символов";
  return null;
}
</script>

<template>
  <form class="guess-form" autocomplete="off" @submit.prevent="onSubmit">
    <UiInput ref="inputField" :model-value="modelValue" class="guess-form__input" label="Введите слово"
      label-visually-hidden name="word" placeholder="Введите слово" :error="error"
      :disabled="loading || localSubmitting" autocomplete="off" autocorrect="off" autocapitalize="none"
      :spellcheck="false" :input-mode="inputLocked ? 'none' : 'text'" error-presentation="popover"
      :shake-key="invalidSubmitCount" :readonly="inputLocked" :tab-index="inputLocked ? -1 : undefined"
      @pointerdown="showLockedInputInfo" @update:model-value="emit('update:modelValue', $event)">
      <template #right="{ error: inputError, errorVisible, showError, hideError }">
        <button v-if="inputError" type="button" class="guess-form__submit guess-form__submit--error"
          :aria-label="inputError" @mouseenter="showInputError($event, showError)" @mouseleave="hideError"
          @pointerdown.prevent.stop="toggleInputError($event, errorVisible, showError, hideError)"
          @focus="showInputError($event, showError)" @blur="hideError" @click.stop>
          <span class="guess-form__error-icon">!</span>
        </button>
        <UiIconButton v-else class="guess-form__submit" type="submit" label="Отправить слово" loading-label="Отправка"
          :loading="localSubmitting" :disabled="loading || localSubmitting || !modelValue.trim()">
          <i class="pi pi-arrow-right" aria-hidden="true"></i>
        </UiIconButton>
      </template>
    </UiInput>
  </form>

  <Popover ref="lockedInputPopover" class="guess-form__locked-popover info-popover">
    <p class="guess-form__locked-popover-text">
      В режиме экранной клавиатуры поле ввода заблокировано. Используйте клавиши ниже или закройте экранную клавиатуру.
    </p>
  </Popover>
</template>

<style scoped>
.guess-form {
  width: 100%;
  min-width: 0;
}

.guess-form :deep(.ui-input) {
  padding-inline-start: 10px;
}

.guess-form__input :deep(.ui-input[readonly]) {
  caret-color: transparent;
  cursor: help;
}

.guess-form :deep(.ui-input-wrap__addon--right.p-inputgroupaddon) {
  flex: 0 0 48px;
  width: 48px;
  min-width: 48px;
  padding: 0;
}

.guess-form__submit {
  align-self: stretch;
  width: 100%;
  min-width: 0;
  border: 0;
  border-radius: 0;
  padding: 0;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: var(--guess-submit-background, transparent);
  color: var(--guess-submit-color, var(--color-primary-600));
  cursor: pointer;
  transition: background-color 0.18s ease, color 0.18s ease;
}

.guess-form__submit.ui-icon-button.p-button.p-button-outlined {
  width: 100%;
  min-width: 0;
  height: 100%;
  border: 0;
  border-radius: 0;
  padding: 0;
  background: var(--guess-submit-background, transparent);
  color: var(--guess-submit-color, var(--color-primary-600));
  box-shadow: none;
  transform: none;
}

.guess-form__submit:disabled {
  background: transparent;
  color: var(--color-gray-500);
  cursor: default;
  opacity: 1;
}

.guess-form__submit.ui-icon-button.p-button.p-button-outlined:disabled {
  border: 0;
  background: transparent;
  color: var(--color-gray-500);
  box-shadow: none;
  opacity: 1;
  transform: none;
}

.guess-form__submit:focus-visible {
  outline: 2px solid var(--p-primary-hover-color);
  outline-offset: -4px;
}

.guess-form__submit:not(.guess-form__submit--error):not(:disabled):active {
  background: var(--guess-submit-active-background, var(--color-gray-100));
  color: var(--guess-submit-active-color, var(--color-primary-600));
}

.guess-form__submit.ui-icon-button.p-button.p-button-outlined:not(:disabled):active {
  border: 0;
  background: var(--guess-submit-active-background, var(--color-gray-100));
  color: var(--guess-submit-active-color, var(--color-primary-600));
  box-shadow: none;
  transform: none;
}

.guess-form__submit--error {
  color: var(--color-red-600);
  cursor: help;
}

.guess-form__submit--error:focus-visible {
  outline-color: rgba(177, 70, 70, 0.28);
}

.guess-form__submit :deep(.pi) {
  font-size: 17px;
}

.guess-form__error-icon {
  width: 20px;
  height: 20px;
  border-radius: 50%;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: var(--color-red-600);
  color: white;
  font-size: 14px;
  font-weight: 700;
}

.guess-form__submit--error:hover .guess-form__error-icon {
  background: #983737;
}

.guess-form__locked-popover-text {
  max-width: 280px;
  margin: 0;
  color: var(--color-gray-700);
  font-size: var(--info-popover-font-size);
  line-height: 1.45;
}

:global(.guess-form__locked-popover.p-popover) {
  max-width: min(310px, calc(100vw - 24px));
}

@media (hover: hover) and (pointer: fine) {
  .guess-form__submit:not(.guess-form__submit--error):not(:disabled):hover {
    background: var(--guess-submit-hover-background, var(--color-gray-100));
    color: var(--guess-submit-hover-color, var(--color-primary-600));
  }

  .guess-form__submit.ui-icon-button.p-button.p-button-outlined:not(:disabled):hover {
    border: 0;
    background: var(--guess-submit-hover-background, var(--color-gray-100));
    color: var(--guess-submit-hover-color, var(--color-primary-600));
    box-shadow: none;
    transform: none;
  }

  .guess-form__submit--error:hover {
    background: rgba(177, 70, 70, 0.08);
    color: #983737;
  }
}

@media (max-width: 480px) {
  .guess-form :deep(.ui-input-wrap.p-inputgroup) {
    height: var(--guess-control-height);
  }

  .guess-form :deep(.ui-input-wrap.p-inputgroup .ui-input.p-inputtext) {
    min-height: 0;
    height: 100%;
    font-size: 17px;
  }

  .guess-form :deep(.ui-input-wrap__addon--right.p-inputgroupaddon) {
    flex-basis: 46px;
    width: 46px;
    min-width: 46px;
  }
}

@media (max-width: 359px) {
  .guess-form :deep(.ui-input-wrap.p-inputgroup .ui-input.p-inputtext) {
    font-size: 16px;
  }

  .guess-form :deep(.ui-input-wrap__addon--right.p-inputgroupaddon) {
    flex-basis: 42px;
    width: 42px;
    min-width: 42px;
  }
}
</style>
