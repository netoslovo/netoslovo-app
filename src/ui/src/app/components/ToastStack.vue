<script setup lang="ts">
import PrimeToast from "primevue/toast";
import type { ToastPassThroughOptions } from "primevue/toast";
import { useToast } from "primevue/usetoast";
import { onBeforeUnmount, onMounted, reactive, ref } from "vue";
import {
  registerToastPublisher,
  type AppToastMessage,
  type ToastStatus,
} from "../../shared/notifications/toastStore";

const toast = useToast();
const maxVisibleToasts = 2;
const visibleToasts: AppToastMessage[] = [];
const swipeOffsets = reactive(new Map<number, { x: number; y: number }>());
const swipingToastId = ref<number | null>(null);
let swipeGesture: {
  id: number;
  pointerId: number;
  startX: number;
  startY: number;
  startedAt: number;
} | null = null;
const toastStatusLabels: Record<ToastStatus, string> = {
  success: "Успех",
  error: "Ошибка",
  info: "Инфо",
};
const toastPassThrough: ToastPassThroughOptions = {
  message: ({ props }) => {
    const isError = props.message?.severity === "error";

    return {
      role: isError ? "alert" : "status",
      "aria-live": isError ? "assertive" : "polite",
      "aria-atomic": "true",
    };
  },
};

let unregisterToastPublisher: (() => void) | null = null;

function onToastAction(
  message: AppToastMessage,
  closeCallback: () => void,
) {
  closeCallback();
  message.onAction?.();
}

function getToastStyle(message: AppToastMessage) {
  const offset = swipeOffsets.get(message.id);

  if (offset === undefined) {
    return {
      "--toast-duration-ms": `${message.life ?? 9000}ms`,
    };
  }

  const distance = Math.hypot(offset.x, offset.y);

  return {
    "--toast-duration-ms": `${message.life ?? 9000}ms`,
    transform: `translate(${offset.x}px, ${offset.y}px)`,
    opacity: Math.max(0.55, 1 - distance / 360),
  };
}

function onSwipeStart(message: AppToastMessage, event: PointerEvent) {
  const id = message.id;
  const target = event.target as HTMLElement;

  if (event.button !== 0 || target.closest("button")) {
    return;
  }

  swipeGesture = {
    id,
    pointerId: event.pointerId,
    startX: event.clientX,
    startY: event.clientY,
    startedAt: performance.now(),
  };
  swipingToastId.value = id;
  swipeOffsets.set(id, { x: 0, y: 0 });
  (event.currentTarget as HTMLElement).setPointerCapture(event.pointerId);
}

function onSwipeMove(message: AppToastMessage, event: PointerEvent) {
  const id = message.id;

  if (swipeGesture?.id !== id || swipeGesture.pointerId !== event.pointerId) {
    return;
  }

  event.preventDefault();
  swipeOffsets.set(id, {
    x: event.clientX - swipeGesture.startX,
    y: event.clientY - swipeGesture.startY,
  });
}

function resetSwipe(id: number) {
  swipingToastId.value = null;
  swipeOffsets.set(id, { x: 0, y: 0 });
  window.setTimeout(() => {
    const offset = swipeOffsets.get(id);

    if (offset?.x === 0 && offset.y === 0) {
      swipeOffsets.delete(id);
    }
  }, 180);
}

function onSwipeEnd(
  message: AppToastMessage,
  event: PointerEvent,
  closeCallback: () => void,
) {
  const gesture = swipeGesture;
  const id = message.id;

  if (gesture?.id !== id || gesture.pointerId !== event.pointerId) {
    return;
  }

  const element = event.currentTarget as HTMLElement;
  const offset = swipeOffsets.get(id) ?? { x: 0, y: 0 };
  const elapsedMs = Math.max(performance.now() - gesture.startedAt, 1);
  const horizontalDistance = Math.abs(offset.x);
  const verticalDistance = Math.abs(offset.y);
  const isVertical = verticalDistance > horizontalDistance;
  const isUpward = isVertical && offset.y < 0;
  const fastSwipe = isUpward
    ? verticalDistance >= 24 && verticalDistance / elapsedMs >= 0.45
    : !isVertical && horizontalDistance >= 24 && horizontalDistance / elapsedMs >= 0.55;
  const farSwipe = isUpward
    ? verticalDistance >= Math.min(64, element.clientHeight * 0.65)
    : !isVertical && horizontalDistance >= Math.min(80, element.clientWidth * 0.25);

  swipeGesture = null;
  element.releasePointerCapture(event.pointerId);

  if (!fastSwipe && !farSwipe) {
    resetSwipe(id);
    return;
  }

  swipingToastId.value = null;
  swipeOffsets.delete(id);
  closeCallback();
}

