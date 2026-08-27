<script setup lang="ts" generic="T">
import { type ComponentPublicInstance } from "vue";
import { useHandoffDelayedLoadingState } from "../../../shared/composables/useSkeletonHandoff";
import UiButton from "../../../shared/ui/UiButton.vue";
import UiSkeletonHandoff from "../../../shared/ui/UiSkeletonHandoff.vue";
import HistoryGameCardSkeleton from "./HistoryGameCardSkeleton.vue";

type HistoryGroup<TItem> = {
  key: string;
  title: string;
  items: TItem[];
};

const props = defineProps<{
  titleId: string;
  title: string;
  groups: HistoryGroup<T>[];
  getItemKey: (item: T) => string;
  hasMore: boolean;
  loading: boolean;
  loadedOnce: boolean;
  loadFailed: boolean;
  errorMessage: string;
  emptyMessage: string;
  setLoadAnchor: (element: HTMLElement | null) => void;
}>();

const emit = defineEmits<{
  loadMore: [];
  retry: [];
}>();

defineSlots<{
  card(props: { item: T }): unknown;
}>();

const loadingState = useHandoffDelayedLoadingState(() => props.loading);

function setAnchor(element: Element | ComponentPublicInstance | null) {
  props.setLoadAnchor(element instanceof HTMLElement ? element : null);
}
</script>

<template>
  <section class="grouped-history-page" :aria-labelledby="titleId">
    <div class="grouped-history">
      <h1 :id="titleId" class="game-page-title">{{ title }}</h1>

      <div class="grouped-history__card">
        <UiSkeletonHandoff
          :skeleton-visible="!loadedOnce && loadingState.visible"
          :content-visible="loadedOnce || loadingState.ready"
        >
          <template #skeleton>
            <HistoryGameCardSkeleton :count="4" />
          </template>

          <p v-if="loadFailed && groups.length === 0" class="grouped-history__empty">
            {{ errorMessage }}
          </p>

          <UiButton
            v-if="loadFailed"
            class="grouped-history__more"
            size="md"
            variant="outlined"
            @click="emit('retry')"
          >
            Повторить
          </UiButton>

          <section v-for="group in groups" :key="group.key" class="grouped-history__group">
            <h2 class="grouped-history__group-title">{{ group.title }}</h2>

            <template v-for="item in group.items" :key="getItemKey(item)">
              <slot name="card" :item="item" />
            </template>
          </section>

          <div
            v-if="!loadFailed"
            :ref="setAnchor"
            class="grouped-history__anchor"
            aria-hidden="true"
          ></div>

          <HistoryGameCardSkeleton
            v-if="loadingState.visible"
            class="grouped-history__loading"
            :count="2"
            :show-group-title="false"
          />

          <UiButton
            v-else-if="hasMore && !loadFailed && loadingState.ready"
            class="grouped-history__more"
            size="md"
            variant="outlined"
            @click="emit('loadMore')"
          >
            Показать ещё
          </UiButton>

          <p v-else-if="!loadFailed && groups.length === 0" class="grouped-history__empty">
            {{ emptyMessage }}
          </p>
        </UiSkeletonHandoff>
      </div>
    </div>
  </section>
</template>

<style scoped>
.grouped-history-page {
  flex: 1 0 auto;
  width: 100%;
  max-width: 760px;
  margin: -10px auto -24px;
  padding: 10px 0 24px;
  display: flex;
  flex-direction: column;
  background: white;
}

.grouped-history {
  flex: 1 0 auto;
  min-height: 100%;
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.grouped-history__card {
  flex: 1 0 auto;
  width: 100%;
  min-width: 0;
  padding: 12px;
  display: flex;
  flex-direction: column;
}

.grouped-history__group {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.grouped-history__group + .grouped-history__group {
  margin-top: 12px;
}

.grouped-history__group-title {
  margin: 2px 0 -1px;
  color: var(--p-text-muted-color);
  font-size: var(--history-group-title-font-size);
  font-weight: 500;
  line-height: 1.3;
}

.grouped-history__anchor {
  height: 1px;
}

.grouped-history__loading {
  margin-top: 8px;
}

.grouped-history__more.ui-button {
  width: 100%;
}

.grouped-history__empty {
  margin: 0;
  color: var(--p-text-muted-color);
  font-size: 14px;
  line-height: 1.4;
  text-align: center;
}

@media (max-width: 640px) {
  .grouped-history {
    gap: 12px;
  }

  .grouped-history__group-title {
    width: min(100%, 360px);
    margin-inline: auto;
  }
}

@media (min-width: 768px) {
  .grouped-history-page {
    margin-top: -20px;
    padding-top: 20px;
  }

  .grouped-history__card {
    padding: 16px;
  }

  .grouped-history__group {
    gap: 14px;
  }

  .grouped-history__group + .grouped-history__group {
    margin-top: 16px;
  }
}
</style>
