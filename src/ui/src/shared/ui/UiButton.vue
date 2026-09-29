<script setup lang="ts">
import Button from "primevue/button";
import { RouterLink, type RouteLocationRaw } from "vue-router";

withDefaults(
  defineProps<{
    type?: "button" | "submit";
    size?: "sm" | "md" | "lg";
    variant?: "primary" | "primary-light" | "tonal" | "soft" | "ghost" | "outlined";
    loading?: boolean;
    loadingLabel?: string;
    disabled?: boolean;
    to?: RouteLocationRaw;
  }>(),
  {
    type: "button",
    size: "md",
    variant: "primary",
    loading: false,
    loadingLabel: "Загрузка",
    disabled: false,
    to: undefined
  }
);
</script>

<template>
  <RouterLink
    v-if="to && !disabled && !loading"
    class="ui-button p-button"
    :class="[`ui-button--${size}`, `ui-button--${variant}`]"
    :to="to"
  >
    <slot />
  </RouterLink>
  <span
    v-else-if="to"
    class="ui-button p-button p-disabled"
    :class="[`ui-button--${size}`, `ui-button--${variant}`]"
    aria-disabled="true"
  >
    <template v-if="loading">
      <i class="ui-button__spinner pi pi-spinner pi-spin" aria-hidden="true"></i>
      <span class="ui-button__loading-label" aria-hidden="true">
        <slot />
      </span>
      <span class="visually-hidden">{{ loadingLabel }}</span>
    </template>
    <slot v-else />
  </span>
  <Button
    v-else
    class="ui-button"
    :class="[`ui-button--${size}`, `ui-button--${variant}`]"
    :type="type"
    :disabled="disabled"
    :loading="loading"
    :severity="variant === 'tonal' || variant === 'soft' || variant === 'ghost' || variant === 'outlined' ? 'secondary' : undefined"
    :variant="variant === 'ghost' ? 'text' : undefined"
    :outlined="variant === 'outlined' || variant === 'primary-light'"
  >
    <template v-if="loading">
      <i class="ui-button__spinner pi pi-spinner pi-spin" aria-hidden="true"></i>
      <span class="ui-button__loading-label" aria-hidden="true">
        <slot />
      </span>
      <span class="visually-hidden">{{ loadingLabel }}</span>
    </template>
    <slot v-else />
  </Button>
</template>

<style scoped>
.ui-button.p-button {
  min-width: 44px;
  position: relative;
  touch-action: manipulation;
  font-weight: 500;
}

a.ui-button.p-button {
  text-decoration: none;
}

.ui-button :deep(.p-button-label) {
  font-weight: 500;
}

.ui-button__loading-label {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  visibility: hidden;
}

.ui-button__spinner {
  width: 16px;
  height: 16px;
  position: absolute;
  inset: 0;
  margin: auto;
  font-size: 16px;
  line-height: 1;
}

.ui-button--sm.p-button {
  min-height: 34px;
  padding-inline: 10px;
  font-size: 13px;
}

.ui-button--md.p-button {
  min-height: 40px;
  padding-inline: 16px;
  font-size: 16px;
}

.ui-button--lg.p-button {
  min-height: 48px;
  padding-inline: 20px;
  font-size: 18px;
}

.ui-button--primary.p-button {
  background: var(--p-primary-color);
  border-color: var(--p-primary-color);
  color: var(--p-primary-contrast-color);
}

.ui-button--primary-light.p-button {
  border-color: var(--color-gray-300);
  background: white;
  color: var(--color-primary-600);
}

.ui-button--tonal.p-button {
  border-color: transparent;
  background: var(--color-primary-100);
  color: var(--color-primary-700);
}

.ui-button--ghost.p-button {
  border-color: transparent;
  background: transparent;
  color: var(--color-gray-700);
}

.ui-button--soft.p-button {
  border-color: var(--color-gray-200);
  background: white;
  color: var(--color-gray-600);
}

.ui-button--outlined.p-button {
  border-color: var(--color-gray-300);
  background: white;
  color: var(--color-gray-700);
}

@media (hover: hover) and (pointer: fine) {
  .ui-button--primary.p-button:not(:disabled):hover {
    background: var(--p-primary-hover-color);
    border-color: var(--p-primary-hover-color);
    color: var(--p-primary-contrast-color);
  }

  .ui-button--primary-light.p-button:not(:disabled):hover {
    border-color: var(--color-gray-400);
    background: var(--color-gray-100);
    color: var(--color-primary-600);
  }

  .ui-button--tonal.p-button:not(:disabled):hover {
    border-color: transparent;
    background: var(--color-primary-200);
    color: var(--color-primary-700);
  }

  .ui-button--ghost.p-button:not(:disabled):hover {
    border-color: transparent;
    background: var(--color-gray-100);
    color: var(--color-gray-800);
  }

  .ui-button--soft.p-button:not(:disabled):hover {
    border-color: var(--color-gray-300);
    background: var(--color-gray-50);
    color: var(--color-gray-700);
  }

  .ui-button--outlined.p-button:not(:disabled):hover {
    border-color: var(--color-gray-400);
    background: var(--color-gray-50);
    color: var(--color-gray-800);
  }
}

@media (hover: none), (pointer: coarse) {
  .ui-button--primary.p-button:not(:disabled):hover {
    background: var(--p-primary-color);
    border-color: var(--p-primary-color);
    color: var(--p-primary-contrast-color);
  }

  .ui-button--primary-light.p-button:not(:disabled):hover {
    border-color: var(--color-gray-300);
    background: white;
    color: var(--color-primary-600);
  }

  .ui-button--tonal.p-button:not(:disabled):hover {
    border-color: transparent;
    background: var(--color-primary-100);
    color: var(--color-primary-700);
  }

  .ui-button--outlined.p-button:not(:disabled):hover {
    border-color: var(--color-gray-300);
    background: white;
    color: var(--color-gray-700);
  }

  .ui-button--ghost.p-button:not(:disabled):hover {
    border-color: transparent;
    background: transparent;
    color: var(--color-gray-700);
  }

  .ui-button--soft.p-button:not(:disabled):hover {
    border-color: var(--color-gray-200);
    background: white;
    color: var(--color-gray-600);
  }
}

.ui-button--primary.p-button:not(:disabled):active {
  background: var(--p-primary-active-color);
  border-color: var(--p-primary-active-color);
  color: var(--p-primary-contrast-color);
}

.ui-button--primary-light.p-button:not(:disabled):active {
  border-color: var(--color-gray-400);
  background: var(--color-gray-100);
  color: var(--color-primary-600);
}

.ui-button--tonal.p-button:not(:disabled):active {
  border-color: transparent;
  background: var(--color-primary-200);
  color: var(--color-primary-700);
}

.ui-button--ghost.p-button:not(:disabled):active {
  border-color: transparent;
  background: var(--color-gray-200);
  color: var(--color-gray-900);
}

.ui-button--soft.p-button:not(:disabled):active {
  border-color: var(--color-gray-300);
  background: var(--color-gray-100);
  color: var(--color-gray-800);
}

.ui-button--primary-light.p-button:disabled,
.ui-button--primary-light.p-button.p-disabled {
  border-color: var(--color-gray-200);
  background: var(--color-gray-100);
  color: var(--color-gray-500);
  opacity: 1;
}

.ui-button--outlined.p-button:not(:disabled):active {
  border-color: var(--color-gray-400);
  background: var(--color-gray-50);
  color: var(--color-gray-800);
}
</style>
