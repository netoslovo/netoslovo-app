import type {
  ArcadeGameDto,
  ArcadeGamesHistoryDto,
  DailyGameDto,
  DailyGameStreakDto,
  DailyGameStreakTierDto,
  DailyGamesHistoryDto,
  DisplayWordDto,
  DisplayWordHideReasonDto,
  GameDto,
  GameStateDto,
  GuessDto,
  GuessHintDto,
  GuessOutcomeDto,
  GuessSourceDto,
  GuessStatusDto,
  HintTypeDto,
  ScoreDetailsDto,
  SharedDailyGameDto,
  SharedGameSpoilersHideReasonDto,
  SharedGuessDto,
  TextHintDto,
  UsedHintDto,
} from "../api/gameDto";
import type {
  ArcadeGame,
  ArcadeGamesHistory,
  DailyGame,
  DailyGameStreak,
  DailyGameStreakTier,
  DailyGamesHistory,
  DisplayWord,
  DisplayWordHideReason,
  Game,
  GameState,
  Guess,
  GuessHint,
  GuessOutcome,
  GuessSource,
  GuessStatus,
  HintType,
  ScoreDetails,
  SharedDailyGame,
  SharedGameSpoilersHideReason,
  SharedGuess,
  TextHint,
  UsedHint,
} from "./game";

const guessStatuses: Record<GuessStatusDto, GuessStatus> = {
  Guessed: "guessed",
  NotGuessed: "notGuessed",
  AlreadyTried: "alreadyTried",
};
const guessSources: Record<GuessSourceDto, GuessSource> = {
  Player: "player",
  Hint: "hint",
};
const gameStates: Record<GameStateDto, GameState> = {
  Active: "active",
  Surrendered: "surrendered",
  Guessed: "guessed",
  Cancelled: "cancelled",
};
const dailyGameStreakTiers: Record<DailyGameStreakTierDto, DailyGameStreakTier> = {
  None: "none",
  Started: "started",
  Steady: "steady",
  Week: "week",
  TwoWeeks: "twoWeeks",
  Month: "month",
  Season: "season",
  Century: "century",
  Legend: "legend",
};
const hintTypes: Record<HintTypeDto, HintType> = {
  RevealHalfwayWord: "revealHalfwayWord",
  RevealLength: "revealLength",
  RevealLetter: "revealLetter",
};
const displayWordHideReasons: Record<DisplayWordHideReasonDto, DisplayWordHideReason> = {
  HiddenForToday: "hiddenForToday",
};
const sharedGameSpoilersHideReasons: Record<
  SharedGameSpoilersHideReasonDto,
  SharedGameSpoilersHideReason
> = {
  ViewerGameNotFinished: "viewerGameNotFinished",
  HiddenForToday: "hiddenForToday",
};

function mapKnownValue<T extends string>(values: Record<string, T>, raw: string, field: string): T {
  const mapped = values[raw];
  if (mapped === undefined) throw new Error(`Unknown ${field}: ${raw}`);
  return mapped;
}

