using bms.Leaf.Common;

namespace bms.Leaf.SnowFlake
{
    public class SnowFlakeIDGenImpl : IDGen
    {
        public SnowFlakeIDGenImpl()
        {

        }

        public Task<Result> GetAsync(string key, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> InitAsync(CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(true);
        }
    }
}
