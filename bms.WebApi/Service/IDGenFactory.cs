﻿using bms.Leaf;
using bms.Leaf.Common;
using bms.Leaf.Segment;
using bms.Leaf.Snowflake;

namespace bms.WebApi.Service
{
    public class IDGenFactory : IDisposable
    {
        private readonly Dictionary<string, IIDGen> _idGens = new Dictionary<string, IIDGen>();
        private readonly IServiceScope _scope;
        private List<string> factoryList;

        public IDGenFactory(IServiceProvider scopeFactory)
        {
            _scope = scopeFactory.CreateScope();

            factoryList = new List<string> { "Segment", "Snowflake" };

            foreach (var factory in factoryList)
            {
                var idGen = Create(factory);
                idGen.InitAsync().Await();
                _idGens[factory] = idGen;
            }
        }

        public IIDGen Get(string name)
        {
            return _idGens[name];
        }
        private IIDGen Create(string name)
        {
            var provider = _scope.ServiceProvider;
            switch (name)
            {
                case "Segment":
                    return provider.GetRequiredService<SegmentIDGenImpl>();
                case "Snowflake":
                    return provider.GetRequiredService<SnowflakeIDGenImpl>();
                default:
                    return provider.GetRequiredService<ZeroIDGen>();
            }
        }
        public void Dispose()
        {
            _scope.Dispose();
        }
    }
}
