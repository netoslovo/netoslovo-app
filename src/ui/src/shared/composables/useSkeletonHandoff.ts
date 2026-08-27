import {
  inject,
  onBeforeUnmount,
  provide,
  readonly,
  ref,
  toValue,
  watch,
  type InjectionKey,
  type MaybeRefOrGetter,
  type Ref,
} from "vue";
import { useDelayedLoading, useDelayedLoadingState } from "./useDelayedLoading";

const handoffDurationMs = 300;
const skeletonHandoffKey: InjectionKey<Readonly<Ref<boolean>>> = Symbol(
  "skeleton-handoff",
);

export function provideSkeletonHandoff(
  genericSkeletonVisible: MaybeRefOrGetter<boolean>,
) {
  const handoffActive = ref(false);
  let timer: ReturnType<typeof setTimeout> | null = null;

  function clearTimer() {
    if (timer) {
      clearTimeout(timer);
      timer = null;
    }
  }

  function armHandoff() {
    clearTimer();
    handoffActive.value = true;
    timer = setTimeout(() => {
      handoffActive.value = false;
      timer = null;
    }, handoffDurationMs);
  }

  watch(
    () => toValue(genericSkeletonVisible),
    (visible, wasVisible) => {
      if (!visible && wasVisible) {
        armHandoff();
      }
    },
  );

  onBeforeUnmount(clearTimer);
  provide(skeletonHandoffKey, readonly(handoffActive));
}

export function useHandoffDelayedLoading(
  loading: MaybeRefOrGetter<boolean>,
  delayMs?: number,
) {
  return useDelayedLoading(
    loading,
    delayMs,
    inject(skeletonHandoffKey) ?? false,
  );
}

export function useHandoffDelayedLoadingState(
  loading: MaybeRefOrGetter<boolean>,
  delayMs?: number,
) {
  return useDelayedLoadingState(
    loading,
    delayMs,
    inject(skeletonHandoffKey) ?? false,
  );
}
