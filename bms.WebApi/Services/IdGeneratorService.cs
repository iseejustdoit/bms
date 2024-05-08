using bms.Leaf;
using bms.WebApi.Common;
using bms.WebApi.Protos;
using Grpc.Core;

namespace bms.WebApi.Services
{
    public class IdGeneratorService : IdGenerator.IdGeneratorBase
    {
        private readonly IEnumerable<IIDGen> _idGens;
        public IdGeneratorService(IEnumerable<IIDGen> idGens)
        {
            _idGens = idGens;
        }
        public override async Task<IdResult> GetSegmentId(KeyRequest request, ServerCallContext context)
        {
            var _idGen = _idGens.FirstOrDefault(p => p.Name == "Segment");
            var id = ResultParser.ParseResult(request.Key, await _idGen.GetAsync(request.Key));
            return new IdResult { Id = id };
        }

        public override async Task<IdResult> GetSnowflakeId(KeyRequest request, ServerCallContext context)
        {
            var _idGen = _idGens.FirstOrDefault(p => p.Name == "Snowflake");
            var id = ResultParser.ParseResult(request.Key, await _idGen.GetAsync(request.Key));
            return new IdResult { Id = id };
        }
    }
}
