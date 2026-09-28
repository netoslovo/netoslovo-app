import { onBeforeUnmount, ref, type ComputedRef } from "vue";
import {
  onBeforeRouteLeave,
  onBeforeRouteUpdate,
  type NavigationGuardReturn,
  useRoute,
  useRouter,
} from "vue-router";
import { authState } from "../../auth/model/authSession";
import { userNoticeCodes } from "../../user-notices/model/userNoticeCodes";
import { useUserNoticeStore } from "../../user-notices/model/userNoticeStore";
import type { GameState } from "../model/game";

export function useGuestLoginPrompt(
  gameState: ComputedRef<GameState | null>,
) {
  const route = useRoute();
  const router = useRouter();
  const userNotices = useUserNoticeStore();
  const visible = ref(false);
  let pendingDestination: string | null = null;
  let pendingAction: (() => unknown) | null = null;
  let resolveNavigation: ((result: NavigationGuardReturn) => void) | null = null;
  let shownForCurrentPage = false;

  onBeforeRouteLeave((to) => guardNavigation(to.fullPath));
  onBeforeRouteUpdate((to) => guardNavigation(to.fullPath));

  async function guardNavigation(
    destination: string,
  ): Promise<NavigationGuardReturn> {
    if (
      visible.value ||
      authState.value?.isAuthenticated !== false ||
      (gameState.value !== "guessed" && gameState.value !== "surrendered")
    ) {
      return visible.value ? false : true;
    }

    if (!await shouldShowPrompt()) {
      return true;
    }

    pendingDestination = destination;
    showPrompt();

    return new Promise<NavigationGuardReturn>((resolve) => {
      resolveNavigation = resolve;
    });
  }

  onBeforeUnmount(() => resolveNavigation?.(false));

  async function runBeforeLeaving(action: () => unknown) {
    if (!await shouldShowPrompt()) {
      await action();
      return;
    }

    pendingAction = action;
    showPrompt();
  }

  async function shouldShowPrompt() {
    if (
      shownForCurrentPage ||
      authState.value?.isAuthenticated !== false ||
      (gameState.value !== "guessed" && gameState.value !== "surrendered")
    ) {
      return false;
    }

    const shouldShow = await userNotices.shouldShowUserNotice(
      userNoticeCodes.guestLoginAfterFinishedGame,
    );
    return shouldShow && authState.value?.isAuthenticated === false;
  }

  function showPrompt() {
    shownForCurrentPage = true;
    visible.value = true;
    void userNotices.saveOneTimeUserNoticeView(
      userNoticeCodes.guestLoginAfterFinishedGame,
    );
  }

  function continueNavigation() {
    const action = pendingAction;
    pendingAction = null;
    if (action) {
      visible.value = false;
      void action();
      return;
    }

    finishPrompt(true);
  }

  function requestLogin() {
    if (pendingAction) {
      pendingAction = null;
      visible.value = false;
      void router.push({
        name: "login",
        query: { returnTo: route.fullPath },
      });
      return;
    }

    const returnTo = pendingDestination;
    finishPrompt({
      name: "login",
      query: returnTo ? { returnTo } : undefined,
    });
  }

  function finishPrompt(result: NavigationGuardReturn) {
    visible.value = false;
    pendingDestination = null;
    const resolve = resolveNavigation;
    resolveNavigation = null;
    resolve?.(result);
  }

  return {
    visible,
    runBeforeLeaving,
    continueNavigation,
    requestLogin,
  };
}
