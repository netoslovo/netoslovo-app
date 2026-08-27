<script setup lang="ts">
import UiSkeleton from "../../../shared/ui/UiSkeleton.vue";

withDefaults(
  defineProps<{
    count?: number;
    columns?: number;
  }>(),
  {
    count: 4,
    columns: 3,
  },
);
</script>

<template>
  <div class="admin-list-skeleton" aria-hidden="true">
    <article v-for="row in count" :key="row" class="admin-list-skeleton__row">
      <div v-for="column in columns" :key="column" class="admin-list-skeleton__cell">
        <UiSkeleton :width="column === 1 ? '72%' : '54%'" height="15px" />
        <UiSkeleton v-if="column === 1" width="44%" height="12px" />
      </div>
      <UiSkeleton width="96px" height="38px" border-radius="8px" />
    </article>
  </div>
</template>

<style scoped>
.admin-list-skeleton {
  width: 100%;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.admin-list-skeleton__row {
  min-height: 78px;
  border: 1px solid var(--color-gray-200);
  border-radius: 8px;
  padding: 12px;
  display: grid;
  grid-template-columns: repeat(v-bind(columns), minmax(0, 1fr)) auto;
  align-items: center;
  gap: 14px;
  background: white;
}

.admin-list-skeleton__cell {
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 7px;
}

@container admin-content (max-width: 720px) {
  .admin-list-skeleton__row {
    grid-template-columns: 1fr;
  }
}
</style>
