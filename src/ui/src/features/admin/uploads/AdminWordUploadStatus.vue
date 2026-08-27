<script setup lang="ts">
import { uploadStateLabels } from "../lib/wordUploads";
import type { WordsVersionUploadState } from "../model/wordUploads";

defineProps<{
  state: WordsVersionUploadState;
}>();
</script>

<template>
  <span class="upload-status" :class="`upload-status--${state.toLowerCase()}`">
    {{ uploadStateLabels[state] }}
  </span>
</template>

<style scoped>
.upload-status {
  border-radius: 999px;
  padding: 5px 9px;
  font-size: 12px;
  font-weight: 500;
  white-space: nowrap;
}

.upload-status.upload-status--active {
  background: var(--color-primary-100);
  color: var(--color-primary-700);
}

.upload-status.upload-status--completed {
  background: var(--color-green-200);
  color: #18563e;
}

.upload-status.upload-status--canceled {
  background: var(--color-gray-100);
  color: var(--color-gray-600);
}

.upload-status.upload-status--timeout,
.upload-status.upload-status--error {
  background: var(--color-red-100);
  color: var(--color-red-700);
}
</style>
