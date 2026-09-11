<script lang="ts">
import type { RouteLocationRaw } from "vue-router";

export type UiMenuItem = {
  label?: string;
  icon?: string;
  description?: string;
  tone?: "neutral" | "danger";
  disabled?: boolean;
  loading?: boolean;
  separator?: boolean;
  items?: UiMenuItem[];
  to?: RouteLocationRaw;
  closeOnActivate?: boolean;
  activate?: (event: Event, close: () => void) => void;
};
</script>

<script setup lang="ts">
import { computed, onBeforeUnmount, ref, useId } from "vue";
import { RouterLink } from "vue-router";
import Button from "primevue/button";
import Menu from "primevue/menu";
import type { MenuItem } from "primevue/menuitem";

const props = withDefaults(
  defineProps<{
    items: UiMenuItem[];
    buttonLabel?: string;
    disabled?: boolean;
    icon?: "burger" | "dots";
    tone?: "header" | "neutral";
    wide?: boolean;
  }>(),
  {
    buttonLabel: "Открыть меню",
    disabled: false,
    icon: "burger",
    tone: "header",
    wide: false,
  },
);

const emit = defineEmits<{
  open: [];
}>();

type MenuRef = {
  toggle: (event: Event) => void;
  hide: () => void;
};

const menu = ref<MenuRef | null>(null);
const open = ref(false);
const menuId = `ui-menu-${useId()}`;
const buttonIcon = computed(() =>
  props.icon === "dots"
    ? "pi pi-ellipsis-v"
    : props.icon === "burger"
      ? "pi pi-bars"
      : undefined,
);
const buttonSeverity = computed(() =>
  props.tone === "neutral" ? "secondary" : undefined,
);
const menuItems = computed(() => props.items.map(normalizeItem));
const menuPt = {
  item: ({ context }: { context: { item: UiMenuItem } }) => {
    return {
      "aria-busy": context.item.loading ? "true" : undefined,
    };
  },
};

function normalizeItem(item: UiMenuItem): UiMenuItem {
  return {
    ...item,
    disabled: item.disabled === true || item.loading === true,
    items: item.items?.map(normalizeItem),
  };
}

function toggle(event: Event) {
  menu.value?.toggle(event);
}

function close() {
  menu.value?.hide();
}

function onShow() {
  open.value = true;
  bindViewportListeners();
  emit("open");
}

function onHide() {
  open.value = false;
  unbindViewportListeners();
}

function bindViewportListeners() {
  window.addEventListener("resize", close);
  window.visualViewport?.addEventListener("resize", close);
  window.visualViewport?.addEventListener("scroll", close);
}

function unbindViewportListeners() {
  window.removeEventListener("resize", close);
  window.visualViewport?.removeEventListener("resize", close);
  window.visualViewport?.removeEventListener("scroll", close);
}

function isPlainPrimaryClick(event: MouseEvent) {
  return (
    event.button === 0 &&
    !event.metaKey &&
    !event.ctrlKey &&
    !event.shiftKey &&
    !event.altKey
  );
}

function onActionClick(event: MouseEvent, menuItem: MenuItem) {
  const item = menuItem as UiMenuItem;

  if (item.disabled || item.loading) {
    event.stopPropagation();
    event.preventDefault();
    return;
  }

  event.stopPropagation();

  item.activate?.(event, close);

  if (!event.defaultPrevented && item.closeOnActivate !== false) {
    close();
  }
}

function onLinkClick(
  event: MouseEvent,
  menuItem: MenuItem,
  navigate: (event?: MouseEvent) => void,
) {
  event.stopPropagation();
  const item = menuItem as UiMenuItem;

  if (item.disabled || item.loading) {
    event.preventDefault();
    return;
  }

  item.activate?.(event, close);

  if (event.defaultPrevented) {
    return;
  }

  if (isPlainPrimaryClick(event) && item.closeOnActivate !== false) {
    close();
  }

  navigate(event);
}

defineExpose({ close });

onBeforeUnmount(unbindViewportListeners);
</script>

