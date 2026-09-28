import {
  type ArcadeGamesHistoryDto,
  type CreateArcadeGameRequest,
  type DailyGameStreakDto,
  type DailyGamesHistoryDto,
  type GameDto,
  type GetByIdArcadeSingleGameResponse,
  type GetDailyGameForDayResponse,
  type GetLatestActiveArcadeSingleGameResponse,
  type GetTodayDailyGameResponse,
  type GuessHintDto,
  type GuessOutcomeDto,
  type MakeGuessRequest,
  type TextHintDto,
  type GetDailyGameShareResponse,
  type GetSharedDailyGameResponse,
  type ShareDailyGameResponse,
} from "./gameDto";
import type {
  ArcadeGame,
  ArcadeGamesHistory,
  ArcadeGameTopPlayers,
  DailyGame,
  DailyGamePlayerStats,
  DailyGameStats,
  DailyGameStreak,
  DailyGameStreakTop,
  DailyGamesHistory,
  Difficulty,
  Game,
  GuessHint,
  GuessOutcome,
  TextHint,
  SharedDailyGame,
} from "../model/game";
import {
  mapArcadeGame,
  mapArcadeGamesHistory,
  mapDailyGame,
  mapDailyGameStreak,
  mapDailyGamesHistory,
  mapGame,
  mapGuessHint,
  mapGuessOutcome,
  mapTextHint,
  mapSharedDailyGame,
} from "../model/gameMappers";
import {
  apiClient,
  type ApiRequestOptions,
  type ApiRequestPolicy,
  withRequestPolicy,
} from "../../../shared/api/httpClient";

type GetDailyGameStatsResponse = { stats: DailyGameStats | null };

const requestPolicies = {
  getDifficulties: { timeoutMs: 8_000, retries: 2 },
  getLatestActiveArcadeGame: { timeoutMs: 7_000, retries: 1 },
  getArcadeGameById: { timeoutMs: 8_000, retries: 1 },
  createGame: { timeoutMs: 12_000, retries: 0 },
  getArcadeHistory: { timeoutMs: 12_000, retries: 2 },
  getArcadeGameTopPlayers: { timeoutMs: 12_000, retries: 2 },
  getDailyHistory: { timeoutMs: 12_000, retries: 2 },
  getTodayDailyGame: { timeoutMs: 7_000, retries: 1 },
  getDailyGameForDay: { timeoutMs: 8_000, retries: 1 },
  getDailyGameStats: { timeoutMs: 10_000, retries: 2 },
  getDailyGamePlayerStats: { timeoutMs: 8_000, retries: 1 },
  getDailyGameCurrentPlayerStreak: { timeoutMs: 10_000, retries: 2 },
  getDailyGameCurrentStreakTop: { timeoutMs: 12_000, retries: 2 },
  getDailyGameLongestStreakTop: { timeoutMs: 12_000, retries: 2 },
  startDailyGameForDay: { timeoutMs: 12_000, retries: 0 },
  makeGuess: { timeoutMs: 15_000, retries: 0 },
  surrender: { timeoutMs: 10_000, retries: 0 },
  revealHalfwayWord: { timeoutMs: 10_000, retries: 0 },
  revealWordLength: { timeoutMs: 10_000, retries: 0 },
  revealRandomLetter: { timeoutMs: 10_000, retries: 0 },
  getDailyGameShare: { timeoutMs: 8_000, retries: 1 },
  shareDailyGame: { timeoutMs: 10_000, retries: 0 },
  unshareDailyGame: { timeoutMs: 10_000, retries: 0 },
  getSharedDailyGame: { timeoutMs: 10_000, retries: 1 },
} satisfies Record<string, ApiRequestPolicy>;

export async function getDifficulties(
  options?: ApiRequestOptions,
): Promise<Difficulty[]> {
  const response = await apiClient.get<Difficulty[]>(
    "/api/games/dictionaries/difficulties",
    withRequestPolicy(requestPolicies.getDifficulties, options),
  );

  return response.data;
}

