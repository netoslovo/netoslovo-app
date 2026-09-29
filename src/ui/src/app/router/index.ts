import { readonly, ref } from "vue";
import {
  createRouter,
  createWebHistory,
  type RouteLocationNormalized,
} from "vue-router";
import { adminDestinations } from "../../features/admin/navigation/adminNavigation";
import {
  adminRoles,
  hasPermission,
  hasRole,
} from "../../features/auth/model/permissions";
import {
  authState,
  authBootstrapState,
  bootstrapAuthState,
  consumeExpectedActorChange,
  isAuthenticated,
  reconcileActorStampChange,
  reconcileAuthenticatedSession,
} from "../../features/auth/model/authSession";
import {
  setActorStampChangeHandler,
  setUnauthorizedResponseHandler,
  type ActorStampChange,
} from "../../shared/api/httpClient";
import { showToast } from "../../shared/notifications/toastStore";

const routeNavigationLoadingValue = ref(false);
export const routeNavigationLoading = readonly(routeNavigationLoadingValue);

const appTitle = "нетослово.рф";

export function getRoutePageTitle(
  route: Pick<RouteLocationNormalized, "meta">,
) {
  return typeof route.meta.pageTitle === "string"
    ? route.meta.pageTitle
    : appTitle;
}

export function getRoutePageIdentity(
  route: Pick<RouteLocationNormalized, "name" | "params">,
) {
  const params = Object.keys(route.params)
    .sort()
    .map((key) => [key, route.params[key]]);

  return JSON.stringify([String(route.name ?? ""), params]);
}

function getDocumentTitle(route: Pick<RouteLocationNormalized, "meta">) {
  const pageTitle = getRoutePageTitle(route);
  return pageTitle === appTitle ? appTitle : `${pageTitle} — ${appTitle}`;
}

const AdminLayout = () => import("../../layouts/AdminLayout.vue");
const AdminHomePage = () => import("../../pages/admin/AdminHomePage.vue");
const AdminModerationPage = () =>
  import("../../pages/admin/AdminModerationPage.vue");
const AdminOverviewPage = () =>
  import("../../pages/admin/AdminOverviewPage.vue");
const AdminSchedulePage = () =>
  import("../../pages/admin/AdminSchedulePage.vue");
const AdminUserNameFilterPage = () =>
  import("../../pages/admin/AdminUserNameFilterPage.vue");
const AdminWordsPage = () => import("../../pages/admin/AdminWordsPage.vue");
const AdminWordUploadsPage = () =>
  import("../../pages/admin/AdminWordUploadsPage.vue");
const ArcadeCreatePage = () => import("../../pages/ArcadeCreatePage.vue");
const ArcadeGamePage = () => import("../../pages/ArcadeGamePage.vue");
const ArcadeHistoryPage = () => import("../../pages/ArcadeHistoryPage.vue");
const ArcadeLeaderboardPage = () =>
  import("../../pages/ArcadeLeaderboardPage.vue");
const DailyGamePage = () => import("../../pages/DailyGamePage.vue");
const DailyHistoryPage = () => import("../../pages/DailyHistoryPage.vue");
const DailyLeaderboardPage = () =>
  import("../../pages/DailyLeaderboardPage.vue");
const SharedDailyGamePage = () => import("../../pages/SharedDailyGamePage.vue");
const HomePage = () => import("../../pages/HomePage.vue");
const LoginPage = () => import("../../pages/LoginPage.vue");
const ProfilePage = () => import("../../pages/ProfilePage.vue");

