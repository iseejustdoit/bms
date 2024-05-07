using bms.Leaf.Common;

namespace bms.Leaf
{
    public interface IIDGen
    {
        string Name { get; }
        Task<Result> GetAsync(string key, CancellationToken cancellationToken = default);
        Task<bool> InitAsync(CancellationToken cancellationToken = default);
    }
}
