import {
  apiClient,
  type ApiRequestOptions,
  type ApiRequestPolicy,
  withRequestPolicy,
} from "../../../shared/api/httpClient";
import type {
  WordsVersionUpload,
  WordsVersionUploadHistorySortField,
  WordsVersionUploadPage,
  WordsVersionUploadSortDirection,
} from "../model/wordUploads";

const sagasAdminUrl = "/api/sagas/admin";
const maxPageSize = 50;

const requestPolicies = {
  createWordsVersionUpload: { timeoutMs: 20_000, retries: 0 },
  getWordsVersionUploads: { timeoutMs: 15_000, retries: 2 },
  getWordsVersionUpload: { timeoutMs: 8_000, retries: 1 },
  getWordsVersionUploadFromHistory: { timeoutMs: 10_000, retries: 1 },
  getWordsVersionUploadHistory: { timeoutMs: 15_000, retries: 2 },
  cancelWordsVersionUpload: { timeoutMs: 10_000, retries: 0 },
} satisfies Record<string, ApiRequestPolicy>;

type GetWordsVersionUploadResponse = { upload: WordsVersionUpload | null };
type GetWordsVersionUploadFromHistoryResponse = {
  upload: WordsVersionUpload | null;
};

export type GetWordsVersionUploadsParams = {
  skip: number;
  take: number;
};

export type GetWordsVersionUploadHistoryParams = {
  skip: number;
  take: number;
  from?: string;
  to?: string;
  wordsVersion?: number;
  sortBy?: WordsVersionUploadHistorySortField;
  sortDirection?: WordsVersionUploadSortDirection;
};

export async function createWordsVersionUpload(
  version: number,
): Promise<string> {
  const response = await apiClient.post<string>(
    `${sagasAdminUrl}/words-version-uploads`,
    { version },
    withRequestPolicy(requestPolicies.createWordsVersionUpload),
  );
  return response.data;
}

export async function getWordsVersionUploads(
  params: GetWordsVersionUploadsParams,
  options?: ApiRequestOptions,
): Promise<WordsVersionUploadPage> {
  const response = await apiClient.get<WordsVersionUploadPage>(
    `${sagasAdminUrl}/words-version-uploads`,
    {
      ...withRequestPolicy(requestPolicies.getWordsVersionUploads, options),
      params: normalizePageParams(params),
    },
  );
  return response.data;
}

export async function getWordsVersionUpload(
  sagaId: string,
  options?: ApiRequestOptions,
): Promise<WordsVersionUpload | null> {
  const response = await apiClient.get<GetWordsVersionUploadResponse>(
    `${sagasAdminUrl}/words-version-uploads/${sagaId}`,
    withRequestPolicy(requestPolicies.getWordsVersionUpload, options),
  );
  return response.data.upload;
}

export async function getWordsVersionUploadFromHistory(
  sagaId: string,
  options?: ApiRequestOptions,
): Promise<WordsVersionUpload | null> {
  const response = await apiClient.get<GetWordsVersionUploadFromHistoryResponse>(
    `${sagasAdminUrl}/words-version-uploads/history/${sagaId}`,
    withRequestPolicy(
      requestPolicies.getWordsVersionUploadFromHistory,
      options,
    ),
  );
  return response.data.upload;
}

export async function getWordsVersionUploadHistory(
  params: GetWordsVersionUploadHistoryParams,
  options?: ApiRequestOptions,
): Promise<WordsVersionUploadPage> {
  const response = await apiClient.get<WordsVersionUploadPage>(
    `${sagasAdminUrl}/words-version-uploads/history`,
    {
      ...withRequestPolicy(requestPolicies.getWordsVersionUploadHistory, options),
      params: normalizeHistoryParams(params),
    },
  );
  return response.data;
}

export async function cancelWordsVersionUpload(sagaId: string): Promise<void> {
  await apiClient.post(
    `${sagasAdminUrl}/words-version-uploads/${sagaId}/cancel`,
    undefined,
    withRequestPolicy(requestPolicies.cancelWordsVersionUpload),
  );
}

function normalizePageParams(params: GetWordsVersionUploadsParams) {
  return {
    skip: params.skip,
    take: Math.min(params.take, maxPageSize),
  };
}

function normalizeHistoryParams(
  params: GetWordsVersionUploadHistoryParams,
) {
  return {
    ...normalizePageParams(params),
    from: params.from || undefined,
    to: params.to || undefined,
    wordsVersion: params.wordsVersion,
    sortBy: params.sortBy,
    sortDirection: params.sortDirection,
  };
}
