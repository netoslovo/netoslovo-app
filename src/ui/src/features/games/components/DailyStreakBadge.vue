<script setup lang="ts">
import { computed } from "vue";
import type { DailyGameStreakTier } from "../model/game";

const props = withDefaults(
  defineProps<{
    value: string | number;
    tier: DailyGameStreakTier;
    label?: string;
  }>(),
  {
    label: undefined,
  },
);

const accessibleLabel = computed(() => props.label ?? String(props.value));
</script>

<template>
  <span class="daily-streak-badge" :class="`daily-streak-badge--${tier}`" :aria-label="accessibleLabel">
    <span aria-hidden="true">{{ value }}</span>
  </span>
</template>

<style scoped>
.daily-streak-badge {
  --daily-streak-badge-border: var(--color-gray-300);
  --daily-streak-badge-bg: var(--color-gray-50);
  --daily-streak-badge-value: var(--color-gray-700);
  box-sizing: border-box;
  min-width: 34px;
  height: 24px;
  padding: 2px 6px;
  border: 1px solid var(--daily-streak-badge-border);
  border-radius: 7px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: var(--daily-streak-badge-bg);
  color: var(--daily-streak-badge-value);
  font-size: 13px;
  font-weight: 600;
  line-height: 1;
  font-variant-numeric: tabular-nums;
}

.daily-streak-badge--started {
  --daily-streak-badge-border: #d8dee3;
  --daily-streak-badge-bg: #ffffff;
  --daily-streak-badge-value: var(--color-primary-600);
}

.daily-streak-badge--steady {
  --daily-streak-badge-border: #d5b28f;
  --daily-streak-badge-bg: #fbf3eb;
  --daily-streak-badge-value: #855322;
}

.daily-streak-badge--week {
  --daily-streak-badge-border: #b9c4cf;
  --daily-streak-badge-bg: #f4f7fa;
  --daily-streak-badge-value: #55616d;
}

.daily-streak-badge--twoWeeks {
  --daily-streak-badge-border: #d8b75a;
  --daily-streak-badge-bg: #fff7df;
  --daily-streak-badge-value: #80610d;
}

.daily-streak-badge--month {
  --daily-streak-badge-border: #76adb8;
  --daily-streak-badge-bg: linear-gradient(135deg, #dff3f5 0%, #f7fbfb 48%, #bfe4e9 100%);
  --daily-streak-badge-value: #32616c;
}

.daily-streak-badge--season {
  --daily-streak-badge-border: #7fa4d8;
  --daily-streak-badge-bg: linear-gradient(135deg, #deedff 0%, #f7faff 48%, #c8d8f7 100%);
  --daily-streak-badge-value: #2d5a91;
}

.daily-streak-badge--century {
  --daily-streak-badge-border: #9f83cb;
  --daily-streak-badge-bg: linear-gradient(135deg, #eee4fb 0%, #fbf8ff 48%, #d8c3f2 100%);
  --daily-streak-badge-value: #593889;
}

.daily-streak-badge--legend {
  --daily-streak-badge-border: #d0ae4b;
  --daily-streak-badge-bg:
    linear-gradient(135deg, rgba(255, 255, 255, 0.58) 0%, transparent 24%),
    linear-gradient(135deg, #fff0b8 0%, #f7fbfc 48%, #f2d37a 100%);
  --daily-streak-badge-value: #70540b;
}

</style>
