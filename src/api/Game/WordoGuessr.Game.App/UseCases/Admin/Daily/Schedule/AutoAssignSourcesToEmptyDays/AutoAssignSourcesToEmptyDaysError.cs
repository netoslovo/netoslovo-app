namespace WordoGuessr.Game.App.UseCases.Admin.Daily.Schedule.AutoAssignSourcesToEmptyDays;

public enum AutoAssignSourcesToEmptyDaysError
{
    NotEnoughApprovedSources,
    TooWideInterval,
    ConcurrencyConflict
}