function onSwipeCancel(message: AppToastMessage, event: PointerEvent) {
  const id = message.id;

  if (swipeGesture?.id !== id || swipeGesture.pointerId !== event.pointerId) {
    return;
  }

  swipeGesture = null;
  resetSwipe(id);
}

function onToastRemoved(message: AppToastMessage) {
  const visibleToastIndex = visibleToasts.findIndex(({ id }) => id === message.id);
  if (visibleToastIndex !== -1) {
    visibleToasts.splice(visibleToastIndex, 1);
  }

  swipeOffsets.delete(message.id);

  if (swipeGesture?.id === message.id) {
    swipeGesture = null;
    swipingToastId.value = null;
  }
}

onMounted(() => {
  unregisterToastPublisher = registerToastPublisher((message) => {
    toast.add(message);
    visibleToasts.push(message);

    if (visibleToasts.length > maxVisibleToasts) {
      toast.remove(visibleToasts[0]);
    }
  });
});

onBeforeUnmount(() => {
  unregisterToastPublisher?.();
});
</script>

<template>
  <PrimeToast
    group="app"
    position="top-center"
    class="toast-stack"
    :pt="toastPassThrough"
    @close="onToastRemoved($event.message as AppToastMessage)"
    @life-end="onToastRemoved($event.message as AppToastMessage)"
  >
    <template #container="{ message, closeCallback }">
      <div
        class="toast"
        :class="[
          `toast--${message.severity ?? 'info'}`,
          {
            'toast--swiping': swipingToastId === message.id,
          },
        ]"
        :style="getToastStyle(message)"
        :data-toast-id="message.id"
        @pointerdown="onSwipeStart(message, $event)"
        @pointermove="onSwipeMove(message, $event)"
        @pointerup="onSwipeEnd(message, $event, closeCallback)"
        @pointercancel="onSwipeCancel(message, $event)"
      >
        <div class="toast__content">
          <div class="toast__meta">
            <span class="toast__badge">
              {{ toastStatusLabels[(message.severity ?? 'info') as ToastStatus] }}
            </span>
            <strong v-if="message.summary" class="toast__title">{{ message.summary }}</strong>
          </div>
          <span>{{ message.detail }}</span>
          <button
            v-if="message.actionLabel"
            class="toast__action"
            type="button"
            @click="onToastAction(message, closeCallback)"
          >
            {{ message.actionLabel }}
          </button>
        </div>
        <button
          class="toast__close"
          type="button"
          aria-label="Закрыть уведомление"
          @click="closeCallback"
        >
          <svg class="toast__timer" viewBox="0 0 20 20" aria-hidden="true">
            <circle class="toast__timer-track" cx="10" cy="10" r="8" />
            <circle class="toast__timer-progress" cx="10" cy="10" r="8" />
          </svg>
          <span class="toast__close-icon" aria-hidden="true">X</span>
        </button>
      </div>
    </template>
  </PrimeToast>
</template>

<style scoped>
:global(.toast-stack.p-toast) {
  width: min(400px, calc(100vw - 24px));
  max-width: calc(100vw - 24px);
  top: calc(var(--app-visual-viewport-top) + 12px + env(safe-area-inset-top)) !important;
  left: 0 !important;
  right: 0 !important;
  bottom: auto !important;
  margin-inline: auto;
  transform: none;
}

:global(.toast-stack .p-toast-message) {
  margin: 0;
  border: 0;
  background: transparent;
  box-shadow: none;
  backdrop-filter: none;
}

:global(.toast-stack > div) {
  display: flex;
  flex-direction: column-reverse;
  gap: 12px;
}

:global(.toast-stack .p-toast-message-enter-active) {
  animation: toast-drop-from-top 240ms cubic-bezier(0.2, 0.8, 0.25, 1) both;
}