const router = createRouter({
  history: createWebHistory(),
  scrollBehavior(to, from, savedPosition) {
    if (savedPosition) {
      return savedPosition;
    }

    return getRoutePageIdentity(to) === getRoutePageIdentity(from)
      ? false
      : { top: 0 };
  },
  routes: [
    {
      path: "/",
      name: "home",
      component: HomePage,
      meta: { pageTitle: "нетослово.рф", showBack: true },
    },
    {
      path: "/game/arcade/new",
      name: "arcade-create",
      component: ArcadeCreatePage,
      meta: { pageTitle: "Новая игра", showBack: true },
    },
    {
      path: "/game/arcade/:id?",
      name: "arcade-game",
      component: ArcadeGamePage,
      meta: { pageTitle: "Случайное слово", showBack: true },
    },
    {
      path: "/arcade/leaderboard",
      name: "arcade-leaderboard",
      component: ArcadeLeaderboardPage,
      meta: { pageTitle: "Лучшие игроки случайных слов", showBack: true },
    },
    {
      path: "/daily/leaderboard",
      name: "daily-leaderboard",
      component: DailyLeaderboardPage,
      meta: { pageTitle: "Лучшие игроки слова дня", showBack: true },
    },
    {
      path: "/s/:publicId",
      alias: "/daily/shared/:publicId",
      name: "shared-daily",
      component: SharedDailyGamePage,
      meta: { pageTitle: "Результат слова дня", showBack: true },
    },
    {
      path: "/daily/:day",
      name: "daily",
      component: DailyGamePage,
      meta: { pageTitle: "Слово дня", showBack: true },
    },
    {
      path: "/history/arcade",
      name: "arcade-history",
      component: ArcadeHistoryPage,
      meta: { pageTitle: "История случайных слов", showBack: true },
    },
    {
      path: "/history/daily",
      name: "daily-history",
      component: DailyHistoryPage,
      meta: { pageTitle: "История слов дня", showBack: true },
    },
    {
      path: "/login",
      name: "login",
      component: LoginPage,
      meta: { pageTitle: "Вход", showBack: true },
    },
    {
      path: "/profile",
      name: "profile",
      component: ProfilePage,
      meta: { pageTitle: "Профиль", requiresAuth: true, showBack: true },
    },
    {
      path: "/admin",
      component: AdminLayout,
      redirect: { name: "admin-home" },
      meta: {
        requiresAuth: true,
        requiredRole: adminRoles.admin,
        wideContent: true,
        showBack: true,
      },
      children: [
        {
          path: "dashboard",
          name: "admin-home",
          component: AdminHomePage,
          meta: { pageTitle: "Администрирование" },
        },
        {
          path: adminDestinations.dailyOverview.path,
          name: adminDestinations.dailyOverview.name,
          component: AdminOverviewPage,
          meta: {
            pageTitle: "Обзор игр дня",
            requiredPermission: adminDestinations.dailyOverview.permission,
          },
        },
        {
          path: adminDestinations.dailyWords.path,
          name: adminDestinations.dailyWords.name,
          component: AdminWordsPage,
          meta: {
            pageTitle: "Рассмотренные слова",
            requiredPermission: adminDestinations.dailyWords.permission,
          },
        },
        {
          path: adminDestinations.dailyModeration.path,
          name: adminDestinations.dailyModeration.name,
          component: AdminModerationPage,
          meta: {
            pageTitle: "Модерация",
            requiredPermission: adminDestinations.dailyModeration.permission,
          },
        },
        {
          path: adminDestinations.dailySchedule.path,
          name: adminDestinations.dailySchedule.name,
          component: AdminSchedulePage,
          meta: {
            pageTitle: "Расписание",
            requiredPermission: adminDestinations.dailySchedule.permission,
          },
        },
        {
          path: adminDestinations.dictionaryUploads.path,
          name: adminDestinations.dictionaryUploads.name,
          component: AdminWordUploadsPage,
          meta: {
            pageTitle: "Загрузка слов",
            requiredPermission: adminDestinations.dictionaryUploads.permission,
          },
        },
        {
          path: adminDestinations.usersNameFilter.path,
          name: adminDestinations.usersNameFilter.name,
          component: AdminUserNameFilterPage,
          meta: {
            pageTitle: "Фильтр UserName",
            requiredPermission: adminDestinations.usersNameFilter.permission,
          },
        },
      ],
    },
    {
      path: "/:pathMatch(.*)*",
      redirect: { name: "home" },
    },
  ],
});

setUnauthorizedResponseHandler(() => {
  if (!isAuthenticated.value) {
    return;
  }

  const returnTo = getProtectedReturnTo();
  void reconcileAuthenticatedSession()
    .then((session) => {
      if (session?.isAuthenticated !== true) {
        returnToLogin(returnTo);
      }
    })
    .catch(() => {
      returnToLogin(returnTo);
    });
});