export function mapGuess(dto: GuessDto): Guess {
  return {
    word: dto.word,
    distance: dto.distance,
    fillPercentage: dto.fillPercentage,
    source: mapKnownValue(guessSources, dto.source, "guess source"),
  };
}
export function mapDisplayWord(dto: DisplayWordDto): DisplayWord {
  return {
    cells: dto.cells ?? null,
    hideReason: dto.hideReason
      ? mapKnownValue(displayWordHideReasons, dto.hideReason, "display word hide reason")
      : null,
  };
}
function mapUsedHint(dto: UsedHintDto): UsedHint {
  return {
    type: mapKnownValue(hintTypes, dto.type, "hint type"),
    penalty: dto.penalty,
    usedAt: dto.usedAt,
  };
}
function mapScoreDetails(dto: ScoreDetailsDto): ScoreDetails {
  return { guessesCount: dto.guessesCount, usedHints: dto.usedHints.map(mapUsedHint) };
}
function mapSharedGuess(dto: SharedGuessDto): SharedGuess {
  return {
    word: dto.word,
    distance: dto.distance,
    order: dto.order,
    fillPercentage: dto.fillPercentage,
    source: mapKnownValue(guessSources, dto.source, "guess source"),
  };
}
export function mapSharedDailyGame(dto: SharedDailyGameDto): SharedDailyGame {
  return {
    gameState: mapKnownValue(gameStates, dto.gameState, "game state"),
    allGuesses: dto.allGuesses.map(mapSharedGuess),
    displayWord: dto.displayWord ? mapDisplayWord(dto.displayWord) : null,
    score: dto.score,
    scoreDetails: mapScoreDetails(dto.scoreDetails),
    playerName: dto.playerName,
    spoilersHideReason: dto.spoilersHideReason
      ? mapKnownValue(
        sharedGameSpoilersHideReasons,
        dto.spoilersHideReason,
        "shared game spoilers hide reason",
      )
      : null,
    day: dto.day,
    playerStats: dto.playerStats,
    gameStats: dto.gameStats,
  };
}
export function mapGuessOutcome(dto: GuessOutcomeDto): GuessOutcome {
  return {
    guessStatus: mapKnownValue(guessStatuses, dto.guessStatus, "guess status"),
    currentGuess: mapGuess(dto.currentGuess),
    allGuesses: dto.allGuesses.map(mapGuess),
    score: dto.score,
    scoreDetails: mapScoreDetails(dto.scoreDetails),
  };
}
export function mapGuessHint(dto: GuessHintDto): GuessHint {
  return { guessOutcome: mapGuessOutcome(dto.guessOutcome), hintsInfo: dto.hintsInfo };
}
export function mapTextHint(dto: TextHintDto): TextHint {
  return {
    displayWord: mapDisplayWord(dto.displayWord),
    hintsInfo: dto.hintsInfo,
    score: dto.score,
    scoreDetails: mapScoreDetails(dto.scoreDetails),
  };
}
export function mapDailyGame(dto: DailyGameDto): DailyGame {
  return {
    day: dto.day,
    state: dto.state ? mapKnownValue(gameStates, dto.state, "game state") : null,
    guessedAtGameDay: dto.guessedAtGameDay ?? null,
    isToday: dto.isToday,
    word: dto.word ? mapDisplayWord(dto.word) : null,
  };
}
export function mapDailyGamesHistory(dto: DailyGamesHistoryDto): DailyGamesHistory {
  return { games: dto.games.map(mapDailyGame), hasMore: dto.hasMore };
}
export function mapDailyGameStreak(dto: DailyGameStreakDto): DailyGameStreak {
  return {
    streak: dto.streak,
    tier: mapKnownValue(dailyGameStreakTiers, dto.tier, "daily game streak tier"),
  };
}
export function mapArcadeGame(dto: ArcadeGameDto): ArcadeGame {
  return {
    id: dto.id,
    difficulty: dto.difficulty,
    createdAt: dto.createdAt,
    state: mapKnownValue(gameStates, dto.state, "game state"),
    word: dto.word ? mapDisplayWord(dto.word) : null,
  };
}
export function mapArcadeGamesHistory(dto: ArcadeGamesHistoryDto): ArcadeGamesHistory {
  return { games: dto.games.map(mapArcadeGame), hasMore: dto.hasMore };
}
export function mapGame(dto: GameDto): Game {
  return {
    id: dto.id,
    difficulty: dto.difficulty,
    gameState: mapKnownValue(gameStates, dto.gameState, "game state"),
    currentGuess: dto.currentGuess ? mapGuess(dto.currentGuess) : null,
    allGuesses: dto.allGuesses.map(mapGuess),
    displayWord: mapDisplayWord(dto.displayWord),
    hintsInfo: dto.hintInfo,
    score: dto.score,
    scoreDetails: mapScoreDetails(dto.scoreDetails),
  };
}
