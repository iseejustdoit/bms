using bms.Leaf.Common;

namespace bms.Leaf.Snowflake
{
    public class SnowflakeIDGenImpl : IDGen
    {
        public SnowflakeIDGenImpl()
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
