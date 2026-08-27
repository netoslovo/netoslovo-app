<script setup lang="ts">
import { ref, watch } from "vue";

const props = defineProps<{
  skeletonVisible: boolean;
  contentVisible: boolean;
}>();

const contentTransitionEnabled = ref(false);

watch(
  () => props.skeletonVisible,
  (visible) => {
    if (visible) contentTransitionEnabled.value = true;
  },
  { flush: "sync" },
);
</script>

<template>
  <div class="ui-skeleton-handoff">
    <Transition name="ui-skeleton-handoff-skeleton" appear>
      <div v-if="skeletonVisible" class="ui-skeleton-handoff__layer ui-skeleton-handoff__layer--skeleton">
        <slot name="skeleton" />
      </div>
    </Transition>

    <Transition name="ui-skeleton-handoff-content" appear :css="contentTransitionEnabled">
      <div v-if="contentVisible" class="ui-skeleton-handoff__layer">
        <slot />
      </div>
    </Transition>
  </div>
</template>

<style scoped>
.ui-skeleton-handoff {
  flex: 1;
  min-width: 0;
  min-height: 0;
  display: grid;
}

.ui-skeleton-handoff__layer {
  min-width: 0;
  min-height: 0;
  grid-area: 1 / 1;
  display: flex;
  flex-direction: column;
  gap: inherit;
}

.ui-skeleton-handoff__layer--skeleton {
  z-index: 1;
  pointer-events: none;
}

</style>
