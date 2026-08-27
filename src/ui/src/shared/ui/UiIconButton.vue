<script setup lang="ts">
import Button from "primevue/button";

withDefaults(
  defineProps<{
    type?: "button" | "submit";
    label: string;
    loadingLabel?: string;
    size?: "sm" | "md";
    loading?: boolean;
    disabled?: boolean;
  }>(),
  {
    type: "button",
    loadingLabel: "Загрузка",
    size: "md",
    loading: false,
    disabled: false
  }
);
</script>

<template>
  <Button
    class="ui-icon-button"
    :class="`ui-icon-button--${size}`"
    :type="type"
    :aria-label="loading ? loadingLabel : label"
    :title="loading ? loadingLabel : label"
    :disabled="disabled"
    :loading="loading"
    severity="secondary"
    outlined
  >
    <i v-if="loading" class="pi pi-spinner pi-spin" aria-hidden="true"></i>
    <slot v-else />
  </Button>
</template>

<style scoped>
.ui-icon-button.p-button {
  width: 48px;
  height: 48px;
  padding: 0;
}

.ui-icon-button--sm.p-button {
  width: 34px;
  min-width: 34px;
  height: 34px;
}

.ui-icon-button :deep(.p-button-label) {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-size: 18px;
}

.ui-icon-button :deep(.pi) {
  font-size: 16px;
}
</style>
