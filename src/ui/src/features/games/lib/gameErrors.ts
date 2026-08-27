import { matchesApiError, type ApiError } from "../../../shared/api/apiError";

export function getCreateGameErrorMessage(error: ApiError) {
  if (
    matchesApiError(error, 409, "AlreadyHasTheSameGame") ||
    matchesApiError(error, 409, "AlreadyHasActiveGame")
  ) {
    return "У вас уже есть активная игра";
  }
  if (matchesApiError(error, 404, "NoMorePossibleGames")) return "Нет доступных игр для выбранной сложности";
  return "Произошла ошибка обработки запроса, попробуйте позже";
}

export function isDailyScheduleNotFound(error: unknown) {
  return (
    matchesApiError(error, 404, "ScheduleNotFound") ||
    matchesApiError(error, 404, "ChallengeNotFound")
  );
}
