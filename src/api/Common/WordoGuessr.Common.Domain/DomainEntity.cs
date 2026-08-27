namespace WordoGuessr.Common.Domain;

public abstract class DomainEntity<TId> where TId : notnull
{
    public TId Id { get; } = default!;
    private List<IDomainEvent> _domainEvents { get; } = [];
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public DomainEntity(TId id)
    {
        Id = id;
    }

    protected DomainEntity() { }

    public override bool Equals(object? obj)
    {
        return obj is DomainEntity<TId> other
            && GetType() == other.GetType()
            && EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(GetType(), Id);
    }

    public static bool operator ==(
        DomainEntity<TId>? left,
        DomainEntity<TId>? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(
        DomainEntity<TId>? left,
        DomainEntity<TId>? right)
    {
        return !Equals(left, right);
    }

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

}