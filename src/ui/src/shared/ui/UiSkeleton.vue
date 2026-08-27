<script setup lang="ts">
import Skeleton from "primevue/skeleton";

withDefaults(
  defineProps<{
    width?: string;
    height?: string;
    borderRadius?: string;
    shape?: "rectangle" | "circle";
  }>(),
  {
    width: "100%",
    height: "1rem",
    borderRadius: "6px",
    shape: "rectangle",
  },
);
</script>

<template>
  <Skeleton class="ui-skeleton" :width="width" :height="height" :border-radius="borderRadius" :shape="shape" />
</template>

<style scoped>
.ui-skeleton {
  --p-skeleton-background: var(--color-gray-100);
  --p-skeleton-animation-background: rgba(255, 255, 255, 0.72);
}

@media (prefers-reduced-motion: reduce) {
  .ui-skeleton::after {
    animation: none;
  }
}
</style>
