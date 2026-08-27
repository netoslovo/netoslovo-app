import { computed, onBeforeUnmount, onMounted, ref } from "vue";
import { getGameSourceForReview, getRandomUnreviewedGameSourceId, getSourceByWord, reviewDailyGameSource } from "../api/adminApi";
import { isApiRequestCanceled, toApiError } from "../../../shared/api/apiError";
import type { Difficulty } from "../../games/model/game";
import type { UnreviewedSource } from "../model/admin";
import { getDifficulties } from "../../games/api/gameApi";
import { adminPermissions, hasPermission } from "../../auth/model/permissions";
import { authState } from "../../auth/model/authSession";
import { showToast } from "../../../shared/notifications/toastStore";
import { useAdminSourceSearch } from "../common/useAdminSourceSearch";
import { useModerationSwipe } from "./useModerationSwipe";

export function useAdminModeration() {

  type SourceMode = "queue" | "manual";
  type QueueState = {
    source: UnreviewedSource | null;
    queueCompleted: boolean;
    error: string | null;
  };
  type ReviewDecision = {
    source: UnreviewedSource;
    approved: boolean;
    mode: SourceMode;
  };

  const closestWordsCountOptions = [0, 5, 10, 20, 50, 100];

  const difficulties = ref<Difficulty[]>([]);
  const difficultyCode = ref("");
  const closestWordsCount = ref(10);
  const source = ref<UnreviewedSource | null>(null);
  const sourceMode = ref<SourceMode>("queue");
  const manualReviewStatus = ref<boolean | null>(null);
  const savedQueueState = ref<QueueState | null>(null);
  const loading = ref(false);
  const neighborsLoading = ref(false);
  const saving = ref(false);
  const queueCompleted = ref(false);
  const error = ref<string | null>(null);
  let loadRequestId = 0;
  let loadAbortController: AbortController | null = null;
  let disposed = false;

  const canReview = computed(() =>
    hasPermission(authState.value, adminPermissions.setSourceReview),
  );
  const {
    dragOffset,
    reset: resetSwipe,
    onPointerDown,
    onPointerMove,
    onPointerEnd,
  } = useModerationSwipe((approved) => void decide(approved));
  const {
    searchWord,
    searchError,
    searching,
    search: searchSource,
    cancel: cancelSearch,
  } = useAdminSourceSearch({
    emptyMessage: "Введите слово",
    notFoundMessage: "Слово не найдено",
    errorMessage: "Не удалось найти слово",
    load: async (word, signal) => {
      const foundSource = await getSourceByWord(word, { signal });
      const sourceForReview = await getGameSourceForReview(
        foundSource.gameSource.gameSourceId,
        closestWordsCount.value,
        { signal },
      );
      return {
        source: sourceForReview,
        approved: foundSource.approved,
      };
    },
    onFound: (result) => {
      showManualSource(result.source, result.approved);
    },
  });

  async function loadSource() {
    loadAbortController?.abort();
    if (!difficultyCode.value) {
      source.value = null;
      queueCompleted.value = false;
      return;
    }
    const requestId = ++loadRequestId;
    const abortController = new AbortController();
    loadAbortController = abortController;
    loading.value = true;
    error.value = null;
    queueCompleted.value = false;
    try {
      const selectedDifficulty = difficultyCode.value;
      const gameSourceId = await getRandomUnreviewedGameSourceId(selectedDifficulty, {
        signal: abortController.signal,
      });
      if (gameSourceId === null) {
        if (requestId === loadRequestId && selectedDifficulty === difficultyCode.value) {
          source.value = null;
          queueCompleted.value = true;
        }
        return;
      }

      const loadedSource = await getGameSourceForReview(
        gameSourceId,
        closestWordsCount.value,
        { signal: abortController.signal },
      );
      if (requestId === loadRequestId && selectedDifficulty === difficultyCode.value) {
        source.value = loadedSource;
      }
    } catch (caught) {
      if (isApiRequestCanceled(caught) || requestId !== loadRequestId) return;
      const apiError = toApiError(caught);
      source.value = null;
      if (apiError.code === "GameSourceNotFound") {
        queueCompleted.value = true;
      } else {
        error.value = "Не удалось загрузить слово для модерации";
        showToast({ status: "error", title: "Модерация", message: error.value });
      }
    } finally {
      if (requestId === loadRequestId) loading.value = false;
      if (loadAbortController === abortController) loadAbortController = null;
    }
  }

  function showManualSource(manualSource: UnreviewedSource, approved: boolean | null) {
    if (sourceMode.value === "queue") {
      savedQueueState.value = {
        source: source.value,
        queueCompleted: queueCompleted.value,
        error: error.value,
      };
    }
    sourceMode.value = "manual";
    source.value = manualSource;
    manualReviewStatus.value = approved;
    queueCompleted.value = false;
    error.value = null;
    resetSwipe();
  }

  function restoreQueue() {
    if (sourceMode.value !== "manual") return;
    const queueState = savedQueueState.value;
    sourceMode.value = "queue";
    source.value = queueState?.source ?? null;
    queueCompleted.value = queueState?.queueCompleted ?? false;
    error.value = queueState?.error ?? null;
    manualReviewStatus.value = null;
    savedQueueState.value = null;
    resetSwipe();
  }

  function changeDifficulty() {
    cancelSearch();
    restoreQueue();
    void loadSource();
  }

  async function decide(approved: boolean) {
    if (!source.value || saving.value || !canReview.value) return;
    const decidedSource = source.value;
    const decidedMode = sourceMode.value;
    const decisionChanged = decidedMode === "queue" || manualReviewStatus.value !== approved;
    const decision: ReviewDecision = { source: decidedSource, approved, mode: decidedMode };
    saving.value = true;
    try {
      await reviewDailyGameSource(decidedSource.gameSource.gameSourceId, approved);
      if (decidedMode === "manual") {
        restoreQueue();
      } else {
        resetSwipe();
        await loadSource();
      }
      if (decisionChanged) {
        showToast({
          status: "success",
          title: "Решение",
          message: `Слово «${decidedSource.gameSource.word}» ${approved ? "одобрено" : "отклонено"}.`,
          actionLabel: "Отменить",
          onAction: () => void undoDecision(decision),
          durationMs: 8000,
        });
      }
    } catch (caught) {
      const apiError = toApiError(caught);
      showToast({
        status: "error",
        title: "Решение",
        message: apiError.code === "AlreadyAssignedToGameLocked"
          ? "Слово уже назначено и не может быть изменено"
          : "Не удалось сохранить решение",
      });
    } finally {
      saving.value = false;
    }
  }

  async function reloadClosestWords() {
    const currentSource = source.value;
    const currentMode = sourceMode.value;
    if (!currentSource || neighborsLoading.value) return;
    neighborsLoading.value = true;
    try {
      const updatedSource = await getGameSourceForReview(
        currentSource.gameSource.gameSourceId,
        closestWordsCount.value,
      );
      if (
        sourceMode.value === currentMode &&
        source.value?.gameSource.gameSourceId === currentSource.gameSource.gameSourceId
      ) {
        source.value = updatedSource;
      }
    } catch {
      showToast({ status: "error", title: "Соседние слова", message: "Не удалось обновить список." });
    } finally {
      neighborsLoading.value = false;
    }
  }

  function skipSource() {
    if (!source.value || loading.value || saving.value) return;
    if (sourceMode.value === "manual") {
      restoreQueue();
    } else {
      void loadSource();
    }
  }

  async function undoDecision(item: ReviewDecision) {
    try {
      await reviewDailyGameSource(item.source.gameSource.gameSourceId, !item.approved);
      if (item.mode === "manual") {
        showManualSource(item.source, !item.approved);
      } else {
        restoreQueue();
        source.value = item.source;
        queueCompleted.value = false;
        error.value = null;
      }
      showToast({
        status: "success",
        title: "Решение",
        message: `Решение для слова «${item.source.gameSource.word}» отменено.`,
      });
    } catch (caught) {
      const apiError = toApiError(caught);
      showToast({
        status: "error",
        title: "Решение",
        message: apiError.code === "AlreadyAssignedToGameLocked"
          ? "Слово уже назначено, отменить одобрение нельзя"
          : "Не удалось отменить последнее решение",
      });
    }
  }

  onMounted(async () => {
    loading.value = true;
    let shouldLoadSource = false;
    try {
      difficulties.value = await getDifficulties();
      if (disposed) return;
      difficultyCode.value = difficulties.value[0]?.code ?? "";
      if (difficultyCode.value) {
        shouldLoadSource = true;
      } else {
        error.value = "Сложности недоступны";
      }
    } catch {
      error.value = "Не удалось загрузить сложности";
      showToast({ status: "error", title: "Сложность", message: "Не удалось загрузить список сложностей." });
    } finally {
      loading.value = false;
    }

    if (shouldLoadSource && !disposed) void loadSource();
  });

  onBeforeUnmount(() => {
    disposed = true;
    cancelSearch();
    loadRequestId += 1;
    loadAbortController?.abort();
  });
  return {
    closestWordsCountOptions, difficulties, difficultyCode, closestWordsCount,
    source, sourceMode, manualReviewStatus,
    searchWord, searchError, searching, loading, neighborsLoading, saving,
    queueCompleted, error, dragOffset, canReview,
    loadSource, searchSource, changeDifficulty, decide, reloadClosestWords,
    skipSource, onPointerDown, onPointerMove, onPointerEnd,
  };
}
