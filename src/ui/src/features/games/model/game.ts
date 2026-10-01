export type Difficulty = { code: string; name: string };
export type GuessStatus = "guessed" | "notGuessed" | "alreadyTried";
export type GuessSource = "player" | "hint";
export type GameState = "active" | "surrendered" | "guessed" | "cancelled";
export type HintType = "revealHalfwayWord" | "revealLength" | "revealLetter";
export type DailyGameStreakTier =
  | "none"
  | "started"
  | "steady"
  | "week"
  | "twoWeeks"
  | "month"
  | "season"
  | "century"
  | "legend";

export type Guess = {
  word: string;
  distance: number;
  fillPercentage: number;
  source: GuessSource;
};

export type DisplayWordCell = { value: string | null; revealed: boolean };
export type DisplayWordHideReason = "hiddenForToday";
export type DisplayWord = {
  cells: DisplayWordCell[] | null;
};
export type LegacyDisplayWord = DisplayWord & {
  hideReason: DisplayWordHideReason | null;
};
export type SecretWordUnavailableReason =
  | "gameInProgress"
  | "gameCancelled"
  | "surrenderedHiddenForToday";
export type GameWord =
  | {
    status: "display";
    displayWord: DisplayWord;
    secretWordUnavailableReason: SecretWordUnavailableReason;
  }
  | { status: "secret"; secretWord: string }
  | {
    status: "displayAndSecret";
    displayWord: DisplayWord;
    secretWord: string;
  }
  | { status: "unavailable"; reason: "gameCancelled" }
  | { status: "legacy"; displayWord: LegacyDisplayWord };

export type HintsInfo = {
  revealLengthHintUsed: boolean;
  revealLetterHintsTotal: number | null;
  revealLetterHintsLeft: number | null;
  neighbourHintsTotal: number;
  neighbourHintsLeft: number;
  revealLengthHintPenalty: number;
  revealLetterHintPenalties: readonly number[] | null;
  revealHalfwayWordHintPenalties: readonly number[];
};

export type UsedHint = { type: HintType; penalty: number; usedAt: string };
export type ScoreDetails = { guessesCount: number; usedHints: readonly UsedHint[] };

export type GuessOutcome = {
  guessStatus: GuessStatus;
  currentGuess: Guess;
  allGuesses: Guess[];
  score: number;
  scoreDetails: ScoreDetails;
};

export type GuessHint = { guessOutcome: GuessOutcome; hintsInfo: HintsInfo };
export type TextHint = {
  displayWord: DisplayWord;
  hintsInfo: HintsInfo;
  score: number;
  scoreDetails: ScoreDetails;
};

export type Game = {
  id: string;
  difficulty: Difficulty;
  gameState: GameState;
  currentGuess: Guess | null;
  allGuesses: Guess[];
  gameWord: GameWord;
  hintsInfo: HintsInfo;
  score: number;
  scoreDetails: ScoreDetails;
};

export type DailyGame = {
  day: string;
  state: GameState | null;
  guessedAtGameDay: boolean | null;
  isToday: boolean;
  gameWord: GameWord | null;
};

export type DailyGamesHistory = { games: DailyGame[]; hasMore: boolean };

export type DailyGameStreak = {
  streak: number;
  tier: DailyGameStreakTier;
};

export type SharedGameSpoilersHideReason =
  | "viewerGameNotFinished"
  | "hiddenForToday";

export type SharedGuess = {
  word: string | null;
  distance: number;
  order: number;
  fillPercentage: number;
  source: GuessSource;
};

export type SharedDailyGame = {
  gameState: GameState;
  allGuesses: SharedGuess[];
  gameWord: GameWord | null;
  score: number;
  scoreDetails: ScoreDetails;
  playerName: string;
  spoilersHideReason: SharedGameSpoilersHideReason | null;
  day: string;
  playerStats: DailyGamePlayerStats | null;
  gameStats: DailyGameStats | null;
};

export type DailyGameStats = {
  medianScore: number;
  medianAttempts: number;
  medianDuration: string;
};

export type DailyGamePlayerStats = {
  score: number;
  attemptsCount: number;
  duration: string;
  scoreBetterThanPercent: number;
  attemptsCountBetterThanPercent: number;
  durationBetterThanPercent: number;
};

export type DailyGameStreakTopEntry = {
  place: number;
  playerName: string;
  streak: number;
};

export type DailyGameStreakTopCurrentPlayerEntry = {
  playerId: string;
  place: number | null;
  playerName: string;
  streak: number;
};

export type DailyGameStreakTop = {
  top: DailyGameStreakTopEntry[];
  currentPlayerTopInfo: DailyGameStreakTopCurrentPlayerEntry;
};

export type ArcadeGameTopPlayersEntry = {
  place: number;
  playerName: string;
  guessedGames: number;
  averageScore: number;
  averageDuration: string;
};

export type ArcadeGameTopPlayersCurrentPlayerEntry = {
  playerId: string;
  place: number | null;
  playerName: string;
  guessedGames: number;
  averageScore: number | null;
  averageDuration: string | null;
};

export type ArcadeGameTopPlayers = {
  top: ArcadeGameTopPlayersEntry[];
  playerTopInfo: ArcadeGameTopPlayersCurrentPlayerEntry;
};

export type ArcadeGame = {
  id: string;
  difficulty: Difficulty;
  createdAt: string;
  state: GameState;
  gameWord: GameWord | null;
};

export type ArcadeGamesHistory = { games: ArcadeGame[]; hasMore: boolean };
