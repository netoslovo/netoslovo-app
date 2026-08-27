import {
  apiClient,
  type ApiRequestPolicy,
  withRequestPolicy,
} from "../../../shared/api/httpClient";

const requestPolicies = {
  shouldShowUserNotice: { timeoutMs: 2_000, retries: 1 },
  saveOneTimeUserNoticeView: { timeoutMs: 5_000, retries: 0 },
  saveRecurringUserNoticeView: { timeoutMs: 5_000, retries: 0 },
} satisfies Record<string, ApiRequestPolicy>;

export async function shouldShowUserNotice(
  noticeCode: string,
): Promise<boolean> {
  const response = await apiClient.get<boolean>(
    "/api/auth/user-notices/should-show",
    {
      ...withRequestPolicy(requestPolicies.shouldShowUserNotice),
      params: { noticeCode },
    },
  );
  return response.data;
}

export async function saveOneTimeUserNoticeView(
  noticeCode: string,
): Promise<void> {
  await apiClient.post<void>(
    "/api/auth/user-notice/views/one-time",
    { noticeCode },
    withRequestPolicy(requestPolicies.saveOneTimeUserNoticeView),
  );
}

export async function saveRecurringUserNoticeView(
  noticeCode: string,
  doNotShowAgain: boolean,
): Promise<void> {
  await apiClient.post<void>(
    "/api/auth/user-notices/views/recurring",
    { noticeCode, doNotShowAgain },
    withRequestPolicy(requestPolicies.saveRecurringUserNoticeView),
  );
}
