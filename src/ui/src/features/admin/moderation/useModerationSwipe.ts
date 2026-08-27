import { ref } from "vue";

export function useModerationSwipe(onDecision: (approved: boolean) => void) {
  const dragOffset = ref(0);
  let pointerStartX: number | null = null;

  function reset() {
    pointerStartX = null;
    dragOffset.value = 0;
  }

  function onPointerDown(event: PointerEvent) {
    if (event.pointerType === "mouse") return;
    pointerStartX = event.clientX;
    (event.currentTarget as HTMLElement).setPointerCapture(event.pointerId);
  }

  function onPointerMove(event: PointerEvent) {
    if (pointerStartX === null) return;
    dragOffset.value = Math.max(-140, Math.min(140, event.clientX - pointerStartX));
  }

  function onPointerEnd() {
    if (pointerStartX === null) return;
    const offset = dragOffset.value;
    reset();
    if (offset > 85) onDecision(true);
    else if (offset < -85) onDecision(false);
  }

  return { dragOffset, reset, onPointerDown, onPointerMove, onPointerEnd };
}
