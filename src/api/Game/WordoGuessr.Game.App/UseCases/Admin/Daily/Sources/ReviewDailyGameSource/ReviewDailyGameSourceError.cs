namespace WordoGuessr.Game.App.UseCases.Admin.Daily.Sources.ReviewDailyGameSource;

public enum ReviewDailyGameSourceError
{
    GameSourceNotFound,
    AlreadyAssignedToGameLocked,
    ConcurrencyConflict
}
