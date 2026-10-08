import type { Difficulty, DisplayWordCell, HintsInfo } from "../model/game";

export type GuessStatusDto = "Guessed" | "NotGuessed" | "AlreadyTried";
export type GuessSourceDto = "Player" | "Hint";
export type GameStateDto = "Active" | "Surrendered" | "Guessed" | "Cancelled";
export type HintTypeDto = "RevealHalfwayWord" | "RevealLength" | "RevealLetter";
export type DailyGameStreakTierDto =
  | "None"
  | "Started"
  | "Steady"
  | "Week"
  | "TwoWeeks"
  | "Month"
  | "Season"
  | "Century"
  | "Legend";

export type GuessDto = {
  word: string;
  distance: number;
  fillPercentage: number;
  source: GuessSourceDto;
};

export type GuessListEntryDto = {
  guess: GuessDto;
  order: number;
};

export type DisplayWordDto = {
  cells: DisplayWordCell[] | null;
};

export type SecretWordUnavailableReasonDto =
  | "GameInProgress"
  | "GameCancelled"
  | "SurrenderedHiddenForToday";
export type GameWordUnavailableReasonDto = "GameCancelled";
export type GameWordDto =
  | {
    status: "display";
    displayWord: DisplayWordDto;
    secretWordUnavailableReason: SecretWordUnavailableReasonDto;
  }
  | { status: "secret"; word: string }
  | {
    status: "displayAndSecret";
    displayWord: DisplayWordDto;
    secretWord: string;
  }
  | { status: "unavailable"; reason: GameWordUnavailableReasonDto };

export type UsedHintDto = { type: HintTypeDto; penalty: number; usedAt: string };
export type ScoreDetailsDto = { guessesCount: number; usedHints: readonly UsedHintDto[] };

export type GuessOutcomeDto = {
  guessStatus: GuessStatusDto;
  currentGuess: GuessDto;
  allGuesses: GuessListEntryDto[];
  score: number;
  scoreDetails: ScoreDetailsDto;
};

export type GuessHintDto = { guessOutcome: GuessOutcomeDto; hintsInfo: HintsInfo };
export type TextHintDto = {
  displayWord: DisplayWordDto;
  hintsInfo: HintsInfo;
  score: number;
  scoreDetails: ScoreDetailsDto;
};

export type GameDto = {
  id: string;
  difficulty: Difficulty;
  gameState: GameStateDto;
  currentGuess: GuessDto | null;
  allGuesses: GuessListEntryDto[];
  gameWord: GameWordDto;
  hintInfo: HintsInfo;
  score: number;
  scoreDetails: ScoreDetailsDto;
};

export type DailyGameDto = {
  day: string;
  state: GameStateDto | null;
  guessedAtGameDay?: boolean | null;
  isToday: boolean;
  gameWord: GameWordDto | null;
};

export type DailyGamesHistoryDto = { games: DailyGameDto[]; hasMore: boolean };

export type DailyGameStreakDto = {
  streak: number;
  tier: DailyGameStreakTierDto;
};

export type SharedGuessDto = {
  word: string | null;
  distance: number;
  order: number;
  fillPercentage: number;
  source: GuessSourceDto;
};

export type VisibleSharedGuessDto = Omit<SharedGuessDto, "word"> & { word: string };
export type HiddenSharedGuessDto = Omit<SharedGuessDto, "word">;
export type SharedGameSpoilersHideReasonDto =
  | "ViewerGameNotFinished"
  | "ViewerSurrenderedHiddenForToday";
export type SharedDailyGameSpoilersDto =
  | {
    visibility: "visible";
    gameWord: GameWordDto;
    guesses: VisibleSharedGuessDto[];
  }
  | {
    visibility: "hidden";
    reason: SharedGameSpoilersHideReasonDto;
    guesses: HiddenSharedGuessDto[];
  };

export type SharedDailyGameDto = {
  gameState: GameStateDto;
  allGuesses: SharedGuessDto[];
  spoilers: SharedDailyGameSpoilersDto;
  score: number;
  scoreDetails: ScoreDetailsDto;
  playerName: string;
  day: string;
  playerStats: DailyGamePlayerStatsDto | null;
  gameStats: DailyGameStatsDto | null;
};

export type DailyGameShareDto = { publicId: string };
export type GetDailyGameShareResponse = { share: DailyGameShareDto | null };
export type ShareDailyGameResponse = { publicId: string };
export type GetSharedDailyGameResponse = { game: SharedDailyGameDto };

export type DailyGameStatsDto = {
  medianScore: number;
  medianAttempts: number;
  medianDuration: string;
};

export type DailyGamePlayerStatsDto = {
  score: number;
  attemptsCount: number;
  duration: string;
  scoreBetterThanPercent: number;
  attemptsCountBetterThanPercent: number;
  durationBetterThanPercent: number;
};

export type ArcadeGameDto = {
  id: string;
  difficulty: Difficulty;
  createdAt: string;
  state: GameStateDto;
  gameWord: GameWordDto;
};

export type ArcadeGamesHistoryDto = { games: ArcadeGameDto[]; hasMore: boolean };
export type GetLatestActiveArcadeSingleGameResponse = { game: ArcadeGameDto | null };
export type GetByIdArcadeSingleGameResponse = { game: GameDto | null };
export type GetTodayDailyGameResponse = { game: DailyGameDto | null };
export type GetDailyGameForDayResponse = { game: GameDto | null };
export type CreateArcadeGameRequest = { difficultyCode: string };
export type MakeGuessRequest = { word: string };
