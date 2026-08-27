namespace WordoGuessr.Game.App.UseCases.Admin.Daily.Schedule.AssignSourceToDay;

public enum AssignSourceToDayError
{
    ApprovedGameSourceNotFound,
    DayFromPastLocked,
    ConcurrencyConflict
}