@keyframes toast-drop-from-top {
  from {
    opacity: 0;
    transform: translateY(-56px);
  }

  72% {
    opacity: 1;
    transform: translateY(2px);
  }

  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@media (prefers-reduced-motion: reduce) {
  :global(.toast-stack .p-toast-message-enter-active),
  :global(.toast-stack .p-toast-message-leave-active) {
    animation: none;
  }
}

.toast {
  --toast-accent: var(--color-gray-500);
  --toast-surface: white;
  --toast-contrast: var(--color-gray-800);
  min-height: 48px;
  position: relative;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 10px 12px;
  border-radius: 6px;
  background: var(--toast-surface);
  box-shadow: var(--shadow-menu);
  color: var(--toast-contrast);
  cursor: grab;
  touch-action: none;
  transition: transform 180ms ease, opacity 180ms ease;
  user-select: none;
}

.toast--swiping {
  cursor: grabbing;
  transition: none;
}

.toast--success {
  --toast-accent: var(--color-notification-success);
}

.toast--error {
  --toast-accent: var(--color-notification-error);
}

.toast--info {
  --toast-accent: var(--color-notification-info);
}

.toast::before {
  content: "";
  width: 6px;
  position: absolute;
  inset: 0 auto 0 0;
  border-radius: 6px 0 0 6px;
  background: var(--toast-accent);
}

.toast__content {
  flex: 1 1 auto;
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 2px;
  overflow-wrap: anywhere;
}

.toast__action {
  width: fit-content;
  padding: 2px 0;
  border: 0;
  cursor: pointer;
  background: transparent;
  color: var(--color-gray-700);
  font: inherit;
  font-weight: 500;
  text-decoration: underline;
  text-underline-offset: 2px;
}

.toast__action:hover {
  text-decoration-thickness: 2px;
}

.toast__meta {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.toast__badge {
  min-height: 22px;
  padding: 0 8px;
  border-radius: 999px;
  display: inline-flex;
  align-items: center;
  background: var(--toast-accent);
  color: white;
  font-size: 11px;
  font-weight: 500;
  letter-spacing: 0.04em;
  text-transform: uppercase;
}

.toast__title {
  font-size: 15px;
  font-weight: 500;
}

.toast__close {
  flex: 0 0 38px;
  width: 38px;
  min-width: 38px;
  height: 38px;
  min-height: 38px;
  position: relative;
  border: 0;
  border-radius: 6px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  background: transparent;
  color: var(--toast-accent);
}

.toast__close:hover {
  background: color-mix(in srgb, var(--toast-accent) 12%, white);
}

.toast__close-icon {
  position: relative;
  z-index: 1;
  font-size: 14px;
  line-height: 1;
}

.toast__timer {
  width: 28px;
  height: 28px;
  position: absolute;
  inset: 5px;
  transform: rotate(-90deg);
}

.toast__timer-track,
.toast__timer-progress {
  fill: none;
  stroke-width: 2;
}

.toast__timer-track {
  stroke: color-mix(in srgb, var(--toast-accent) 18%, white);
}

.toast__timer-progress {
  stroke: currentColor;
  stroke-linecap: round;
  stroke-dasharray: 50.27;
  stroke-dashoffset: 0;
  animation: toast-timer var(--toast-duration-ms, 9000ms) linear forwards;
}

@keyframes toast-timer {
  from {
    stroke-dashoffset: 0;
  }

  to {
    stroke-dashoffset: 50.27;
  }
}

@media (prefers-reduced-motion: reduce) {
  .toast__timer-progress {
    animation: none;
  }
}

@media (max-width: 768px) {
  :global(.toast-stack.p-toast) {
    width: min(390px, calc(100vw - 32px));
  }

  .toast {
    min-height: 46px;
    gap: 10px;
    padding: 9px 11px;
  }
}

@media (max-width: 480px) {
  :global(.toast-stack.p-toast) {
    width: min(370px, calc(100vw - 16px));
    top: calc(var(--app-visual-viewport-top) + 8px + env(safe-area-inset-top)) !important;
  }

  .toast {
    gap: 10px;
    padding: 10px 10px 10px 12px;
    font-size: 15px;
  }

  .toast::before {
    width: 5px;
  }

  .toast__meta {
    gap: 6px;
  }

  .toast__badge {
    min-height: 22px;
    padding: 0 8px;
    font-size: 11px;
  }

  .toast__title {
    font-size: 16px;
  }

  .toast__close {
    flex-basis: 38px;
    width: 38px;
    min-width: 38px;
    height: 38px;
    min-height: 38px;
  }

  .toast__timer {
    width: 26px;
    height: 26px;
    inset: 6px;
  }
}
</style>
