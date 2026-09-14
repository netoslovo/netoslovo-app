import { nextTick, onBeforeUnmount, onMounted, ref, watch, type ComponentPublicInstance, type Ref } from "vue";

type ScrollCommand = { kind: "center"; key: string } | { kind: "position"; top: number; behavior: ScrollBehavior };

export function useGameplayScroll(
  scrollport: Ref<HTMLElement | null>,
  content: Ref<HTMLElement | null>,
  enabled: Readonly<Ref<boolean>>,
  dialogOpen: Readonly<Ref<boolean>>,
) {
  const showScrollTop = ref(false);
  const scrollbarWidth = ref(0);
  const rows = new Map<string, HTMLElement>();
  let command: ScrollCommand | null = null;
  let frame = 0;
  let disposed = false;
  let resizeObserver: ResizeObserver | null = null;

  function registerGuess(key: string, element: Element | ComponentPublicInstance | null) {
    if (element instanceof HTMLElement) rows.set(key, element);
    else rows.delete(key);
  }

  function updateScrollPosition() {
    showScrollTop.value = (scrollport.value?.scrollTop ?? 0) > 1;
  }

  function applyCommand() {
    frame = 0;
    if (!enabled.value || !scrollport.value || !command) return;
    // Returning from a menu must restore the background even under a dialog.
    if (dialogOpen.value && command.kind === "center") return;
    const behavior = window.matchMedia("(prefers-reduced-motion: reduce)").matches
      ? "instant" : command.kind === "center" ? "smooth" : command.behavior;
    if (command.kind === "center") {
      rows.get(command.key)?.scrollIntoView({ block: "center", inline: "nearest", behavior });
    } else {
      scrollport.value.scrollTo({ top: command.top, behavior });
    }
    updateScrollPosition();
  }

  function scheduleCommand() {
    if (!disposed && !frame) frame = requestAnimationFrame(applyCommand);
  }

  function onResize() {
    const container = scrollport.value;
    scrollbarWidth.value = container ? container.offsetWidth - container.clientWidth : 0;
    scheduleCommand();
  }

  function centerGuess(key: string) {
    command = { kind: "center", key };
    void nextTick(scheduleCommand);
  }

  function cancelCentering() {
    if (command && enabled.value && scrollport.value) {
      scrollport.value.scrollTo({ top: scrollport.value.scrollTop, behavior: "instant" });
    }
    command = null;
  }

  function restorePosition(top: number, behavior: ScrollBehavior = "instant") {
    command = { kind: "position", top, behavior };
    cancelAnimationFrame(frame);
    applyCommand();
  }

  function scrollToTop() {
    restorePosition(0, "smooth");
  }

  watch([enabled, dialogOpen], scheduleCommand, { flush: "post" });
  onMounted(() => {
    resizeObserver = new ResizeObserver(onResize);
    if (scrollport.value) resizeObserver.observe(scrollport.value);
    if (content.value) resizeObserver.observe(content.value);
    updateScrollPosition();
  });
  onBeforeUnmount(() => {
    disposed = true;
    cancelAnimationFrame(frame);
    resizeObserver?.disconnect();
    rows.clear();
  });

  return { showScrollTop, scrollbarWidth, registerGuess, updateScrollPosition, centerGuess, cancelCentering, restorePosition, scrollToTop };
}
