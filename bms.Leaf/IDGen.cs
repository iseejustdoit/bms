using bms.Leaf.Common;

namespace bms.Leaf
{
    public interface IDGen
    {
        Task<Result> GetAsync(string key, CancellationToken cancellationToken = default);
        Task<bool> InitAsync(CancellationToken cancellationToken = default);
    }
}
