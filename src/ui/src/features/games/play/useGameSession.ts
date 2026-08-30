import { computed, onBeforeUnmount, ref, toValue, watch, type ComputedRef, type MaybeRefOrGetter } from "vue";
import { sessionRevision } from "../../auth/model/authSession";
import {
  isApiRequestCanceled,
  matchesApiError,
  toApiError,
  type ApiError,
} from "../../../shared/api/apiError";
import { showToast } from "../../../shared/notifications/toastStore";
import {
  createGame,
  getArcadeGameById,
  getDailyGameForDay,
  getDifficulties,
  getLatestActiveArcadeGame,
  makeGuess,
  revealHalfwayWord,
  revealRandomLetter,
  revealWordLength,
  startDailyGameForDay,
  surrender,
} from "../api/gameApi";
import type { Difficulty, Game } from "../model/game";
import {
  classifyGameActionError,
  shouldRefreshGameAfterActionError,
  type GameActionKind,
  type GameActionResult,
} from "../lib/gameActionOutcomes";
import { getCreateGameErrorMessage, isDailyScheduleNotFound } from "../lib/gameErrors";

export type GameMode = "arcade" | "daily";
export type GuessPresentationEvent = {
  id: number;
  word: string;
  kind: "insert" | "repeat";
  previousFillPercentage: number;
};

