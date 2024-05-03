using bms.Leaf;
using bms.Leaf.Common;
using bms.Leaf.Segment;
using bms.Leaf.Snowflake;

namespace bms.WebApi.Service
{
    public class ServiceFactory
    {
        private readonly Dictionary<string, IDGen> _idGens = new Dictionary<string, IDGen>();

        private readonly IServiceScopeFactory _scopeFactory;

        public ServiceFactory(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;

            var idGenSegment = Create("Segment");
            idGenSegment.InitAsync().Await();
            _idGens["Segment"] = idGenSegment;

            var idGenSnowflake = Create("Snowflake");
            idGenSnowflake.InitAsync().Await();
            _idGens["Snowflake"] = idGenSnowflake;
        }
        public IDGen Get(string name)
        {
            return _idGens[name];
        }
        public IDGen Create(string name)
        {
            using var scope = _scopeFactory.CreateScope();
            var provider = scope.ServiceProvider;
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
    }
}
