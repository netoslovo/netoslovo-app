import { computed, readonly, ref } from "vue";
import {
  getCurrentAuthState,
  logout,
  requestEmailOtp,
  verifyEmailOtp,
} from "../api/authApi";
import { toApiError } from "../../../shared/api/apiError";
import type { AuthSession } from "./auth";

export type OtpRequestResult =
  | { status: "completed"; challengeId: string }
  | { status: "alreadyAuthenticated" };

export type OtpVerifyResult =
  | { status: "completed" }
  | { status: "alreadyAuthenticated" }
  | { status: "otpNotFound" }
  | { status: "invalidOtpState" }
  | { status: "invalidGuestSession" }
  | { status: "emailAlreadyRegistered" }
  | { status: "concurrencyFailure" }
  | { status: "getOrAddUserError" };

export type OtpVerifyFailureStatus = Exclude<
  OtpVerifyResult["status"],
  "completed" | "alreadyAuthenticated"
>;

type VerifyEmailOtpErrorCode =
  | "OtpNotFound"
  | "InvalidOtpState"
  | "AlreadyAuthenticated"
  | "InvalidGuestSession"
  | "EmailAlreadyRegistered"
  | "ConcurrencyFailure"
  | "GetOrAddUserError";

const authStateValue = ref<AuthSession | null>(null);
const sessionRevisionValue = ref(0);
const authBootstrapStateValue = ref<"idle" | "loading" | "ready" | "error">(
  "idle",
);
let authBootstrapPromise: Promise<void> | null = null;
let authRefreshPromise: Promise<AuthSession> | null = null;
let sessionBeforeRefresh: AuthSession | null = null;
let refreshMarksSessionChanged = false;
type ActorKind = "authenticated" | "guest";
let expectedActorChange: { kind: ActorKind; token: symbol } | null = null;
export const authState = readonly(authStateValue);
export const authBootstrapState = readonly(authBootstrapStateValue);
export const sessionRevision = readonly(sessionRevisionValue);
export const isAuthenticated = computed(
  () => authStateValue.value?.isAuthenticated === true,
);

export function bootstrapAuthState() {
  if (authRefreshPromise) {
    return authRefreshPromise.then(() => undefined);
  }

  if (authBootstrapStateValue.value === "ready") {
    return Promise.resolve();
  }

  if (authBootstrapPromise) {
    return authBootstrapPromise;
  }

  authBootstrapStateValue.value = "loading";

  authBootstrapPromise = getCurrentAuthState()
    .then((authSession) => {
      authStateValue.value = authSession;
      authBootstrapStateValue.value = "ready";
    })
    .catch((error: unknown) => {
      authStateValue.value = null;
      authBootstrapStateValue.value = "error";
      throw error;
    })
    .finally(() => {
      authBootstrapPromise = null;
    });

  return authBootstrapPromise;
}

function refreshAuthSession(sessionChanged: boolean) {
  sessionBeforeRefresh ??= authStateValue.value;
  authStateValue.value = null;
  refreshMarksSessionChanged ||= sessionChanged;

  if (authRefreshPromise) {
    return authRefreshPromise;
  }

  authRefreshPromise = getCurrentAuthState()
    .then((authSession) => {
      authStateValue.value = authSession;
      authBootstrapStateValue.value = "ready";
      if (
        refreshMarksSessionChanged ||
        (
          sessionBeforeRefresh !== null &&
          !isSameSession(sessionBeforeRefresh, authSession)
        )
      ) {
        markSessionChanged();
      }
      return authSession;
    })
    .catch((error: unknown) => {
      authBootstrapStateValue.value = "error";
      throw error;
    })
    .finally(() => {
      authRefreshPromise = null;
      sessionBeforeRefresh = null;
      refreshMarksSessionChanged = false;
    });

  return authRefreshPromise;
}

function markSessionChanged() {
  sessionRevisionValue.value += 1;
}

export async function requestLoginCode(
  email: string,
): Promise<OtpRequestResult> {
  try {
    const challengeId = await requestEmailOtp(email);
    return { status: "completed", challengeId };
  } catch (error) {
    if (await recoverAuthenticatedConflict(error)) {
      return { status: "alreadyAuthenticated" };
    }

    throw error;
  }
}

export async function verifyLoginCode(
  challengeId: string,
  code: string,
): Promise<OtpVerifyResult> {
  const expectedChangeToken = beginExpectedActorChange("authenticated");
  try {
    await verifyEmailOtp(challengeId, code);
    await refreshAuthSession(true);
    return { status: "completed" };
  } catch (error) {
    if (await recoverAuthenticatedConflict(error)) {
      return { status: "alreadyAuthenticated" };
    }

    const verifyError = mapVerifyEmailOtpError(toApiError(error).code);
    if (verifyError) {
      return { status: verifyError };
    }

    throw error;
  } finally {
    clearExpectedActorChange(expectedChangeToken);
  }
}

export async function logoutUser() {
  const expectedChangeToken = beginExpectedActorChange("guest");
  try {
    await logout();
    await refreshAuthSession(true);
  } finally {
    clearExpectedActorChange(expectedChangeToken);
  }
}

export async function refreshAuthState() {
  await refreshAuthSession(true);
}

export async function reconcileAuthenticatedSession() {
  if (authStateValue.value?.isAuthenticated !== true) {
    return authStateValue.value;
  }

  return refreshAuthSession(false);
}

export function reconcileActorStampChange() {
  return refreshAuthSession(false);
}

export function consumeExpectedActorChange(kind: ActorKind) {
  if (expectedActorChange?.kind !== kind) {
    return false;
  }

  expectedActorChange = null;
  return true;
}

function beginExpectedActorChange(kind: ActorKind) {
  const token = Symbol();
  expectedActorChange = { kind, token };
  return token;
}

function clearExpectedActorChange(token: symbol) {
  if (expectedActorChange?.token === token) {
    expectedActorChange = null;
  }
}

function isSameSession(
  previous: AuthSession | null,
  current: AuthSession,
) {
  return previous?.isAuthenticated === current.isAuthenticated &&
    previous.id === current.id &&
    previous.userName === current.userName &&
    previous.email === current.email &&
    arraysEqual(previous.roles, current.roles) &&
    arraysEqual(previous.permissions, current.permissions);
}

function arraysEqual(
  left: readonly string[],
  right: readonly string[],
) {
  return left.length === right.length &&
    left.every((value) => right.includes(value));
}

async function recoverAuthenticatedConflict(error: unknown): Promise<boolean> {
  if (toApiError(error).status !== 409) {
    return false;
  }

  const authSession = await refreshAuthSession(false);
  const authenticated = authSession.isAuthenticated;
  return authenticated;
}

function mapVerifyEmailOtpError(
  code: string | undefined,
): Exclude<OtpVerifyResult["status"], "completed"> | null {
  const errorCode = code as VerifyEmailOtpErrorCode | undefined;
  switch (errorCode) {
    case "OtpNotFound":
      return "otpNotFound";
    case "InvalidOtpState":
      return "invalidOtpState";
    case "AlreadyAuthenticated":
      return "alreadyAuthenticated";
    case "InvalidGuestSession":
      return "invalidGuestSession";
    case "EmailAlreadyRegistered":
      return "emailAlreadyRegistered";
    case "ConcurrencyFailure":
      return "concurrencyFailure";
    case "GetOrAddUserError":
      return "getOrAddUserError";
    default:
      return null;
  }
}
