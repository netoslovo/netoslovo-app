import type {
  AdminGameSourceWithReview,
  DailyGameSchedules,
  DailyGameSourceReviews,
  GameSourceReviewSortField,
  SortDirection,
  UnreviewedSource,
} from "../model/admin";
import {
  apiClient,
  type ApiRequestOptions,
  type ApiRequestPolicy,
  withRequestPolicy,
} from "../../../shared/api/httpClient";

const adminDailyUrl = "/api/games/admin/daily";

const requestPolicies = {
  getDailySourceReviews: { timeoutMs: 15_000, retries: 2 },
  getApprovedUnassignedDailySourcesCount: { timeoutMs: 8_000, retries: 1 },
  getRandomUnreviewedGameSourceId: { timeoutMs: 8_000, retries: 1 },
  getGameSourceForReview: { timeoutMs: 15_000, retries: 1 },
  getSourceByWord: { timeoutMs: 10_000, retries: 1 },
  reviewDailyGameSource: { timeoutMs: 10_000, retries: 0 },
  getDailySchedule: { timeoutMs: 15_000, retries: 2 },
  getUnassignedDailyScheduleCount: { timeoutMs: 8_000, retries: 1 },
  assignSourceToDay: { timeoutMs: 10_000, retries: 0 },
  unassignSourceFromDay: { timeoutMs: 10_000, retries: 0 },
  autoAssignSources: { timeoutMs: 30_000, retries: 0 },
} satisfies Record<string, ApiRequestPolicy>;

type GetRandomUnreviewedGameSourceIdResponse = {
  gameSourceId: number | null;
};

export type GetReviewsParams = {
  skip: number;
  take: number;
  sortBy: GameSourceReviewSortField;
  sortDirection: SortDirection;
  difficultyCodeFilter?: string;
  existsInLatestVersionFilter?: boolean;
  approvedFilter?: boolean;
  scheduledFilter?: boolean;
  wordFilter?: string;
};

export async function getDailySourceReviews(
  params: GetReviewsParams,
  options?: ApiRequestOptions,
): Promise<DailyGameSourceReviews> {
  const response = await apiClient.get<DailyGameSourceReviews>(
    `${adminDailyUrl}/sources/reviews`,
    {
      ...withRequestPolicy(requestPolicies.getDailySourceReviews, options),
      params,
    },
  );
  return response.data;
}

export async function getApprovedUnassignedDailySourcesCount(
  options?: ApiRequestOptions,
): Promise<number> {
  const response = await apiClient.get<number>(
    `${adminDailyUrl}/sources/approved-unassigned/count`,
    withRequestPolicy(
      requestPolicies.getApprovedUnassignedDailySourcesCount,
      options,
    ),
  );
  return response.data;
}

export async function getRandomUnreviewedGameSourceId(
  difficultyCode: string,
  options?: ApiRequestOptions,
): Promise<number | null> {
  const response = await apiClient.get<GetRandomUnreviewedGameSourceIdResponse>(
    `${adminDailyUrl}/sources/unreviewed/random-id`,
    {
      ...withRequestPolicy(
        requestPolicies.getRandomUnreviewedGameSourceId,
        options,
      ),
      params: { difficultyCode },
    },
  );
  return response.data.gameSourceId;
}

export async function getGameSourceForReview(
  gameSourceId: number,
  closestWordsCount: number,
  options?: ApiRequestOptions,
): Promise<UnreviewedSource> {
  const response = await apiClient.get<UnreviewedSource>(
    `${adminDailyUrl}/sources/${gameSourceId}/for-review`,
    {
      ...withRequestPolicy(requestPolicies.getGameSourceForReview, options),
      params: { closestWordsCount },
    },
  );
  return response.data;
}

export async function getSourceByWord(
  word: string,
  options?: ApiRequestOptions,
): Promise<AdminGameSourceWithReview> {
  const response = await apiClient.get<AdminGameSourceWithReview>(
    `${adminDailyUrl}/sources/by-word`,
    {
      ...withRequestPolicy(requestPolicies.getSourceByWord, options),
      params: { word },
    },
  );
  return response.data;
}

export async function reviewDailyGameSource(
  gameSourceId: number,
  approved: boolean,
): Promise<void> {
  await apiClient.put(
    `${adminDailyUrl}/sources/${gameSourceId}/review`,
    { approved },
    withRequestPolicy(requestPolicies.reviewDailyGameSource),
  );
}

export type GetScheduleParams = {
  from: string;
  to: string;
  skip: number;
  take: number;
  sortDirection: SortDirection;
  assignedFilter?: boolean;
};

export async function getDailySchedule(
  params: GetScheduleParams,
  options?: ApiRequestOptions,
): Promise<DailyGameSchedules> {
  const response = await apiClient.get<DailyGameSchedules>(
    `${adminDailyUrl}/schedule`,
    {
      ...withRequestPolicy(requestPolicies.getDailySchedule, options),
      params,
    },
  );
  return response.data;
}

export async function getUnassignedDailyScheduleCount(
  from: string,
  to: string,
  options?: ApiRequestOptions,
): Promise<number> {
  const response = await apiClient.get<number>(
    `${adminDailyUrl}/schedule/unassigned/count`,
    {
      ...withRequestPolicy(
        requestPolicies.getUnassignedDailyScheduleCount,
        options,
      ),
      params: { from, to },
    },
  );
  return response.data;
}

export async function assignSourceToDay(
  day: string,
  approvedGameSourceId: number,
): Promise<void> {
  await apiClient.put(
    `${adminDailyUrl}/schedule/${day}/source/${approvedGameSourceId}`,
    undefined,
    withRequestPolicy(requestPolicies.assignSourceToDay),
  );
}

export async function unassignSourceFromDay(day: string): Promise<void> {
  await apiClient.delete(
    `${adminDailyUrl}/schedule/${day}/source`,
    withRequestPolicy(requestPolicies.unassignSourceFromDay),
  );
}

export async function autoAssignSources(
  from: string,
  to: string,
): Promise<void> {
  await apiClient.post(`${adminDailyUrl}/sources/auto-assign`, undefined, {
    ...withRequestPolicy(requestPolicies.autoAssignSources),
    params: { from, to },
  });
}
