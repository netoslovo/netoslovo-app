<script setup lang="ts">
import { nextTick, onBeforeUnmount, onMounted, ref, watch } from "vue";
import UiIconButton from "../../../shared/ui/UiIconButton.vue";
import UiInput from "../../../shared/ui/UiInput.vue";

type InputRef = {
  $el?: HTMLElement;
  focusInput: () => void;
};

const props = defineProps<{
  modelValue: string;
  loading: boolean;
  submit: (word: string) => Promise<{ clear: boolean }>;
}>();

const emit = defineEmits<{
  "update:modelValue": [value: string];
}>();

const inputField = ref<InputRef | null>(null);
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

onMounted(() => {
  document.addEventListener("pointerdown", clearRequiredErrorOutsideInput);
  void focusInput();
});

onBeforeUnmount(() => {
  document.removeEventListener("pointerdown", clearRequiredErrorOutsideInput);
});

defineExpose({ submit: onSubmit, focusInput });

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
  await nextTick();
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

function validateWord(value: string, showRequired: boolean) {
  if (!value) return showRequired ? "Введите слово" : null;
  if (!/^[а-яА-ЯёЁ-]+$/.test(value)) return "Введите слово на русском языке";
  if (value.length > 25) return "Слово не должно быть длиннее 25 символов";
  return null;
}
</script>

<template>
  <form class="guess-form" autocomplete="off" @submit.prevent="onSubmit">
    <div class="guess-form__row">
      <UiInput ref="inputField" :model-value="modelValue" label="Введите слово" label-visually-hidden name="word"
        placeholder="Введите слово" :error="error" :disabled="loading || localSubmitting" autocomplete="off"
        autocorrect="off" autocapitalize="none" :spellcheck="false" input-mode="text" enter-key-hint="send"
        error-presentation="popover" :shake-key="invalidSubmitCount"
        @update:model-value="emit('update:modelValue', $event)">
        <template #right="{ error: inputError, errorVisible, showError, hideError }">
          <button v-if="inputError" type="button" class="guess-form__submit guess-form__submit--error"
            :aria-label="inputError" @mouseenter="showInputError($event, showError)" @mouseleave="hideError"
            @pointerdown.prevent.stop="toggleInputError($event, errorVisible, showError, hideError)"
            @focus="showInputError($event, showError)" @blur="hideError" @click.stop>
            <span class="guess-form__error-icon">!</span>
          </button>
          <UiIconButton v-else class="guess-form__submit" type="submit" label="Отправить слово"
            loading-label="Отправка" :loading="localSubmitting" :disabled="loading">
            <i class="pi pi-arrow-right" aria-hidden="true"></i>
          </UiIconButton>
        </template>
      </UiInput>

      <div class="guess-form__controls">
        <slot name="controls"></slot>
      </div>
    </div>
  </form>
</template>

<style scoped>
.guess-form { width: 100%; min-width: 0; }
.guess-form__row { min-width: 0; display: grid; grid-template-columns: minmax(0, 1fr) auto; align-items: center; gap: 8px; }
.guess-form__controls { display: flex; align-items: center; gap: 8px; }
.guess-form :deep(.ui-input) { padding-inline-start: 10px; }
.guess-form :deep(.ui-input-wrap__addon--right.p-inputgroupaddon) { min-width: 48px; padding: 0; }
.guess-form__submit { align-self: stretch; width: 48px; border: 0; padding: 0; display: inline-flex; align-items: center; justify-content: center; background: transparent; color: var(--color-primary-600); cursor: pointer; transition: background-color 0.18s ease, color 0.18s ease; }
.guess-form__submit:disabled { color: var(--color-gray-500); cursor: default; }
.guess-form__submit:focus-visible { outline: 2px solid var(--p-primary-hover-color); outline-offset: -4px; }
.guess-form__submit:not(.guess-form__submit--error):not(:disabled):active { background: var(--color-gray-100); color: var(--color-primary-600); }
.guess-form__submit--error { color: var(--color-red-600); cursor: help; }
.guess-form__submit--error:focus-visible { outline-color: rgba(177, 70, 70, 0.28); }
.guess-form__submit :deep(.pi) { font-size: 17px; }
.guess-form__error-icon { width: 20px; height: 20px; border-radius: 50%; display: inline-flex; align-items: center; justify-content: center; background: var(--color-red-600); color: white; font-size: 14px; font-weight: 700; }
.guess-form__submit--error:hover .guess-form__error-icon { background: #983737; }

@media (hover: hover) and (pointer: fine) {
  .guess-form__submit:not(.guess-form__submit--error):not(:disabled):hover { background: var(--color-gray-100); color: var(--color-primary-600); }
  .guess-form__submit--error:hover { background: rgba(177, 70, 70, 0.08); color: #983737; }
}

@media (max-width: 480px) {
  .guess-form__row, .guess-form__controls { gap: 6px; }
  .guess-form :deep(.ui-input-wrap.p-inputgroup) { height: var(--guess-control-height); }
  .guess-form :deep(.ui-input-wrap.p-inputgroup .ui-input.p-inputtext) { min-height: 0; height: 100%; font-size: 17px; }
  .guess-form :deep(.ui-input-wrap__addon--right.p-inputgroupaddon), .guess-form__submit { width: 46px; min-width: 46px; }
}

@media (max-width: 359px) {
  .guess-form__row, .guess-form__controls { gap: 4px; }
  .guess-form :deep(.ui-input-wrap.p-inputgroup .ui-input.p-inputtext) { font-size: 16px; }
  .guess-form :deep(.ui-input-wrap__addon--right.p-inputgroupaddon), .guess-form__submit { width: 42px; min-width: 42px; }
}
</style>
