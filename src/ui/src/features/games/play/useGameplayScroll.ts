import { computed, nextTick, onBeforeUnmount, onMounted, ref, watch, type ComponentPublicInstance, type Ref } from "vue";

type ScrollCommand = { kind: "center"; key: string } | { kind: "list" } | { kind: "position"; top: number; behavior: ScrollBehavior };

export function useGameplayScroll(
  scrollport: Ref<HTMLElement | null>,
  content: Ref<HTMLElement | null>,
  enabled: Readonly<Ref<boolean>>,
  dialogOpen: Readonly<Ref<boolean>>,
) {
  const showScrollTop = ref(false);
  const listHeadingAbove = ref(false);
  const scrollbarWidth = ref(0);
  const rows = new Map<string, HTMLElement>();
  let listHeading: HTMLElement | null = null;
  const command = ref<ScrollCommand | null>(null);
  const scrollUpLabel = computed(() => listHeadingAbove.value
    ? "К началу списка попыток" : "Наверх");
  const scrollUpIcon = computed(() => listHeadingAbove.value
    ? "pi pi-chevron-up" : "pi pi-angle-double-up");
  let frame = 0;
  let disposed = false;
  let resizeObserver: ResizeObserver | null = null;

  function registerGuess(key: string, element: Element | ComponentPublicInstance | null) {
    if (element instanceof HTMLElement) rows.set(key, element);
    else rows.delete(key);
  }

  function registerListHeading(element: Element | ComponentPublicInstance | null) {
    listHeading = element instanceof HTMLElement ? element : null;
    void nextTick(updateScrollPosition);
  }

  function updateScrollPosition() {
    const container = scrollport.value;
    showScrollTop.value = (container?.scrollTop ?? 0) > 1;
    listHeadingAbove.value = false;
    if (!enabled.value || !container || !listHeading) return;

    const headingTop = listHeading.getBoundingClientRect().top;
    const containerTop = container.getBoundingClientRect().top + container.clientTop;
    const margin = parseFloat(getComputedStyle(listHeading).scrollMarginTop) || 0;
    // Allow for subpixel rounding at the browser's final scroll position.
    listHeadingAbove.value = headingTop < containerTop + margin - 1;
  }

  function applyCommand() {
    frame = 0;
    if (!enabled.value || !scrollport.value || !command.value) return;
    // Returning from a menu must restore the background even under a dialog.
    if (dialogOpen.value && command.value.kind !== "position") return;
    const behavior = window.matchMedia("(prefers-reduced-motion: reduce)").matches
      ? "instant" : command.value.kind === "position" ? command.value.behavior : "smooth";
    if (command.value.kind === "center") {
      rows.get(command.value.key)?.scrollIntoView({ block: "center", inline: "nearest", behavior });
    } else if (command.value.kind === "list") {
      listHeading?.scrollIntoView({ block: "start", inline: "nearest", behavior });
    } else {
      scrollport.value.scrollTo({ top: command.value.top, behavior });
    }
    updateScrollPosition();
  }

  function scheduleCommand() {
    if (!disposed && !frame) frame = requestAnimationFrame(applyCommand);
  }

  function onResize() {
    const container = scrollport.value;
    scrollbarWidth.value = container ? container.offsetWidth - container.clientWidth : 0;
    updateScrollPosition();
    scheduleCommand();
  }

  function centerGuess(key: string) {
    command.value = { kind: "center", key };
    void nextTick(scheduleCommand);
  }

  function cancelCentering() {
    if (command.value && enabled.value && scrollport.value) {
      scrollport.value.scrollTo({ top: scrollport.value.scrollTop, behavior: "instant" });
    }
    command.value = null;
  }

  function restorePosition(top: number, behavior: ScrollBehavior = "instant") {
    command.value = { kind: "position", top, behavior };
    cancelAnimationFrame(frame);
    applyCommand();
  }

  function scrollUp() {
    updateScrollPosition();
    if (!listHeadingAbove.value) {
      scrollToTop();
      return;
    }
    command.value = { kind: "list" };
    void nextTick(scheduleCommand);
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
    listHeading = null;
  });

  return { showScrollTop, scrollbarWidth, registerListHeading, scrollUp, scrollUpLabel, scrollUpIcon, registerGuess, updateScrollPosition, centerGuess, cancelCentering, restorePosition, scrollToTop };
}
