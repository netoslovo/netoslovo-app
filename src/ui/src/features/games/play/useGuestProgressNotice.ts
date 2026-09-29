import { ref, watch, type Ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import { authState } from "../../auth/model/authSession";
import { showToast } from "../../../shared/notifications/toastStore";
import { userNoticeCodes } from "../../user-notices/model/userNoticeCodes";
import { useUserNoticeStore } from "../../user-notices/model/userNoticeStore";
import type { Game, GameState } from "../model/game";

export function useGuestProgressNotice(game: Readonly<Ref<Game | null>>) {
  const route = useRoute();
  const router = useRouter();
  const userNotices = useUserNoticeStore();
  const visible = ref(false);
  const saving = ref(false);
  let visibilityRequestId = 0;

  watch(
    [
      () => game.value?.id ?? null,
      () => game.value?.gameState ?? null,
      () => authState.value?.id ?? null,
      () => authState.value?.isAuthenticated ?? null,
    ],
    () => void refreshVisibility(),
    { immediate: true },
  );

  async function refreshVisibility() {
    const requestId = ++visibilityRequestId;
    const current = game.value;
    const actorId = authState.value?.id ?? null;
    visible.value = false;

    if (
      current === null ||
      !isFinishedState(current.gameState) ||
      authState.value?.isAuthenticated !== false
    ) {
      return;
    }

    const shouldShow = await userNotices.shouldShowUserNotice(
      userNoticeCodes.guestLoginAfterFinishedGame,
    );
    if (
      requestId !== visibilityRequestId ||
      game.value?.id !== current.id ||
      authState.value?.id !== actorId ||
      authState.value?.isAuthenticated !== false
    ) {
      return;
    }

    visible.value = shouldShow;
  }

  async function dismiss(doNotShowAgain: boolean) {
    if (await savePreference(doNotShowAgain)) visible.value = false;
  }

  async function requestLogin(doNotShowAgain: boolean) {
    if (!await savePreference(doNotShowAgain)) return;

    visible.value = false;
    await router.push({
      name: "login",
      query: { returnTo: route.fullPath },
    });
  }

  async function savePreference(doNotShowAgain: boolean) {
    const request = userNotices.saveRecurringUserNoticeView(
      userNoticeCodes.guestLoginAfterFinishedGame,
      doNotShowAgain,
    );
    if (!doNotShowAgain) {
      void request.catch(() => undefined);
      return true;
    }

    saving.value = true;
    try {
      await request;
      return true;
    } catch {
      showToast({
        status: "error",
        title: "Обработка запроса",
        message: "Не удалось сохранить настройку. Попробуйте ещё раз.",
      });
      return false;
    } finally {
      saving.value = false;
    }
  }

  return {
    visible,
    saving,
    dismiss,
    requestLogin,
  };
}

function isFinishedState(state: GameState) {
  return state === "guessed" || state === "surrendered";
}
