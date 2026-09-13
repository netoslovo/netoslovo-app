const noViewportChangeDelayMs = 180;
const maximumWaitMs = 700;
const stableFramesRequired = 2;
let pendingViewportStability: Promise<void> | null = null;

type ViewportGeometry = {
  height: number;
  offsetTop: number;
  innerHeight: number;
};

function isTextEntryElement(element: Element | null): element is HTMLElement {
  return element instanceof HTMLInputElement
    || element instanceof HTMLTextAreaElement
    || (element instanceof HTMLElement && element.isContentEditable);
}

function readViewportGeometry(): ViewportGeometry {
  const viewport = window.visualViewport;
  return {
    height: viewport?.height ?? window.innerHeight,
    offsetTop: viewport?.offsetTop ?? 0,
    innerHeight: window.innerHeight,
  };
}

function geometryMatches(left: ViewportGeometry, right: ViewportGeometry) {
  return Math.abs(left.height - right.height) < 0.5
    && Math.abs(left.offsetTop - right.offsetTop) < 0.5
    && Math.abs(left.innerHeight - right.innerHeight) < 0.5;
}

export async function dismissKeyboardAndWaitForViewport() {
  if (pendingViewportStability) {
    await pendingViewportStability;
    return;
  }

  if (!window.matchMedia("(max-width: 1024px)").matches) return;

  const activeElement = document.activeElement;
  if (!isTextEntryElement(activeElement)) return;

  activeElement.blur();

  const viewportStability = new Promise<void>((resolve) => {
    let animationFrame: number | null = null;
    let lastGeometry = readViewportGeometry();
    let stableFrames = 0;
    let viewportChanged = false;

    const unchangedTimer = window.setTimeout(finish, noViewportChangeDelayMs);
    const maximumTimer = window.setTimeout(finish, maximumWaitMs);

    function finish() {
      if (animationFrame !== null) window.cancelAnimationFrame(animationFrame);
      window.clearTimeout(unchangedTimer);
      window.clearTimeout(maximumTimer);
      window.removeEventListener("resize", onViewportChange);
      window.visualViewport?.removeEventListener("resize", onViewportChange);
      window.visualViewport?.removeEventListener("scroll", onViewportChange);
      resolve();
    }

    function checkStability() {
      animationFrame = null;
      const geometry = readViewportGeometry();
      stableFrames = geometryMatches(lastGeometry, geometry) ? stableFrames + 1 : 0;
      lastGeometry = geometry;

      if (viewportChanged && stableFrames >= stableFramesRequired) {
        finish();
        return;
      }

      animationFrame = window.requestAnimationFrame(checkStability);
    }

    function onViewportChange() {
      viewportChanged = true;
      stableFrames = 0;
      window.clearTimeout(unchangedTimer);
      if (animationFrame === null) animationFrame = window.requestAnimationFrame(checkStability);
    }

    window.addEventListener("resize", onViewportChange);
    window.visualViewport?.addEventListener("resize", onViewportChange);
    window.visualViewport?.addEventListener("scroll", onViewportChange);
  });

  pendingViewportStability = viewportStability;
  try {
    await viewportStability;
  } finally {
    if (pendingViewportStability === viewportStability) pendingViewportStability = null;
  }
}
