<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, ref, useId, watch } from "vue";
import InputGroup from "primevue/inputgroup";
import InputGroupAddon from "primevue/inputgroupaddon";
import InputText from "primevue/inputtext";
import FloatLabel from "primevue/floatlabel";
import Message from "primevue/message";
import Popover from "primevue/popover";

const props = withDefaults(defineProps<{
  modelValue: string;
  label: string;
  inputId?: string;
  labelVisuallyHidden?: boolean;
  floatingLabel?: boolean;
  name?: string;
  type?: string;
  autocomplete?: string;
  autocorrect?: "on" | "off";
  autocapitalize?: "none" | "off" | "sentences" | "words" | "characters";
  spellcheck?: boolean;
  inputMode?: "none" | "text" | "decimal" | "numeric" | "tel" | "search" | "email" | "url";
  enterKeyHint?: "enter" | "done" | "go" | "next" | "previous" | "search" | "send";
  placeholder?: string;
  maxLength?: number;
  error?: string | null;
  disabled?: boolean;
  readonly?: boolean;
  tabIndex?: number;
  showErrorMessage?: boolean;
  shakeKey?: number;
  errorPresentation?: "inline" | "popover";
}>(), {
  labelVisuallyHidden: false,
  floatingLabel: false,
  showErrorMessage: false,
  shakeKey: 0,
  errorPresentation: "inline"
});

const emit = defineEmits<{
  "update:modelValue": [value: string];
}>();

type PrimeInputRef = {
  $el?: HTMLInputElement;
};

type PopoverRef = {
  show: (event: Event, target?: HTMLElement) => void;
  hide: () => void;
};

const input = ref<PrimeInputRef | null>(null);
const warningTrigger = ref<HTMLElement | null>(null);
const errorPopover = ref<PopoverRef | null>(null);
const errorPopoverVisible = ref(false);
const isShaking = ref(false);
const instanceId = useId();
const resolvedInputId = computed(() => props.inputId ?? `ui-input-${instanceId}`);
const errorId = `ui-input-error-${instanceId}`;
const errorDescriptionId = computed(() =>
  props.error && (isPopoverError() || props.showErrorMessage) ? errorId : undefined
);
let shakeTimeoutId: number | null = null;
let popoverEventCounter = 0;

function focusInput(options?: FocusOptions) {
  input.value?.$el?.focus(options);
}

defineExpose({
  focusInput,
});

function isPopoverError() {
  return props.errorPresentation === "popover";
}

function showErrorPopover(target = warningTrigger.value) {
  if (!props.error || !isPopoverError() || target === null) {
    return;
  }

  popoverEventCounter += 1;
  const event = new Event(`input-error-${popoverEventCounter}`);
  errorPopover.value?.show(event, target);
}

function hideErrorPopover() {
  errorPopover.value?.hide();
}

function onWarningEnter() {
  if (!props.error || props.showErrorMessage || !isPopoverError()) {
    return;
  }

  showErrorPopover();
}

function onWarningLeave() {
  if (props.showErrorMessage || !isPopoverError()) {
    return;
  }

  hideErrorPopover();
}

function onWarningPointerDown(event: PointerEvent) {
  event.preventDefault();

  if (event.pointerType === "mouse") {
    return;
  }

  event.stopPropagation();

  if (errorPopoverVisible.value) {
    hideErrorPopover();
    return;
  }

  showErrorPopover();
}

watch(() => props.shakeKey, (value, previousValue) => {
  if (!value || value === previousValue) {
    return;
  }

  if (shakeTimeoutId !== null) {
    window.clearTimeout(shakeTimeoutId);
  }

  isShaking.value = false;
  requestAnimationFrame(() => {
    isShaking.value = true;
    shakeTimeoutId = window.setTimeout(() => {
      isShaking.value = false;
      shakeTimeoutId = null;
    }, 320);
  });
});

watch(
  () => [props.error, props.showErrorMessage, props.errorPresentation] as const,
  async ([error, showErrorMessage, errorPresentation]) => {
    if (!error || errorPresentation !== "popover") {
      hideErrorPopover();
      return;
    }

    if (!showErrorMessage) {
      return;
    }

    await nextTick();
    showErrorPopover();
  }
);

onBeforeUnmount(() => {
  if (shakeTimeoutId !== null) {
    window.clearTimeout(shakeTimeoutId);
  }
});
</script>

