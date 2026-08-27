import { computed, onBeforeUnmount, ref, watch } from "vue";
import { authBootstrapState, sessionRevision } from "../../auth/model/authSession";
import { isApiRequestCanceled, toApiError } from "../../../shared/api/apiError";
import { showToast } from "../../../shared/notifications/toastStore";
import { createGame, getDifficulties } from "../api/gameApi";
import type { Difficulty, Game } from "../model/game";
import { getCreateGameErrorMessage } from "../lib/gameErrors";

export function useArcadeCreation(onCreated: (game: Game) => unknown) {
  const difficulties = ref<Difficulty[]>([]);
  const selectedDifficultyCode = ref("easy");
  const loading = ref(false);
  const creating = ref(false);
  const failed = ref(false);
  let requestId = 0;
  let controller: AbortController | null = null;

  const gameplayLocked = computed(() => authBootstrapState.value === "error" || failed.value);
  const canStart = computed(() =>
    authBootstrapState.value === "ready" && difficulties.value.length > 0 && !creating.value,
  );

  watch([authBootstrapState, sessionRevision], () => {
    if (authBootstrapState.value === "ready") void loadDifficulties();
  }, { immediate: true });
  onBeforeUnmount(() => controller?.abort());

  async function loadDifficulties() {
    controller?.abort();
    const abortController = new AbortController();
    controller = abortController;
    const currentRequest = ++requestId;
    loading.value = true;
    failed.value = false;
    try {
      const loadedDifficulties = await getDifficulties({ signal: abortController.signal });
      if (currentRequest !== requestId) return;
      difficulties.value = loadedDifficulties;
      selectedDifficultyCode.value = difficulties.value[0]?.code ?? "easy";
    } catch (error) {
      if (isApiRequestCanceled(error) || currentRequest !== requestId) return;
      difficulties.value = [];
      failed.value = true;
      showToast({
        status: "error",
        title: "Игра",
        message: "Не удалось загрузить данные игры. Попробуйте обновить страницу.",
      });
    } finally {
      if (currentRequest === requestId) loading.value = false;
      if (controller === abortController) controller = null;
    }
  }

  async function create() {
    if (!canStart.value) return;
    creating.value = true;
    try {
      await onCreated(await createGame(selectedDifficultyCode.value));
    } catch (error) {
      showToast({ status: "error", title: "Новая игра", message: getCreateGameErrorMessage(toApiError(error)) });
    } finally {
      creating.value = false;
    }
  }

  return { difficulties, selectedDifficultyCode, loading, creating, gameplayLocked, canStart, create };
}
