<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, onMounted, ref, watch } from "vue";
import UiIconButton from "../../../shared/ui/UiIconButton.vue";
import UiInput from "../../../shared/ui/UiInput.vue";

type InputRef = {
  $el?: HTMLElement;
  focusInput: (options?: FocusOptions) => void;
};

const props = defineProps<{
  modelValue: string;
  loading: boolean;
  submit: (word: string) => Promise<{ clear: boolean }>;
}>();

const emit = defineEmits<{
  "update:modelValue": [value: string];
}>();

const mobileLayoutMedia = window.matchMedia("(width < 1024px)");
const mobileLayout = ref(mobileLayoutMedia.matches);
function updateMobileLayout() {
  mobileLayout.value = mobileLayoutMedia.matches;
}

const inputField = ref<InputRef | null>(null);
const error = ref<string | null>(null);
const hasInput = computed(() => Boolean(props.modelValue.trim()));
const displayedError = computed(() => mobileLayout.value && !hasInput.value ? null : error.value);
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
  mobileLayoutMedia.addEventListener("change", updateMobileLayout);
  document.addEventListener("pointerdown", clearRequiredErrorOutsideInput);
  void focusInput();
});

onBeforeUnmount(() => {
  mobileLayoutMedia.removeEventListener("change", updateMobileLayout);
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
  await nextTick();
  inputField.value?.focusInput({ preventScroll: true });
}

async function onSubmit() {
  if (props.loading || localSubmitting.value) return;

  const normalizedWord = props.modelValue.trim();
  if (mobileLayout.value && !normalizedWord) {
    error.value = null;
    hasInvalidSubmitAttempt.value = false;
    await focusInput();
    return;
  }

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
    <UiInput ref="inputField" :model-value="modelValue" class="guess-form__input" label="Введите слово"
      label-visually-hidden name="game-guess" type="text" placeholder="Введите слово" :error="displayedError" autocomplete="off"
      autocorrect="on" autocapitalize="none" :spellcheck="true" input-mode="text"
      enter-key-hint="send"
      error-presentation="popover"
      :shake-key="invalidSubmitCount" @update:model-value="emit('update:modelValue', $event)">
      <template #right="{ error: inputError, errorVisible, showError, hideError }">
        <slot name="before-submit"></slot>
        <button v-if="inputError" type="button" class="guess-form__submit guess-form__submit--error"
          :aria-label="inputError" @mouseenter="showInputError($event, showError)" @mouseleave="hideError"
          @pointerdown.prevent.stop="toggleInputError($event, errorVisible, showError, hideError)"
          @focus="showInputError($event, showError)" @blur="hideError" @click.stop>
          <span class="guess-form__error-icon">!</span>
        </button>
        <UiIconButton v-else class="guess-form__submit" type="submit" label="Отправить слово" loading-label="Отправка"
          :loading="localSubmitting" :disabled="loading || localSubmitting || !modelValue.trim()" @pointerdown.prevent>
          <i class="pi pi-arrow-right" aria-hidden="true"></i>
        </UiIconButton>
      </template>
    </UiInput>

  </form>

</template>

<style scoped>
.guess-form {
  width: 100%;
  min-width: 0;
}

.guess-form__input {
  min-width: 0;
}

.guess-form :deep(.ui-input) {
  padding-inline-start: 10px;
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
  outline: none;
  box-shadow: var(--focus-ring-primary);
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
  background: transparent;
  color: var(--color-red-600);
  cursor: help;
}

.guess-form__submit--error:focus-visible {
  box-shadow: var(--focus-ring-danger);
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

@media (width < 1024px) {
  .guess-form :deep(.ui-input-wrap.p-inputgroup .ui-input.p-inputtext) {
    min-height: 0;
    height: 100%;
    padding-block: 0;
    line-height: normal;
  }
}

@media (min-width: 481px) and (width < 1024px) {
  .guess-form :deep(.ui-input-wrap.p-inputgroup) {
    height: var(--guess-control-height);
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
}

@media (max-width: 359px) {
  .guess-form :deep(.ui-input-wrap.p-inputgroup .ui-input.p-inputtext) {
    font-size: 16px;
  }
}

@media (width < 1024px) {
  .guess-form :deep(.ui-input-wrap.p-inputgroup),
  .guess-form :deep(.ui-input-wrap.p-inputgroup:is(:focus-within, .ui-input-wrap--invalid)) {
    height: var(--guess-control-height);
    border: 0;
    border-radius: 0;
    background: transparent;
    box-shadow: none;
  }

  .guess-form :deep(.ui-input-wrap__addon--right.p-inputgroupaddon) {
    flex: 0 0 auto;
    width: auto;
    min-width: 0;
    border: 0;
  }

  .guess-form .guess-form__submit,
  .guess-form .guess-form__submit.ui-icon-button.p-button.p-button-outlined {
    flex: 0 0 var(--guess-control-height);
    width: var(--guess-control-height);
  }

  .guess-form .guess-form__submit--error:is(:hover, :active) {
    background: transparent;
  }
}

@media (width < 1024px) {
  .guess-form :deep(.ui-input.p-inputtext) {
    padding-inline: 8px;
  }

  .guess-form__submit :deep(.p-button-label) {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    line-height: 1;
  }

  .guess-form__submit :deep(.pi) {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 18px;
    height: 18px;
    font-size: 18px;
    line-height: 1;
  }
}

@media (width >= 1024px) {
  .guess-form :deep(.ui-input-wrap.p-inputgroup) {
    border-color: var(--color-gray-200);
    border-radius: 10px;
    box-shadow: inset 0 1px 2px rgb(25 32 43 / 3%);
  }

  .guess-form :deep(.ui-input-wrap.p-inputgroup:focus-within) {
    border-color: var(--focus-border-primary);
    box-shadow: var(--focus-ring-primary);
  }

  .guess-form :deep(.ui-input-wrap.ui-input-wrap--invalid.p-inputgroup),
  .guess-form :deep(.ui-input-wrap.ui-input-wrap--invalid.p-inputgroup:focus-within) {
    border-color: var(--focus-border-danger);
    box-shadow: var(--focus-ring-danger);
  }

  .guess-form :deep(.ui-input-wrap__addon--right.p-inputgroupaddon) {
    border-left-color: var(--color-gray-100);
  }
}

@media (width < 1024px) {
  .guess-form :deep(.ui-input-container),
  .guess-form :deep(.ui-input-wrap.p-inputgroup .ui-input.p-inputtext),
  .guess-form :deep(.ui-input-wrap.p-inputgroup .ui-input.p-inputtext:is(:enabled, :focus)),
  .guess-form :deep(.ui-input-wrap.p-inputgroup .ui-input-wrap__addon.p-inputgroupaddon) {
    background: transparent;
  }
}

</style>
