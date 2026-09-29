<script setup lang="ts">
import Dialog from "primevue/dialog";
import { onBeforeUnmount, onMounted, ref } from "vue";
import { userNoticeCodes } from "../../user-notices/model/userNoticeCodes";
import { useUserNoticeStore } from "../../user-notices/model/userNoticeStore";
import DailyGameShareCard from "./DailyGameShareCard.vue";

const props = defineProps<{
  gameId: string;
  day: string;
  attentionEnabled: boolean;
}>();

const userNotices = useUserNoticeStore();
const dialogOpen = ref(false);
const attentionVisible = ref(false);
let mounted = false;
let noticeEligibility: boolean | null = null;
let noticeSaveRequested = false;
let noticeSaveStarted = false;

onMounted(async () => {
  mounted = true;
  if (!props.attentionEnabled) return;

  const shouldShow = await userNotices.shouldShowUserNotice(
    userNoticeCodes.dailyResultShareButton,
  );
  if (!mounted) return;

  noticeEligibility = shouldShow;
  if (!shouldShow) return;

  if (noticeSaveRequested) {
    void saveNoticeView();
    return;
  }

  attentionVisible.value = true;
});

onBeforeUnmount(() => {
  mounted = false;
});

function openDialog() {
  attentionVisible.value = false;
  dialogOpen.value = true;

  if (!props.attentionEnabled) return;

  noticeSaveRequested = true;
  if (noticeEligibility === true) {
    void saveNoticeView();
  }
}

async function saveNoticeView() {
  if (noticeSaveStarted) return;
  noticeSaveStarted = true;
  await userNotices.saveOneTimeUserNoticeView(
    userNoticeCodes.dailyResultShareButton,
  );
}

</script>

<template>
  <button class="daily-game-share-trigger"
    :class="{ 'daily-game-share-trigger--attention': attentionVisible }" type="button"
    aria-label="Поделиться результатом" title="Поделиться результатом" aria-haspopup="dialog"
    aria-controls="daily-game-share-dialog" :aria-expanded="dialogOpen"
    @click="openDialog">
    <i class="pi pi-share-alt" aria-hidden="true"></i>
  </button>

  <Dialog id="daily-game-share-dialog" v-model:visible="dialogOpen" modal dismissable-mask
    header="Поделиться результатом" class="game-dialog daily-game-share-dialog">
    <DailyGameShareCard v-if="dialogOpen" :game-id="gameId" :day="day" />
  </Dialog>
</template>

<style scoped>
.daily-game-share-trigger {
  width: 34px;
  min-width: 34px;
  height: 34px;
  position: relative;
  padding: 0;
  border: 0;
  border-radius: 8px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: transparent;
  color: var(--color-gray-600);
  cursor: pointer;
  transition:
    color 0.16s ease,
    background-color 0.16s ease,
    transform 0.1s ease;
}

.daily-game-share-trigger .pi {
  font-size: 16px;
  line-height: 1;
}

.daily-game-share-trigger::after {
  content: "";
  position: absolute;
  inset: 3px;
  border: 1px solid transparent;
  border-radius: 9px;
  pointer-events: none;
}

.daily-game-share-trigger--attention::after {
  animation: daily-game-share-halo 3.4s ease-out infinite;
  animation-delay: 1.2s;
}

.daily-game-share-trigger--attention .pi {
  animation: daily-game-share-icon-pulse 3.4s ease-in-out infinite;
  animation-delay: 1.2s;
}

.daily-game-share-trigger:active {
  background: var(--color-gray-100);
  color: var(--color-primary-700);
  transform: translateY(1px);
}

.daily-game-share-trigger:focus-visible {
  outline: 2px solid var(--color-primary-600);
  outline-offset: 2px;
}

@media (hover: hover) and (pointer: fine) {
  .daily-game-share-trigger:hover {
    background: var(--color-gray-50);
    color: var(--color-primary-700);
  }

}

@media (prefers-reduced-motion: reduce) {
  .daily-game-share-trigger--attention::after,
  .daily-game-share-trigger--attention .pi {
    animation: none;
  }
}

@keyframes daily-game-share-halo {
  0%,
  18%,
  100% {
    border-color: transparent;
    box-shadow: 0 0 0 0 rgba(34, 197, 94, 0);
  }

  5% {
    border-color: rgba(34, 197, 94, 0.35);
    box-shadow: 0 0 0 0 rgba(34, 197, 94, 0.2);
  }

  12% {
    border-color: rgba(34, 197, 94, 0);
    box-shadow: 0 0 0 6px rgba(34, 197, 94, 0);
  }
}

@keyframes daily-game-share-icon-pulse {
  0%,
  18%,
  100% {
    transform: scale(1);
  }

  7% {
    transform: scale(1.1);
  }

  13% {
    transform: scale(1);
  }
}
</style>
