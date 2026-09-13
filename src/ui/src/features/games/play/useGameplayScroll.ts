import { onBeforeUnmount, onMounted, ref, watch, type Ref } from "vue";

export function useGameplayScroll(
  scrollport: Ref<HTMLElement | null>,
  content: Ref<HTMLElement | null>,
  enabled: Readonly<Ref<boolean>>,
) {
  const showScrollTop = ref(false);
  let targetKey: string | null = null;
  let resizeObserver: ResizeObserver | null = null;

  function updateScrollPosition() {
    showScrollTop.value = (scrollport.value?.scrollTop ?? 0) > 1;
  }

  function centerTarget() {
    const container = scrollport.value;
    if (!enabled.value || !container || !targetKey) return;
    const target = Array.from(content.value?.querySelectorAll<HTMLElement>("[data-game-scroll-anchor]") ?? [])
      .find((element) => element.dataset.gameScrollAnchor === targetKey);
    if (!target) return;

    const row = target.getBoundingClientRect();
    const bounds = container.getBoundingClientRect();
    const top = container.scrollTop + row.top + row.height / 2
      - (bounds.top + container.clientTop + container.clientHeight / 2);
    const maximumTop = Math.max(0, container.scrollHeight - container.clientHeight);
    container.scrollTo({ top: Math.max(0, Math.min(maximumTop, top)), behavior: "instant" });
    updateScrollPosition();
  }

  function centerGuess(key: string) {
    if (!enabled.value) return;
    targetKey = key;
    centerTarget();
  }

  function cancelCentering() {
    targetKey = null;
  }

  function scrollToTop() {
    cancelCentering();
    scrollport.value?.scrollTo({
      top: 0,
      behavior: window.matchMedia("(prefers-reduced-motion: reduce)").matches ? "instant" : "smooth",
    });
  }

  watch(enabled, (active) => {
    if (!active) cancelCentering();
  }, { flush: "sync" });

  onMounted(() => {
    // Layout owns visibility. Resizing the list or its content only re-centers
    // the current request; ordinary scroll events never re-enable following.
    resizeObserver = new ResizeObserver(centerTarget);
    if (scrollport.value) resizeObserver.observe(scrollport.value);
    if (content.value) resizeObserver.observe(content.value);
    updateScrollPosition();
  });

  onBeforeUnmount(() => resizeObserver?.disconnect());

  return { showScrollTop, updateScrollPosition, centerGuess, cancelCentering, scrollToTop };
}