<template>
  <div class="ui-field">
    <label
      v-if="!floatingLabel"
      class="ui-field__label"
      :class="{
        'ui-field__label--visually-hidden': labelVisuallyHidden,
        'ui-field__label--invalid': Boolean(error)
      }"
      :for="resolvedInputId"
    >
      {{ label }}
    </label>
    <component
      :is="floatingLabel ? FloatLabel : 'div'"
      class="ui-input-container"
      :class="{
        'ui-input-container--floating': floatingLabel,
        'ui-input-container--with-left': floatingLabel && Boolean($slots.left),
        'ui-input-container--invalid': floatingLabel && Boolean(error)
      }"
      :variant="floatingLabel ? 'in' : undefined"
    >
      <InputGroup
        class="ui-input-wrap"
        :class="{
          'ui-input-wrap--invalid': Boolean(error),
          'ui-input-wrap--disabled': disabled,
          'ui-input-wrap--shake': isShaking
        }"
      >
        <InputGroupAddon v-if="$slots.left" class="ui-input-wrap__addon ui-input-wrap__addon--left">
          <slot name="left"></slot>
        </InputGroupAddon>
        <InputText
          ref="input"
          class="ui-input"
          :id="resolvedInputId"
          :name="name"
          :type="type ?? 'text'"
          :autocomplete="autocomplete"
          :autocorrect="autocorrect"
          :autocapitalize="autocapitalize"
          :spellcheck="spellcheck"
          :inputmode="inputMode"
          :enterkeyhint="enterKeyHint"
          :placeholder="floatingLabel ? undefined : placeholder"
          :maxlength="maxLength"
          :model-value="modelValue"
          :disabled="disabled"
          :readonly="readonly"
          :tabindex="tabIndex"
          :aria-invalid="error ? 'true' : undefined"
          :aria-describedby="errorDescriptionId"
          fluid
          @update:model-value="emit('update:modelValue', $event ?? '')"
        />
        <InputGroupAddon
          v-if="$slots.right || error"
          class="ui-input-wrap__addon ui-input-wrap__addon--right"
          :class="{ 'ui-input-wrap__addon--warning': error && !$slots.right }"
        >
          <slot
            name="right"
            :error="error"
            :error-visible="errorPopoverVisible"
            :show-error="showErrorPopover"
            :hide-error="hideErrorPopover"
          >
            <button
              v-if="error"
              ref="warningTrigger"
              type="button"
              class="ui-input-wrap__warning"
              :class="{ 'ui-input-wrap__warning--interactive': isPopoverError() }"
              :aria-label="error"
              @mouseenter="onWarningEnter"
              @mouseleave="onWarningLeave"
              @pointerdown="onWarningPointerDown"
              @focus="onWarningEnter"
              @blur="onWarningLeave"
              @click.stop
            >
              <span class="ui-input-wrap__warning-icon">!</span>
            </button>
          </slot>
        </InputGroupAddon>
      </InputGroup>
      <label v-if="floatingLabel" class="ui-input-container__label" :for="resolvedInputId">
        {{ label }}
      </label>
    </component>
    <Popover
      v-if="errorPresentation === 'popover'"
      ref="errorPopover"
      class="ui-input-error"
      @show="errorPopoverVisible = true"
      @hide="errorPopoverVisible = false"
    >
      <div class="ui-input-error__content">{{ error }}</div>
    </Popover>
    <span
      v-if="errorPresentation === 'popover' && error"
      :id="errorId"
      class="ui-field__error-description"
    >
      {{ error }}
    </span>
    <Message
      v-if="errorPresentation === 'inline' && showErrorMessage && error"
      :id="errorId"
      severity="error"
      size="small"
      variant="simple"
      class="ui-field__error"
    >
      {{ error }}
    </Message>
  </div>
</template>

<style scoped>
.ui-field {
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 8px;
  color: var(--color-gray-700);
}

.ui-field__label {
  color: color-mix(in srgb, var(--p-text-muted-color) 72%, transparent);
  font-size: 15px;
}

.ui-field:focus-within > .ui-field__label {
  color: var(--color-gray-700);
}

.ui-field > .ui-field__label--invalid,
.ui-field:focus-within > .ui-field__label--invalid {
  color: var(--color-red-600);
}

.ui-field__label--visually-hidden {
  position: absolute;
  width: 1px;
  height: 1px;
  padding: 0;
  margin: -1px;
  overflow: hidden;
  clip: rect(0 0 0 0);
  white-space: nowrap;
  border: 0;
}

.ui-input-wrap {
  width: 100%;
  min-width: 0;
}

.ui-input-container {
  width: 100%;
  min-width: 0;
}

.ui-input-container--floating :deep(.ui-input.p-inputtext) {
  padding-block-start: 20px;
  padding-block-end: 6px;
}

.ui-input-container--floating :deep(.ui-input-container__label) {
  inset-inline-start: 13px;
  z-index: 1;
  color: color-mix(in srgb, var(--p-text-muted-color) 72%, transparent) !important;
  font-weight: 400;
}

.ui-input-container--floating:has(input:focus) :deep(.ui-input-container__label) {
  color: var(--color-gray-700) !important;
}

.ui-input-container--floating:has(input:autofill) :deep(.ui-input-container__label),
.ui-input-container--floating:has(input:-webkit-autofill) :deep(.ui-input-container__label) {
  top: var(--p-floatlabel-in-active-top);
  transform: translateY(0);
  font-size: var(--p-floatlabel-active-font-size);
  font-weight: var(--p-floatlabel-active-font-weight);
}

