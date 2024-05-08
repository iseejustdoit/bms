
namespace bms.Leaf.SnowFlake
{
    public interface ISnowflakeRedisHolder
    {
        int GetWorkerId();
        Task<bool> InitAsync(CancellationToken cancellationToken);
    }
}
