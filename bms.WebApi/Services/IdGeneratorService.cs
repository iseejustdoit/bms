using bms.Leaf;
using bms.WebApi.Common;
using bms.WebApi.Protos;
using Grpc.Core;

namespace bms.WebApi.Services
{
    /// <summary>
    /// 生成ID的服务。
    /// </summary>
    /// <remarks>
    /// 初始化 <see cref="IdGeneratorService"/> 类的新实例。
    /// </remarks>
    /// <param name="idGens">ID生成器的集合。</param>
    public class IdGeneratorService(IEnumerable<IIDGen> idGens) : IdGenerator.IdGeneratorBase
    {

        /// <summary>
        /// 获取段ID。
        /// </summary>
        /// <param name="request">键请求。</param>
        /// <param name="context">服务器调用上下文。</param>
        /// <returns>ID结果。</returns>
        public override async Task<IdResult> GetSegmentId(KeyRequest request, ServerCallContext context)
        {
            var _idGen = idGens.FirstOrDefault(p => p.Name == "Segment") ?? throw new InvalidOperationException("Segment ID generator not found.");
            var id = ResultParser.ParseResult(request.Key, await _idGen.GetAsync(request.Key));
            return new IdResult { Id = id };
        }

        /// <summary>
        /// 获取雪花ID。
        /// </summary>
        /// <param name="request">键请求。</param>
        /// <param name="context">服务器调用上下文。</param>
        /// <returns>ID结果。</returns>
        public override async Task<IdResult> GetSnowflakeId(KeyRequest request, ServerCallContext context)
        {
            var _idGen = idGens.FirstOrDefault(p => p.Name == "Snowflake") ?? throw new InvalidOperationException("Snowflake ID generator not found.");
            var id = ResultParser.ParseResult(request.Key, await _idGen.GetAsync(request.Key));
            return new IdResult { Id = id };
        }
    }
}