.ui-input-container--floating.ui-input-container--with-left :deep(.ui-input-container__label) {
  inset-inline-start: 55px;
}

.ui-input-container--floating.ui-input-container--invalid :deep(.ui-input-container__label),
.ui-input-container--floating.ui-input-container--invalid:has(input:focus) :deep(.ui-input-container__label) {
  color: var(--color-red-600) !important;
}

.ui-input-wrap.p-inputgroup {
  align-items: stretch;
  overflow: hidden;
  border: 1px solid var(--color-gray-300);
  border-radius: 8px;
  background: white;
  transition:
    border-color 0.18s ease,
    box-shadow 0.18s ease,
    background-color 0.18s ease;
}

.ui-input-wrap.p-inputgroup:focus-within {
  border-color: var(--color-primary-500);
  box-shadow: 0 0 0 3px color-mix(in srgb, var(--p-primary-color) 16%, transparent);
}

.ui-input-wrap--invalid.p-inputgroup {
  border-color: var(--color-red-600);
  box-shadow: 0 0 0 3px rgba(177, 70, 70, 0.12);
}

.ui-input-wrap--invalid.p-inputgroup:focus-within {
  border-color: var(--color-red-600);
  box-shadow: 0 0 0 3px rgba(177, 70, 70, 0.18);
}

.ui-input-wrap--disabled.p-inputgroup {
  background: var(--color-gray-100);
}

.ui-input-wrap :deep(.ui-input.p-inputtext) {
  flex: 1 1 auto;
  min-width: 0;
  min-height: 48px;
  font-size: 18px;
  border: none;
  border-radius: 0;
  background: transparent;
  box-shadow: none;
}

.ui-input-wrap :deep(.ui-input.p-inputtext:focus) {
  outline: none;
  box-shadow: none;
}

.ui-input-wrap :deep(.ui-input.p-inputtext::placeholder) {
  color: color-mix(in srgb, var(--p-text-muted-color) 78%, transparent);
  opacity: 1;
}

.ui-input-wrap__addon.p-inputgroupaddon {
  min-width: 42px;
  padding-inline: 12px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border: none;
  border-radius: 0;
  background: transparent;
  color: var(--p-surface-500);
  line-height: 1;
}

.ui-input-wrap__addon--left.p-inputgroupaddon {
  border-right: 1px solid var(--color-gray-200);
}

.ui-input-wrap__addon--right.p-inputgroupaddon {
  border-left: 1px solid var(--color-gray-200);
}

.ui-input-wrap__addon--warning.p-inputgroupaddon {
  padding: 0;
}

.ui-input-wrap--invalid .ui-input-wrap__addon--right.p-inputgroupaddon {
  border-left-color: rgba(177, 70, 70, 0.28);
}

.ui-input-wrap--shake.p-inputgroup {
  animation: input-shake 0.32s ease-in-out;
}

.ui-input-wrap__warning {
  align-self: stretch;
  width: 100%;
  border: 0;
  padding: 0;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: transparent;
  cursor: default;
}

.ui-input-wrap__warning--interactive {
  cursor: help;
}

.ui-input-wrap__warning-icon {
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
  transition:
    background-color 0.18s ease,
    transform 0.18s ease;
}

.ui-input-wrap__warning:focus-visible .ui-input-wrap__warning-icon {
  background: #983737;
}

@media (hover: hover) {
  .ui-input-wrap__warning:hover .ui-input-wrap__warning-icon {
    background: #983737;
  }
}

.ui-input-wrap__warning:focus-visible {
  outline: 2px solid rgba(177, 70, 70, 0.28);
  outline-offset: 2px;
}

.ui-field__error {
  color: var(--color-red-600);
  font-size: 14px;
}

.ui-field__error :deep(.p-message-text) {
  font-weight: 400;
  line-height: 1.4;
}

.ui-field__error-description {
  position: absolute;
  width: 1px;
  height: 1px;
  overflow: hidden;
  clip: rect(0 0 0 0);
  white-space: nowrap;
}

.ui-input-error__content {
  color: var(--color-red-600);
  font-size: 14px;
  line-height: 1.4;
}

:global(.ui-input-error.p-popover) {
  max-width: min(320px, calc(100vw - 24px));
  box-shadow: var(--shadow-menu);
  color: var(--color-gray-700);
}

:global(.ui-input-error.p-popover .p-popover-content) {
  padding: 10px 12px;
}

@keyframes input-shake {
  0%,
  100% {
    transform: translateX(0);
  }

  20% {
    transform: translateX(-3px);
  }

  40% {
    transform: translateX(3px);
  }

  60% {
    transform: translateX(-2px);
  }

  80% {
    transform: translateX(2px);
  }
}

@media (prefers-reduced-motion: reduce) {
  .ui-input-wrap--shake.p-inputgroup {
    animation: none;
  }
}

</style>
