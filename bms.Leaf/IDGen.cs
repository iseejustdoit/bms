using bms.Leaf.Common;

namespace bms.Leaf
{
    public interface IDGen
    {
        Task<Result> GetAsync(string key);
        Task<bool> InitAsync();
    }
}
