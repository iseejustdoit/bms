﻿namespace bms.Leaf.Common
{
    public class ZeroIDGen : IIDGen
    {
        public string Name => "Zero";

        public async Task<Result> GetAsync(string key, CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(new Result(0, Status.SUCCESS));
        }

        public async Task<bool> InitAsync(CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(true);
        }
    }
}
