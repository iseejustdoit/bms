namespace bms.Leaf.Common
{
    public class ZeroIDGen : IDGen
    {
        public async Task<Result> GetAsync(string key)
        {
            return await Task.FromResult(new Result(0, Status.SUCCESS));
        }

        public async Task<bool> InitAsync()
        {
            return await Task.FromResult(true);
        }
    }
}
