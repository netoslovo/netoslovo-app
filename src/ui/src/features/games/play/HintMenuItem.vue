<script setup lang="ts">
import Popover from "primevue/popover";
import { computed } from "vue";
import { useInfoPopover } from "../../../shared/composables/useInfoPopover";
import UiButton from "../../../shared/ui/UiButton.vue";

const props = withDefaults(
  defineProps<{
    title: string;
    icon: string;
    description: string | string[];
    disabled?: boolean;
    unavailable?: boolean;
    loading?: boolean;
    disabledReason?: string;
    remaining?: number | null;
    total?: number | null;
    scorePenalty?: number | null;
  }>(),
  {
    disabled: false,
    unavailable: false,
    loading: false,
    disabledReason: undefined,
    remaining: null,
    total: null,
    scorePenalty: null,
  },
);

const emit = defineEmits<{
  activate: [];
}>();

const {
  triggerId: infoTriggerId,
  panelId: infoPanelId,
  popoverPt: infoPopoverPt,
  setPopover: setInfoPopover,
  visible: infoPopoverVisible,
  show: showInfo,
  hide: hideInfo,
  onPointerDown: onInfoPointerDown,
  onShow: onInfoPopoverShow,
  onHide: onInfoPopoverHide,
} = useInfoPopover();

const hasCounter = computed(
  () => props.remaining != null && props.total != null,
);

const isLimitReached = computed(() => {
  const remaining = props.remaining;

  return remaining != null && remaining <= 0;
});

const isDisabled = computed(
  () => props.disabled || props.unavailable || props.loading || isLimitReached.value,
);

const isUnavailable = computed(
  () => props.unavailable || isLimitReached.value,
);

const hintText = computed(() => {
  if (props.disabledReason) {
    return props.disabledReason;
  }

  return null;
});

const counterLabel = computed(() =>
  hasCounter.value ? `${props.remaining}/${props.total}` : "",
);

const infoLabel = computed(() => `Подробнее о: ${props.title}`);
const hasMeta = computed(() => hasCounter.value || props.scorePenalty != null);
const actionLabel = computed(() =>
  isUnavailable.value ? "Недоступно" : "Использовать",
);

function onClick() {
  if (isDisabled.value) {
    return;
  }

  emit("activate");
}

</script>

<template>
  <article class="hint-menu-item" :class="{ 'hint-menu-item--disabled': isDisabled }">
    <header class="hint-menu-item__header">
      <span class="hint-menu-item__icon-box">
        <i class="hint-menu-item__icon" :class="icon" aria-hidden="true"></i>
      </span>

      <h3 class="hint-menu-item__title">{{ title }}</h3>

      <button class="hint-menu-item__info" :class="{ 'hint-menu-item__info--open': infoPopoverVisible }" type="button"
        :id="infoTriggerId" :aria-describedby="infoPanelId" :aria-label="infoLabel" @mouseenter="showInfo"
        @mouseleave="hideInfo" @pointerdown="onInfoPointerDown" @focus="showInfo" @blur="hideInfo" @click.stop>
        <i class="pi pi-info-circle" aria-hidden="true"></i>
      </button>
    </header>

    <Popover :ref="setInfoPopover" :pt="infoPopoverPt" class="hint-menu-item__popover info-popover"
      @show="onInfoPopoverShow" @hide="onInfoPopoverHide">
      <ul v-if="Array.isArray(description)" class="hint-menu-item__popover-content hint-menu-item__description-list">
        <li v-for="item in description" :key="item">
          {{ item }}
        </li>
      </ul>
      <p v-else class="hint-menu-item__popover-content">
        {{ description }}
      </p>
    </Popover>

    <div v-if="hasMeta" class="hint-menu-item__meta">
      <span v-if="hasCounter" class="hint-menu-item__meta-chip">
        <span class="hint-menu-item__meta-label">Осталось</span>
        <span class="hint-menu-item__counter" :class="{ 'hint-menu-item__counter--empty': isLimitReached }">
          {{ counterLabel }}
        </span>
      </span>

      <span v-if="scorePenalty != null" class="hint-menu-item__meta-chip">
        <span class="hint-menu-item__meta-label">К счёту</span>
        <span class="hint-menu-item__penalty">+{{ scorePenalty }}</span>
      </span>
    </div>

    <p v-if="hintText" class="hint-menu-item__hint">
      {{ hintText }}
    </p>

    <UiButton class="hint-menu-item__action" size="sm" variant="primary-light" :loading="loading"
      loading-label="Использование подсказки" :disabled="isDisabled"
      :title="hintText ?? undefined" @click="onClick">
      {{ actionLabel }}
    </UiButton>
  </article>
