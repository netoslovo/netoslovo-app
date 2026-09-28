<script setup lang="ts">
import { RouterLink } from "vue-router";
import type { AdminNavigationSection } from "./adminNavigation";

defineProps<{
  sections: AdminNavigationSection[];
}>();
</script>

<template>
  <nav class="admin-navigation" aria-label="Меню администрирования">
    <RouterLink :to="{ name: 'admin-home' }" class="admin-navigation__link">
      <i class="pi pi-home" aria-hidden="true"></i>
      <span>Дашбоард</span>
    </RouterLink>
    <section
      v-for="section in sections"
      :key="section.id"
      class="admin-navigation__group"
    >
      <h2 class="admin-navigation__heading">{{ section.title }}</h2>
      <RouterLink
        v-for="destination in section.destinations"
        :key="destination.name"
        :to="{ name: destination.name }"
        class="admin-navigation__link"
      >
        <i :class="destination.icon" aria-hidden="true"></i>
        <span>{{ destination.label }}</span>
      </RouterLink>
    </section>
  </nav>
</template>

<style scoped>
.admin-navigation {
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.admin-navigation__group {
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 5px;
}

.admin-navigation__heading {
  margin: 0;
  padding: 0 9px;
  color: var(--p-text-muted-color);
  font-size: 12px;
  font-weight: 600;
}

.admin-navigation__link {
  min-width: 0;
  min-height: 42px;
  border-radius: 8px;
  padding: 8px 10px;
  display: flex;
  align-items: center;
  gap: 9px;
  color: var(--color-gray-700);
  font-weight: 500;
  line-height: 1.25;
  text-decoration: none;
}

.admin-navigation__link.router-link-active {
  color: var(--color-primary-700);
  background: var(--color-primary-50);
}

@media (hover: hover) and (pointer: fine) {
  .admin-navigation__link:hover {
    color: var(--color-primary-700);
    background: var(--color-primary-50);
  }
}

.admin-navigation__link:focus-visible {
  outline: none;
  box-shadow: var(--focus-ring-primary);
}
</style>
