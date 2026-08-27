<script setup lang="ts">
import Drawer from "primevue/drawer";
import { computed, ref, watch } from "vue";
import { RouterView, useRoute } from "vue-router";
import AdminNavigation from "../features/admin/navigation/AdminNavigation.vue";
import { getPermittedAdminSections } from "../features/admin/navigation/adminNavigation";
import "../features/admin/styles/admin.css";
import { authState } from "../features/auth/model/authSession";
import UiButton from "../shared/ui/UiButton.vue";

const route = useRoute();
const drawerVisible = ref(false);
const sections = computed(() => getPermittedAdminSections(authState.value));

watch(
  () => route.fullPath,
  () => {
    drawerVisible.value = false;
  },
);
</script>

<template>
  <div class="admin-layout">
    <aside class="admin-sidebar" aria-label="Администрирование">
      <div class="admin-sidebar__header">
        <span>Меню</span>
      </div>
      <div class="admin-sidebar__content">
        <AdminNavigation :sections="sections" />
      </div>
    </aside>

    <div class="admin-main">
      <div class="admin-toolbar">
        <UiButton
          class="admin-toolbar__menu"
          size="sm"
          variant="soft"
          aria-haspopup="dialog"
          :aria-expanded="drawerVisible"
          aria-controls="admin-navigation-drawer"
          @click="drawerVisible = true"
        >
          <i class="pi pi-bars" aria-hidden="true"></i>
          <span>Меню</span>
        </UiButton>
      </div>

      <div class="admin-content">
        <RouterView />
      </div>
    </div>

    <Drawer
      v-model:visible="drawerVisible"
      class="admin-drawer"
      header="Меню"
      position="left"
      modal
      block-scroll
      :pt="{ root: { id: 'admin-navigation-drawer' } }"
    >
      <AdminNavigation :sections="sections" />
    </Drawer>
  </div>
</template>

<style scoped>
.admin-layout {
  flex: 1;
  width: 100%;
  max-width: 100%;
  min-width: 0;
  display: grid;
  grid-template-columns: 20rem minmax(0, 1fr);
  align-items: start;
  gap: 22px;
}

.admin-sidebar {
  min-width: 0;
  border-style: solid;
  border-color: var(--p-drawer-border-color);
  border-width: 0 1px 0 0;
  display: flex;
  flex-direction: column;
  background: var(--p-drawer-background);
  box-shadow: var(--p-drawer-shadow);
  color: var(--p-drawer-color);
}

.admin-sidebar__header {
  flex-shrink: 0;
  padding: var(--p-drawer-header-padding);
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.admin-sidebar__header span {
  font-size: var(--p-drawer-title-font-size);
  font-weight: var(--p-drawer-title-font-weight);
}

.admin-sidebar__content {
  width: 100%;
  min-width: 0;
  padding: var(--p-drawer-content-padding);
}

.admin-main,
.admin-content {
  width: 100%;
  min-width: 0;
}

.admin-content {
  padding-bottom: 24px;
  container: admin-content / inline-size;
}

.admin-toolbar {
  display: none;
}

.admin-drawer {
  width: 20rem;
  max-width: 100%;
}

@media (max-width: 899px) {
  .admin-layout {
    display: block;
  }

  .admin-sidebar {
    display: none;
  }

  .admin-toolbar {
    min-width: 0;
    border-bottom: 1px solid var(--color-primary-100);
    margin-bottom: 16px;
    padding-bottom: 12px;
    display: flex;
    align-items: center;
  }

  .admin-toolbar__menu {
    min-height: 40px;
  }
}
</style>