setActorStampChangeHandler((change) => {
  const protectedReturnTo = getProtectedReturnTo();
  const unexpectedGuestReturnTo = getUnexpectedGuestReturnTo(change);
  const currentKind = getActorKind(change.current);
  if (currentKind !== null && consumeExpectedActorChange(currentKind)) {
    return;
  }

  void reconcileActorStampChange()
    .then((session) => {
      const unexpectedlyBecameGuest =
        changedFromAuthenticatedToGuest(change) &&
        session.isAuthenticated === false;

      if (unexpectedlyBecameGuest) {
        showSessionEnded();
        returnToLogin(unexpectedGuestReturnTo);
        return;
      }

      leaveInvalidProtectedRoute(protectedReturnTo);
    })
    .catch(() => {
      if (changedFromAuthenticatedToGuest(change)) {
        showSessionEnded();
        returnToLogin(unexpectedGuestReturnTo);
        return;
      }

      returnToLogin(protectedReturnTo);
    });
});

function getActorKind(stamp: string) {
  if (stamp === "guest") {
    return "guest";
  }

  return stamp.startsWith("authenticated:") ? "authenticated" : null;
}

function changedFromAuthenticatedToGuest(change: ActorStampChange) {
  return (
    change.previous.startsWith("authenticated:") && change.current === "guest"
  );
}

function showSessionEnded() {
  showToast({
    status: "info",
    title: "Сессия завершена",
    message: "Ваша сессия была завершена. Войдите снова, чтобы продолжить.",
  });
}

function getUnexpectedGuestReturnTo(change: ActorStampChange) {
  if (!changedFromAuthenticatedToGuest(change)) {
    return null;
  }

  const currentRoute = router.currentRoute.value;
  return currentRoute.name === "login" ? null : currentRoute.fullPath;
}

function getProtectedReturnTo() {
  const currentRoute = router.currentRoute.value;
  const requiresAuth = currentRoute.matched.some(
    (route) => route.meta.requiresAuth,
  );
  return requiresAuth ? currentRoute.fullPath : null;
}

function leaveInvalidProtectedRoute(returnTo: string | null) {
  if (!returnTo) {
    return;
  }

  if (!isAuthenticated.value) {
    returnToLogin(returnTo);
    return;
  }

  const currentRoute = router.currentRoute.value;
  const requiredRole = currentRoute.matched.find(
    (route) => typeof route.meta.requiredRole === "string",
  )?.meta.requiredRole;
  const requiredPermission = currentRoute.matched.find(
    (route) => typeof route.meta.requiredPermission === "string",
  )?.meta.requiredPermission;

  if (
    (typeof requiredRole === "string" &&
      !hasRole(authState.value, requiredRole)) ||
    (typeof requiredPermission === "string" &&
      !hasPermission(authState.value, requiredPermission))
  ) {
    void router.replace({ name: "home", query: { accessDenied: "admin" } });
  }
}

function returnToLogin(returnTo: string | null) {
  if (!returnTo) {
    return;
  }

  void router.replace({
    name: "login",
    query: { returnTo },
  });
}

router.beforeEach(async (to) => {
  routeNavigationLoadingValue.value = true;

  const requiresAuth = to.matched.some((route) => route.meta.requiresAuth);
  const requiredRole = to.matched.find(
    (route) => typeof route.meta.requiredRole === "string",
  )?.meta.requiredRole;
  const requiredPermission = to.matched.find(
    (route) => typeof route.meta.requiredPermission === "string",
  )?.meta.requiredPermission;

  if (authBootstrapState.value === "error") {
    try {
      await bootstrapAuthState();
    } catch {
      return requiresAuth || requiredRole || requiredPermission
        ? { name: "home" }
        : true;
    }
  }

  if (!requiresAuth && !requiredRole && !requiredPermission) {
    return true;
  }

  try {
    await bootstrapAuthState();
  } catch {
    return { name: "home" };
  }

  if (!isAuthenticated.value) {
    return { name: "login" };
  }

  if (
    typeof requiredRole === "string" &&
    !hasRole(authState.value, requiredRole)
  ) {
    return { name: "home", query: { accessDenied: "admin" } };
  }

  if (
    typeof requiredPermission === "string" &&
    !hasPermission(authState.value, requiredPermission)
  ) {
    return { name: "home", query: { accessDenied: "admin" } };
  }

  return true;
});

router.afterEach((to, _from, failure) => {
  routeNavigationLoadingValue.value = false;

  if (!failure) {
    document.title = getDocumentTitle(to);
  }
});

router.onError((_error, to) => {
  routeNavigationLoadingValue.value = false;
  showToast({
    status: "error",
    title: "Навигация",
    message: "Не удалось открыть страницу. Попробуйте ещё раз.",
    actionLabel: "Повторить",
    onAction: () => window.location.assign(router.resolve(to).href),
  });
});

export default router;
