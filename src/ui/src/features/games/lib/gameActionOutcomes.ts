import { matchesApiError, type ApiError } from "../../../shared/api/apiError";

export type GameActionKind = "guess" | "surrender" | "halfway-word" | "word-length" | "random-letter";
export type DictionaryChangedAction = "guess" | "hint";

export type GameActionResult =
  | { status: "success" }
  | { status: "dictionary-changed"; action: DictionaryChangedAction }
  | { status: "source-removed" }
  | { status: "failed"; title: string; message: string; wordNotFound?: boolean };

export function shouldRefreshGameAfterActionError(error: ApiError) {
  return matchesApiError(error, 409, "GameSourceNotFound")
    || matchesApiError(error, 409, "WordsVersionChanged")
    || matchesApiError(error, 409, "GameStateChanged");
}

export function classifyGameActionError(
  action: GameActionKind,
  error: ApiError,
  word?: string,
): GameActionResult {
  if (matchesApiError(error, 409, "WordsVersionChanged")) {
    return {
      status: "dictionary-changed",
      action: action === "guess" ? "guess" : "hint",
    };
  }

  if (matchesApiError(error, 409, "GameSourceNotFound")) {
    return { status: "source-removed" };
  }

  const title = getActionTitle(action);
  const message = getActionErrorMessage(action, error, word);

  return {
    status: "failed",
    title,
    message,
    ...(action === "guess" && matchesApiError(error, 404, "WordNotFound") && { wordNotFound: true }),
  };
}

function getActionTitle(action: GameActionKind) {
  switch (action) {
    case "guess": return "Попытка";
    case "surrender": return "Завершение игры";
    case "halfway-word": return "Промежуточное слово";
    case "word-length": return "Длина слова";
    case "random-letter": return "Случайная буква";
  }
}

function getActionErrorMessage(action: GameActionKind, error: ApiError, word?: string) {
  if (matchesApiError(error, 404, "GameNotFound")) return "Игра не найдена";
  if (matchesApiError(error, 409, "GameStateChanged")) return "Состояние игры изменилось, попробуйте еще раз";
  if (matchesApiError(error, 409, "GameFinished")) return "Игра уже завершена";

  switch (action) {
    case "guess":
      if (matchesApiError(error, 404, "WordNotFound")) return `Слово "${word ?? ""}" не найдено`;
      if (matchesApiError(error, 422, "InvalidInput")) return `Некорректное слово: "${word ?? ""}"`;
      break;
    case "halfway-word":
      if (matchesApiError(error, 409, "TooCloseToTarget")) {
        return "Нельзя использовать эту подсказку, если вы уже находитесь на расстоянии 1 от загаданного слова";
      }
      if (matchesApiError(error, 409, "UsageLimit")) return "Вы уже использовали все подсказки этого типа";
      break;
    case "word-length":
      if (matchesApiError(error, 409, "LengthAlreadyRevealed")) return "Длина слова уже открыта";
      break;
    case "random-letter":
      if (matchesApiError(error, 409, "LengthShouldBeRevealedFirst")) return "Сначала откройте длину слова";
      if (matchesApiError(error, 409, "RevealLetterLimit")) return "Все доступные буквы уже открыты";
      break;
    case "surrender":
      break;
  }

  return "Произошла ошибка обработки запроса, попробуйте позже";
}
