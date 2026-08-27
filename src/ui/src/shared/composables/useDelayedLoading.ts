import {
  computed,
  onBeforeUnmount,
  reactive,
  readonly,
  ref,
  toValue,
  watch,
  type MaybeRefOrGetter,
} from "vue";

const defaultDelayMs = 180;

export function useDelayedLoading(
  loading: MaybeRefOrGetter<boolean>,
  delayMs = defaultDelayMs,
  immediate: MaybeRefOrGetter<boolean> = false,
) {
  return createDelayedLoadingRefs(loading, delayMs, immediate).visible;
}

export function useDelayedLoadingState(
  loading: MaybeRefOrGetter<boolean>,
  delayMs = defaultDelayMs,
  immediate: MaybeRefOrGetter<boolean> = false,
) {
  return readonly(reactive(createDelayedLoadingRefs(loading, delayMs, immediate)));
}

function createDelayedLoadingRefs(
  loading: MaybeRefOrGetter<boolean>,
  delayMs: number,
  immediate: MaybeRefOrGetter<boolean>,
) {
  const visible = ref(false);
  const active = computed(() => toValue(loading));
  const ready = computed(() => !active.value);
  let timer: ReturnType<typeof setTimeout> | null = null;

  function clearTimer() {
    if (timer) {
      clearTimeout(timer);
      timer = null;
    }
  }

  watch(
    active,
    (isLoading) => {
      clearTimer();

      if (!isLoading) {
        visible.value = false;
        return;
      }

      if (toValue(immediate)) {
        visible.value = true;
        return;
      }

      timer = setTimeout(() => {
        visible.value = true;
        timer = null;
      }, delayMs);
    },
    { immediate: true },
  );

  onBeforeUnmount(clearTimer);

  return {
    visible: readonly(visible),
    ready,
  };
}