<template>
  <div class="ui-menu">
    <Button
      class="ui-menu__button"
      :class="[
        `ui-menu__button--${props.tone}`,
        `ui-menu__button--${props.icon}`,
        { 'ui-menu__button--disabled': props.disabled },
      ]"
      :aria-label="props.buttonLabel"
      aria-haspopup="menu"
      :aria-expanded="open ? 'true' : 'false'"
      :aria-controls="menuId"
      :icon="buttonIcon"
      :severity="buttonSeverity"
      :disabled="props.disabled"
      :text="props.tone === 'header'"
      :outlined="props.tone === 'neutral'"
      @click="toggle"
    />
    <Menu
      :id="menuId"
      ref="menu"
      class="ui-menu__list"
      :class="{ 'ui-menu__list--wide': props.wide }"
      :model="menuItems"
      :pt="menuPt"
      :aria-label="props.buttonLabel"
      popup
      @show="onShow"
      @hide="onHide"
    >
      <template v-if="$slots.header" #start>
        <slot name="header"></slot>
      </template>
      <template #item="{ item, props: itemProps }">
        <RouterLink v-if="item.to" v-slot="{ href, navigate }" custom :to="item.to">
          <a
            v-bind="itemProps.action"
            class="ui-menu__action"
            :class="[
              `ui-menu__action--${item.tone ?? 'neutral'}`,
              {
                'ui-menu__action--without-icon': !item.icon && !item.loading,
              },
            ]"
            :href="href"
            @click="onLinkClick($event, item, navigate)"
          >
            <i v-if="item.loading" class="ui-menu__spinner pi pi-spinner pi-spin" aria-hidden="true"></i>
            <i v-else-if="item.icon" class="ui-menu__icon" :class="item.icon" aria-hidden="true"></i>

            <span class="ui-menu__content">
              <span class="ui-menu__title">{{ item.label }}</span>
              <span v-if="item.description" class="ui-menu__description">
                {{ item.description }}
              </span>
            </span>
          </a>
        </RouterLink>
        <a
          v-else
          v-bind="itemProps.action"
          class="ui-menu__action"
          :class="[
            `ui-menu__action--${item.tone ?? 'neutral'}`,
            {
              'ui-menu__action--without-icon': !item.icon && !item.loading,
            },
          ]"
          @click="onActionClick($event, item)"
        >
          <i v-if="item.loading" class="ui-menu__spinner pi pi-spinner pi-spin" aria-hidden="true"></i>
          <i v-else-if="item.icon" class="ui-menu__icon" :class="item.icon" aria-hidden="true"></i>

          <span class="ui-menu__content">
            <span class="ui-menu__title">{{ item.label }}</span>
            <span v-if="item.description" class="ui-menu__description">
              {{ item.description }}
            </span>
          </span>
        </a>
      </template>
      <template #submenulabel="{ item }">
        <span class="ui-menu__group-label" :title="String(item.label ?? '')">
          {{ item.label }}
        </span>
      </template>
    </Menu>
  </div>
</template>

<style scoped>
.ui-menu {
  position: relative;
  justify-self: end;
  flex-shrink: 0;
}

.ui-menu__button.p-button {
  min-width: 48px;
  width: 48px;
  height: 48px;
  padding: 0;
  border-radius: 8px;
  background: white;
  touch-action: manipulation;
  -webkit-tap-highlight-color: transparent;
  transition:
    background-color 0.18s ease,
    border-color 0.18s ease,
    color 0.18s ease,
    box-shadow 0.18s ease;
}

.ui-menu__button.p-button.p-button-icon-only {
  width: var(--app-header-side-size, 48px);
  min-width: var(--app-header-side-size, 48px);
  height: var(--app-header-side-size, 48px);
}

.ui-menu__button--header.p-button {
  border-color: transparent;
  background: transparent;
  color: var(--p-primary-100);
}

.ui-menu__button--disabled.p-button,
.ui-menu__button.p-button:disabled {
  opacity: 0.42;
  cursor: default;
  box-shadow: none;
}

.ui-menu__button--header.ui-menu__button--disabled.p-button,
.ui-menu__button--header.p-button:disabled {
  border-color: transparent;
  background: transparent;
  color: color-mix(in srgb, var(--p-primary-100) 62%, transparent);
}

.ui-menu__button--header.p-button:not(:disabled):active {
  border-color: transparent;
  background: color-mix(in srgb, var(--p-primary-100) 30%, transparent);
  color: var(--p-primary-100);
}

.ui-menu__button--neutral.p-button.p-button-outlined:not(:disabled):active {
  border-color: var(--color-gray-400);
  background: var(--color-gray-100);
  color: var(--color-gray-800);
}

.ui-menu__button--header.p-button:focus-visible {
  outline-color: var(--p-primary-100);
  background: color-mix(in srgb, var(--p-primary-100) 18%, transparent);
  color: var(--p-primary-100);
}

.ui-menu__button--neutral.p-button {
  color: var(--color-gray-600);
}

.ui-menu__button :deep(.p-button-icon) {
  font-size: 18px;
}

@media (hover: hover) and (pointer: fine) {
  .ui-menu__button--header.p-button:not(:disabled):hover {
    border-color: transparent;
    background: color-mix(in srgb, var(--p-primary-100) 30%, transparent);
    color: var(--p-primary-100);
  }

  .ui-menu__button--neutral.p-button.p-button-outlined:not(:disabled):hover {
    border-color: var(--color-gray-400);
    background: var(--color-gray-100);
    color: var(--color-gray-800);
  }
}

