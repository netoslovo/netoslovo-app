import { onBeforeUnmount, ref, watch, type ComputedRef } from "vue";
import { isApiRequestCanceled, matchesApiError } from "../../../shared/api/apiError";
import { getDailyGamePlayerStats, getDailyGameStats } from "../api/gameApi";
import type { DailyGamePlayerStats, DailyGameStats } from "../model/game";

export type DailyGameResultStatsState = {
  aggregate: DailyGameAggregateStatsState;
  player: DailyGamePlayerStatsState;
};

type DailyGameAggregateStatsState =
  | { status: "idle" | "loading" | "insufficient" | "failed" }
  | { status: "available"; data: DailyGameStats };

type DailyGamePlayerStatsState =
  | { status: "idle" | "loading" | "delayed" | "failed" }
  | { status: "available"; data: DailyGamePlayerStats };

const playerStatsInitialDelayMs = 500;
const playerStatsRetryDelaysMs = [500, 1000];

export function useDailyGameResultStats(
  day: ComputedRef<string | null>,
  enabled: ComputedRef<boolean>,
  includePlayerStats: ComputedRef<boolean>,
) {
  const state = ref<DailyGameResultStatsState>(createInitialState());
  let gameStatsRequestId = 0;
  let playerStatsRequestId = 0;
  let gameStatsController: AbortController | null = null;
  let playerStatsController: AbortController | null = null;

  watch([day, enabled], () => void loadGameStats(), { immediate: true });
  watch([day, enabled, includePlayerStats], () => void loadPlayerStats(true), { immediate: true });

  onBeforeUnmount(() => {
    gameStatsRequestId += 1;
    playerStatsRequestId += 1;
    gameStatsController?.abort();
    playerStatsController?.abort();
  });

  async function load() {
    await Promise.all([loadGameStats(), loadPlayerStats(false)]);
  }

  async function loadGameStats() {
    gameStatsController?.abort();
    const currentDay = day.value;
    const currentRequestId = ++gameStatsRequestId;

    if (!enabled.value || !currentDay) {
      resetState();
      return;
    }

    const abortController = new AbortController();
    gameStatsController = abortController;

    state.value = {
      ...state.value,
      aggregate: { status: "loading" },
    };

    try {
      const gameStatsResult = await fetchGameStats(currentDay, abortController.signal);

      if (currentRequestId !== gameStatsRequestId) return;

      state.value = {
        ...state.value,
        aggregate: gameStatsResult
          ? { status: "available", data: gameStatsResult }
          : { status: "insufficient" },
      };
    } catch (error) {
      if (isApiRequestCanceled(error) || currentRequestId !== gameStatsRequestId) return;

      state.value = {
        ...state.value,
        aggregate: { status: "failed" },
      };
      cancelPendingPlayerStats("failed");
    } finally {
      if (gameStatsController === abortController) {
        gameStatsController = null;
      }
    }
  }

  async function loadPlayerStats(delayFirstRequest: boolean) {
    playerStatsController?.abort();
    const currentDay = day.value;
    const currentRequestId = ++playerStatsRequestId;

    if (!enabled.value || !currentDay || !includePlayerStats.value) {
      state.value = {
        ...state.value,
        player: { status: "idle" },
      };
      return;
    }

    const abortController = new AbortController();
    playerStatsController = abortController;

    state.value = {
      ...state.value,
      player: { status: "loading" },
    };

    try {
      if (delayFirstRequest) {
        await waitForPlayerStats(playerStatsInitialDelayMs, abortController.signal);
      }

      const playerStats = await fetchPlayerStats(currentDay, abortController.signal);

      if (currentRequestId !== playerStatsRequestId) return;

      state.value = {
        ...state.value,
        player: playerStats
          ? { status: "available", data: playerStats }
          : { status: "delayed" },
      };
      if (!playerStats) cancelPendingGameStats();
    } catch (error) {
      if (isApiRequestCanceled(error) || currentRequestId !== playerStatsRequestId) return;

      state.value = {
        ...state.value,
        player: { status: "failed" },
      };
      cancelPendingGameStats("failed");
    } finally {
      if (playerStatsController === abortController) {
        playerStatsController = null;
      }
    }
  }

  return {
    state,
    load,
  };

  function resetState() {
    gameStatsController?.abort();
    playerStatsController?.abort();
    state.value = createInitialState();
  }

  function cancelPendingGameStats(status: "idle" | "failed" = "idle") {
    if (!gameStatsController) return;

    gameStatsRequestId += 1;
    gameStatsController.abort();
    gameStatsController = null;
    state.value = {
      ...state.value,
      aggregate: { status },
    };
  }

  function cancelPendingPlayerStats(status: "delayed" | "failed") {
    if (!includePlayerStats.value || !playerStatsController) return;

    playerStatsRequestId += 1;
    playerStatsController.abort();
    playerStatsController = null;
    state.value = {
      ...state.value,
      player: { status },
    };
  }
}

function createInitialState(): DailyGameResultStatsState {
  return {
    aggregate: { status: "idle" },
    player: { status: "idle" },
  };
}

async function fetchPlayerStats(day: string, signal: AbortSignal): Promise<DailyGamePlayerStats | null> {
  for (let attempt = 0; ; attempt += 1) {
    try {
      return await getDailyGamePlayerStats(day, { signal });
    } catch (error) {
      if (!matchesApiError(error, 404, "GameNotFound")) throw error;
      if (attempt >= playerStatsRetryDelaysMs.length) return null;

      await waitForPlayerStats(playerStatsRetryDelaysMs[attempt], signal);
    }
  }
}

function waitForPlayerStats(delayMs: number, signal: AbortSignal) {
  return new Promise<void>((resolve, reject) => {
    if (signal.aborted) {
      reject(createAbortError());
      return;
    }

    const timeoutId = window.setTimeout(() => {
      signal.removeEventListener("abort", handleAbort);
      resolve();
    }, delayMs);
    const handleAbort = () => {
      window.clearTimeout(timeoutId);
      reject(createAbortError());
    };
    signal.addEventListener("abort", handleAbort, { once: true });
  });
}

function createAbortError() {
  return new DOMException("The operation was aborted", "AbortError");
}

async function fetchGameStats(
  day: string,
  signal: AbortSignal,
): Promise<DailyGameStats | null> {
  return getDailyGameStats(day, { signal });
}
