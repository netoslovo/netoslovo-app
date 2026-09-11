import { onBeforeUnmount, onMounted } from "vue";

const viewportTopProperty = "--app-visual-viewport-top";
const viewportBottomProperty = "--app-visual-viewport-bottom";

export function useVisualViewportCssVariables() {
  let animationFrame: number | null = null;
  let previousViewportTop: number | null = null;
  let previousViewportBottom: number | null = null;

  function updateViewportVariables() {
    animationFrame = null;

    const viewport = window.visualViewport;
    const viewportTop = Math.max(0, Math.round(viewport?.offsetTop ?? 0));
    // Embedded browsers may shrink or pan only the visual viewport when their keyboard opens.
    const viewportBottom = !viewport || Math.abs(viewport.scale - 1) > 0.01
      ? 0
      : Math.max(
        0,
        Math.round(window.innerHeight - viewportTop - viewport.height),
      );
    const rootStyle = document.documentElement.style;

    if (viewportTop !== previousViewportTop) {
      previousViewportTop = viewportTop;
      rootStyle.setProperty(viewportTopProperty, `${viewportTop}px`);
    }

    if (viewportBottom !== previousViewportBottom) {
      previousViewportBottom = viewportBottom;
      rootStyle.setProperty(viewportBottomProperty, `${viewportBottom}px`);
    }
  }

  function scheduleViewportUpdate() {
    if (animationFrame !== null) return;
    animationFrame = window.requestAnimationFrame(updateViewportVariables);
  }

  onMounted(() => {
    scheduleViewportUpdate();
    window.addEventListener("resize", scheduleViewportUpdate);
    window.visualViewport?.addEventListener("resize", scheduleViewportUpdate);
    window.visualViewport?.addEventListener("scroll", scheduleViewportUpdate);
  });

  onBeforeUnmount(() => {
    if (animationFrame !== null) window.cancelAnimationFrame(animationFrame);
    window.removeEventListener("resize", scheduleViewportUpdate);
    window.visualViewport?.removeEventListener("resize", scheduleViewportUpdate);
    window.visualViewport?.removeEventListener("scroll", scheduleViewportUpdate);

    const rootStyle = document.documentElement.style;
    rootStyle.removeProperty(viewportTopProperty);
    rootStyle.removeProperty(viewportBottomProperty);
  });
}