@media (hover: none), (pointer: coarse) {
  .ui-menu__button--header.p-button:not(:disabled):hover {
    border-color: transparent;
    background: transparent;
    color: var(--p-primary-100);
  }

  .ui-menu__button--neutral.p-button.p-button-outlined:not(:disabled):hover {
    border-color: var(--color-gray-300);
    background: white;
    color: var(--color-gray-600);
  }

  .ui-menu__button--header.p-button:not(:disabled):active {
    background: color-mix(in srgb, var(--p-primary-100) 30%, transparent);
  }

  .ui-menu__button--neutral.p-button.p-button-outlined:not(:disabled):active {
    border-color: var(--color-gray-400);
    background: var(--color-gray-100);
    color: var(--color-gray-800);
  }
}

:global(.ui-menu__list.p-menu) {
  min-width: 220px;
  max-width: calc(100vw - 24px);
  padding: 8px 0;
  box-shadow: var(--shadow-menu);
  color: var(--color-gray-700);
}

:global(.ui-menu__list--wide.p-menu) {
  min-width: 292px;
}

:global(.ui-menu__list.p-menu .p-menu-list) {
  max-height: min(70dvh, 520px);
  overflow-y: auto;
  overscroll-behavior: contain;
}

:global(.ui-menu__list.p-menu .p-menu-submenu-label) {
  max-width: 220px;
  min-height: 32px;
  padding: 0 14px;
  display: flex;
  align-items: center;
  overflow: hidden;
  color: var(--p-text-muted-color);
  font-size: 14px;
  font-weight: 400;
  text-overflow: ellipsis;
  white-space: nowrap;
  overflow-wrap: anywhere;
}

.ui-menu__group-label {
  min-width: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

:global(.ui-menu__list.p-menu .p-menu-separator) {
  width: calc(100% - 24px);
  margin: 4px 12px 6px;
  border-color: var(--color-gray-200);
}

:global(.ui-menu__list.p-menu .p-menu-item-content) {
  border-radius: 8px;
  background: transparent;
}

:global(.ui-menu__list.p-menu .p-menu-item-content:hover),
:global(.ui-menu__list.p-menu .p-menu-item[data-p-focused="true"] > .p-menu-item-content) {
  background: var(--color-gray-50);
}

:global(.ui-menu__list.p-menu .p-menu-item[data-p-disabled="true"] > .p-menu-item-content:hover) {
  background: transparent;
}

.ui-menu__action {
  width: calc(100% - 12px);
  min-width: 208px;
  min-height: 54px;
  margin: 3px 6px;
  padding: 9px 10px;
  display: grid;
  grid-template-columns: 28px minmax(0, 1fr);
  align-items: center;
  gap: 10px;
  border-radius: 8px;
  color: var(--color-gray-700);
  text-align: left;
  text-decoration: none;
  cursor: pointer;
}

.ui-menu__action--without-icon {
  grid-template-columns: minmax(0, 1fr);
}

:global(.ui-menu__list.p-menu .p-menu-item[data-p-disabled="true"]) .ui-menu__action {
  opacity: 0.5;
  cursor: default;
}

.ui-menu__icon {
  width: 28px;
  height: 28px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border-radius: 8px;
  color: var(--p-surface-500);
  font-size: 15px;
  text-align: center;
}

.ui-menu__spinner {
  justify-self: center;
  color: var(--color-primary-600);
  font-size: 18px;
}

.ui-menu__content {
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 3px;
}

.ui-menu__title {
  font-size: 14px;
  font-weight: 500;
  line-height: 1.25;
}

.ui-menu__description {
  color: var(--p-text-muted-color);
  font-size: 12px;
  line-height: 1.3;
}

.ui-menu__action--danger .ui-menu__title {
  color: var(--color-red-600);
}

@media (min-width: 768px) {
  .ui-menu__button--header.p-button {
    width: var(--app-header-side-size);
    height: var(--app-header-side-size);
  }
}

@media (max-width: 480px) {
  :global(.ui-menu__list--wide.p-menu) {
    width: min(320px, calc(100vw - 24px));
    min-width: 0;
  }

  :global(.ui-menu__list.p-menu .p-menu-submenu-label) {
    min-height: 30px;
    padding-inline: 12px;
    font-size: clamp(14px, 4vw, 15px);
  }

  .ui-menu__action {
    width: calc(100% - 12px);
    min-width: 0;
    min-height: 52px;
    padding: 8px clamp(8px, 2.7vw, 10px);
    grid-template-columns: 28px minmax(0, 1fr);
    gap: clamp(8px, 2.7vw, 10px);
  }

  .ui-menu__action--without-icon {
    grid-template-columns: minmax(0, 1fr);
  }

  .ui-menu__title {
    font-size: clamp(14px, 4vw, 15px);
  }

  .ui-menu__description {
    font-size: clamp(12px, 3.5vw, 13px);
  }
}
</style>
