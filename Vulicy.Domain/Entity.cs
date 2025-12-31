namespace Vulicy.Domain;

public interface IEntity
{
    DateTime CreatedDateTime { get; set; }
    DateTime ModifiedDateTime { get; set; }
}

public interface IHistoricEntity<TKey>
{
    TKey Id { get; set; }
    DateTime ChangeDateTime { get; set; }
}

public class Entity<TKey> : IEntity
{
    public TKey Id { get; set; } = default!;
    public DateTime CreatedDateTime { get; set; }
    public DateTime ModifiedDateTime { get; set; }
}
