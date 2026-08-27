import {
  apiClient,
  type ApiRequestOptions,
  type ApiRequestPolicy,
  withRequestPolicy,
} from "../../../shared/api/httpClient";

const authAdminUrl = "/api/auth/admin";

const requestPolicies = {
  getCurrentUserNameFilterVersion: { timeoutMs: 8_000, retries: 1 },
  publishUserNameFilterVersion: { timeoutMs: 15_000, retries: 0 },
} satisfies Record<string, ApiRequestPolicy>;

export async function getCurrentUserNameFilterVersion(
  options?: ApiRequestOptions,
): Promise<number> {
  const response = await apiClient.get<number>(
    `${authAdminUrl}/username-filter/version/`,
    withRequestPolicy(requestPolicies.getCurrentUserNameFilterVersion, options),
  );
  return response.data;
}

export async function publishUserNameFilterVersion(version: number): Promise<void> {
  await apiClient.post(
    `${authAdminUrl}/username-filter/version/${version}`,
    undefined,
    withRequestPolicy(requestPolicies.publishUserNameFilterVersion),
  );
}
