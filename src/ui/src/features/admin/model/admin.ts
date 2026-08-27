import type { Difficulty } from "../../games/model/game";

export type AdminGameSource = {
  gameSourceId: number;
  word: string;
  difficulty: Difficulty;
};

export type AdminGameSourceWithReview = {
  gameSource: AdminGameSource;
  approved: boolean | null;
};

export type UnreviewedSource = {
  gameSource: AdminGameSource;
  closestWords: string[];
};

export type DailyGameSourceReview = {
  gameSource: AdminGameSource;
  existsInLatestVersion: boolean;
  approved: boolean;
  scheduled: boolean;
  createdAt: string;
  updatedAt: string;
};

export type DailyGameSourceReviews = {
  reviews: DailyGameSourceReview[];
  hasMore: boolean;
};

export type DailyGameSchedule = {
  day: string;
  gameSource: AdminGameSource | null;
  locked: boolean;
};

export type DailyGameSchedules = {
  schedules: DailyGameSchedule[];
  hasMore: boolean;
};

export type GameSourceReviewSortField =
  | "CreatedAt"
  | "UpdatedAt"
  | "GameSourceId";

export type SortDirection = "Asc" | "Desc";
