namespace WordoGuessr.Game.Domain;

public sealed class SingleGameDailySchedule
{
    public DateOnly Day { get; }
    public long ApprovedGameSourceId { get; private set; }
    public ApprovedDailyGameSource ApprovedGameSource { get; } = null!;
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public SingleGameDailySchedule(long approvedGameSourceId, DateOnly day, DateTimeOffset createdAt)
    {
        ApprovedGameSourceId = approvedGameSourceId;
        Day = day;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    private SingleGameDailySchedule() { }

    public void Assign(long approvedGameSourceId, DateTimeOffset at)
    {
        ApprovedGameSourceId = approvedGameSourceId;
        UpdatedAt = at;
    }
}