export async function getLatestActiveArcadeGame(
  options?: ApiRequestOptions,
): Promise<ArcadeGame | null> {
  const response = await apiClient.get<GetLatestActiveArcadeSingleGameResponse>(
    "/api/games/single/arcade/latest-active",
    withRequestPolicy(requestPolicies.getLatestActiveArcadeGame, options),
  );

  return response.data.game ? mapArcadeGame(response.data.game) : null;
}

export async function getArcadeGameById(
  id: string,
  options?: ApiRequestOptions,
): Promise<Game | null> {
  const response = await apiClient.get<GetByIdArcadeSingleGameResponse>(
    `/api/games/single/arcade/${id}`,
    withRequestPolicy(requestPolicies.getArcadeGameById, options),
  );

  return response.data.game ? mapGame(response.data.game) : null;
}

export async function createGame(difficultyCode: string): Promise<Game> {
  const request: CreateArcadeGameRequest = {
    difficultyCode,
  };

  const response = await apiClient.post<GameDto>(
    "/api/games/single/arcade",
    request,
    withRequestPolicy(requestPolicies.createGame),
  );

  return mapGame(response.data);
}

export async function getArcadeHistory(
  skip: number,
  take: number,
  options?: ApiRequestOptions,
): Promise<ArcadeGamesHistory> {
  const response = await apiClient.get<ArcadeGamesHistoryDto>(
    "/api/games/single/arcade",
    {
      ...withRequestPolicy(requestPolicies.getArcadeHistory, options),
      params: {
        skip,
        take,
      },
    },
  );

  return mapArcadeGamesHistory(response.data);
}

export async function getArcadeGameTopPlayers(
  difficultyCode: string,
  topN: number,
  options?: ApiRequestOptions,
): Promise<ArcadeGameTopPlayers> {
  const response = await apiClient.get<ArcadeGameTopPlayers>(
    "/api/games/single/arcade/stats/players/top",
    {
      ...withRequestPolicy(requestPolicies.getArcadeGameTopPlayers, options),
      params: { difficultyCode, topN },
    },
  );

  return response.data;
}

export async function getDailyHistory(
  skip: number,
  take: number,
  options?: ApiRequestOptions,
): Promise<DailyGamesHistory> {
  const response = await apiClient.get<DailyGamesHistoryDto>(
    "/api/games/single/daily",
    {
      ...withRequestPolicy(requestPolicies.getDailyHistory, options),
      params: {
        skip,
        take,
      },
    },
  );

  return mapDailyGamesHistory(response.data);
}

export async function getTodayDailyGame(
  options?: ApiRequestOptions,
): Promise<DailyGame | null> {
  const response = await apiClient.get<GetTodayDailyGameResponse>(
    "/api/games/single/daily/today",
    withRequestPolicy(requestPolicies.getTodayDailyGame, options),
  );

  return response.data.game ? mapDailyGame(response.data.game) : null;
}

export async function getDailyGameForDay(
  day: string,
  options?: ApiRequestOptions,
): Promise<Game | null> {
  const response = await apiClient.get<GetDailyGameForDayResponse>(
    `/api/games/single/daily/${day}`,
    withRequestPolicy(requestPolicies.getDailyGameForDay, options),
  );

  return response.data.game ? mapGame(response.data.game) : null;
}

export async function getDailyGameStats(
  day: string,
  options?: ApiRequestOptions,
): Promise<DailyGameStats | null> {
  const response = await apiClient.get<GetDailyGameStatsResponse>(
    `/api/games/single/daily/${day}/stats`,
    withRequestPolicy(requestPolicies.getDailyGameStats, options),
  );

  return response.data.stats;
}

export async function getDailyGamePlayerStats(
  day: string,
  options?: ApiRequestOptions,
): Promise<DailyGamePlayerStats> {
  const response = await apiClient.get<DailyGamePlayerStats>(
    `/api/games/single/daily/${day}/stats/player`,
    withRequestPolicy(requestPolicies.getDailyGamePlayerStats, options),
  );

  return response.data;
}

export async function getDailyGameCurrentPlayerStreak(
  options?: ApiRequestOptions,
): Promise<DailyGameStreak> {
  const response = await apiClient.get<DailyGameStreakDto>(
    "/api/games/single/daily/stats/player/streak",
    withRequestPolicy(requestPolicies.getDailyGameCurrentPlayerStreak, options),
  );

  return mapDailyGameStreak(response.data);
}

