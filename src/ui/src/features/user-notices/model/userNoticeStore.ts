import { authState } from "../../auth/model/authSession";
import {
  saveOneTimeUserNoticeView as saveOneTimeUserNoticeViewRequest,
  saveRecurringUserNoticeView as saveRecurringUserNoticeViewRequest,
  shouldShowUserNotice as shouldShowUserNoticeRequest,
} from "../api/userNoticeApi";
import type { UserNoticeCode } from "./userNoticeCodes";

const cachedVisibility = new Map<UserNoticeCode, boolean>();
const pendingVisibility = new Map<UserNoticeCode, Promise<boolean>>();
const pendingOneTimeViews = new Map<UserNoticeCode, Promise<boolean>>();
let cacheOwnerId: string | null = null;
let cacheRevision = 0;

function syncCacheOwner() {
  const ownerId = authState.value?.id ?? null;
  if (ownerId === cacheOwnerId) {
    return ownerId;
  }

  cacheOwnerId = ownerId;
  cacheRevision += 1;
  cachedVisibility.clear();
  pendingVisibility.clear();
  pendingOneTimeViews.clear();
  return ownerId;
}

async function shouldShowUserNotice(code: UserNoticeCode): Promise<boolean> {
  const ownerId = syncCacheOwner();
  if (ownerId === null) {
    return false;
  }

  const cached = cachedVisibility.get(code);
  if (cached !== undefined) {
    return cached;
  }

  const pending = pendingVisibility.get(code);
  if (pending) {
    return pending;
  }

  const requestRevision = cacheRevision;
  const request = shouldShowUserNoticeRequest(code)
    .then((shouldShow) => {
      syncCacheOwner();
      if (
        cacheOwnerId !== ownerId ||
        cacheRevision !== requestRevision
      ) {
        return false;
      }

      cachedVisibility.set(code, shouldShow);
      return shouldShow;
    })
    .catch(() => false)
    .finally(() => {
      if (pendingVisibility.get(code) === request) {
        pendingVisibility.delete(code);
      }
    });

  pendingVisibility.set(code, request);
  return request;
}

function saveOneTimeUserNoticeView(code: UserNoticeCode): Promise<boolean> {
  const ownerId = syncCacheOwner();
  if (ownerId === null) return Promise.resolve(false);
  if (cachedVisibility.get(code) === false) return Promise.resolve(true);

  const pending = pendingOneTimeViews.get(code);
  if (pending) return pending;

  const request = saveOneTimeUserNoticeViewRequest(code)
    .then(() => markHiddenForOwner(code, ownerId))
    .catch(() => false)
    .finally(() => {
      if (pendingOneTimeViews.get(code) === request) {
        pendingOneTimeViews.delete(code);
      }
    });
  pendingOneTimeViews.set(code, request);
  return request;
}

function saveRecurringUserNoticeView(
  code: UserNoticeCode,
  doNotShowAgain: boolean,
): Promise<void> {
  const ownerId = syncCacheOwner();
  return saveRecurringUserNoticeViewRequest(code, doNotShowAgain)
    .then(() => {
      if (doNotShowAgain) {
        markHiddenForOwner(code, ownerId);
      }
    });
}

function markHiddenForOwner(
  code: UserNoticeCode,
  requestOwnerId: string | null,
): boolean {
  syncCacheOwner();
  if (requestOwnerId === null || cacheOwnerId !== requestOwnerId) {
    return false;
  }

  cacheRevision += 1;
  pendingVisibility.clear();
  cachedVisibility.set(code, false);
  return true;
}

const userNoticeStore = {
  shouldShowUserNotice,
  saveOneTimeUserNoticeView,
  saveRecurringUserNoticeView,
};

export function useUserNoticeStore() {
  return userNoticeStore;
}
