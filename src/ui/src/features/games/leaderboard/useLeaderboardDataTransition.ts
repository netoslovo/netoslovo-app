import { nextTick, ref, toValue, watch, type MaybeRefOrGetter } from "vue";

export function useLeaderboardDataTransition(
  skeletonVisible: MaybeRefOrGetter<boolean>,
) {
  const enabled = ref(false);
  let skeletonWasShown = false;

  watch(
    () => toValue(skeletonVisible),
    (visible) => {
      if (visible) skeletonWasShown = true;
    },
    { flush: "sync" },
  );

  function startLoad() {
    skeletonWasShown = toValue(skeletonVisible);
    enabled.value = false;
  }

  function revealData(renderData: () => void) {
    enabled.value = !skeletonWasShown && !toValue(skeletonVisible);
    renderData();

    void nextTick(() => {
      enabled.value = true;
    });
  }

  return {
    enabled,
    startLoad,
    revealData,
  };
}
