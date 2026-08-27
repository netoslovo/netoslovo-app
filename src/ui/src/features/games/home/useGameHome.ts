import { computed, onBeforeUnmount, ref, watch } from "vue";
import {
  authBootstrapState,
  bootstrapAuthState,
  isAuthenticated,
  sessionRevision,
} from "../../auth/model/authSession";
import { isApiRequestCanceled } from "../../../shared/api/apiError";
import {
  getDailyGameCurrentPlayerStreak,
  getDailyHistory,
  getLatestActiveArcadeGame,
  getTodayDailyGame,
} from "../api/gameApi";
import type { ArcadeGame, DailyGame, DailyGameStreak } from "../model/game";
import { isDailyScheduleNotFound } from "../lib/gameErrors";

type LoadResult<T> = {
  data: T | null;
  failed: boolean;
};

export function useGameHome() {
  const arcadeGame = ref<ArcadeGame | null>(null);
  const dailyGames = ref<DailyGame[]>([]);
  const dailyStreak = ref<DailyGameStreak | null>(null);
  const arcadeFailed = ref(false);
  const todayFailed = ref(false);
  const dailyHistoryFailed = ref(false);
  const dailyStreakFailed = ref(false);
  const loading = ref(false);
  let requestId = 0;
  let controller: AbortController | null = null;

  const authFailed = computed(() => authBootstrapState.value === "error");
  const allRequestsFailed = computed(
    () =>
      arcadeFailed.value &&
      todayFailed.value &&
      dailyHistoryFailed.value &&
      dailyStreakFailed.value,
  );
  const statusMessage = computed(() =>
    authFailed.value || allRequestsFailed.value
      ? "Игра временно недоступна. Попробуйте позже."
      : null,
  );

  watch(
    [authBootstrapState, sessionRevision],
    () => {
      if (authBootstrapState.value === "ready") {
        void load();
      }
    },
    { immediate: true },
  );

  onBeforeUnmount(() => controller?.abort());

  async function load() {
    controller?.abort();
    const abortController = new AbortController();
    controller = abortController;
    const currentRequest = ++requestId;
    loading.value = true;
    arcadeFailed.value = false;
    todayFailed.value = false;
    dailyHistoryFailed.value = false;
    dailyStreakFailed.value = false;
    try {
      const [activeArcade, today, history, currentStreak] = await Promise.all([
        loadRequest(() =>
          getLatestActiveArcadeGame({ signal: abortController.signal }),
        ),
        loadRequest(() => getToday(abortController.signal)),
        loadRequest(() =>
          getDailyHistory(0, 7, { signal: abortController.signal }),
        ),
        loadRequest(() =>
          getDailyGameCurrentPlayerStreak({ signal: abortController.signal }),
        ),
      ]);
      if (currentRequest !== requestId) return;
      arcadeGame.value = activeArcade.data;
      dailyStreak.value = currentStreak.data;
      arcadeFailed.value = activeArcade.failed;
      todayFailed.value = today.failed;
      dailyHistoryFailed.value = history.failed;
      dailyStreakFailed.value = currentStreak.failed;
      dailyGames.value = today.data
        ? [
            today.data,
            ...(history.data?.games ?? []).filter(
              (item) => item.day !== today.data?.day,
            ),
          ].slice(0, 7)
        : (history.data?.games ?? []);
    } catch (error) {
      if (isApiRequestCanceled(error) || currentRequest !== requestId) return;
      arcadeGame.value = null;
      dailyStreak.value = null;
      arcadeFailed.value = true;
      todayFailed.value = true;
      dailyHistoryFailed.value = true;
      dailyStreakFailed.value = true;
      dailyGames.value = [];
    } finally {
      if (currentRequest === requestId) loading.value = false;
      if (controller === abortController) controller = null;
    }
  }

  async function getToday(signal: AbortSignal) {
    try {
      return await getTodayDailyGame({ signal });
    } catch (error) {
      if (isDailyScheduleNotFound(error)) return null;
      throw error;
    }
  }

  async function loadRequest<T>(
    request: () => Promise<T>,
  ): Promise<LoadResult<T>> {
    try {
      return { data: await request(), failed: false };
    } catch (error) {
      if (isApiRequestCanceled(error)) throw error;
      return { data: null, failed: true };
    }
  }

  async function retry() {
    if (authFailed.value) {
      try {
        await bootstrapAuthState();
      } catch { }
      return;
    }
    await load();
  }

  return {
    arcadeGame,
    dailyGames,
    dailyStreak,
    arcadeFailed,
    todayFailed,
    dailyHistoryFailed,
    dailyStreakFailed,
    loading,
    authFailed,
    authenticated: isAuthenticated,
    statusMessage,
    retry,
  };
}