export function useGameSession(
  mode: GameMode,
  routeKey: ComputedRef<string | null>,
  onMissing?: () => void,
  replayAvailable: MaybeRefOrGetter<boolean> = true,
) {
  const game = ref<Game | null>(null);
  const loading = ref(false);
  const unavailable = ref(false);
  const failed = ref(false);
  const guessing = ref(false);
  const finishedGameRefreshing = ref(false);
  const animatedHintWord = ref<string | null>(null);
  const guessPresentationEvent = ref<GuessPresentationEvent | null>(null);
  const difficulties = ref<Difficulty[]>([]);
  const selectedDifficultyCode = ref("easy");
  const creating = ref(false);
  const replayDifficultiesRequestLoading = ref(false);
  const replayDifficultiesLoaded = ref(false);
  let loadRequestId = 0;
  let actionRequestId = 0;
  let guessPresentationEventId = 0;
  let controller: AbortController | null = null;

  const currentGuess = computed(() => game.value?.currentGuess ?? null);
  const finished = computed(() => game.value?.gameState !== "active");
  const replayDifficultiesLoading = computed(() =>
    replayDifficultiesRequestLoading.value ||
    (
      mode === "arcade" &&
      game.value !== null &&
      game.value.gameState !== "active" &&
      !replayDifficultiesLoaded.value
    ),
  );
  const canReplay = computed(() =>
    mode === "arcade" &&
    toValue(replayAvailable) &&
    replayDifficultiesLoaded.value &&
    difficulties.value.length > 0 &&
    !creating.value,
  );

  watch([routeKey, sessionRevision], () => void load(), { immediate: true });
  watch(
    () => toValue(replayAvailable),
    (available) => {
      if (available && game.value && game.value.gameState !== "active") void loadReplayDifficulties();
    },
  );
  onBeforeUnmount(() => {
    loadRequestId += 1;
    actionRequestId += 1;
    controller?.abort();
  });

  async function load() {
    controller?.abort();
    const abortController = new AbortController();
    controller = abortController;
    const requestId = ++loadRequestId;
    actionRequestId += 1;
    loading.value = true;
    unavailable.value = false;
    failed.value = false;
    animatedHintWord.value = null;
    guessPresentationEvent.value = null;
    replayDifficultiesRequestLoading.value = false;
    replayDifficultiesLoaded.value = difficulties.value.length > 0;
    try {
      const loadedGame = mode === "daily"
        ? await loadDaily(routeKey.value, abortController.signal)
        : await loadArcade(routeKey.value, abortController.signal);
      if (requestId !== loadRequestId) return;
      game.value = loadedGame;
      if (!loadedGame && mode === "arcade") onMissing?.();
      loading.value = false;
      if (loadedGame?.gameState !== "active" && mode === "arcade" && toValue(replayAvailable)) {
        await loadReplayDifficulties(requestId, abortController.signal);
      }
    } catch (error) {
      if (isApiRequestCanceled(error) || requestId !== loadRequestId) return;
      if (mode === "daily" && isDailyScheduleNotFound(error)) {
        unavailable.value = true;
        game.value = null;
        return;
      }
      game.value = null;
      failed.value = true;
      showToast({
        status: "error",
        title: "Загрузка игры",
        message: mode === "daily" ? "Не удалось загрузить слово дня" : "Не удалось загрузить случайную игру",
      });
    } finally {
      if (requestId === loadRequestId) loading.value = false;
      if (controller === abortController) controller = null;
    }
  }

  async function loadArcade(id: string | null, signal: AbortSignal) {
    if (id) return getArcadeGameById(id, { signal });
    const summary = await getLatestActiveArcadeGame({ signal });
    return summary ? getArcadeGameById(summary.id, { signal }) : null;
  }

  async function loadDaily(day: string | null, signal: AbortSignal) {
    if (!day) return null;
    const existing = await getDailyGameForDay(day, { signal });
    if (existing) return existing;
    try {
      return await startDailyGameForDay(day, { signal });
    } catch (error) {
      if (!matchesApiError(error, 409, "AlreadyHasActiveGame")) throw error;
      const recovered = await getDailyGameForDay(day, { signal });
      if (!recovered) throw error;
      return recovered;
    }
  }

  async function loadReplayDifficulties(requestId = loadRequestId, signal?: AbortSignal) {
    if (difficulties.value.length > 0) {
      replayDifficultiesLoaded.value = true;
      return;
    }

    replayDifficultiesRequestLoading.value = true;
    try {
      const values = await getDifficulties({ signal });
      if (requestId !== loadRequestId) return;
      difficulties.value = values;
      replayDifficultiesLoaded.value = true;
      selectedDifficultyCode.value = game.value?.difficulty.code ?? values[0]?.code ?? "easy";
    } catch (error) {
      if (isApiRequestCanceled(error) || requestId !== loadRequestId) return;
      replayDifficultiesLoaded.value = true;
    } finally {
      if (requestId === loadRequestId) replayDifficultiesRequestLoading.value = false;
    }
  }

  function captureAction() {
    const active = game.value;
    if (!active) return null;
    return { id: ++actionRequestId, gameId: active.id, routeKey: routeKey.value };
  }

  function isCurrent(context: NonNullable<ReturnType<typeof captureAction>>) {
    return context.id === actionRequestId && context.gameId === game.value?.id && context.routeKey === routeKey.value;
  }

  async function runAction(
    kind: GameActionKind,
    action: (active: Game) => Promise<void>,
    word?: string,
  ): Promise<GameActionResult> {
    const context = captureAction();
    if (!context || !game.value) {
      return {
        status: "failed",
        title: "Игра",
        message: "Игра не найдена",
      };
    }
    guessing.value = true;
    try {
      await action(game.value);
      return { status: "success" };
    } catch (error) {
      const apiError = toApiError(error);
      if (!isCurrent(context)) return { status: "success" };
      try {
        await refreshOnConflict(apiError);
      } catch (refreshError) {
        return classifyGameActionError(kind, toApiError(refreshError), word);
      }
      return classifyGameActionError(kind, apiError, word);
    } finally {
      if (isCurrent(context)) guessing.value = false;
    }
  }

  async function submitGuess(word: string) {
    return runAction("guess", async (active) => {
      const outcome = await makeGuess(active.id, word);
      if (active.id !== game.value?.id) return;
      game.value = {
        ...active,
        currentGuess: outcome.currentGuess,
        allGuesses: outcome.allGuesses,
        score: outcome.score,
        scoreDetails: outcome.scoreDetails,
        gameState: outcome.guessStatus === "guessed" ? "guessed" : active.gameState,
      };
      animatedHintWord.value = null;
      publishGuessPresentation(
        outcome.currentGuess.word,
        outcome.guessStatus === "alreadyTried",
        active.currentGuess?.fillPercentage ?? 0,
      );
      if (outcome.guessStatus === "alreadyTried") {
        showToast({ status: "info", title: "Повтор слова", message: `Вы уже пробовали слово "${outcome.currentGuess.word}".` });
      }
      if (outcome.guessStatus === "guessed") await refreshFinishedGame();
    }, word);
  }

  async function surrenderGame() {
    return runAction("surrender", async (active) => {
      await surrender(active.id);
      if (active.id !== game.value?.id) return;
      game.value = { ...active, gameState: "surrendered", currentGuess: null };
      await refreshFinishedGame();
    });
  }

  async function revealHalfwayWordGame() {
    return runAction("halfway-word", async (active) => {
      const hint = await revealHalfwayWord(active.id);
      if (active.id !== game.value?.id) return;
      const guess = hint.guessOutcome.currentGuess;
      game.value = {
        ...active,
        currentGuess: guess,
        allGuesses: hint.guessOutcome.allGuesses,
        hintsInfo: hint.hintsInfo,
        score: hint.guessOutcome.score,
        scoreDetails: hint.guessOutcome.scoreDetails,
      };
      publishGuessPresentation(
        guess.word,
        hint.guessOutcome.guessStatus === "alreadyTried",
        active.currentGuess?.fillPercentage ?? 0,
      );
      animatedHintWord.value = guess.word;
      showToast({ status: "info", title: "Промежуточное слово", message: "Подсказка открыта." });
    });
  }

  async function revealWordLengthGame() {
    return applyTextHint("word-length", revealWordLength, "Длина слова");
  }

  async function revealRandomLetterGame() {
    return applyTextHint("random-letter", revealRandomLetter, "Случайная буква");
  }

  async function applyTextHint(kind: GameActionKind, request: typeof revealWordLength, title: string) {
    return runAction(kind, async (active) => {
      const hint = await request(active.id);
      if (active.id !== game.value?.id) return;
      game.value = {
        ...active,
        displayWord: hint.displayWord,
        hintsInfo: hint.hintsInfo,
        score: hint.score,
        scoreDetails: hint.scoreDetails,
      };
      showToast({ status: "info", title, message: "Подсказка открыта." });
    });
  }

  function publishGuessPresentation(word: string, repeated: boolean, previousFillPercentage: number) {
    guessPresentationEvent.value = {
      id: ++guessPresentationEventId,
      word,
      kind: repeated ? "repeat" : "insert",
      previousFillPercentage,
    };
  }

  async function refreshFinishedGame() {
    const id = game.value?.id;
    finishedGameRefreshing.value = true;
    try {
      const refreshed = mode === "daily" && routeKey.value
        ? await getDailyGameForDay(routeKey.value)
        : id ? await getArcadeGameById(id) : null;
      if (id && game.value?.id === id && refreshed) game.value = refreshed;
      if (mode === "arcade" && toValue(replayAvailable)) await loadReplayDifficulties();
    } finally {
      finishedGameRefreshing.value = false;
    }
  }

  async function refreshCurrentGame() {
    const id = game.value?.id;
    const refreshed = mode === "daily" && routeKey.value
      ? await getDailyGameForDay(routeKey.value)
      : id ? await getArcadeGameById(id) : null;
    if (id && game.value?.id === id && refreshed) game.value = refreshed;
    if (refreshed?.gameState !== "active" && mode === "arcade" && toValue(replayAvailable)) {
      await loadReplayDifficulties();
    }
  }

  async function refreshOnConflict(error: ApiError) {
    if (shouldRefreshGameAfterActionError(error)) {
      await refreshCurrentGame();
    }
  }

  async function replay(onCreated: (game: Game) => unknown) {
    if (!canReplay.value) return;
    creating.value = true;
    try {
      await onCreated(await createGame(selectedDifficultyCode.value));
    } catch (error) {
      showToast({ status: "error", title: "Новая игра", message: getCreateGameErrorMessage(toApiError(error)) });
    } finally {
      creating.value = false;
    }
  }

  return {
    game,
    loading,
    unavailable,
    failed,
    guessing,
    finishedGameRefreshing,
    animatedHintWord,
    guessPresentationEvent,
    currentGuess,
    finished,
    difficulties,
    selectedDifficultyCode,
    creating,
    replayDifficultiesLoading,
    canReplay,
    load,
    submitGuess,
    surrenderGame,
    revealHalfwayWordGame,
    revealWordLengthGame,
    revealRandomLetterGame,
    replay,
  };
}