export async function getDailyGameCurrentStreakTop(
  topN: number,
  options?: ApiRequestOptions,
): Promise<DailyGameStreakTop> {
  const response = await apiClient.get<DailyGameStreakTop>(
    "/api/games/single/daily/stats/streak/current/top",
    {
      ...withRequestPolicy(requestPolicies.getDailyGameCurrentStreakTop, options),
      params: { topN },
    },
  );

  return response.data;
}

export async function getDailyGameLongestStreakTop(
  topN: number,
  options?: ApiRequestOptions,
): Promise<DailyGameStreakTop> {
  const response = await apiClient.get<DailyGameStreakTop>(
    "/api/games/single/daily/stats/streak/longest/top",
    {
      ...withRequestPolicy(requestPolicies.getDailyGameLongestStreakTop, options),
      params: { topN },
    },
  );

  return response.data;
}

export async function startDailyGameForDay(
  day: string,
  options?: ApiRequestOptions,
): Promise<Game> {
  const response = await apiClient.post<GameDto>(
    `/api/games/single/daily/${day}`,
    undefined,
    withRequestPolicy(requestPolicies.startDailyGameForDay, options),
  );

  return mapGame(response.data);
}

export async function makeGuess(
  gameId: string,
  word: string,
): Promise<GuessOutcome> {
  const request: MakeGuessRequest = {
    word,
  };

  const response = await apiClient.post<GuessOutcomeDto>(
    `/api/games/single/${gameId}/guesses`,
    request,
    withRequestPolicy(requestPolicies.makeGuess),
  );

  return mapGuessOutcome(response.data);
}

export async function surrender(gameId: string): Promise<string> {
  const response = await apiClient.post<string>(
    `/api/games/single/${gameId}/surrender`,
    undefined,
    withRequestPolicy(requestPolicies.surrender),
  );

  return response.data;
}

export async function revealHalfwayWord(gameId: string): Promise<GuessHint> {
  const response = await apiClient.post<GuessHintDto>(
    `/api/games/single/${gameId}/hints/reveal-halfway-word`,
    undefined,
    withRequestPolicy(requestPolicies.revealHalfwayWord),
  );

  return mapGuessHint(response.data);
}

export async function revealWordLength(gameId: string): Promise<TextHint> {
  const response = await apiClient.post<TextHintDto>(
    `/api/games/single/${gameId}/hints/word-length`,
    undefined,
    withRequestPolicy(requestPolicies.revealWordLength),
  );

  return mapTextHint(response.data);
}

export async function revealRandomLetter(gameId: string): Promise<TextHint> {
  const response = await apiClient.post<TextHintDto>(
    `/api/games/single/${gameId}/hints/random-letter`,
    undefined,
    withRequestPolicy(requestPolicies.revealRandomLetter),
  );

  return mapTextHint(response.data);
}

export async function getDailyGameShare(
  gameId: string,
  options?: ApiRequestOptions,
): Promise<string | null> {
  const response = await apiClient.get<GetDailyGameShareResponse>(
    `/api/games/single/daily/${gameId}/share`,
    withRequestPolicy(requestPolicies.getDailyGameShare, options),
  );

  return response.data.share?.publicId ?? null;
}

export async function shareDailyGame(gameId: string): Promise<string> {
  const response = await apiClient.post<ShareDailyGameResponse>(
    `/api/games/single/daily/${gameId}/share`,
    undefined,
    withRequestPolicy(requestPolicies.shareDailyGame),
  );

  return response.data.publicId;
}

export async function unshareDailyGame(gameId: string): Promise<void> {
  await apiClient.delete(
    `/api/games/single/daily/${gameId}/share`,
    withRequestPolicy(requestPolicies.unshareDailyGame),
  );
}

export async function getSharedDailyGame(
  publicId: string,
  options?: ApiRequestOptions,
): Promise<SharedDailyGame> {
  const response = await apiClient.get<GetSharedDailyGameResponse>(
    `/api/games/single/daily/shared/${publicId}`,
    withRequestPolicy(requestPolicies.getSharedDailyGame, options),
  );

  return mapSharedDailyGame(response.data.game);
}
