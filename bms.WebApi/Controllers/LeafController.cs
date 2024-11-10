using bms.Leaf;
using bms.WebApi.Common;
using bms.WebApi.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace bms.WebApi.Controllers
{
    /// <summary>
    /// 叶子控制器
    /// </summary>
    /// <remarks>
    /// 构造函数
    /// </remarks>
    /// <param name="idGens">ID生成器集合</param>
    [ApiController]
    public class LeafController(IEnumerable<IIDGen> idGens) : ControllerBase
    {
        private readonly IIDGen _segmentIdGen = idGens.FirstOrDefault(p => p.Name == "Segment") ?? throw new ArgumentNullException(nameof(idGens), "Segment ID generator not found");
        private readonly IIDGen _snowflakeIdGen = idGens.FirstOrDefault(p => p.Name == "Snowflake") ?? throw new ArgumentNullException(nameof(idGens), "Snowflake ID generator not found");

        /// <summary>
        /// 获取号段ID
        /// </summary>
        /// <param name="key">业务Key</param>
        /// <returns>返回号段ID</returns>
        [HttpGet("api/segment/get/{key}")]
        public async Task<IActionResult> GetSegmentId(string key)
        {
            return await ParseAsync(async () =>
            {
                return Ok(ResultParser.ParseResult(key, await _segmentIdGen.GetAsync(key)));
            });
        }

        /// <summary>
        /// 获取雪花ID
        /// </summary>
        /// <param name="key">业务Key(可以随便填)</param>
        /// <returns>返回雪花ID</returns>
        [HttpGet("api/snowflake/get/{key}")]
        public async Task<IActionResult> GetSnowflakeId(string key)
        {
            return await ParseAsync(async () =>
            {
                return Ok(ResultParser.ParseResult(key, await _snowflakeIdGen.GetAsync(key)));
            });
        }

        /// <summary>
        /// 解析异步操作
        /// </summary>
        /// <param name="func">异步操作函数</param>
        /// <returns>返回操作结果</returns>
        private async Task<IActionResult> ParseAsync(Func<Task<IActionResult>> func)
        {
            try
            {
                return await func.Invoke();
            }
            catch (NoKeyException)
            {
                return BadRequest("Key is none");
            }
            catch (LeafServerException ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