</template>

<style scoped>
.hint-menu-item {
  min-width: 284px;
  border: 1px solid var(--color-gray-200);
  margin: 6px 8px;
  padding: 10px;
  border-radius: 12px;
  background: #ffffff;
  box-shadow: 0 2px 7px rgba(25, 32, 43, 0.04);
}

.hint-menu-item__header {
  display: grid;
  grid-template-columns: 32px minmax(0, 1fr) 28px;
  align-items: center;
  gap: 10px;
}

.hint-menu-item__icon-box {
  width: 32px;
  height: 32px;
  border-radius: 9px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: var(--color-primary-50);
  color: var(--color-primary-600);
}

.hint-menu-item--disabled .hint-menu-item__icon-box {
  background: var(--color-gray-100);
  color: var(--color-gray-500);
}

.hint-menu-item__icon {
  font-size: 15px;
  line-height: 1;
}

.hint-menu-item__title {
  min-width: 0;
  margin: 0;
  color: var(--color-gray-800);
  font-size: 14px;
  font-weight: 500;
  line-height: 1.25;
}

.hint-menu-item__meta {
  margin-top: 10px;
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}

.hint-menu-item__meta-chip {
  min-height: 24px;
  border: 1px solid var(--color-gray-200);
  padding: 4px 7px;
  display: flex;
  align-items: center;
  gap: 5px;
  border-radius: 999px;
  background: var(--color-gray-50);
}

.hint-menu-item__meta-label {
  color: var(--p-text-muted-color);
  font-size: 11px;
  font-weight: 400;
  line-height: 1;
}

.hint-menu-item__hint {
  margin: 9px 0 0;
  color: var(--p-text-muted-color);
  font-size: 12px;
  line-height: 1.35;
}

.hint-menu-item__counter {
  min-width: 0;
  color: var(--color-primary-600);
  font-size: 12px;
  font-weight: 500;
  line-height: 1.35;
  text-align: right;
}

.hint-menu-item__penalty {
  color: var(--color-red-600);
  font-size: 12px;
  font-weight: 500;
  line-height: 1.35;
  text-align: right;
}

.hint-menu-item__counter--empty {
  color: var(--color-gray-600);
}

.hint-menu-item__info {
  width: 28px;
  min-width: 28px;
  height: 28px;
  min-height: 28px;
  border: 0;
  padding: 0;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
  background: transparent;
  color: var(--p-surface-500);
  cursor: help;
  transition:
    color 0.16s ease,
    background-color 0.16s ease;
}

.hint-menu-item__info--open {
  background: var(--color-primary-50);
  color: var(--color-primary-600);
}

@media (hover: hover) and (pointer: fine) {
  .hint-menu-item__info:hover {
    background: var(--color-primary-50);
    color: var(--color-primary-600);
  }
}

.hint-menu-item__info:focus-visible {
  outline: none;
}

.hint-menu-item__info .pi {
  font-size: 15px;
}

.hint-menu-item__action.p-button {
  width: 100%;
  margin-top: 10px;
  font-size: 14px;
}

.hint-menu-item__popover-content {
  margin: 0;
  font-size: var(--info-popover-font-size);
  line-height: 1.45;
}

.hint-menu-item__description-list {
  padding-inline-start: 18px;
}

:global(.hint-menu-item__popover.p-popover) {
  max-width: min(310px, calc(100vw - 24px));
  color: var(--color-gray-700);
}

@media (max-width: 480px) {
  .hint-menu-item {
    min-width: 0;
    margin: 4px 6px;
    padding: 8px;
    border-radius: 10px;
  }

  .hint-menu-item__header {
    grid-template-columns: 28px minmax(0, 1fr) 24px;
    gap: clamp(7px, 2.2vw, 9px);
  }

  .hint-menu-item__icon-box {
    width: 28px;
    height: 28px;
    border-radius: 8px;
  }

  .hint-menu-item__meta {
    margin-top: 7px;
    gap: 4px;
  }

  .hint-menu-item__title {
    font-size: clamp(14px, 4vw, 15px);
  }

  .hint-menu-item__meta-chip {
    min-height: 22px;
    padding: 3px 6px;
    gap: 4px;
  }

  .hint-menu-item__hint {
    margin-top: 6px;
  }

  .hint-menu-item__info {
    width: 24px;
    min-width: 24px;
    height: 24px;
    min-height: 24px;
  }

  .hint-menu-item__action.p-button {
    min-height: 32px;
    margin-top: 7px;
    padding: 6px 8px;
    font-size: clamp(13px, 3.7vw, 14px);
  }
}
</style>
