<script setup lang="ts">
import { useId } from "vue";

withDefaults(
  defineProps<{
    title: string;
    icon?: string;
    compact?: boolean;
    dismissible?: boolean;
    dismissDisabled?: boolean;
  }>(),
  {
    icon: "pi pi-info-circle",
    compact: false,
    dismissible: false,
    dismissDisabled: false,
  },
);

const emit = defineEmits<{
  dismiss: [];
}>();

const titleId = `ui-action-notice-${useId()}`;
</script>

<template>
  <section class="ui-action-notice" :class="{ 'ui-action-notice--compact': compact }"
    :aria-labelledby="titleId">
    <button v-if="dismissible" type="button" class="ui-action-notice__dismiss" aria-label="Закрыть"
      :disabled="dismissDisabled" @click="emit('dismiss')">
      <i class="pi pi-times" aria-hidden="true"></i>
    </button>

    <div class="ui-action-notice__content" :class="{ 'ui-action-notice__content--dismissible': dismissible }">
      <span class="ui-action-notice__icon" aria-hidden="true">
        <i :class="icon"></i>
      </span>
      <h2 :id="titleId" class="ui-action-notice__title">
        <slot name="title">{{ title }}</slot>
      </h2>
    </div>

    <div v-if="!compact && $slots.default" class="ui-action-notice__text">
      <slot />
    </div>

    <div v-if="$slots.preference" class="ui-action-notice__preference">
      <slot name="preference" />
    </div>

    <div v-if="$slots.actions" class="ui-action-notice__actions">
      <slot name="actions" />
    </div>
  </section>
</template>

<style scoped>
.ui-action-notice {
  --ui-action-notice-icon-size: 22px;
  width: 100%;
  min-width: 0;
  position: relative;
  display: flex;
  flex-direction: column;
  gap: 6px;
  padding: 10px;
  border: 1px solid var(--color-primary-100);
  border-radius: 8px;
  background: var(--color-primary-50);
}

.ui-action-notice__content {
  min-width: 0;
  display: flex;
  align-items: flex-start;
  gap: 9px;
}

.ui-action-notice__icon {
  width: var(--ui-action-notice-icon-size);
  min-width: var(--ui-action-notice-icon-size);
  height: var(--ui-action-notice-icon-size);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  color: var(--color-primary-600);
}

.ui-action-notice__icon .pi {
  font-size: 15px;
}

.ui-action-notice__content--dismissible {
  padding-right: 28px;
}

.ui-action-notice__dismiss {
  appearance: none;
  -webkit-tap-highlight-color: transparent;
  width: 28px;
  height: 28px;
  position: absolute;
  top: 6px;
  right: 6px;
  border: 0;
  border-radius: 50%;
  padding: 0;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: transparent;
  color: var(--color-gray-500);
  cursor: pointer;
}

.ui-action-notice__dismiss .pi {
  font-size: 13px;
}

.ui-action-notice__dismiss:disabled {
  opacity: 0.45;
  cursor: default;
}

.ui-action-notice__dismiss:focus-visible {
  outline: none;
  box-shadow: var(--focus-ring-primary);
}

.ui-action-notice__title {
  min-width: 0;
  min-height: var(--ui-action-notice-icon-size);
  margin: 0;
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 4px;
  overflow-wrap: anywhere;
  color: var(--color-gray-800);
  font-size: 14px;
  font-weight: 500;
  line-height: 1.35;
}

.ui-action-notice__text {
  color: var(--color-gray-600);
  font-size: 13px;
  line-height: 1.4;
}

.ui-action-notice__text :deep(p) {
  margin: 0;
}

.ui-action-notice__preference {
  display: flex;
  align-items: center;
  gap: 6px;
  padding-left: 0;
  color: var(--color-gray-500);
  font-size: 12px;
  line-height: 1.25;
}

.ui-action-notice__preference :deep(.p-checkbox) {
  width: 16px;
  height: 16px;
}

.ui-action-notice__preference :deep(.p-checkbox-box) {
  width: 16px;
  height: 16px;
}

.ui-action-notice__actions {
  display: flex;
  justify-content: flex-end;
  flex-wrap: wrap;
  gap: 8px;
}

.ui-action-notice:not(.ui-action-notice--compact) .ui-action-notice__actions {
  margin-top: 4px;
}

@media (width < 768px) {
  .ui-action-notice__actions {
    flex-direction: column-reverse;
  }

  .ui-action-notice__actions :deep(.ui-button) {
    width: 100%;
  }
}

@media (min-width: 768px) {
  .ui-action-notice {
    --ui-action-notice-icon-size: 24px;
    gap: 8px;
    padding: 12px;
  }

  .ui-action-notice__icon .pi {
    font-size: 16px;
  }

  .ui-action-notice__title {
    font-size: 15px;
  }

  .ui-action-notice__text {
    font-size: 14px;
  }

  .ui-action-notice__preference {
    font-size: 13px;
  }

  .ui-action-notice__dismiss {
    top: 8px;
    right: 8px;
  }

  .ui-action-notice--compact {
    display: grid;
    grid-template-columns: minmax(0, 1fr) auto;
    align-items: center;
  }

  .ui-action-notice--compact .ui-action-notice__content {
    grid-column: 1;
  }

  .ui-action-notice--compact .ui-action-notice__actions {
    grid-column: 2;
    align-self: center;
  }
}

@media (hover: hover) and (pointer: fine) {
  .ui-action-notice__dismiss:not(:disabled):hover {
    background: color-mix(in srgb, white 75%, transparent);
    color: var(--color-gray-700);
  }
}
</style>
