import { onBeforeUnmount, onMounted } from "vue";

const viewportTopProperty = "--app-visual-viewport-top";
const viewportHeightProperty = "--app-visual-viewport-height";
const viewportSafeBottomProperty = "--app-visual-viewport-safe-bottom";

export function useVisualViewportCssVariables() {
  let animationFrame: number | null = null;
  let previousViewportTop: number | null = null;
  let previousViewportHeight: number | null = null;
  let previousSafeBottomEnabled: boolean | null = null;

  function updateViewportVariables() {
    animationFrame = null;

    const viewport = window.visualViewport;
    // Pinch zoom should magnify the layout, not make it reflow like a keyboard resize.
    if (!viewport || Math.abs(viewport.scale - 1) > 0.01) return;

    const viewportHeight = Math.max(0, Math.round(viewport.height));
    const layoutViewportHeight = document.documentElement.clientHeight;
    const maxViewportTop = Math.max(0, layoutViewportHeight - viewportHeight);
    // Track keyboard panning without applying native overscroll a second time.
    const viewportTop = Math.min(maxViewportTop, Math.max(0, Math.round(viewport.offsetTop)));
    const safeBottomEnabled = viewport.offsetTop + viewport.height >= layoutViewportHeight - 1;
    const rootStyle = document.documentElement.style;

    if (viewportTop !== previousViewportTop) {
      previousViewportTop = viewportTop;
      rootStyle.setProperty(viewportTopProperty, `${viewportTop}px`);
    }

    if (viewportHeight !== previousViewportHeight) {
      previousViewportHeight = viewportHeight;
      rootStyle.setProperty(viewportHeightProperty, `${viewportHeight}px`);
    }

    if (safeBottomEnabled !== previousSafeBottomEnabled) {
      previousSafeBottomEnabled = safeBottomEnabled;
      rootStyle.setProperty(
        viewportSafeBottomProperty,
        safeBottomEnabled ? "env(safe-area-inset-bottom)" : "0px",
      );
    }
  }

  function scheduleViewportUpdate() {
    if (animationFrame !== null) return;
    animationFrame = window.requestAnimationFrame(updateViewportVariables);
  }

  onMounted(() => {
    updateViewportVariables();
    window.addEventListener("resize", scheduleViewportUpdate);
    window.addEventListener("pageshow", scheduleViewportUpdate);
    window.visualViewport?.addEventListener("resize", scheduleViewportUpdate);
    window.visualViewport?.addEventListener("scroll", scheduleViewportUpdate);
  });

  onBeforeUnmount(() => {
    if (animationFrame !== null) window.cancelAnimationFrame(animationFrame);
    window.removeEventListener("resize", scheduleViewportUpdate);
    window.removeEventListener("pageshow", scheduleViewportUpdate);
    window.visualViewport?.removeEventListener("resize", scheduleViewportUpdate);
    window.visualViewport?.removeEventListener("scroll", scheduleViewportUpdate);

    const rootStyle = document.documentElement.style;
    rootStyle.removeProperty(viewportTopProperty);
    rootStyle.removeProperty(viewportHeightProperty);
    rootStyle.removeProperty(viewportSafeBottomProperty);
  });
}
