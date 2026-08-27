import {
  apiClient,
  type ApiRequestOptions,
  type ApiRequestPolicy,
  withRequestPolicy,
} from "../../../shared/api/httpClient";
import type { AuthSession, Profile } from "../model/auth";

const requestPolicies = {
  getCurrentAuthState: { timeoutMs: 5_000, retries: 1 },
  getProfile: { timeoutMs: 8_000, retries: 1 },
  changeUserName: { timeoutMs: 10_000, retries: 0 },
  requestEmailOtp: { timeoutMs: 15_000, retries: 0 },
  verifyEmailOtp: { timeoutMs: 10_000, retries: 0 },
  logout: { timeoutMs: 8_000, retries: 0 },
} satisfies Record<string, ApiRequestPolicy>;

export async function getCurrentAuthState(): Promise<AuthSession> {
  const response = await apiClient.get<AuthSession>(
    "/api/auth/me",
    withRequestPolicy(requestPolicies.getCurrentAuthState),
  );
  return response.data;
}

export async function getProfile(
  options?: ApiRequestOptions,
): Promise<Profile> {
  const response = await apiClient.get<Profile>(
    "/api/auth/profile",
    withRequestPolicy(requestPolicies.getProfile, options),
  );
  return response.data;
}

export async function changeUserName(newUserName: string): Promise<void> {
  await apiClient.put<void>(
    "/api/auth/username",
    { newUserName },
    withRequestPolicy(requestPolicies.changeUserName),
  );
}

export async function requestEmailOtp(email: string): Promise<string> {
  const response = await apiClient.post<string>(
    "/api/auth/email/request-otp",
    { email },
    withRequestPolicy(requestPolicies.requestEmailOtp),
  );
  return response.data;
}

export async function verifyEmailOtp(
  challengeId: string,
  code: string,
): Promise<void> {
  await apiClient.post<void>(
    "/api/auth/email/verify-otp",
    { challengeId, code },
    withRequestPolicy(requestPolicies.verifyEmailOtp),
  );
}

export async function logout(): Promise<void> {
  await apiClient.post(
    "/api/auth/logout",
    undefined,
    withRequestPolicy(requestPolicies.logout),
  );
}
